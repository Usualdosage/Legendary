// <copyright file="CauseLight.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Models.Spells
{
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;

    /// <summary>
    /// Casts the cause light spell.
    /// </summary>
    public class CauseLight : Spell
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CauseLight"/> class.
        /// </summary>
        /// <param name="communicator">ICommunicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="combat">The combat generator.</param>
        public CauseLight(ICommunicator communicator, IRandom random, Combat combat)
            : base(communicator, random, combat)
        {
            this.Name = "Cause Light";
            this.ManaCost = 10;
            this.CanInvoke = true;
            this.IsAffect = false;
            this.HitDice = 1;
            this.DamageDice = 8;
            this.AffectDuration = 0;
            this.DamageNoun = "spell";
        }

        /// <inheritdoc/>
        public override async Task PreAction(Character actor, Character? target, CancellationToken cancellationToken = default)
        {
            if (target == null)
            {
                await this.Communicator.SendToPlayer(actor, "Cast the spell on whom?", cancellationToken);
            }
            else
            {
                await base.PreAction(actor, target, cancellationToken);
            }
        }

        /// <inheritdoc/>
        public override async Task Act(Character actor, Character? target, CancellationToken cancellationToken)
        {
            var result = this.Random.Next(1, 8) + (actor.Level / 10);

            if (target == null)
            {
                await this.Communicator.SendToPlayer(actor, "Cast the spell on whom?", cancellationToken);
            }
            else
            {
                await this.DamageToTarget(actor, target, cancellationToken);
            }
        }
    }
}
