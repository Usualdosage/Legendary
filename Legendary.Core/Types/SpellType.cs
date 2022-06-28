// <copyright file="SpellType.cs" company="Legendary™">
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
    /// The type of spell.
    /// </summary>
    public enum SpellType
    {
        /// <summary>
        /// Spell.
        /// </summary>
        Spell = 0,

        /// <summary>
        /// Negative.
        /// </summary>
        Negative = 1,

        /// <summary>
        /// Maledictive.
        /// </summary>
        Maledictive = 2,

        /// <summary>
        /// Afflictive.
        /// </summary>
        Afflictive = 3,

        /// <summary>
        /// Death.
        /// </summary>
        Death = 4,
    }
}
