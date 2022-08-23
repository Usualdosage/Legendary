// <copyright file="Terrain.cs" company="Legendary™">
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
        /// City.
        /// </summary>
        City = 0,

        /// <summary>
        /// Mountains.
        /// </summary>
        Mountains = 1,

        /// <summary>
        /// Hills.
        /// </summary>
        Hills = 2,

        /// <summary>
        /// Grasslands.
        /// </summary>
        Grasslands = 3,

        /// <summary>
        /// Water.
        /// </summary>
        Water = 4,

        /// <summary>
        /// Swamp.
        /// </summary>
        Swamp = 5,

        /// <summary>
        /// Air.
        /// </summary>
        Air = 6,

        /// <summary>
        /// Beach.
        /// </summary>
        Beach = 7,

        /// <summary>
        /// Ethereal.
        /// </summary>
        Ethereal = 8,

        /// <summary>
        /// Forest.
        /// </summary>
        Forest = 9,

        /// <summary>
        /// Jungle.
        /// </summary>
        Jungle = 10,

        /// <summary>
        /// Desert.
        /// </summary>
        Desert = 11,

        /// <summary>
        /// Snow.
        /// </summary>
        Snow = 12,

        /// <summary>
        /// Shallow water.
        /// </summary>
        Shallows = 13,
    }
}