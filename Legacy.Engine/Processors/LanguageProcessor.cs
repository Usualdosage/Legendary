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
    using System.Net.Http;
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
    using Newtonsoft.Json;

    /// <summary>
    /// Processes an input, and returns an AI response.
    /// </summary>
    public partial class LanguageProcessor : ILanguageProcessor
    {
        private readonly ILogger logger;
        private readonly IRandom random;
        private readonly ICommunicator communicator;
        private readonly IServerSettings serverSettings;
        private readonly IEnvironment environment;
        private readonly IWorld world;
        private readonly QuestProcessor questProcessor;
        private readonly string url = "https://api.openai.com/v1/chat/completions";
        private readonly string imageUrl = "https://api.openai.com/v1/images/generations";
        private readonly Dictionary<Mobile, bool> processingDictionary;
        private readonly Dictionary<Mobile, List<dynamic>> mobileTrainingData;
        private Persona? basePersona;

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageProcessor"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="serverSettings">The server settings.</param>
        /// <param name="communicator">The communicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="world">The world service.</param>
        /// <param name="questProcessor">The quest processor.</param>
        public LanguageProcessor(ILogger logger, IServerSettings serverSettings, ICommunicator communicator, IRandom random, IEnvironment environment, IWorld world, QuestProcessor questProcessor)
        {
            this.logger = logger;
            this.serverSettings = serverSettings;
            this.random = random;
            this.communicator = communicator;
            this.environment = environment;
            this.world = world;
            this.mobileTrainingData = new Dictionary<Mobile, List<dynamic>>();
            this.processingDictionary = new Dictionary<Mobile, bool>();
            this.questProcessor = questProcessor;
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
                if (character != null && !string.IsNullOrWhiteSpace(character.FirstName) && engagedMobile != null && !string.IsNullOrWhiteSpace(engagedMobile.FirstName) && !string.IsNullOrWhiteSpace(engagedMobile.PersonaFile))
                {
                    var persona = this.world.Personas.FirstOrDefault(p => p.Name == engagedMobile.PersonaFile.Replace(".json", string.Empty));

                    if (persona != null && !string.IsNullOrWhiteSpace(persona.Name))
                    {
                        this.logger.Debug($"{persona.Name ?? engagedMobile.FirstName.FirstCharToUpper()} will engage with {character.FirstName}.", this.communicator);

                        // We haven't trained this mobile yet, so train it.
                        if (!this.mobileTrainingData.ContainsKey(engagedMobile))
                        {
                            this.mobileTrainingData.Add(engagedMobile, this.Train(persona, character, engagedMobile));
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

                        // We can get some weird messaging back from the AI, so we need to process that.
                        if (message.Contains("non-explicit") || message.ToLower().Contains("that line of interaction"))
                        {
                            // Went too far, AI is pissed now. Just send a message to the player because the AI is being prude.No need to embarass them.
                            if (engagedMobile.XActive.HasValue && engagedMobile.XActive.Value)
                            {
                                await this.communicator.SendToPlayer(character, $"{engagedMobile.FirstName.FirstCharToUpper()} sadly moves away from you, and gets dressed.", cancellationToken);
                            }
                            else
                            {
                                await this.communicator.SendToPlayer(character, $"{engagedMobile.FirstName.FirstCharToUpper()} sadly walks away from you.", cancellationToken);
                            }

                            engagedMobile.XActive = false;

                            return (null, null);
                        }

                        // See if any parts of the message complete a quest.
                        await this.questProcessor.CheckQuest(message, character, engagedMobile, cancellationToken);

                        // Parse all of the resulting language.
                        try
                        {
                            // Send a message to the UI to clear the speech bubble.
                            await this.communicator.SendToPlayer(character, $"CLEARCHAT:{engagedMobile.CharacterId}", cancellationToken);

                            // Log the raw message.
                            this.logger.Info($"Raw message result from {persona.Name}: {message}", this.communicator);

                            // Clean and process all of the output from the AI engine.
                            var result = (ProcessOutput(message, persona, engagedMobile), engagedMobile);

                            if (result.Item1 != null)
                            {
                                // Add the input as a memory, only if the mob actually created a response.
                                try
                                {
                                    if (input.Contains(':'))
                                    {
                                        input = input.Split(':')[1].Trim();
                                    }

                                    this.world.AddMemory(character, engagedMobile, $"{input}");
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

        private static string[]? ProcessOutput(string message, Persona persona, Mobile mobile)
        {
            List<string> messages = new ();

            message = message.Trim();

            var sbMessage = new StringBuilder();
            bool sentenceStart = false;
            bool emoteStart = false;

            foreach (var character in message)
            {
                if (character == '{')
                {
                    sentenceStart = true;
                    sbMessage.Append("say ");
                    continue;
                }

                if (character == '[')
                {
                    emoteStart = true;
                    sbMessage.Append("emote ");
                    continue;
                }

                if (character == '}' && sentenceStart)
                {
                    sentenceStart = false;
                    messages.Add(sbMessage.ToString());
                    sbMessage = new StringBuilder();
                    continue;
                }

                if (character == ']' && emoteStart)
                {
                    emoteStart = false;
                    messages.Add(sbMessage.ToString());
                    sbMessage = new StringBuilder();
                    continue;
                }

                sbMessage.Append(character);
            }

            List<string> cleaned = new List<string>();

            // Clean up the messages
            foreach (var action in messages)
            {
                var clean = CleanSentence(mobile, persona, action);

                if (!string.IsNullOrWhiteSpace(clean))
                {
                    cleaned.Add(clean);
                }
            }

            return cleaned.ToArray();
        }

        private static string? CleanSentence(Mobile mobile, Persona persona, string sentence)
        {
            sentence = sentence.ReplaceFirst(persona.Name ?? mobile.FirstName, string.Empty);
            sentence = sentence.Replace("CASSANOVA-DEACTIVATE", string.Empty);
            sentence = sentence.Replace("CASSANOVA-ACTIVATE", string.Empty);
            sentence = sentence.Trim();
            return sentence;
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
        private List<dynamic> Train(Persona persona, Character character, Mobile mobile)
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
                $"The person you are speaking to may have an alignment of {character.Alignment} and an ethos of {character.Ethos}.",
                $"The person you are speaking to appears to be a {character.Gender} {character.Race}.",
            };

            // Add the base behaviors for all AI mobs.
            if (this.basePersona != null)
            {
                trainingInformation.AddRange(this.basePersona.Background);
            }
            else
            {
                // Load the base AI persona for ALL AI mobs.
                this.basePersona = this.world.Personas.First(p => p.Id == 0);

                if (this.basePersona.Background != null)
                {
                    trainingInformation.AddRange(this.basePersona.Background);
                }
            }

            // Mobiles should respect the Gods.
            if (character.Level >= 90 && character.Level <= 95)
            {
                trainingInformation.Add("This being is a lesser deity.");
            }
            else if (character.Level > 95 && character.Level <= 99)
            {
                trainingInformation.Add("This being is a greater deity!");
            }
            else if (character.Level > 100)
            {
                trainingInformation.Add("This being before you is one of the most powerful beings in the universe!");
            }

            var memories = this.world.GetMemories(character, mobile);

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
                    ShareClient share = new (connectionString, shareName);
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
            int chance = 20;

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
                    chance += this.random.Next(70, 90);
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