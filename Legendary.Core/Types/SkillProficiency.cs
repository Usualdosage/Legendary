// <copyright file="SkillProficiency.cs" company="Legendary™">
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
    /// Represent's a player's skill proficiency.
    /// </summary>
    public class SkillProficiency
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SkillProficiency"/> class.
        /// </summary>
        /// <param name="skill">The skill.</param>
        /// <param name="proficiency">The proficiency.</param>
        public SkillProficiency(IAction skill, int proficiency)
        {
            this.Skill = skill;
            this.Proficiency = proficiency;
        }

        /// <summary>
        /// Gets or sets the skill.
        /// </summary>
        public IAction Skill { get; set; }

        /// <summary>
        /// Gets or sets the skill proficiency.
        /// </summary>
        public int Proficiency { get; set; }
    }
}
