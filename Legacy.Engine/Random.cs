// <copyright file="Random.cs" company="Legendary™">
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
    using Legendary.Engine.Contracts;

    /// <summary>
    /// Concrete implementation of a random number generator.
    /// </summary>
    public class Random : IRandom
    {
        private readonly RandomNumberGenerator randomNumberGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="Random"/> class.
        /// </summary>
        public Random()
        {
            this.randomNumberGenerator = RandomNumberGenerator.Create();
        }

        /// <inheritdoc/>
        public int Next(int min, int max)
        {
            // Match Next of Random where max is exclusive
            max--;

            // 4 bytes.
            var bytes = new byte[sizeof(int)];
            this.randomNumberGenerator.GetNonZeroBytes(bytes);
            var val = BitConverter.ToInt32(bytes);

            // Constrain our values to between our min and max.
            var result = ((((val - min) % (max - min + 1)) + (max - min + 1)) % (max - min + 1)) + min;
            return result;
        }
    }
}
