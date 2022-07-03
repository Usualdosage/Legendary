// <copyright file="Crypt.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Methods for encrypting and/or hashing things.
    /// </summary>
    public static class Crypt
    {
        /// <summary>
        /// Computes a SHA-256 hash of a string value.
        /// </summary>
        /// <param name="rawData">The string value.</param>
        /// <returns>Hashed string.</returns>
        public static string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256.
            using SHA256 sha256Hash = SHA256.Create();

            // ComputeHash - returns byte array.
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

            // Convert byte array to a string.
            StringBuilder builder = new ();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }

            return builder.ToString();
        }
    }
}
