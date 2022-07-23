// <copyright file="CommandArgs.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Models
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Xml;

    /// <summary>
    /// Represents arguments typed in by players that are parsed so the application can interpret them.
    /// </summary>
    public class CommandArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandArgs"/> class.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="method">The method.</param>
        /// <param name="target">The target.</param>
        public CommandArgs(string action, string? method, string? target)
        {
            this.Action = action;
            this.Method = method;
            this.Target = target;
        }

        /// <summary>
        /// Gets the action.
        /// </summary>
        public string Action { get; private set; }

        /// <summary>
        /// Gets the method.
        /// </summary>
        public string? Method { get; private set; }

        /// <summary>
        /// Gets the target.
        /// </summary>
        public string? Target { get; private set; }

        /// <summary>
        /// Parses an input into a command args object.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>CommandArgs.</returns>
        public static CommandArgs? ParseCommand(string input)
        {
            // Strip out common HTML tags.
            string result = Regex.Replace(input, @"<[^>]*>", string.Empty);

            // Use cases:
            // 'shield bash' target
            // c light target
            // cast 'cure light'
            // cast cu
            // cast 'cure serious'
            // cast 'cure serious' target
            // say hello
            // say hello there!
            // tell Bob Hello there!
            var words = Regex.Matches(result, @"\w+|'[\w\s]*'");

            if (words.Count == 1)
            {
                return new CommandArgs(words[0].Value, null, null);
            }
            else if (words.Count == 2)
            {
                var method = words[1].Value.Replace("\'", string.Empty);

                if (IsCommunication(words[0].Value))
                {
                    return ProcessSentence(words);
                }
                else
                {
                    return new CommandArgs(words[0].Value, method, null);
                }
            }
            else if (words.Count == 3)
            {
                if (IsCommunication(words[0].Value))
                {
                    return ProcessSentence(words);
                }
                else
                {
                    var method = words[1].Value.Replace("\'", string.Empty);
                    return new CommandArgs(words[0].Value, method, words[2].Value);
                }
            }
            else
            {
                if (IsCommunication(words[0].Value))
                {
                    return ProcessSentence(words);
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Determines if an input is a type of communication, which is handled differently than standard commands.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>True if it's a communication action.</returns>
        private static bool IsCommunication(string action)
        {
            switch (action.ToLower().Trim())
            {
                default:
                    return false;
                case "say":
                case "tell":
                case "yell":
                case "pray":
                case "newbie":
                    return true;
            }
        }

        private static CommandArgs ProcessSentence(MatchCollection matches)
        {
            var command = matches[0].Value.ToLower().Trim();
            string[] words = matches.Select(m => m.Value).ToArray();

            switch (command)
            {
                // A tell will have the target as the second word.
                case "tell":
                    {
                        var sentence = string.Join(' ', words, 2, words.Length - 2);
                        return new CommandArgs(command, FormatSentence(sentence), words[1]);
                    }

                default:
                case "say":
                case "yell":
                case "pray":
                case "newbie":
                case "emote":
                    {
                        var sentence = string.Join(' ', words, 1, words.Length - 1);
                        return new CommandArgs(command, FormatSentence(sentence), words[1]);
                    }
            }
        }

        /// <summary>
        /// Uppercases the first word and adds punctuation if not provided.
        /// </summary>
        /// <param name="sentence">The sentence.</param>
        /// <returns>String.</returns>
        private static string FormatSentence(string sentence)
        {
            sentence = char.ToUpper(sentence[0]) + sentence[1..];

            if (!char.IsPunctuation(sentence[sentence.Length - 1]))
            {
                sentence += ".";
            }

            return sentence;
        }
    }
}