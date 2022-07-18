// <copyright file="AudioChannel.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Core.Types
{
    using System;

    /// <summary>
    /// Defines which audio channel a sound will play on.
    /// </summary>
    public enum AudioChannel
    {
        /// <summary>
        /// Background.
        /// </summary>
        Background = 0,

        /// <summary>
        /// Actor.
        /// </summary>
        Actor = 1,

        /// <summary>
        /// Target.
        /// </summary>
        Target = 2,

        /// <summary>
        /// Other (Room).
        /// </summary>
        Martial = 3,

        /// <summary>
        /// Other (Room).
        /// </summary>
        Spell = 4,

        /// <summary>
        /// Weather sounds.
        /// </summary>
        Weather = 5,

        /// <summary>
        /// Background sound effects.
        /// </summary>
        BackgroundSFX = 6,

        /// <summary>
        /// Unused.
        /// </summary>
        Unused1 = 7,

        /// <summary>
        /// Unused.
        /// </summary>
        Unused2 = 8,
    }
}