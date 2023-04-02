// <copyright file="LanguageProcessor.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Processors
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Numerics;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using Azure;
    using Azure.Storage.Files.Shares;
    using Legendary.Core;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Data.Contracts;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Extensions;
    using Legendary.Engine.Helpers;
    using Legendary.Engine.Models;
    using Legendary.Engine.Models.Output;
    using Legendary.Engine.Output;
    using Microsoft.AspNetCore.DataProtection.KeyManagement;
    using Newtonsoft.Json;
    using RestSharp;
    using static System.Net.Mime.MediaTypeNames;
    using static System.Runtime.InteropServices.JavaScript.JSType;

    /// <summary>
    /// Processes an input, and returns an AI response.
    /// </summary>
    public partial class LanguageProcessor : ILanguageProcessor
    {
        private readonly ILogger logger;
        private readonly IRandom random;
        private readonly ICommunicator communicator;
        private readonly IServerSettings serverSettings;
        private readonly ILanguageGenerator generator;
        private readonly IEnvironment environment;
        private readonly IDataService dataService;
        private readonly string url = "https://api.openai.com/v1/chat/completions";
        private readonly string imageUrl = "https://api.openai.com/v1/images/generations";
        private Dictionary<Mobile, List<dynamic>> mobileTrainingData;
        private Dictionary<Mobile, bool> processingDictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageProcessor"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="serverSettings">The server settings.</param>
        /// <param name="generator">The language generator.</param>
        /// <param name="communicator">The communicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="dataService">The data service.</param>
        public LanguageProcessor(ILogger logger, IServerSettings serverSettings, ILanguageGenerator generator, ICommunicator communicator, IRandom random, IEnvironment environment, IDataService dataService)
        {
            this.logger = logger;
            this.serverSettings = serverSettings;
            LoadParser();
            this.generator = generator;
            this.random = random;
            this.communicator = communicator;
            this.environment = environment;
            this.dataService = dataService;
            this.mobileTrainingData = new Dictionary<Mobile, List<dynamic>>();
            this.processingDictionary = new Dictionary<Mobile, bool>();
        }

        /// <summary>
        /// Reloads the parser to capture any updates.
        /// </summary>
        public static void LoadParser()
        {
            var parserContent = File.ReadAllText(@"Data/parser.json");

            var parser = JsonConvert.DeserializeObject<Parser>(parserContent);
        }

        /// <summary>
        /// Processes the message.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="mobiles">The mobiles.</param>
        /// <param name="input">The input string.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A tuple of messages and the mobile generating them.</returns>
        public async Task<(string[]?, Mobile?)> Process(Character character, List<Mobile> mobiles, string input, CancellationToken cancellationToken = default)
        {
            Mobile? engagedMobile = null;
            var maxChance = 0;

            // Get the mobile that has the highest chance of engagement.
            foreach (var mobile in mobiles)
            {
                if (mobile.UseAI)
                {
                    // Result is true/false if the mob will engage, and the % chance.
                    var engageResult = this.WillEngage(character, mobile, input);
                    if (engageResult.Item1 && engageResult.Item2 > maxChance)
                    {
                        engagedMobile = mobile;
                        maxChance = engageResult.Item2;
                    }
                }
            }

            if (engagedMobile != null)
            {
                if (character != null && !string.IsNullOrWhiteSpace(character.FirstName) && engagedMobile != null && !string.IsNullOrWhiteSpace(engagedMobile.FirstName))
                {
                    var persona = Persona.Load(engagedMobile);

                    if (persona != null && !string.IsNullOrWhiteSpace(persona.Name))
                    {
                        this.logger.Debug($"{persona.Name ?? engagedMobile.FirstName.FirstCharToUpper()} will engage with {character.FirstName}.", this.communicator);

                        // We haven't trained this mobile yet, so train it.
                        if (!this.mobileTrainingData.ContainsKey(engagedMobile))
                        {
                            this.mobileTrainingData.Add(engagedMobile, await this.Train(persona, character, engagedMobile));
                        }

                        await this.communicator.SendToPlayer(character, $"<span class='chat-bubble-{engagedMobile.CharacterId}'>{persona.Name ?? engagedMobile.FirstName.FirstCharToUpper()}<img class='typing' src='img/typing.gif'></span>", cancellationToken);

                        // Keep the AI mobs single-threaded, or things go off the rails fast.
                        if (!this.processingDictionary.ContainsKey(engagedMobile))
                        {
                            this.processingDictionary.Add(engagedMobile, true);
                        }
                        else
                        {
                            // If the mobile is currently engaged (in the middle of sending an API call) just return null.
                            if (this.processingDictionary[engagedMobile] == true)
                            {
                                return (null, null);
                            }

                            this.processingDictionary[engagedMobile] = true;
                        }

                        // Chat response for the mobile with the training data.
                        var message = await this.Chat(character, this.mobileTrainingData[engagedMobile], CleanInput(input, character.FirstName));

                        // See things are getting...interesting.
                        await this.CheckConvertToXMob(message, engagedMobile);

                        // Parse all of the resulting language.
                        try
                        {
                            // Send a message to the UI to clear the speec bubble.
                            await this.communicator.SendToPlayer(character, $"CLEARCHAT:{engagedMobile.CharacterId}", cancellationToken);

                            // Clean and process all of the output from the AI engine.
                            var result = (ProcessOutput(message, persona, engagedMobile), engagedMobile);

                            if (result.Item1 != null)
                            {
                                // Add the input as a memory, only if the mob actually created a response.
                                try
                                {
                                    await this.dataService.AddMemory(character, engagedMobile, $"{input}");
                                }
                                catch (Exception exc)
                                {
                                    this.logger.Error($"AddMemory: {exc}", this.communicator);
                                }
                            }

                            return result;
                        }
                        catch (Exception exc)
                        {
                            await this.communicator.SendToPlayer(character, $"{persona.Name ?? engagedMobile.FirstName.FirstCharToUpper()} looks a bit confused.", cancellationToken);
                            this.logger.Error(exc, this.communicator);
                            return (null, null);
                        }
                        finally
                        {
                            // No longer processing, so set to false;
                            this.processingDictionary[engagedMobile] = false;
                        }
                    }
                    else
                    {
                        this.logger.Debug($"{engagedMobile.FirstName.FirstCharToUpper()} did not have a valid persona file or it could not be loaded.", this.communicator);
                        return (null, null);
                    }
                }
                else
                {
                    this.logger.Debug($"Target or mobile was null.", this.communicator);
                    return (null, null);
                }
            }
            else
            {
                // Let's just return a random mobile.
                var randomMobile = mobiles[this.random.Next(0, mobiles.Count - 1)];
                return (null, randomMobile);
            }
        }

        /// <summary>
        /// If a mob doesn't perform a verbal response, it may execute an emote.
        /// </summary>
        /// <param name="character">The actor.</param>
        /// <param name="mobile">The target.</param>
        /// <param name="input">The input.</param>
        /// <returns>String.</returns>
        public string? ProcessEmote(Character character, Mobile mobile, string input)
        {
            // Process actions for humanoids.
            if ((int)mobile.Race <= 13)
            {
                if (this.random.Next(0, 100) < 10)
                {
                    this.logger.Debug($"{mobile.FirstName.FirstCharToUpper()} did a random action.", this.communicator);

                    string action = string.Empty;

                    switch (mobile.Emotion)
                    {
                        default:
                        case Core.Types.Emotion.Neutral:
                            action = Constants.EMOTE_ACTION_NEUTRAL[this.random.Next(0, Constants.EMOTE_ACTION_NEUTRAL.Count - 1)];
                            break;
                        case Core.Types.Emotion.Happy:
                            action = Constants.EMOTE_ACTION_HAPPY[this.random.Next(0, Constants.EMOTE_ACTION_HAPPY.Count - 1)];
                            break;
                        case Core.Types.Emotion.Angry:
                            action = Constants.EMOTE_ACTION_ANGRY[this.random.Next(0, Constants.EMOTE_ACTION_ANGRY.Count - 1)];
                            break;
                        case Core.Types.Emotion.Sad:
                            action = Constants.EMOTE_ACTION_SAD[this.random.Next(0, Constants.EMOTE_ACTION_SAD.Count - 1)];
                            break;
                    }

                    if (!PlayerHelper.CanPlayerSeePlayer(this.environment, this.communicator, character, mobile))
                    {
                        action = action.Replace("{0}", "Someone");
                        action = action.Replace("{1}", "their");
                    }
                    else
                    {
                        action = action.Replace("{0}", mobile.FirstName.FirstCharToUpper());
                        action = action.Replace("{1}", mobile.Pronoun);
                    }

                    return action;
                }
                else if (this.random.Next(0, 100) < 5)
                {
                    // Do a random emote.
                    var emote = Emotes.Get(this.random.Next(0, Emotes.Actions.Count - 1));

                    if (emote != null)
                    {
                        if (!PlayerHelper.CanPlayerSeePlayer(this.environment, this.communicator, character, mobile))
                        {
                            return emote.ToRoom.Replace("{0}", "Someone");
                        }
                        else
                        {
                            return emote.ToRoom.Replace("{0}", mobile.FirstName.FirstCharToUpper());
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                if (this.random.Next(0, 100) < 10)
                {
                    this.logger.Debug($"{mobile.FirstName.FirstCharToUpper()} did a random animal action.", this.communicator);
                    var action = Constants.EMOTE_ANIMAL_ACTION[this.random.Next(0, Constants.EMOTE_ANIMAL_ACTION.Count - 1)];

                    action = action.Replace("{0}", mobile.FirstName.FirstCharToUpper());
                    action = action.Replace("{1}", mobile.Pronoun);

                    return action;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Generates an image for a character based on their description.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <returns>URL to the image (when complete).</returns>
        public async Task<string?> GenerateImage(Character character)
        {
            if (string.IsNullOrWhiteSpace(character.LongDescription))
            {
                return null;
            }

            string desc = character.LongDescription.Substring(0, Math.Min(character.LongDescription.Length, 300));

            var request = new
            {
                prompt = $"Generate a photorealistic painting of a {character.Age} year old {character.Race} {character.Gender} that looks like the following description: {desc}",
                n = 1,
                size = "512x512",
            };

            var imageUrl = string.Empty;

            using (var httpClient = new HttpClient())
            {
                // Send the request and capture the response
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {this.serverSettings.ChatGPTAPIKey}");

                var requestJson = JsonConvert.SerializeObject(request);
                var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");
                requestContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                using var httpResponseMessage = await httpClient.PostAsync(this.imageUrl, requestContent);

                var jsonString = await httpResponseMessage.Content.ReadAsStringAsync();

                var responseObject = JsonConvert.DeserializeAnonymousType(jsonString, new
                {
                    data = new[] { new { url = string.Empty } },
                });

                imageUrl = responseObject.data[0].url;
            }

            // Upload to Azure and store.
            using (var httpClient = new HttpClient())
            {
                using (var stream = await httpClient.GetStreamAsync(imageUrl))
                {
                    using (var mstream = new MemoryStream())
                    {
                        await stream.CopyToAsync(mstream);
                        mstream.Seek(0, SeekOrigin.Begin);

                        var avatarImage = await this.UploadAvatarImage(character, mstream);

                        return avatarImage;
                    }
                }
            }
        }

        /// <summary>
        /// Sends a message to the ChatGPT server to process as a chat.
        /// </summary>
        /// <param name="character">The character speaking to the chat bot.</param>
        /// <param name="trainingData">The training data.</param>
        /// <param name="message">The message to send.</param>
        /// <returns>String.</returns>
        public async Task<string> Chat(Character character, List<dynamic> trainingData, string message)
        {
            trainingData.Add(new { role = "user", content = $"{character.FirstName}: {message}" });

            // Create the request for the API sending the latest collection of chat messages
            var request = new
            {
                messages = trainingData,
                model = "gpt-4",
                max_tokens = 2048,
            };

            try
            {
                // Send the request and capture the response
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {this.serverSettings.ChatGPTAPIKey}");

                var requestJson = JsonConvert.SerializeObject(request);
                var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

                using var httpResponseMessage = await httpClient.PostAsync(this.url, requestContent);

                var jsonString = await httpResponseMessage.Content.ReadAsStringAsync();

                var responseObject = JsonConvert.DeserializeAnonymousType(jsonString, new
                {
                    choices = new[] { new { message = new { role = string.Empty, content = string.Empty } } },
                    error = new { message = string.Empty },
                });

                if (responseObject != null)
                {
                    // Check for errors
                    if (!string.IsNullOrEmpty(responseObject?.error?.message))
                    {
                        return responseObject.error.message;
                    }
                    else
                    {
                        // Add the message object to the message collection so the bot "remembers"
                        var messageObject = responseObject?.choices[0]?.message;

                        if (messageObject != null)
                        {
                            trainingData.Add(messageObject);
                            return messageObject.content;
                        }
                        else
                        {
                            return string.Empty;
                        }
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception exc)
            {
                this.logger.Error(exc, this.communicator);
                return string.Empty;
            }
        }

        /// <summary>
        /// Parses an AI response into say messages and emotes.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="persona">The mobile persona.</param>
        /// <param name="mobile">The mobile.</param>
        /// <returns>Cleaned output message.</returns>
        private static string[]? ProcessOutput(string message, Persona persona, Mobile mobile)
        {
            List<string> messages = new List<string>();

            message = message.Trim();

            if (message.Contains('*'))
            {
                List<string> sentences = new List<string>();
                StringBuilder sbFormat = new StringBuilder();

                bool open = true;

                for (int i = 0; i < message.Length; i++)
                {
                    if (message[i] == '*')
                    {
                        sbFormat.AppendFormat(open ? "[{0}" : "{0}]", message[i]);
                        open = !open;
                    }
                    else
                    {
                        sbFormat.Append(message[i]);
                    }
                }

                StringBuilder sb = new StringBuilder();

                foreach (char c in sbFormat.ToString())
                {
                    if (c == '[')
                    {
                        if (sb.Length > 0)
                        {
                            sentences.Add(sb.ToString());
                            sb = new StringBuilder();
                            sb.Append(c);
                        }
                        else
                        {
                            sb.Append(c);
                        }
                    }
                    else if (c == ']')
                    {
                        sb.Append(c);
                        sentences.Add(sb.ToString());
                        sb = new StringBuilder();
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }

                sentences.Add(sb.ToString());

                foreach (var sentence in sentences)
                {
                    if (string.IsNullOrWhiteSpace(sentence))
                    {
                        continue;
                    }
                    else if (sentence[0] == '[')
                    {
                        // This is an emote
                        messages.Add($"{persona.Name?.FirstCharToUpper() ?? mobile.FirstName.FirstCharToUpper()} {CleanSentence(mobile, persona, sentence, true)?.FirstCharToLower()}");
                    }
                    else
                    {
                        // This is an emote, but not an emote with brackets, so clean it up.
                        if (sentence.StartsWith("EMOTE"))
                        {
                            var cleaned = CleanSentence(mobile, persona, sentence, true);

                            if (!string.IsNullOrWhiteSpace(cleaned))
                            {
                                messages.Add($"{persona.Name?.FirstCharToUpper() ?? mobile.FirstName.FirstCharToUpper()} {cleaned}");
                            }
                        }
                        else
                        {
                            // This is a sentence (probably).
                            var cleaned = CleanSentence(mobile, persona, sentence, false)?.FirstCharToUpper();

                            if (!string.IsNullOrWhiteSpace(cleaned))
                            {
                                messages.Add($"{persona.Name ?? mobile.FirstName} says \"<span class='say'>{cleaned}</span>\"");
                            }
                        }
                    }
                }
            }
            else
            {
                var sentence = CleanSentence(mobile, persona, message, false)?.FirstCharToUpper();

                if (!string.IsNullOrWhiteSpace(sentence))
                {
                    messages.Add($"{persona.Name ?? mobile.FirstName} says \"<span class='say'>{sentence}</span>\"");
                }
            }

            return messages.ToArray();
        }

        private static string? CleanSentence(Mobile mobile, Persona persona, string sentence, bool isEmote)
        {
            // TODO: Get a regex to replace the punctuation/characters we don't want.
            sentence = sentence.Replace("[", string.Empty).Replace("]", string.Empty).Replace("*", string.Empty).Replace("*", string.Empty);
            sentence = sentence.ReplaceFirst(persona.Name?.FirstCharToLower() ?? mobile.FirstName.FirstCharToLower(), string.Empty);
            sentence = sentence.ReplaceFirst("She", string.Empty);
            sentence = sentence.ReplaceFirst("He", string.Empty);
            sentence = sentence.ReplaceFirst("They", string.Empty);
            sentence = sentence.Replace("EMOTE", string.Empty);
            sentence = sentence.Replace("ACTIVATE", string.Empty);
            sentence = sentence.Replace("DEACTIVATE", string.Empty);
            sentence = sentence.Replace(":", string.Empty);

            if (isEmote)
            {
                sentence = sentence.Replace("my", mobile.Pronoun);
            }

            sentence = sentence.Trim();

            // Remove leading punctuation if we have it.
            if (sentence.Length > 0 && char.IsPunctuation(sentence[0]))
            {
                sentence = sentence.Substring(1, sentence.Length - 1);
            }

            sentence = sentence.Trim();

            // Get rid of any instances of the name
            if (!string.IsNullOrWhiteSpace(persona.Name))
            {
                string[] nameParts = persona.Name.Split();

                foreach (var n in nameParts)
                {
                    sentence = sentence.Replace(n, string.Empty, StringComparison.CurrentCultureIgnoreCase);
                }
            }

            // End with punctuation if we didn't.
            if (!string.IsNullOrWhiteSpace(sentence) && !char.IsPunctuation(sentence[sentence.Length - 1]))
            {
                sentence = sentence + ".";
            }

            if (!string.IsNullOrWhiteSpace(sentence))
            {
                return sentence.FirstCharToUpper().Trim();
            }
            else
            {
                return null;
            }
        }

        [GeneratedRegex("http[^\\s]+")]
        private static partial Regex HttpRegex();

        [GeneratedRegex("<.*?>")]
        private static partial Regex MarkupRegex();

        /// <summary>
        /// Remove any HTML or links.
        /// </summary>
        /// <param name="input">The input to clean.</param>
        /// <param name="actor">The actor.</param>
        /// <returns>Formatted input.</returns>
        private static string CleanInput(string input, string actor)
        {
            var cleaned = HttpRegex().Replace(input, string.Empty);
            cleaned = MarkupRegex().Replace(cleaned, string.Empty);
            cleaned = cleaned.Replace(actor, string.Empty);
            cleaned = HttpUtility.HtmlDecode(cleaned);
            cleaned = cleaned.Replace("says", string.Empty);
            cleaned = cleaned.Replace("error:", string.Empty);
            return cleaned;
        }

        /// <summary>
        /// Trains the ChatGPT model on a particular persona.
        /// </summary>
        /// <param name="persona">The persona.</param>
        /// <param name="character">The character the mob is engaged with.</param>
        /// <param name="mobile">The mobile who is engaged.</param>
        private async Task<List<dynamic>> Train(Persona persona, Character character, Mobile mobile)
        {
            var trainingInformation = new List<string>
            {
                $"Your name is {persona.Name}.",
                $"Your race is {mobile.Race}.",
                $"Your age is {mobile.Age}.",
                $"Your class is {persona.Class}.",
                $"Your attitude is {persona.Attitude}.",
                $"You are speaking with {character.FirstName} {character.LastName}.",
                $"Your gender is {mobile.Gender}.",
                $"Your alignment is {mobile.Alignment}, and your ethos is {mobile.Ethos}.",
                $"The person you are speaking to has an alignment of {character.Alignment} and an ethos of {character.Ethos}.",
                $"The person you are speaking to is a {character.Gender} {character.Race}.",
            };

            var memories = await this.dataService.GetMemories(character, mobile);

            if (memories != null)
            {
                // TODO: Add date of last interaction to training data.
                trainingInformation.Add($"You have the following memories of {character.FirstName}:");
                trainingInformation.AddRange(memories);
            }

            if (persona.Background != null)
            {
                trainingInformation.AddRange(persona.Background);
            }

            var messages = new List<dynamic>
            {
                new
                {
                    role = "system",
                    content = string.Join(" ", trainingInformation),
                },
                new
                {
                    role = "assistant",
                    content = persona.Prompt,
                },
            };

            return messages;
        }

        /// <summary>
        /// Converts this mobile to "adult" mode.
        /// </summary>
        /// <param name="message">The message to check for keywords.</param>
        /// <param name="mobile">The mobile.</param>
        private async Task CheckConvertToXMob(string message, Mobile mobile)
        {
            if (mobile.UseAI && !string.IsNullOrWhiteSpace(mobile.XImage))
            {
                if (message.Contains("DEACTIVATE"))
                {
                    if (mobile.XActive.HasValue && mobile.XActive.Value && !string.IsNullOrWhiteSpace(mobile.XImage))
                    {
                        mobile.XActive = false;
                        await this.communicator.SendToRoom(mobile.Location, $"{mobile.FirstName.FirstCharToUpper()} frowns an puts {mobile.Pronoun} clothing back on.");
                    }
                    else
                    {
                        mobile.XActive = false;
                    }
                }
                else if (message.Contains("ACTIVATE"))
                {
                    if (!string.IsNullOrWhiteSpace(mobile.XImage))
                    {
                        mobile.XActive = true;
                        await this.communicator.SendToRoom(mobile.Location, $"{mobile.FirstName.FirstCharToUpper()} gently removes {mobile.Pronoun} clothing with a grin.");
                    }
                    else
                    {
                        mobile.XActive = false;
                    }
                }
                else
                {
                    mobile.XActive = false;
                }
            }
        }

        /// <summary>
        /// Uploads an avatar image to the Azure storage.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="stream">The memory stream.</param>
        /// <returns>String of uploaded URL.</returns>
        private async Task<string?> UploadAvatarImage(Character character, Stream stream)
        {
            try
            {
                string? connectionString = this.serverSettings.AzureDefaultConnectionString;

                if (!string.IsNullOrWhiteSpace(connectionString))
                {
                    // Name of the share, directory, and file we'll create
                    string shareName = "avatars";
                    string fileName = $"{character.FirstName}{character.LastName}.png";
                    ShareClient share = new ShareClient(connectionString, shareName);
                    share.CreateIfNotExists();

                    ShareDirectoryClient directory = share.GetDirectoryClient("/");

                    try
                    {
                        // Get a reference to a file and upload it
                        ShareFileClient file = directory.GetFileClient(fileName);

                        string imageUrl = $"https://legendaryweb.file.core.windows.net/avatars/{fileName}?{this.serverSettings.AzureStorageKey}";

                        var response = await file.CreateAsync(stream.Length);

                        await file.UploadRangeAsync(new HttpRange(0, stream.Length), stream);

                        return imageUrl;
                    }
                    catch (Exception exc)
                    {
                        this.logger.Error(exc, this.communicator);
                        return null;
                    }
                }
                else
                {
                    this.logger.Error("Azure default connection string is not configured.", this.communicator);
                    return null;
                }
            }
            catch (Exception exc)
            {
                this.logger.Error(exc, this.communicator);
                return null;
            }
        }

        private (bool, int) WillEngage(Character actor, Mobile target, string message)
        {
            // Base chance is 10%
            bool engage = false;
            int chance = 10;

            if (target != null && !string.IsNullOrWhiteSpace(target.FirstName))
            {
                // If the person the mob was speaking to has left, reset.
                if (target.PlayerTarget != null && !this.communicator.IsInRoom(target.Location, target.PlayerTarget))
                {
                    target.PlayerTarget = null;
                    target.XActive = false;
                    return (false, 0);
                }

                // If the player and target are already engaged in conversation, there is a 70-90% chance it will speak.
                if (target.PlayerTarget == actor.FirstName)
                {
                    chance += this.random.Next(75, 95);
                    this.logger.Debug($"{target.FirstName.FirstCharToUpper()} is engaged with {actor.FirstName}. Chance: {chance}.", this.communicator);
                }
                else
                {
                    // Different person speaking to the mob, give it a 10-25% additional chance to speak to the new character.
                    chance += this.random.Next(10, 25);

                    this.logger.Debug($"{target.FirstName.FirstCharToUpper()} is distracted. Chance: {chance}.", this.communicator);

                    // Mob is engaged to a target, but someone else is speaking. 10% chance to engage with them instead.
                    if (string.IsNullOrWhiteSpace(target.PlayerTarget) || target.FirstName != actor.FirstName)
                    {
                        if (this.random.Next(0, 100) < 35)
                        {
                            this.logger.Debug($"{target.FirstName.FirstCharToUpper()} decided to engage {actor.FirstName}.", this.communicator);
                            target.XActive = false;
                            return (true, chance);
                        }
                    }
                }

                // If the input contains the target name, there is a 50-80% increase in the odds it will speak.
                if (message.Contains(target.FirstName))
                {
                    this.logger.Debug($"{target.FirstName.FirstCharToUpper()} mentioned by name by {actor.FirstName}. Chance: {chance}.", this.communicator);
                    chance += this.random.Next(55, 85);
                }

                // If we overmax chance, give a 1% rate of failure.
                engage = this.random.Next(0, Math.Max(chance + 1, 100)) < chance;

                if (engage)
                {
                    this.logger.Debug($"{target.FirstName.FirstCharToUpper()} has become engaged with {actor.FirstName}. Chance: {chance}.", this.communicator);

                    // Set a flag on the target and actor showing they are engaged in conversation.
                    target.PlayerTarget = actor.FirstName;
                }
            }

            return (engage, chance);
        }
    }
}