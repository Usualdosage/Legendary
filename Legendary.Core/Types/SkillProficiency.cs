// <copyright file="ISkill.cs" company="Legendary">
//  Copyright © 2021-2022 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Core.Types
{
    using Legendary.Core.Contracts;

    public class SkillProficiency
    {
		public SkillProficiency(ISkill skill, int proficiency)
		{
			this.Skill = skill;
			this.Proficiency = proficiency;
		}

        /// <summary>
        /// Gets or sets the skill.
        /// </summary>
        public ISkill Skill { get; set; }

        /// <summary>
        /// Gets or sets the skill proficiency.
        /// </summary>
        public int Proficiency { get; set; }
    }
}

