﻿// <copyright file="HealingGroup.cs" company="Legendary™">
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
        public override Dictionary<IAction, int> Group1
        {
            get => new Dictionary<IAction, int>()
            {
                { new Armor(this.Communicator, this.Random, this.Combat), 1 },
                { new CureLight(this.Communicator, this.Random, this.Combat), 1 },
            };
        }

        /// <inheritdoc/>
        public override Dictionary<IAction, int> Group2
        {
            get => new Dictionary<IAction, int>()
            {
                { new CureSerious(this.Communicator, this.Random, this.Combat), 5 },
            };
        }

        /// <inheritdoc/>
        public override Dictionary<IAction, int> Group3 { get => new Dictionary<IAction, int>(); }

        /// <inheritdoc/>
        public override Dictionary<IAction, int> Group4 { get => new Dictionary<IAction, int>(); }

        /// <inheritdoc/>
        public override Dictionary<IAction, int> Group5 { get => new Dictionary<IAction, int>(); }
    }
}