// <copyright file="ItemKind.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Core.Types
{
    /// <summary>
    /// Enumeration of kind of an item.
    /// </summary>
    public enum ItemKind
    {
        /// <summary>
        /// Common.
        /// </summary>
        Common = 0,

        /// <summary>
        /// Rare.
        /// </summary>
        Rare = 1,

        /// <summary>
        /// Unique.
        /// </summary>
        Unique = 2,

        /// <summary>
        /// Legendary.
        /// </summary>
        Legendary = 3,
    }
}