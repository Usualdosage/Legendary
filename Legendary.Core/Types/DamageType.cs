// <copyright file="DamageType.cs" company="Legendary™">
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
    /// The type of damage from an attack.
    /// </summary>
    public enum DamageType
    {
        /// <summary>
        /// None.
        /// </summary>
        None = 0,

        /// <summary>
        /// Fire.
        /// </summary>
        Fire = 1,

        /// <summary>
        /// Water.
        /// </summary>
        Water = 2,

        /// <summary>
        /// Energy.
        /// </summary>
        Energy = 3,

        /// <summary>
        /// Blunt.
        /// </summary>
        Blunt = 4,

        /// <summary>
        /// Slash.
        /// </summary>
        Slash = 5,

        /// <summary>
        /// Pierce.
        /// </summary>
        Pierce = 6,

        /// <summary>
        /// Air.
        /// </summary>
        Air = 7,

        /// <summary>
        /// Earth.
        /// </summary>
        Earth = 8,

        /// <summary>
        /// Lightning.
        /// </summary>
        Lightning = 9,

        /// <summary>
        /// Negative.
        /// </summary>
        Negative = 10,

        /// <summary>
        /// Afflictive.
        /// </summary>
        Afflictive = 11,

        /// <summary>
        /// Maledictive.
        /// </summary>
        Maledictive = 12,
    }
}
