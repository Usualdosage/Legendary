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
    using Legendary.Engine.Models.Spells;

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
        /// <param name="combat">The combat engine.</param>
        public HealingGroup(ICommunicator communicator, IRandom random, Combat combat)
            : base(communicator, random, combat)
        {
        }

        /// <inheritdoc/>
        public override string Name => "Healing Group";

        /// <inheritdoc/>
        public override SchoolType SchoolType => SchoolType.Divinity;

        /// <inheritdoc/>
        public override List<IAction> Group1
        {
            get => new List<IAction>()
            {
                { new Armor(this.Communicator, this.Random, this.Combat) },
                { new CureLight(this.Communicator, this.Random, this.Combat) },
                { new CauseLight(this.Communicator, this.Random, this.Combat) },
            };
        }

        /// <inheritdoc/>
        public override List<IAction> Group2
        {
            get => new List<IAction>()
            {
                { new CureSerious(this.Communicator, this.Random, this.Combat) },
                { new CauseSerious(this.Communicator, this.Random, this.Combat) },
                { new CureBlindness(this.Communicator, this.Random, this.Combat) },
                { new Blindness(this.Communicator, this.Random, this.Combat) },
            };
        }

        /// <inheritdoc/>
        public override List<IAction> Group3
        {
            get => new List<IAction>()
            {
                { new CureCritical(this.Communicator, this.Random, this.Combat) },
                { new Harm(this.Communicator, this.Random, this.Combat) },
            };
        }

        /// <inheritdoc/>
        public override List<IAction> Group4
        {
            get => new List<IAction>()
            {
                { new Poison(this.Communicator, this.Random, this.Combat) },
            };
        }

        /// <inheritdoc/>
        public override List<IAction> Group5 { get => new List<IAction>(); }
    }
}