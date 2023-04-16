// <copyright file="SpellHelper.cs" company="Legendary™">
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
    /// Helper class for parsing spells.
    /// </summary>
    public static class SpellHelper
    {
        /// <summary>
        /// Gets a spell from the assembly using a given spell name.
        /// </summary>
        /// <param name="spellName">The spell name.</param>
        /// <param name="communicator">The communicator.</param>
        /// <param name="random">The random generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat engine.</param>
        /// <returns>Action.</returns>
        public static Spell? ResolveSpell(string spellName, ICommunicator communicator, IRandom random, IWorld world, ILogger logger, CombatProcessor combat)
        {
            var engine = Assembly.Load("Legendary.Engine");

            spellName = spellName.Replace(" ", string.Empty);

            var spell = engine.GetTypes().FirstOrDefault(t => t.Namespace == "Legendary.Engine.Models.Spells" && t.Name.ToLower() == spellName.ToLower());

            if (spell != null)
            {
                var spellInstance = Activator.CreateInstance(spell, communicator, random, world, logger, combat);

                if (spellInstance != null && spellInstance is Spell)
                {
                    return spellInstance as Spell;
                }
            }

            return null;
        }

        /// <summary>
        /// Checks to see if the spell went up.
        /// </summary>
        /// <param name="spellName">The spell name.</param>
        /// <param name="actor">The actor.</param>
        /// <param name="random">The RNG.</param>
        /// <returns>True if succeeded.</returns>
        public static bool CheckSuccess(string spellName, Character actor, IRandom random)
        {
            var spellProficiency = actor.GetSpellProficiency(spellName);

            if (spellProficiency != null)
            {
                return random.Next(1, 101) <= spellProficiency.Proficiency;
            }

            return false;
        }

        /// <summary>
        /// Checks to see if the named spell has improved.
        /// </summary>
        /// <param name="spellName">The spell name.</param>
        /// <param name="actor">The actor.</param>
        /// <param name="random">The RNG.</param>
        /// <param name="communicator">The communicator.</param>
        /// <param name="awardProcessor">The award processor.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        public static async Task<bool> CheckImprove(string spellName, Character actor, IRandom random, ICommunicator communicator, AwardProcessor awardProcessor, CancellationToken cancellationToken = default)
        {
            int maxImprove = (int)Math.Max(10, actor.Int.Current);

            var spellProficiency = actor.GetSpellProficiency(spellName);

            if (spellProficiency != null)
            {
                if (spellProficiency.Proficiency == 100)
                {
                    return false;
                }

                spellProficiency.Progress += random.Next(0, maxImprove);

                if (spellProficiency.Progress >= 100)
                {
                    spellProficiency.Proficiency += 1;
                    spellProficiency.Progress = 0;

                    if (spellProficiency.Proficiency == 100)
                    {
                        await communicator.SendToPlayer(actor, $"You have now mastered [{spellName}]!", cancellationToken);
                        actor.Experience += random.Next(1000, 2000);
                        await awardProcessor.GrantAward((int)AwardType.Adept, actor, $"mastered {spellName}", cancellationToken);
                        await communicator.SaveCharacter(actor);
                        return true;
                    }
                    else
                    {
                        await communicator.SendToPlayer(actor, $"You have become better at {spellName}!", cancellationToken);
                        actor.Experience += random.Next(100, 200);
                        await communicator.SaveCharacter(actor);
                        return false;
                    }
                }
            }

            return false;
        }
    }
}