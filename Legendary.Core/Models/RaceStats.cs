// <copyright file="RaceStats.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Core.Models
{
    using System;
    using System.Collections.Generic;
    using Legendary.Core.Contracts;
    using Legendary.Core.Types;

    /// <summary>
    /// Static information about races, like exp penalty, maximums and minimums.
    /// </summary>
    public class RaceStats
    {
        /// <summary>
        /// Gets or sets the experience penalty.
        /// </summary>
        public int ExperiencePenalty { get; set; }

        /// <summary>
        /// Gets or sets the max strength for this race.
        /// </summary>
        public int StrMax { get; set; }

        /// <summary>
        /// Gets or sets the max dex for this race.
        /// </summary>
        public int DexMax { get; set; }

        /// <summary>
        /// Gets or sets the max int for this race.
        /// </summary>
        public int IntMax { get; set; }

        /// <summary>
        /// Gets or sets the max wis for this race.
        /// </summary>
        public int WisMax { get; set; }

        /// <summary>
        /// Gets or sets the max con for this race.
        /// </summary>
        public int ConMax { get; set; }

        /// <summary>
        /// Gets or sets the possible alignments for this race.
        /// </summary>
        public List<Alignment>? Alignments { get; set; }

        /// <summary>
        /// Gets or sets the size of the race.
        /// </summary>
        public Size? Size { get; set; }

        /// <summary>
        /// Gets or sets the racial abilities this race has. Includes languages.
        /// </summary>
        public List<string>? Abilities { get; set; }
    }
}