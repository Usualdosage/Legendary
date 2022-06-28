// <copyright file="IRandom.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Contracts
{
    using System.Security.Cryptography;

    /// <summary>
    /// Implementation context for a random number generator.
    /// </summary>
    public interface IRandom
    {
        /// <summary>
        /// Gets the next random number in a specified range.
        /// </summary>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <returns>System.Int32.</returns>
        int Next(int min, int max);
    }
}
