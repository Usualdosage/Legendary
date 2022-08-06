// <copyright file="StringExtensions.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Legendary.Core.Models;

    /// <summary>
    /// Extensions for strings.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Converts the first character of a string to uppercase.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>String.</returns>
        /// <exception cref="ArgumentNullException">Thrown if null is received.</exception>
        /// <exception cref="ArgumentException">Thrown if empty string is received.</exception>
        public static string FirstCharToUpper(this string input)
        {
#pragma warning disable SA1122 // Use string.Empty for empty strings
            return input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1))
            };
#pragma warning restore SA1122 // Use string.Empty for empty strings
        }

        /// <summary>
        /// Get the most likely name from a list of targets for a given input.
        /// </summary>
        /// <param name="targets">The list of targets.</param>
        /// <param name="input">The string to match.</param>
        /// <returns>Mobile.</returns>
        public static Mobile? ParseTargetName(this List<Mobile> targets, string input)
        {
            var bestMatch = new Dictionary<int, Mobile>();

            foreach (var target in targets)
            {
                List<string> allTokens = new List<string>();
                var firstNameTokens = target.FirstName?.ToLower().Split(' ').ToList();
                var lastNameTokens = target.LastName?.ToLower().Split(' ').ToList();

                if (firstNameTokens != null)
                {
                    allTokens.AddRange(firstNameTokens);
                }

                if (lastNameTokens != null)
                {
                    allTokens.AddRange(lastNameTokens);
                }

                int matchCount = 0;

                foreach (var token in allTokens)
                {
                    if (Regex.IsMatch(token, input))
                    {
                        matchCount += 1;
                    }
                }

                if (!bestMatch.ContainsValue(target))
                {
                    bestMatch.Add(matchCount, target);
                }
            }

            return bestMatch.OrderByDescending(b => b.Key).FirstOrDefault().Value;
        }

        /// <summary>
        /// Get the most likely name from a list of items for a given input.
        /// </summary>
        /// <param name="items">The list of items.</param>
        /// <param name="input">The string to match.</param>
        /// <returns>Mobile.</returns>
        public static Item? ParseItemName(this List<Item> items, string input)
        {
            var bestMatch = new Dictionary<Item, int>();

            foreach (var item in items)
            {
                List<string> allTokens = new List<string>();

                var nameTokens = item.Name.Split(' ');
                var shortDescTokens = item.ShortDescription?.Split(' ');

                if (nameTokens != null)
                {
                    allTokens.AddRange(nameTokens);
                }

                if (shortDescTokens != null)
                {
                    allTokens.AddRange(shortDescTokens);
                }

                int matchCount = 0;

                foreach (var token in allTokens)
                {
                    if (Regex.IsMatch(token, input))
                    {
                        matchCount += 1;
                    }
                }

                if (!bestMatch.ContainsKey(item))
                {
                    bestMatch.Add(item, matchCount);
                }
                else
                {
                    bestMatch[item] += matchCount;
                }
            }

            return bestMatch.OrderByDescending(b => b.Value).FirstOrDefault().Key;
        }
    }
}