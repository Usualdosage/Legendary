// <copyright file="LanguageGroup.cs" company="Legendary™">
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
    /// Skills available in the language group.
    /// </summary>
    public class LanguageGroup : ActionTree
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageGroup"/> class.
        /// </summary>
        /// <param name="communicator">The communicator.</param>
        /// <param name="random">The random generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat generator.</param>
        public LanguageGroup(ICommunicator communicator, IRandom random, IWorld world, ILogger logger, Combat combat)
            : base(communicator, random, world, logger, combat)
        {
        }

        /// <inheritdoc/>
        public override string Name => "Language Group";

        /// <inheritdoc/>
        public override SchoolType SchoolType => SchoolType.General;

        /// <inheritdoc/>
        public override List<IAction> Group1
        {
            get => new List<IAction>()
            {
                { new Common(this.Communicator, this.Random,  this.World, this.Logger, this.Combat) },
                { new StoneGiant(this.Communicator, this.Random,  this.World, this.Logger, this.Combat) },
                { new FireGiant(this.Communicator, this.Random,  this.World, this.Logger, this.Combat) },
                { new StormGiant(this.Communicator, this.Random,  this.World, this.Logger, this.Combat) },
            };
        }

        /// <inheritdoc/>
        public override List<IAction> Group2
        {
            get => new List<IAction>()
            {
                { new HalfElf(this.Communicator, this.Random,  this.World, this.Logger, this.Combat) },
                { new Dwarf(this.Communicator, this.Random,  this.World, this.Logger, this.Combat) },
                { new Duergar(this.Communicator, this.Random,  this.World, this.Logger, this.Combat) },
            };
        }

        /// <inheritdoc/>
        public override List<IAction> Group3
        {
            get => new List<IAction>()
            {
                { new HalfOrc(this.Communicator, this.Random,  this.World, this.Logger, this.Combat) },
                { new Elf(this.Communicator, this.Random,  this.World, this.Logger, this.Combat) },
                { new Drow(this.Communicator, this.Random,  this.World, this.Logger, this.Combat) },
            };
        }

        /// <inheritdoc/>
        public override List<IAction> Group4
        {
            get => new List<IAction>()
            {
                { new Avian(this.Communicator, this.Random,  this.World, this.Logger, this.Combat) },
                { new Halfling(this.Communicator, this.Random,  this.World, this.Logger, this.Combat) },
            };
        }

        /// <inheritdoc/>
        public override List<IAction> Group5
        {
            get => new List<IAction>()
            {
                { new Gnome(this.Communicator, this.Random,  this.World, this.Logger, this.Combat) },
                { new Faerie(this.Communicator, this.Random,  this.World, this.Logger, this.Combat) },
            };
        }
    }
}