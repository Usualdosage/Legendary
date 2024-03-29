﻿// <copyright file="CureSerious.cs" company="Legendary™">
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
    using Legendary.Engine.Processors;

    /// <summary>
    /// Casts the cure serious spell.
    /// </summary>
    public class CureSerious : Spell
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CureSerious"/> class.
        /// </summary>
        /// <param name="communicator">ICommunicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat generator.</param>
        public CureSerious(ICommunicator communicator, IRandom random, IWorld world, ILogger logger, CombatProcessor combat)
            : base(communicator, random, world, logger, combat)
        {
            this.Name = "Cure Serious";
            this.ManaCost = 20;
            this.CanInvoke = true;
            this.IsAffect = false;
            this.AffectDuration = 0;
        }

        /// <inheritdoc/>
        public override async Task Act(Character actor, Character? target, Item? itemTarget, CancellationToken cancellationToken)
        {
            var result = this.Random.Next(2, 16) + (actor.Level / 10);

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
                    await this.Communicator.SendToPlayer(actor, "You feel much better!", cancellationToken);
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
                        await base.Act(actor, target, itemTarget, cancellationToken);
                        await this.Communicator.SendToPlayer(target, "You feel much better!", cancellationToken);
                        await this.Communicator.PlaySound(target, Core.Types.AudioChannel.Spell, Sounds.CURELIGHT, cancellationToken);
                        var diff = target.Health.Max - target.Health.Current;
                        target.Health.Current += Math.Min(result, diff);
                    }
                }
            }
        }
    }
}
