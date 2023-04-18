// <copyright file="MIRPProcessor.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Processors
{
    using System;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Engine.Models;

    /// <summary>
    /// Main processor for Mob-Item-Room Programs.
    /// </summary>
    public class MIRPProcessor : IMIRPProcessor
    {
        private readonly ICommunicator communicator;
        private readonly IWorld world;
        private readonly ActionProcessor actionProcessor;
        private readonly CombatProcessor combatProcessor;
        private readonly AwardProcessor awardProcessor;
        private readonly SkillProcessor skillProcessor;
        private readonly SpellProcessor spellProcessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="MIRPProcessor"/> class.
        /// </summary>
        /// <param name="communicator">The communicator.</param>
        /// <param name="world">The world.</param>
        /// <param name="actionProcessor">The action processor.</param>
        /// <param name="combatProcessor">The combat processor.</param>
        /// <param name="awardProcessor">The award processor.</param>
        /// <param name="skillProcessor">The skill processor.</param>
        /// <param name="spellProcessor">The spell processor.</param>
        public MIRPProcessor(ICommunicator communicator, IWorld world, ActionProcessor actionProcessor, CombatProcessor combatProcessor, AwardProcessor awardProcessor, SkillProcessor skillProcessor, SpellProcessor spellProcessor)
        {
            this.communicator = communicator;
            this.world = world;
            this.actionProcessor = actionProcessor;
            this.combatProcessor = combatProcessor;
            this.skillProcessor = skillProcessor;
            this.spellProcessor = spellProcessor;
            this.awardProcessor = awardProcessor;
        }

        /// <summary>
        /// Creates an instance of a BaseMIRP derived type based on the type name. Used to load program files.
        /// </summary>
        /// <param name="source">The source object invoking the program.</param>
        /// <returns>Instance of a BaseMIRP derived type.</returns>
        public object? CreateProgramInstance(object source)
        {
            string assemblyPath = "Legendary.Engine.Programs.";

            switch (source)
            {
                default:
                case Mobile:
                    {
                        var mob = (Mobile)source;
                        assemblyPath += $"Mobiles.{mob.Program?.Replace(".cs", string.Empty)}";
                        break;
                    }

                case Item:
                    {
                        var itm = (Item)source;
                        assemblyPath += $"Items.{itm.Program?.Replace(".cs", string.Empty)}";
                        break;
                    }

                case Room:
                    {
                        var rm = (Room)source;
                        assemblyPath += $"Rooms.{rm.Program?.Replace(".cs", string.Empty)}";
                        break;
                    }
            }

            try
            {
                Type? type = Type.GetType(assemblyPath, true);

                if (type != null)
                {
                    var result = Activator.CreateInstance(type, this.communicator, this.world, this.actionProcessor, this.combatProcessor, this.awardProcessor, this.skillProcessor, this.spellProcessor);

                    if (result != null && result is MIRP baseMIRP)
                    {
                        return baseMIRP;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
