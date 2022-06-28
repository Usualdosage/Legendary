// <copyright file="LanguageGenerator.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Legendary.Engine.Contracts;

    /// <summary>
    /// Generates unique words from training data.
    /// </summary>
    public sealed class LanguageGenerator
    {
        private readonly IEnumerable<string> words = "lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor incididunt ut labore et dolore magna aliqua Ut enim ad minim veniam quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur excepteur sint occaecat cupidatat non proident sunt in culpa qui officia deserunt mollit anim id est laborum".Split(' ');
        private readonly IRandom random;
        private readonly HashSet<string> enders = new ();
        private readonly IList<string> starters = new List<string>();

        private readonly Dictionary<char, IList<string>> gramDict =
            Enumerable
                .Range('a', 'z')
                .ToDictionary(a => (char)a, _ => (IList<string>)new List<string>());

        private readonly byte[] randomBytes = new byte[4];

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageGenerator"/> class.
        /// </summary>
        /// <param name="random">The random number generator.</param>
        public LanguageGenerator(IRandom random)
        {
            this.random = random;

            int gramLen = this.random.Next(1, 2);

            foreach (var word in this.words.Select(w => w.Trim().ToLower()).Where(w => w.Length > gramLen)
                .Where(w => Regex.IsMatch(w, "^[a-z]+$")))
            {
                this.starters.Add(word[..gramLen]);
                this.enders.Add(word.Substring(word.Length - gramLen, gramLen));
                for (var i = 0; i < word.Length - gramLen; i++)
                {
                    var currentLetter = word[i];
                    if (!this.gramDict.TryGetValue(currentLetter, out var grams))
                    {
                        i = word.Length;
                        continue;
                    }

                    grams.Add(word.Substring(i + 1, gramLen));
                }
            }
        }

        /// <summary>
        /// Builds a random sentence from a random sentence.
        /// </summary>
        /// <param name="sentence">The source sentence.</param>
        /// <returns>New sentence.</returns>
        public string BuildSentence(string sentence)
        {
            // Get rid of punctuation.
            Regex.Replace(sentence, @"[^\w\s]", string.Empty);

            // Split into words.
            var words = sentence.Split(' ');

            StringBuilder sb = new ();
            foreach (var word in words)
            {
                sb.Append(this.BuildPseudoWord(word.Length) + " ");
            }

            var result = sb.ToString().Trim();

            // Uppercase the first letter.
            result = char.ToUpper(result[0]) + result[1..];

            // Add some emphasis.
            return result + "!";
        }

        private string BuildPseudoWord(int length)
        {
            var result = new StringBuilder(this.GetRandomStarter());
            var lastGram = string.Empty;
            while (result.Length < length || !this.enders.Contains(lastGram))
            {
                lastGram = this.GetRandomGram(result[^1]);
                result.Append(lastGram);
            }

            return result.ToString();
        }

        private string GetRandomStarter() => this.GetRandomElement(this.starters);

        private string GetRandomGram(char preceding) =>
            this.GetRandomElement(this.gramDict[preceding]);

        private T GetRandomElement<T>(IList<T> collection) =>
            collection[this.random.Next(0, collection.Count - 1)];

        private int GetRandomUnsigned(int min, int max)
        {
            return this.random.Next(min, max);
        }
    }
}
