﻿// <copyright file="SpellProficiency.cs" company="Legendary™">
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
    /// Represent's a player's spell proficiency.
    /// </summary>
    public class SpellProficiency
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpellProficiency"/> class.
        /// </summary>
        /// <param name="spellName">The spell.</param>
        /// <param name="proficiency">The proficiency percentage.</param>
        public SpellProficiency(string spellName, int proficiency)
        {
            this.SpellName = spellName;
            this.Proficiency = proficiency;
        }

        /// <summary>
        /// Gets or sets the Spell.
        /// </summary>
        public string SpellName { get; set; }

        /// <summary>
        /// Gets or sets the Spell proficiency.
        /// </summary>
        public int Proficiency { get; set; }

        /// <summary>
        /// Gets or sets the progress to the next increment.
        /// </summary>
        public int Progress { get; set; } = 0;
    }
}
