// <copyright file="MartialGroup.cs" company="Legendary™">
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

    /// <summary>
    /// Spells available in the martial group.
    /// </summary>
    public class MartialGroup : ActionTree
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MartialGroup"/> class.
        /// </summary>
        /// <param name="communicator">The communicator.</param>
        /// <param name="random">The random generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat generator.</param>
        public MartialGroup(ICommunicator communicator, IRandom random, IWorld world, ILogger logger, Combat combat)
            : base(communicator, random, world, logger, combat)
        {
        }

        /// <inheritdoc/>
        public override string Name => "Martial Group";

        /// <inheritdoc/>
        public override SchoolType SchoolType => SchoolType.War;

        /// <inheritdoc/>
        public override List<IAction> Group1
        {
            get => new List<IAction>()
            {
                { new HandToHand(this.Communicator, this.Random,  this.World, this.Logger, this.Combat) },
                { new Recall(this.Communicator, this.Random,  this.World, this.Logger, this.Combat) },
                { new Dodge(this.Communicator, this.Random,  this.World, this.Logger, this.Combat) },
            };
        }

        /// <inheritdoc/>
        public override List<IAction> Group2
        {
            get => new List<IAction>()
            {
                { new Parry(this.Communicator, this.Random, this.World, this.Logger, this.Combat) },
                { new FastHealing(this.Communicator, this.Random, this.World, this.Logger, this.Combat) },
                { new SmashDoor(this.Communicator, this.Random,  this.World, this.Logger, this.Combat) },
            };
        }

        /// <inheritdoc/>
        public override List<IAction> Group3
        {
            get => new List<IAction>()
            {
                { new SecondAttack(this.Communicator, this.Random, this.World, this.Logger, this.Combat) },
                { new EvasiveManeuvers(this.Communicator, this.Random, this.World, this.Logger, this.Combat) },
            };
        }

        /// <inheritdoc/>
        public override List<IAction> Group4
        {
            get => new List<IAction>()
            {
                { new ThirdAttack(this.Communicator, this.Random, this.World, this.Logger, this.Combat) },
            };
        }

        /// <inheritdoc/>
        public override List<IAction> Group5
        {
            get => new List<IAction>()
            {
                { new FourthAttack(this.Communicator, this.Random, this.World, this.Logger, this.Combat) },
            };
        }
    }
}