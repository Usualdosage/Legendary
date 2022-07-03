// <copyright file="ItemFlags.cs" company="Legendary™">
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
    /// Flags that can be applied to items.
    /// </summary>
    public enum ItemFlags : short
    {
        /// <summary>
        /// No flags applied.
        /// </summary>
        None = 0,

        /// <summary>
        /// Glowing.
        /// </summary>
        Glowing = 1,

        /// <summary>
        /// Humming.
        /// </summary>
        Humming = 2,

        /// <summary>
        /// Sharp.
        /// </summary>
        Sharp = 3,

        /// <summary>
        /// Magical.
        /// </summary>
        Magical = 4,
}
}