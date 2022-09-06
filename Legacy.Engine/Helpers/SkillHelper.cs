// <copyright file="SkillHelper.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Helpers
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Legendary.Core.Contracts;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Models;

    /// <summary>
    /// Helper class for parsing skills.
    /// </summary>
    public static class SkillHelper
    {
        /// <summary>
        /// Gets a skill from the assembly using a given skill name.
        /// </summary>
        /// <param name="skillName">The skill name.</param>
        /// <param name="communicator">The communicator.</param>
        /// <param name="random">The random generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat engine.</param>
        /// <returns>Action.</returns>
        public static Skill? ResolveSkill(string skillName, ICommunicator communicator, IRandom random, IWorld world, ILogger logger, Combat combat)
        {
            var engine = Assembly.Load("Legendary.Engine");

            skillName = skillName.Replace(" ", string.Empty);

            var skill = engine.GetTypes().FirstOrDefault(t => t.Namespace == "Legendary.Engine.Models.Skills" && t.Name.ToLower() == skillName.ToLower());

            if (skill != null)
            {
                var skillInstance = Activator.CreateInstance(skill, communicator, random, world, logger, combat);

                if (skillInstance != null && skillInstance is Skill)
                {
                    return skillInstance as Skill;
                }
            }

            return null;
        }
    }
}