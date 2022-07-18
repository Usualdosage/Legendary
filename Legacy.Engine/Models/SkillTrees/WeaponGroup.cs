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
    using System;
    using System.Collections.Generic;
    using Legendary.Core.Contracts;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Models.Skills;

    /// <summary>
    /// Spells available in the air group.
    /// </summary>
    public class WeaponGroup : IActionTree
    {
        private readonly ICommunicator communicator;
        private readonly IRandom random;
        private readonly Combat combat;

        /// <summary>
        /// Initializes a new instance of the <see cref="WeaponGroup"/> class.
        /// </summary>
        /// <param name="communicator">The communicator.</param>
        /// <param name="random">The random generator.</param>
        /// <param name="combat">The combat engine.</param>
        public WeaponGroup(ICommunicator communicator, IRandom random, Combat combat)
        {
            this.communicator = communicator;
            this.random = random;
            this.combat = combat;
        }

        /// <inheritdoc/>
        public string Name => "Weapon Group";

        /// <inheritdoc/>
        public Dictionary<IAction, int> Group1
        {
            get => new Dictionary<IAction, int>()
            {
                { new EdgedWeapons(this.communicator, this.random, this.combat), 1 },
                { new BluntWeapons(this.communicator, this.random, this.combat), 1 },
                { new PiercingWeapons(this.communicator, this.random, this.combat), 1 },
            };
        }

        /// <inheritdoc/>
        public Dictionary<IAction, int> Group2 { get => new Dictionary<IAction, int>(); }

        /// <inheritdoc/>
        public Dictionary<IAction, int> Group3 { get => new Dictionary<IAction, int>(); }

        /// <inheritdoc/>
        public Dictionary<IAction, int> Group4 { get => new Dictionary<IAction, int>(); }

        /// <inheritdoc/>
        public Dictionary<IAction, int> Group5 { get => new Dictionary<IAction, int>(); }
    }
}