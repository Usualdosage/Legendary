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
    using Legendary.Core.Contracts;

    /// <summary>
    /// Concrete implementation of a random number generator.
    /// </summary>
    public class Random : IRandom
    {
        private readonly System.Random random;

        /// <summary>
        /// Initializes a new instance of the <see cref="Random"/> class.
        /// </summary>
        public Random()
        {
            this.random = new System.Random(DateTime.Now.Millisecond);
        }

        /// <inheritdoc/>
        public int Next(int min, int max)
        {
            try
            {
                return this.random.Next(min, max);
            }
            catch
            {
                return 0;
            }
        }

        /// <inheritdoc/>
        public decimal Next(decimal from, decimal to)
        {
            byte fromScale = new System.Data.SqlTypes.SqlDecimal(from).Scale;
            byte toScale = new System.Data.SqlTypes.SqlDecimal(to).Scale;

            byte scale = (byte)(fromScale + toScale);
            if (scale > 28)
            {
                scale = 28;
            }

            decimal r = new decimal(this.random.Next(), this.random.Next(), this.random.Next(), false, scale);
            if (Math.Sign(from) == Math.Sign(to) || from == 0 || to == 0)
            {
                return decimal.Remainder(r, to - from) + from;
            }

            bool getFromNegativeRange = (double)from + (this.random.NextDouble() * ((double)to - (double)from)) < 0;
            return getFromNegativeRange ? decimal.Remainder(r, -from) + from : decimal.Remainder(r, to);
        }
    }
}
