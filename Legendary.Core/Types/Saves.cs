// <copyright file="Saves.cs" company="Legendary™">
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
    /// Saving throw values for players.
    /// </summary>
    public class Saves
    {
        /// <summary>
        /// Gets or sets the save vs. spell.
        /// </summary>
        public int Spell { get; set; } = 8;

        /// <summary>
        /// Gets or sets the save vs. negative.
        /// </summary>
        public int Negative { get; set; } = 8;

        /// <summary>
        /// Gets or sets the save vs. maledictive.
        /// </summary>
        public int Maledictive { get; set; } = 8;

        /// <summary>
        /// Gets or sets the save vs. afflictive.
        /// </summary>
        public int Afflictive { get; set; } = 8;

        /// <summary>
        /// Gets or sets the save vs. death.
        /// </summary>
        public int Death { get; set; } = 8;
    }
}
