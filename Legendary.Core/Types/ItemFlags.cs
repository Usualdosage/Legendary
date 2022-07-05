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
        /// Glowing (default: yellow).
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

        /// <summary>
        /// Can be used by good alignment.
        /// </summary>
        Good = 5,

        /// <summary>
        /// Can be used by evil alignment.
        /// </summary>
        Evil = 6,

        /// <summary>
        /// Can be used by neutral alignment.
        /// </summary>
        Neutral = 7,

        /// <summary>
        /// Glowing blue.
        /// </summary>
        GlowBlue = 8,

        /// <summary>
        /// Glowing red.
        /// </summary>
        GlowRed = 9,

        /// <summary>
        /// Glowing orange.
        /// </summary>
        GlowOrange = 10,

        /// <summary>
        /// Glowing green.
        /// </summary>
        GlowGreen = 11,

        /// <summary>
        /// Glowing white.
        /// </summary>
        GlowWhite = 12,

        /// <summary>
        /// Darkly glowing.
        /// </summary>
        GlowDark = 13,

        /// <summary>
        /// Glowing purple.
        /// </summary>
        GlowPurple = 14,

        /// <summary>
        /// Holy item.
        /// </summary>
        Holy = 15,

        /// <summary>
        /// Invisible.
        /// </summary>
        Invisible = 16,
    }
}