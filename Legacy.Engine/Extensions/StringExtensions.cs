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
    }
}