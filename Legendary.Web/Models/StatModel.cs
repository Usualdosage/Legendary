// <copyright file="StatModel.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Web.Models
{
    using System;
    using System.Collections.Generic;
    using Legendary.Core.Types;

    /// <summary>
    /// Model for generating character stats.
    /// </summary>
    [Serializable]
    public class StatModel
    {
        /// <summary>
        /// Gets or sets the strength.
        /// </summary>
        public int Str { get; set; } = 12;

        /// <summary>
        /// Gets or sets the wisdom.
        /// </summary>
        public int Wis { get; set; } = 12;

        /// <summary>
        /// Gets or sets the intelligence.
        /// </summary>
        public int Int { get; set; } = 12;

        /// <summary>
        /// Gets or sets the dexterity.
        /// </summary>
        public int Dex { get; set; } = 12;

        /// <summary>
        /// Gets or sets the constitution.
        /// </summary>
        public int Con { get; set; } = 12;

        /// <summary>
        /// Gets or sets the model message.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Gets the player races.
        /// </summary>
        public List<Race> PlayerRaces { get; } = new List<Race>()
        {
            Race.Avian,
            Race.Human,
            Race.Elf,
            Race.Dwarf,
            Race.HalfElf,
            Race.HalfOrc,
            Race.Gnome,
            Race.StoneGiant,
            Race.StormGiant,
            Race.FireGiant,
            Race.Drow,
            Race.Duergar,
            Race.Halfling,
            Race.Faerie,
        };

        /// <summary>
        /// Gets or sets the allowed alignments for this race.
        /// </summary>
        public List<Alignment>? Alignments { get; set; } = new List<Alignment>()
        {
            Alignment.Good,
            Alignment.Neutral,
            Alignment.Evil,
        };

        /// <summary>
        /// Gets or sets the selected alignment.
        /// </summary>
        public Alignment SelectedAlignment { get; set; }

        /// <summary>
        /// Gets or sets the selected race.
        /// </summary>
        public Race SelectedRace { get; set; }
    }
}