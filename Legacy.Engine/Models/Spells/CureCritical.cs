﻿// <copyright file="CureCritical.cs" company="Legendary™">
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
    /// Casts the cure serious spell.
    /// </summary>
    public class CureCritical : Spell
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CureCritical"/> class.
        /// </summary>
        /// <param name="communicator">ICommunicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat generator.</param>
        public CureCritical(ICommunicator communicator, IRandom random, IWorld world, ILogger logger, Combat combat)
            : base(communicator, random, world, logger, combat)
        {
            this.Name = "Cure Critical";
            this.ManaCost = 30;
            this.CanInvoke = true;
            this.IsAffect = false;
            this.AffectDuration = 0;
        }

        /// <inheritdoc/>
        public override async Task Act(Character actor, Character? target, Item? itemTarget, CancellationToken cancellationToken)
        {
            var result = this.Random.Next(3, 24) + (actor.Level / 10);

            if (target == null)
            {
                await this.Communicator.PlaySound(actor, Core.Types.AudioChannel.Spell, Sounds.CURELIGHT, cancellationToken);

                if (actor.Health.Current >= actor.Health.Max)
                {
                    await this.Communicator.SendToPlayer(actor, "You are already completely healthy.", cancellationToken);
                }
                else
                {
                    await this.Communicator.SendToPlayer(actor, "You feel a LOT better!", cancellationToken);
                    var diff = actor.Health.Max - actor.Health.Current;
                    actor.Health.Current += Math.Min(result, diff);
                }
            }
            else
            {
                if (target.Location.Value != actor.Location.Value)
                {
                    await this.Communicator.SendToPlayer(actor, "They aren't here.", cancellationToken);
                }
                else
                {
                    if (target.Health.Current >= target.Health.Max)
                    {
                        await this.Communicator.SendToPlayer(actor, "They are already completely healthy.", cancellationToken);
                    }
                    else
                    {
                        await this.Communicator.SendToPlayer(target, "You feel a LOT better!", cancellationToken);
                        await this.Communicator.PlaySound(target, Core.Types.AudioChannel.Spell, Sounds.CURELIGHT, cancellationToken);
                        var diff = target.Health.Max - target.Health.Current;
                        target.Health.Current += Math.Min(result, diff);
                    }
                }
            }
        }
    }
}
