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
    using System;
    using System.Collections.Generic;
    using Legendary.Core.Contracts;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Models.Skills;

    /// <summary>
    /// Spells available in the air group.
    /// </summary>
    public class MartialGroup : ActionTree
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MartialGroup"/> class.
        /// </summary>
        /// <param name="communicator">The communicator.</param>
        /// <param name="random">The random generator.</param>
        /// <param name="combat">The combat engine.</param>
        public MartialGroup(ICommunicator communicator, IRandom random, Combat combat)
            : base(communicator, random, combat)
        {
        }

        /// <inheritdoc/>
        public override string Name => "Martial Group";

        /// <inheritdoc/>
        public override List<IAction> Group1
        {
            get => new List<IAction>()
            {
                { new HandToHand(this.Communicator, this.Random, this.Combat) },
                { new Recall(this.Communicator, this.Random, this.Combat) },
            };
        }

        /// <inheritdoc/>
        public override List<IAction> Group2 { get => new List<IAction>(); }

        /// <inheritdoc/>
        public override List<IAction> Group3 { get => new List<IAction>(); }

        /// <inheritdoc/>
        public override List<IAction> Group4 { get => new List<IAction>(); }

        /// <inheritdoc/>
        public override List<IAction> Group5 { get => new List<IAction>(); }
    }
}