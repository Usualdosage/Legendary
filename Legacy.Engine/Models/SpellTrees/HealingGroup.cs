// <copyright file="HealingGroup.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Models.SpellTrees
{
    using System.Collections.Generic;
    using Legendary.Core.Contracts;
    using Legendary.Core.Types;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Models.Spells;
    using Legendary.Engine.Processors;

    /// <summary>
    /// Spells available in the healing group.
    /// </summary>
    public class HealingGroup : ActionTree
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HealingGroup"/> class.
        /// </summary>
        /// <param name="communicator">The communicator.</param>
        /// <param name="random">The random generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat generator.</param>
        public HealingGroup(ICommunicator communicator, IRandom random, IWorld world, ILogger logger, CombatProcessor combat)
            : base(communicator, random, world, logger, combat)
        {
        }

        /// <inheritdoc/>
        public override string Name => "Healing Group";

        /// <inheritdoc/>
        public override SchoolType SchoolType => SchoolType.Divinity;

        /// <inheritdoc/>
        public override List<IAction> Group1
        {
            get => new ()
            {
                { new Armor(this.Communicator, this.Random, this.World, this.Logger, this.CombatProcessor) },
                { new CureLight(this.Communicator, this.Random, this.World, this.Logger,  this.CombatProcessor) },
                { new CauseLight(this.Communicator, this.Random, this.World, this.Logger,  this.CombatProcessor) },
            };
        }

        /// <inheritdoc/>
        public override List<IAction> Group2
        {
            get => new ()
            {
                { new CureSerious(this.Communicator, this.Random, this.World, this.Logger,  this.CombatProcessor) },
                { new CauseSerious(this.Communicator, this.Random, this.World, this.Logger,  this.CombatProcessor) },
                { new CureBlindness(this.Communicator, this.Random, this.World, this.Logger,  this.CombatProcessor) },
                { new Blindness(this.Communicator, this.Random, this.World, this.Logger,  this.CombatProcessor) },
            };
        }

        /// <inheritdoc/>
        public override List<IAction> Group3
        {
            get => new ()
            {
                { new CureCritical(this.Communicator, this.Random, this.World, this.Logger,  this.CombatProcessor) },
                { new Harm(this.Communicator, this.Random, this.World, this.Logger,  this.CombatProcessor) },
            };
        }

        /// <inheritdoc/>
        public override List<IAction> Group4
        {
            get => new ()
            {
                { new Poison(this.Communicator, this.Random, this.World, this.Logger,  this.CombatProcessor) },
                { new Sanctuary(this.Communicator, this.Random, this.World, this.Logger,  this.CombatProcessor) },
            };
        }

        /// <inheritdoc/>
        public override List<IAction> Group5
        {
            get => new ()
            {
                { new Heal(this.Communicator, this.Random, this.World, this.Logger,  this.CombatProcessor) },
                { new RestoreMana(this.Communicator, this.Random, this.World, this.Logger, this.CombatProcessor) },
            };
        }
    }
}