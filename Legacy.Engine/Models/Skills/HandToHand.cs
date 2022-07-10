// <copyright file="HandToHand.cs" company="Legendary™">
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

    /// <summary>
    /// Recalls the player to their hometown recall point.
    /// </summary>
    public class HandToHand : Skill
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HandToHand"/> class.
        /// </summary>
        /// <param name="communicator">ICommunicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="combat">The combat generator.</param>
        public HandToHand(ICommunicator communicator, IRandom random, Combat combat)
            : base(communicator, random, combat)
        {
            this.Name = "HandToHand";
            this.ManaCost = 0;
            this.CanInvoke = false;
            this.DamageType = Core.Types.DamageType.Blunt;
            this.IsAffect = false;
            this.AffectDuration = 0;
            this.DamageModifier = 0;
            this.HitDice = 1;
            this.DamageDice = 4;
        }

        /// <inheritdoc/>
        public override Task PreAction(UserData actor, UserData? target, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override Task Act(UserData actor, UserData? target, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override Task PostAction(UserData actor, UserData? target, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
