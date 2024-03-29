﻿// <copyright file="RoomFlags.cs" company="Legendary™">
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
    /// Flags applied to rooms.
    /// </summary>
    public enum RoomFlags
    {
        /// <summary>
        /// Room is always dark.
        /// </summary>
        Dark = 0,

        /// <summary>
        /// Unaffected by weather.
        /// </summary>
        Indoors = 1,

        /// <summary>
        /// Open space for a room flag.
        /// </summary>
        Unused = 2,

        /// <summary>
        /// Mobs cannot enter or exist in this room.
        /// </summary>
        NoMobs = 3,

        /// <summary>
        /// Mobs will autospawn in this area if players are present.
        /// </summary>
        AutoSpawn = 4,

        /// <summary>
        /// Room is always bright.
        /// </summary>
        Bright = 5,
    }
}