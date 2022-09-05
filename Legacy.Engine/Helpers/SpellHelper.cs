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
    using Legendary.Core.Contracts;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Models;

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
        public static Spell? ResolveSpell(string spellName, ICommunicator communicator, IRandom random, IWorld world, ILogger logger, Combat combat)
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
    }
}