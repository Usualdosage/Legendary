// <copyright file="ListExtensions.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Core.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using Legendary.Core.Types;

    /// <summary>
    /// Extensions for list types.
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Checks if a flag exists, and if not, adds it.
        /// </summary>
        /// <param name="list">The list to check.</param>
        /// <param name="flag">The flag to add.</param>
        public static void AddIfNotExists(this IList<CharacterFlags> list, CharacterFlags flag)
        {
            if (list.Any(l => l == flag))
            {
                return;
            }
            else
            {
                list.Add(flag);
            }
        }

        /// <summary>
        /// Checks if a flag exists, and if it does, removes it.
        /// </summary>
        /// <param name="list">The list to check.</param>
        /// <param name="flag">The flag to add.</param>
        public static void RemoveIfExists(this IList<CharacterFlags> list, CharacterFlags flag)
        {
            if (list.Any(l => l == flag))
            {
                list.Remove(flag);
            }
        }
    }
}
