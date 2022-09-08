// <copyright file="Size.cs" company="Legendary™">
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
    /// Size of character or creature based on race.
    /// </summary>
    public enum Size
    {
        /// <summary>
        /// Tiny.
        /// </summary>
        Tiny = 0,

        /// <summary>
        /// Small.
        /// </summary>
        Small = 1,

        /// <summary>
        /// Medium.
        /// </summary>
        Medium = 2,

        /// <summary>
        /// Large.
        /// </summary>
        Large = 3,

        /// <summary>
        /// Extra large.
        /// </summary>
        ExtraLarge = 4,

        /// <summary>
        /// Giant.
        /// </summary>
        Giant = 5,
    }
}