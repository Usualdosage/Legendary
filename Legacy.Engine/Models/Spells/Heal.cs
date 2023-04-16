// <copyright file="Heal.cs" company="Legendary™">
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
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Processors;

    /// <summary>
    /// Casts the heal spell.
    /// </summary>
    public class Heal : Spell
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Heal"/> class.
        /// </summary>
        /// <param name="communicator">ICommunicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat generator.</param>
        public Heal(ICommunicator communicator, IRandom random, IWorld world, ILogger logger, CombatProcessor combat)
            : base(communicator, random, world, logger, combat)
        {
            this.Name = "Heal";
            this.ManaCost = 90;
            this.CanInvoke = true;
            this.IsAffect = false;
            this.AffectDuration = 0;
        }

        /// <inheritdoc/>
        public override async Task Act(Character actor, Character? target, Item? itemTarget, CancellationToken cancellationToken)
        {
            var result = this.Random.Next(3, 48) + (actor.Level / 5);

            if (target == null)
            {
                if (actor.AffectedBy.Any(a => a.Name?.ToLower() == EffectName.BLINDNESS))
                {
                    actor.AffectedBy.RemoveAll(r => r.Name == EffectName.BLINDNESS);
                    await this.Communicator.SendToPlayer(actor, "You can see again!", cancellationToken);
                }

                if (actor.AffectedBy.Any(a => a.Name?.ToLower() == EffectName.POISON))
                {
                    actor.AffectedBy.RemoveAll(r => r.Name == EffectName.POISON);
                    await this.Communicator.SendToPlayer(actor, "You stop feeling so sick.", cancellationToken);
                }

                await this.Communicator.PlaySound(actor, Core.Types.AudioChannel.Spell, Sounds.HEAL, cancellationToken);
                await base.Act(actor, target, itemTarget, cancellationToken);
                await this.Communicator.SendToPlayer(actor, "You feel a wave of warmth and joy wash over you!", cancellationToken);
                var diff = actor.Health.Max - actor.Health.Current;
                actor.Health.Current += Math.Min(result, diff);
            }
            else
            {
                if (target.Location.Value != actor.Location.Value)
                {
                    await this.Communicator.SendToPlayer(actor, "They aren't here.", cancellationToken);
                }
                else
                {
                    if (target.AffectedBy.Any(a => a.Name?.ToLower() == EffectName.BLINDNESS))
                    {
                        target.AffectedBy.RemoveAll(r => r.Name == EffectName.BLINDNESS);
                        await this.Communicator.SendToPlayer(target, "You can see again!", cancellationToken);
                    }

                    if (target.AffectedBy.Any(a => a.Name?.ToLower() == EffectName.POISON))
                    {
                        target.AffectedBy.RemoveAll(r => r.Name == EffectName.POISON);
                        await this.Communicator.SendToPlayer(target, "You stop feeling so sick.", cancellationToken);
                    }

                    await base.Act(actor, target, itemTarget, cancellationToken);
                    await this.Communicator.SendToPlayer(target, "You feel a wave of warmth and joy wash over you!", cancellationToken);
                    await this.Communicator.PlaySound(target, Core.Types.AudioChannel.Spell, Sounds.HEAL, cancellationToken);
                    var diff = target.Health.Max - target.Health.Current;
                    target.Health.Current += Math.Min(result, diff);
                }
            }
        }
    }
}
