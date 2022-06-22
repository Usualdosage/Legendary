// <copyright file="ISpell.cs" company="Legendary">
//  Copyright © 2021-2022 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Core.Types
{
    using Legendary.Core.Contracts;

    public class SpellProficiency
    {
        public SpellProficiency(ISpell Spell, int proficiency)
        {
            this.Spell = Spell;
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

