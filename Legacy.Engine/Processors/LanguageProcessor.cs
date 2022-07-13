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
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using Legendary.Core;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Models;
    using Newtonsoft.Json;
    using RestSharp;

    /// <summary>
    /// Processes an input, and returns an AI response.
    /// </summary>
    public class LanguageProcessor : ILanguageProcessor
    {
        private readonly ILogger logger;
        private readonly IRandom random;
        private readonly ICommunicator communicator;
        private readonly IServerSettings serverSettings;
        private readonly LanguageGenerator generator;
        private List<string>? excludeWords;
        private Dictionary<string, string>? replaceWords;

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageProcessor"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="serverSettings">The server settings.</param>
        /// <param name="generator">The language generator.</param>
        /// <param name="communicator">The communicator.</param>
        /// <param name="random">The random number generator.</param>
        public LanguageProcessor(ILogger logger, IServerSettings serverSettings, LanguageGenerator generator, ICommunicator communicator, IRandom random)
        {
            this.logger = logger;
            this.serverSettings = serverSettings;
            this.LoadParser();
            this.generator = generator;
            this.random = random;
            this.communicator = communicator;
        }

        /// <summary>
        /// Reloads the parser to capture any updates.
        /// </summary>
        public void LoadParser()
        {
            var parserContent = File.ReadAllText(@"Data/parser.json");

            var parser = JsonConvert.DeserializeObject<Parser>(parserContent);

            if (parser != null)
            {
                this.excludeWords = parser.Exclude;
                this.replaceWords = parser.Replace;
            }
        }

        /// <summary>
        /// Processes the message.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="mobile">The mobile.</param>
        /// <param name="input">The input string.</param>
        /// <param name="situation">The sitrep.</param>
        /// <returns>string.</returns>
        public string? Process(Character character, Mobile mobile, string input, string situation)
        {
            if (mobile.UseAI)
            {
                if (character != null && !string.IsNullOrWhiteSpace(character.FirstName) && mobile != null && !string.IsNullOrWhiteSpace(mobile.FirstName))
                {
                    if (this.WillEngage(character, mobile, input))
                    {
                        this.logger.Debug($"{mobile.FirstName} will engage with {character.FirstName}.");

                        string? response = string.Empty;

                        Thread thread = new Thread(() =>
                        {
                            response = this.Request(CleanInput(input, character.FirstName), situation, character.FirstName, mobile.FirstName).Result;
                        });

                        thread.Start();
                        thread.Join();
                        return response;

                        // return await this.Request(CleanInput(input, character.FirstName), situation, character.FirstName, mobile.FirstName);
                    }
                    else
                    {
                        // Give it a 10% chance to say something about not wanting to talk right now if it doesn't engage.
                        var chance = this.random.Next(0, 100);
                        if (chance < 10)
                        {
                            this.logger.Debug($"{mobile.FirstName} ignored {character.FirstName}.");
                            return Constants.IGNORE_MESSAGE[this.random.Next(0, Constants.IGNORE_MESSAGE.Count - 1)];
                        }
                        else
                        {
                            this.logger.Debug($"{mobile.FirstName} took no action.");
                            return null;
                        }
                    }
                }
                else
                {
                    this.logger.Debug($"Target or mobile was null.");
                    return null;
                }
            }
            else
            {
                // Use standard language processor.
                return null;
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
            if (this.random.Next(0, 100) < 10)
            {
                this.logger.Debug($"{mobile.FirstName} did a random action.");
                var action = Constants.EMOTE_ACTION[this.random.Next(0, Constants.EMOTE_ACTION.Count - 1)];

                action = action.Replace("{0}", mobile.FirstName);
                action = action.Replace("{1}", mobile.Pronoun);

                return action;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Remove any HTML or links.
        /// </summary>
        /// <param name="input">The input to clean.</param>
        /// <param name="actor">The actor.</param>
        /// <returns>Formatted input.</returns>
        private static string CleanInput(string input, string actor)
        {
            var cleaned = Regex.Replace(input, @"http[^\s]+", string.Empty);
            cleaned = Regex.Replace(cleaned, "<.*?>", string.Empty);
            cleaned = cleaned.Replace(actor, string.Empty);
            cleaned = HttpUtility.HtmlDecode(cleaned);
            cleaned = cleaned.Replace("says", string.Empty);
            cleaned = cleaned.Replace("error:", string.Empty);
            return cleaned;
        }

        private static string FormatSentence(string input)
        {
            input = input.Trim();

            input = input.Replace("error: ", string.Empty);

            // Capitalize the first letter.
            input = char.ToUpper(input[0]) + input[1..];

            // If we don't have punctuation at the end, add a period by default.
            if (!char.IsPunctuation(input[^1]))
            {
                input += '.';
            }

            return input;
        }

        private static string Unpunctuate(string input)
        {
            return Regex.Replace(input, @"[^\w\s]", string.Empty);
        }

        private bool WillEngage(Character actor, Mobile target, string message)
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
                }

                // If the player and target are already engaged in conversation, there is a 60-80% chance it will speak.
                if (target.PlayerTarget?.FirstName == actor.FirstName)
                {
                    chance += this.random.Next(60, 85);
                    this.logger.Debug($"{target.FirstName} is engaged with {actor.FirstName}. Chance: {chance}.");
                }
                else
                {
                    // Different person speaking to the mob, give it a 10-25% additional chance to speak to the new character.
                    chance += this.random.Next(10, 25);
                    this.logger.Debug($"{target.FirstName} is distracted. Chance: {chance}.");

                    // Mob is engaged to a target, but someone else is speaking. 10% chance to engage with them instead.
                    if (target.PlayerTarget != null || target.PlayerTarget?.FirstName != actor.FirstName)
                    {
                        if (this.random.Next(0, 100) < 30)
                        {
                            this.logger.Debug($"{target.FirstName} decided to engage {actor.FirstName}.");
                            return true;
                        }
                    }
                }

                // If the input contains the target name, there is a 50-80% increase in the odds it will speak.
                if (message.Contains(target.FirstName))
                {
                    this.logger.Debug($"{target.FirstName} mentioned by name by {actor.FirstName}. Chance: {chance}.");
                    chance += this.random.Next(50, 80);
                }

                // If we overmax chance, give a 1% rate of failure.
                engage = this.random.Next(0, Math.Max(chance + 1, 100)) < chance;

                if (engage)
                {
                    this.logger.Debug($"{target.FirstName} has become engaged with {actor.FirstName}. Chance: {chance}.");

                    // Set a flag on the target and actor showing they are engaged in conversation.
                    target.PlayerTarget = actor;
                }
            }

            return engage;
        }

        private string GetErrorMessage()
        {
            return Constants.CONNECTION_ERROR[this.random.Next(0, Constants.CONNECTION_ERROR.Count - 1)];
        }

        private async Task<string> Request(string input, string situation, string actor, string target)
        {
            try
            {
                using (var client = new RestClient($"https://waifu.p.rapidapi.com/path?user_id=sample_user_id&message={input}&from_name={actor}&to_name={target}&situation={situation}&translate_from=auto&translate_to=auto"))
                {
                    var request = new RestRequest("/", Method.Post);
                    request.AddHeader("content-type", "application/json");
                    request.AddHeader("X-RapidAPI-Key", this.serverSettings.RapidAPIKey ?? string.Empty);
                    request.AddHeader("X-RapidAPI-Host", "waifu.p.rapidapi.com");
                    request.AddParameter("application/json", "{}", ParameterType.RequestBody);
                    RestResponse response = await client.ExecuteAsync(request);
                    if (response != null && !string.IsNullOrWhiteSpace(response.Content))
                    {
                        try
                        {
                            var results = response.Content;

                            if (string.IsNullOrWhiteSpace(results))
                            {
                                return string.Empty;
                            }

                            var words = results.Split(' ');

                            StringBuilder sb = new StringBuilder();

                            foreach (var word in words)
                            {
                                // Check to see if we want to replace an uppercase word with a random word. Don't replace actor or target.
                                var replacementWord = this.Replace(word, actor, target);

                                sb.Append(replacementWord);

                                sb.Append(' ');
                            }

                            // Remove trailing space.
                            sb.Remove(sb.Length - 1, 1);

                            // Remove any HTML or links.
                            var cleaned = Regex.Replace(sb.ToString(), @"http[^\s]+", string.Empty);
                            cleaned = Regex.Replace(cleaned, "<.*?>", string.Empty);

                            // Add any necessary punctuation.
                            return FormatSentence(cleaned);
                        }
                        catch (System.Exception)
                        {
                            return FormatSentence(response.Content);
                        }
                    }
                }

                return this.GetErrorMessage();
            }
            catch
            {
                return this.GetErrorMessage();
            }
        }

        private string Replace(string word, string actor, string target)
        {
            // Ensure we're formatting an actual word.
            if (string.IsNullOrWhiteSpace(word))
            {
                return word;
            }

            // Lowercase and remove all punctuation for our test word. This word does not get returned, it's just for parsing tests.
            var testWord = Unpunctuate(word.ToLower());

            // If it's a proper case word and it's not the name of the actor or the target.
            if (char.IsUpper(word[0]) && word != actor && word != target)
            {
                if (this.replaceWords != null && this.replaceWords.ContainsKey(testWord))
                {
                    // Word should be replaced with another word.
                    var replaceWith = this.replaceWords.First(r => r.Key == word.ToLower()).Value;

                    // Uppercase the word.
                    return char.ToUpper(replaceWith[0]) + replaceWith[1..];
                }

                if (this.excludeWords != null && this.excludeWords.Contains(testWord))
                {
                    // Word is excluded from replacement.
                    return word;
                }
                else
                {
                    // Word should be replaced with a random word. The generator returns only uppercase words.
                    return this.generator.BuildSentence(testWord) + $"[{word}]";
                }
            }
            else
            {
                // It's not proper cased, so just see if we want to use a replacement word.
                if (this.replaceWords != null && this.replaceWords.ContainsKey(testWord))
                {
                    // Word should be replaced with another word.
                    return this.replaceWords.First(r => r.Key == word.ToLower()).Value + $"[{word}]";
                }
                else
                {
                    // No changes, use the word as-is.
                    return word;
                }
            }
        }
    }
}