// <copyright file="WaterGroup.cs" company="Legendary™">
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
    /// Spells available in the Water group.
    /// </summary>
    public class WaterGroup : ActionTree
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WaterGroup"/> class.
        /// </summary>
        /// <param name="communicator">The communicator.</param>
        /// <param name="random">The random generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat generator.</param>
        public WaterGroup(ICommunicator communicator, IRandom random, IWorld world, ILogger logger, CombatProcessor combat)
            : base(communicator, random, world, logger, combat)
        {
        }

        /// <inheritdoc/>
        public override string Name => "Water Group";

        /// <inheritdoc/>
        public override SchoolType SchoolType => SchoolType.Magic;

        /// <inheritdoc/>
        public override List<IAction> Group1 { get => new (); }

        /// <inheritdoc/>
        public override List<IAction> Group2 { get => new (); }

        /// <inheritdoc/>
        public override List<IAction> Group3
        {
            get => new ()
            {
                { new Iceball(this.Communicator, this.Random, this.World, this.Logger, this.CombatProcessor) },
            };
        }

        /// <inheritdoc/>
        public override List<IAction> Group4 { get => new (); }

        /// <inheritdoc/>
        public override List<IAction> Group5 { get => new (); }
    }
}