// <copyright file="MobileFlags.cs" company="Legendary™">
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
    /// Flags that can be applied to a mobile.
    /// </summary>
    public enum MobileFlags : short
    {
        /// <summary>
        /// No flags applied.
        /// </summary>
        None = 0,

        /// <summary>
        /// Whether the mob is aggressive or not.
        /// </summary>
        Aggressive = 1,

        /// <summary>
        /// Chance the mob will wander around.
        /// </summary>
        Wander = 2,

        /// <summary>
        /// Can buy and sell items.
        /// </summary>
        Shopkeeper = 3,

        /// <summary>
        /// Can heal players.
        /// </summary>
        Priest = 4,

        /// <summary>
        /// Can train attributes.
        /// </summary>
        Trainer = 5,

        /// <summary>
        /// Can practice skills.
        /// </summary>
        Guildmaster = 6,

        /// <summary>
        /// Can teach new skill/spell trees.
        /// </summary>
        Teacher = 7,
    }
}