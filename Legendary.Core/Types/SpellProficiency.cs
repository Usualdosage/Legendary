// <copyright file="SpellProficiency.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Core.Types
{
    using Legendary.Core.Contracts;

    /// <summary>
    /// Represent's a player's spell proficiency.
    /// </summary>
    public class SpellProficiency
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpellProficiency"/> class.
        /// </summary>
        /// <param name="spell">The spell.</param>
        /// <param name="proficiency">The proficiency percentage.</param>
        public SpellProficiency(ISpell spell, int proficiency)
        {
            this.Spell = spell;
            this.Proficiency = proficiency;
        }

        /// <summary>
        /// Gets or sets the Spell.
        /// </summary>
        public ISpell Spell { get; set; }

        /// <summary>
        /// Gets or sets the Spell proficiency.
        /// </summary>
        public int Proficiency { get; set; }
    }
}
