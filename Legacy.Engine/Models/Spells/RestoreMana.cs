﻿// <copyright file="RestoreMana.cs" company="Legendary™">
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
    /// Casts the restore mana spell.
    /// </summary>
    public class RestoreMana : Spell
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RestoreMana"/> class.
        /// </summary>
        /// <param name="communicator">ICommunicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat generator.</param>
        public RestoreMana(ICommunicator communicator, IRandom random, IWorld world, ILogger logger, CombatProcessor combat)
            : base(communicator, random, world, logger, combat)
        {
            this.Name = "Restore Mana";
            this.ManaCost = 30;
            this.CanInvoke = true;
            this.IsAffect = false;
            this.AffectDuration = 0;
        }

        /// <inheritdoc/>
        public override async Task Act(Character actor, Character? target, Item? itemTarget, CancellationToken cancellationToken)
        {
            await this.Communicator.PlaySound(actor, Core.Types.AudioChannel.Spell, Sounds.CURELIGHT, cancellationToken);

            var result = this.Random.Next(30, 50) + (actor.Level / 10);

            if (target == null)
            {
                if (actor.Health.Current >= actor.Health.Max)
                {
                    await this.Communicator.SendToPlayer(actor, "You are already completely mentally energized.", cancellationToken);
                }
                else
                {
                    await base.Act(actor, target, itemTarget, cancellationToken);
                    await this.Communicator.SendToPlayer(actor, "You feel energy course through you!", cancellationToken);
                    var diff = actor.Mana.Max - actor.Mana.Current;
                    actor.Mana.Current += Math.Min(result, diff);

                    var diff2 = actor.Health.Max - actor.Health.Current;
                    actor.Health.Current -= Math.Min(10, diff2);
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
                    if (target.Mana.Current >= target.Mana.Max)
                    {
                        await this.Communicator.SendToPlayer(actor, "They are already completely mentally energized.", cancellationToken);
                    }
                    else
                    {
                        await base.Act(actor, target, itemTarget, cancellationToken);
                        await this.Communicator.SendToPlayer(target, "You feel energy course through you!", cancellationToken);
                        await this.Communicator.PlaySound(target, Core.Types.AudioChannel.Spell, Sounds.CURELIGHT, cancellationToken);
                        var diff = target.Mana.Max - target.Mana.Current;
                        target.Mana.Current += Math.Min(result, diff);

                        var diff2 = actor.Health.Max - actor.Health.Current;
                        actor.Health.Current -= Math.Min(10, diff2);
                    }
                }
            }
        }
    }
}
