// <copyright file="Staffs.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Models.Skills
{
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Engine.Contracts;

    /// <summary>
    /// Staffs skill.
    /// </summary>
    public class Staffs : Skill
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Staffs"/> class.
        /// </summary>
        /// <param name="communicator">ICommunicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat generator.</param>
        public Staffs(ICommunicator communicator, IRandom random, IWorld world, ILogger logger, Combat combat)
            : base(communicator, random, world, logger, combat)
        {
            this.Name = "Staffs";
            this.ManaCost = 0;
            this.CanInvoke = false;
            this.DamageType = Core.Types.DamageType.Blunt;
            this.IsAffect = false;
            this.AffectDuration = 0;
            this.DamageModifier = 0;
            this.HitDice = 0;
            this.DamageDice = 0;
            this.DamageNoun = "whack";
        }
    }
}
