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
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Core.Types;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Models;
    using Legendary.Engine.Processors;

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

        /// <summary>
        /// Checks to see if the skill went up.
        /// </summary>
        /// <param name="skillName">The skill name.</param>
        /// <param name="actor">The actor.</param>
        /// <param name="random">The RNG.</param>
        /// <returns>True if succeeded.</returns>
        public static bool CheckSuccess(string skillName, Character actor, IRandom random)
        {
            var skillProficiency = actor.GetSkillProficiency(skillName);

            if (skillProficiency != null)
            {
                return random.Next(1, 101) <= skillProficiency.Proficiency;
            }

            return false;
        }

        /// <summary>
        /// Checks to see if the named skill has improved.
        /// </summary>
        /// <param name="skillName">The skill name.</param>
        /// <param name="actor">The actor.</param>
        /// <param name="random">The RNG.</param>
        /// <param name="communicator">The communicator.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        public static async Task<bool> CheckImprove(string skillName, Character actor, IRandom random, ICommunicator communicator, CancellationToken cancellationToken = default)
        {
            int maxImprove = (int)Math.Max(10, actor.Int.Current);

            var skillProficiency = actor.GetSkillProficiency(skillName);

            if (skillProficiency != null)
            {
                if (skillProficiency.Proficiency == 100)
                {
                    return false;
                }

                skillProficiency.Progress += random.Next(0, maxImprove);

                if (skillProficiency.Progress >= 100)
                {
                    skillProficiency.Proficiency += 1;
                    skillProficiency.Progress = 0;

                    if (skillProficiency.Proficiency == 100)
                    {
                        await communicator.SendToPlayer(actor, $"You have now mastered [{skillName}]!", cancellationToken);
                        actor.Experience += random.Next(1000, 2000);
                        await communicator.SaveCharacter(actor);
                        return true;
                    }
                    else
                    {
                        await communicator.SendToPlayer(actor, $"You have become better at {skillName}!", cancellationToken);
                        actor.Experience += random.Next(100, 200);
                        await communicator.SaveCharacter(actor);
                        return false;
                    }
                }

                return false;
            }

            return false;
        }
    }
}