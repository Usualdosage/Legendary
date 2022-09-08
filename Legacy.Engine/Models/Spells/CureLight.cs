// <copyright file="CureLight.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Models.Spells
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Engine.Contracts;

    /// <summary>
    /// Casts the cure light spell.
    /// </summary>
    public class CureLight : Spell
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CureLight"/> class.
        /// </summary>
        /// <param name="communicator">ICommunicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat generator.</param>
        public CureLight(ICommunicator communicator, IRandom random, IWorld world, ILogger logger, Combat combat)
            : base(communicator, random, world, logger, combat)
        {
            this.Name = "Cure Light";
            this.ManaCost = 10;
            this.CanInvoke = true;
            this.IsAffect = false;
            this.AffectDuration = 0;
        }

        /// <inheritdoc/>
        public override async Task Act(Character actor, Character? target, Item? itemTarget, CancellationToken cancellationToken)
        {
            var result = this.Random.Next(1, 8) + (actor.Level / 10);

            if (target == null)
            {
                if (actor.Health.Current >= actor.Health.Max)
                {
                    await this.Communicator.SendToPlayer(actor, "You are already completely healthy.", cancellationToken);
                }
                else
                {
                    await this.Communicator.PlaySound(actor, Core.Types.AudioChannel.Spell, Sounds.CURELIGHT, cancellationToken);
                    await base.Act(actor, target, itemTarget, cancellationToken);
                    await this.Communicator.SendToPlayer(actor, "You feel a little better.", cancellationToken);
                    var diff = actor.Health.Max - actor.Health.Current;
                    actor.Health.Current += Math.Min(result, diff);
                }
            }
            else
            {
                await this.Communicator.SendToPlayer(actor, "You can't cast this spell on others.", cancellationToken);
            }
        }
    }
}
