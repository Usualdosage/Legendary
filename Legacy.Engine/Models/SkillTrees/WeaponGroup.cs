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
    using Legendary.Engine.Models.Skills;

    /// <summary>
    /// Spells available in the air group.
    /// </summary>
    public class WeaponGroup : ActionTree
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WeaponGroup"/> class.
        /// </summary>
        /// <param name="communicator">The communicator.</param>
        /// <param name="random">The random generator.</param>
        /// <param name="combat">The combat engine.</param>
        public WeaponGroup(ICommunicator communicator, IRandom random, Combat combat)
            : base(communicator, random, combat)
        {
        }

        /// <inheritdoc/>
        public override string Name => "Weapon Group";

        /// <inheritdoc/>
        public override SchoolType SchoolType => SchoolType.War;

        /// <inheritdoc/>
        public override List<IAction> Group1
        {
            get => new List<IAction>()
            {
                { new EdgedWeapons(this.Communicator, this.Random, this.Combat) },
                { new BluntWeapons(this.Communicator, this.Random, this.Combat) },
                { new PiercingWeapons(this.Communicator, this.Random, this.Combat) },
            };
        }

        /// <inheritdoc/>
        public override List<IAction> Group2
        {
            get => new List<IAction>()
            {
                { new Polearms(this.Communicator, this.Random, this.Combat) },
                { new Flails(this.Communicator, this.Random, this.Combat) },
                { new Whips(this.Communicator, this.Random, this.Combat) },
                { new Staffs(this.Communicator, this.Random, this.Combat) },
            };
        }

        /// <inheritdoc/>
        public override List<IAction> Group3 { get => new List<IAction>(); }

        /// <inheritdoc/>
        public override List<IAction> Group4 { get => new List<IAction>(); }

        /// <inheritdoc/>
        public override List<IAction> Group5 { get => new List<IAction>(); }
    }
}