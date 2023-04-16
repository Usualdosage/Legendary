// <copyright file="WeaponGroup.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Models.SkillTrees
{
    using System.Collections.Generic;
    using Legendary.Core.Contracts;
    using Legendary.Core.Types;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Models.Skills;
    using Legendary.Engine.Processors;

    /// <summary>
    /// Spells available in the weapon group.
    /// </summary>
    public class WeaponGroup : ActionTree
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WeaponGroup"/> class.
        /// </summary>
        /// <param name="communicator">The communicator.</param>
        /// <param name="random">The random generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat generator.</param>
        public WeaponGroup(ICommunicator communicator, IRandom random, IWorld world, ILogger logger, CombatProcessor combat)
            : base(communicator, random, world, logger, combat)
        {
        }

        /// <inheritdoc/>
        public override string Name => "Weapon Group";

        /// <inheritdoc/>
        public override SchoolType SchoolType => SchoolType.War;

        /// <inheritdoc/>
        public override List<IAction> Group1
        {
            get => new ()
            {
                { new EdgedWeapons(this.Communicator, this.Random, this.World, this.Logger, this.CombatProcessor) },
                { new BluntWeapons(this.Communicator, this.Random, this.World, this.Logger,  this.CombatProcessor) },
                { new PiercingWeapons(this.Communicator, this.Random, this.World, this.Logger, this.CombatProcessor) },
            };
        }

        /// <inheritdoc/>
        public override List<IAction> Group2
        {
            get => new ()
            {
                { new Polearms(this.Communicator, this.Random, this.World, this.Logger, this.CombatProcessor) },
                { new Flails(this.Communicator, this.Random, this.World, this.Logger, this.CombatProcessor) },
                { new Whips(this.Communicator, this.Random, this.World, this.Logger, this.CombatProcessor) },
                { new Staffs(this.Communicator, this.Random, this.World, this.Logger, this.CombatProcessor) },
            };
        }

        /// <inheritdoc/>
        public override List<IAction> Group3
        {
            get => new ()
            {
                { new TwoHandedWeapons(this.Communicator, this.Random, this.World, this.Logger, this.CombatProcessor) },
                { new Disarm(this.Communicator, this.Random, this.World, this.Logger, this.CombatProcessor) },
            };
        }

        /// <inheritdoc/>
        public override List<IAction> Group4
        {
            get => new ()
            {
                { new Exotics(this.Communicator, this.Random, this.World, this.Logger, this.CombatProcessor) },
            };
        }

        /// <inheritdoc/>
        public override List<IAction> Group5 { get => new (); }
    }
}