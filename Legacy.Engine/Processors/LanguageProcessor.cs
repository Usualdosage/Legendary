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
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Web;
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
        private readonly IRandom random;
        private readonly LanguageGenerator generator;
        private readonly List<string> connectionError = new List<string>()
        {
            "I'm sorry, I didn't understand that.",
            "I'm not sure what to say.",
            "Hm. I can't think of a response to that.",
            "Sorry, I'm not thinking straight right now.",
            "I'm a little confused.",
            "Maybe ask me that again? I'm not sure I heard you right.",
            "I'm a little tired and not thinking straight right now.",
        };

        private List<string>? excludeWords;
        private Dictionary<string, string>? replaceWords;

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageProcessor"/> class.
        /// </summary>
        /// <param name="generator">The language generator.</param>
        /// <param name="random">The random number generator.</param>
        public LanguageProcessor(LanguageGenerator generator, IRandom random)
        {
            this.LoadParser();
            this.generator = generator;
            this.random = random;
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
        public async Task<string?> Process(Character character, Mobile mobile, string input, string situation)
        {
            if (character != null && !string.IsNullOrWhiteSpace(character.FirstName) && mobile != null && !string.IsNullOrWhiteSpace(mobile.FirstName))
            {
                return await this.Request(CleanInput(input, character.FirstName), situation, character.FirstName, mobile.FirstName);
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
        /// <returns>Formatted input.</returns>
        private static string CleanInput(string input, string actor)
        {
            var cleaned = Regex.Replace(input, @"http[^\s]+", string.Empty);
            cleaned = Regex.Replace(cleaned, "<.*?>", string.Empty);
            cleaned = cleaned.Replace(actor, string.Empty);
            cleaned = HttpUtility.HtmlDecode(cleaned);
            cleaned = cleaned.Replace("says", string.Empty);
            return cleaned;
        }

        private static string Punctuate(string input)
        {
            if (!char.IsPunctuation(input[^1]))
            {
                input += ".";
            }

            return input;
        }

        private static string Unpunctuate(string input)
        {
            return Regex.Replace(input, @"[^\w\s]", string.Empty);
        }

        private string GetErrorMessage()
        {
            return this.connectionError[this.random.Next(0, this.connectionError.Count - 1)];
        }

        private async Task<string> Request(string input, string situation, string actor, string target)
        {
            try
            {
                using (var client = new RestClient($"https://waifu.p.rapidapi.com/path?user_id=sample_user_id&message={input}&from_name={actor}&to_name={target}&situation={situation}&translate_from=auto&translate_to=auto"))
                {
                    var request = new RestRequest("/", Method.Post);
                    request.AddHeader("content-type", "application/json");
                    request.AddHeader("X-RapidAPI-Key", "d6bb62c61dmsh51e8d5bfeea7c44p169130jsnd3f6f238e340");
                    request.AddHeader("X-RapidAPI-Host", "waifu.p.rapidapi.com");
                    request.AddParameter("application/json", "{}", ParameterType.RequestBody);
                    RestResponse response = await client.ExecuteAsync(request);
                    if (response != null && !string.IsNullOrWhiteSpace(response.Content))
                    {
                        var results = response.Content;

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
                        return Punctuate(cleaned);
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