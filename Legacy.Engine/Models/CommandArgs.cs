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
        /// <param name="index">The index of the target, if multiple specified.</param>
        public CommandArgs(string action, string? method, string? target, int index = 0)
        {
            this.Action = action;
            this.Method = method;
            this.Target = target;
            this.Index = index;
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
        /// Gets or sets the index of the action.
        /// </summary>
        public int Index { get; set; } = 0;

        /// <summary>
        /// Parses an input into a command args object.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>CommandArgs.</returns>
        public static CommandArgs? ParseCommand(string input)
        {
            // Strip out common HTML tags.
            string result = Regex.Replace(input, @"<[^>]*>", string.Empty);

            if (IsCommunication(result))
            {
                return ProcessSentence(result);
            }
            else
            {
                var words = Regex.Matches(result, @"\w+|""[\w\s]*""");

                if (words.Count == 1)
                {
                    // get
                    return new CommandArgs(words[0].Value, null, null);
                }
                else if (words.Count == 2)
                {
                    // get dagger
                    var method = words[1].Value.Replace("\"", string.Empty);
                    return new CommandArgs(words[0].Value, method, null, 0);
                }
                else if (words.Count == 3 && words[0].Value.ToLower() != "emote")
                {
                    if (int.TryParse(words[1].Value, out int index))
                    {
                        // get 2.dagger
                        var method = words[2].Value.Replace("\"", string.Empty);
                        return new CommandArgs(words[0].Value, method, null, index);
                    }
                    else
                    {
                        // get dagger backpack
                        var method = words[1].Value.Replace("\"", string.Empty);
                        return new CommandArgs(words[0].Value, method, words[2].Value, 0);
                    }
                }
                else if (words.Count == 4 && words[0].Value.ToLower() != "emote")
                {
                    // get 2.dagger backpack
                    if (int.TryParse(words[1].Value, out int index))
                    {
                        var method = words[2].Value.Replace("\"", string.Empty);
                        return new CommandArgs(words[0].Value, method, words[3].Value, index);
                    }
                    else
                    {
                        var method = words[1].Value.Replace("\"", string.Empty);
                        return new CommandArgs(words[0].Value, method, words[3].Value, 0);
                    }
                }
                else
                {
                    var emote = words.Skip(1).Take(words.Count - 1).Select(w => w.Value).ToArray();
                    return new CommandArgs(words[0].Value, string.Join(' ', emote), string.Empty);
                }
            }
        }

        /// <summary>
        /// TODO: Not sure this is the best approach for this. Probably stands to be refactored.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>True if communication action.</returns>
        private static bool IsCommunication(string message)
        {
            if (message.StartsWith("say ") ||
                message.StartsWith("sa ") ||
                message.StartsWith("tell ") ||
                message.StartsWith("yell ") ||
                message.StartsWith("gt ") ||
                message.StartsWith("gte ") ||
                message.StartsWith("gtel ") ||
                message.StartsWith("gtell ") ||
                message.StartsWith("pray ") ||
                message.StartsWith("newbie "))
            {
                return true;
            }

            return false;
        }

        private static CommandArgs ProcessSentence(string input)
        {
            var words = input.Split(' ');

            string action = words[0].Trim();

            switch (action)
            {
                // A tell will have the target as the second word.
                case "tell":
                    {
                        var sentence = string.Join(' ', words, 2, words.Length - 2);
                        return new CommandArgs(action, FormatSentence(sentence), words[1]);
                    }

                default:
                case "say":
                case "yell":
                case "pray":
                case "newbie":
                case "emote":
                    {
                        var sentence = string.Join(' ', words, 1, words.Length - 1);
                        return new CommandArgs(action, FormatSentence(sentence), words[1]);
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