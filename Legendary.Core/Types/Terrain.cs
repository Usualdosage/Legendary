﻿// <copyright file="Terrain.cs" company="Legendary™">
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
    /// Depending on the terrain, impacts movement, weather, and other game aspects.
    /// </summary>
    public enum Terrain : short
    {
        /// <summary>
        /// City. - 7
        /// </summary>
        City = 0,

        /// <summary>
        /// Mountains. - 7
        /// </summary>
        Mountains = 1,

        /// <summary>
        /// Hills. - 7
        /// </summary>
        Hills = 2,

        /// <summary>
        /// Grasslands. - 7
        /// </summary>
        Grasslands = 3,

        /// <summary>
        /// Water. - 7
        /// </summary>
        Water = 4,

        /// <summary>
        /// Swamp. - 7
        /// </summary>
        Swamp = 5,

        /// <summary>
        /// Air. - 7
        /// </summary>
        Air = 6,

        /// <summary>
        /// Beach. - 7
        /// </summary>
        Beach = 7,

        /// <summary>
        /// Ethereal. - 7
        /// </summary>
        Ethereal = 8,

        /// <summary>
        /// Forest. - 7
        /// </summary>
        Forest = 9,

        /// <summary>
        /// Jungle. - 7
        /// </summary>
        Jungle = 10,

        /// <summary>
        /// Desert. - 7
        /// </summary>
        Desert = 11,

        /// <summary>
        /// Snow. - 7
        /// </summary>
        Snow = 12,

        /// <summary>
        /// Shallow water. - 7
        /// </summary>
        Shallows = 13,
    }
}