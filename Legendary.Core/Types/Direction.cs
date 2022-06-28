// <copyright file="Direction.cs" company="Legendary™">
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
    /// All possible directions for movement from a room.
    /// </summary>
    public enum Direction : short
    {
        /// <summary>
        /// North.
        /// </summary>
        North = 0,

        /// <summary>
        /// South.
        /// </summary>
        South = 1,

        /// <summary>
        /// West.
        /// </summary>
        West = 2,

        /// <summary>
        /// East.
        /// </summary>
        East = 3,

        /// <summary>
        /// Northeast.
        /// </summary>
        NorthEast = 4,

        /// <summary>
        /// Northwest.
        /// </summary>
        NorthWest = 5,

        /// <summary>
        /// Southeast.
        /// </summary>
        SouthEast = 6,

        /// <summary>
        /// Southwest.
        /// </summary>
        SouthWest = 7,

        /// <summary>
        /// Up.
        /// </summary>
        Up = 8,

        /// <summary>
        /// Down.
        /// </summary>
        Down = 9,
    }
}