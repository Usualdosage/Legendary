// <copyright file="IRandom.cs" company="Legendary">
//  Copyright © 2021 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
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



