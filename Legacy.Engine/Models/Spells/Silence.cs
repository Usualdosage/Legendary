// <copyright file="Silence.cs" company="Legendary™">
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
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Extensions;

    /// <summary>
    /// Casts the silence spell.
    /// </summary>
    public class Silence : Spell
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Silence"/> class.
        /// </summary>
        /// <param name="communicator">ICommunicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat generator.</param>
        public Silence(ICommunicator communicator, IRandom random, IWorld world, ILogger logger, Combat combat)
            : base(communicator, random, world, logger, combat)
        {
            this.Name = "Silence";
            this.ManaCost = 40;
            this.CanInvoke = true;
            this.DamageType = Core.Types.DamageType.Afflictive;
            this.IsAffect = true;
        }

        /// <inheritdoc/>
        public override async Task Act(Character actor, Character? target, Item? itemTarget, CancellationToken cancellationToken)
        {
            if (target == null)
            {
                await this.Communicator.SendToPlayer(actor, "Cast silence on whom?", cancellationToken);
            }
            else
            {
                if (target.Location.Value != actor.Location.Value)
                {
                    await this.Communicator.SendToPlayer(actor, "They aren't here.", cancellationToken);
                }
                else
                {
                    if (target.IsAffectedBy(this))
                    {
                        await this.Communicator.SendToPlayer(actor, $"{target.FirstName} is already blinded.", cancellationToken);
                        return;
                    }

                    if (this.Combat.DidSave(target, this))
                    {
                        await base.Act(actor, target, itemTarget, cancellationToken);

                        await this.Communicator.SendToPlayer(actor, $"{target.FirstName.FirstCharToUpper()}'s mouth wrinkles slightly, but returns to normal.", cancellationToken);
                        await this.Communicator.SendToPlayer(target, $"You feel your mouth wrinkle slightly, but it returns to normal.", cancellationToken);

                        if (target != null)
                        {
                            await this.Combat.StartFighting(actor, target, cancellationToken);
                        }
                    }
                    else
                    {
                        var effect = new Effect()
                        {
                            Effector = actor,
                            Action = this,
                            Name = this.Name,
                            Duration = Math.Max(1, actor.Level / 8),
                        };

                        await base.Act(actor, target, itemTarget, cancellationToken);

                        await this.Communicator.SendToPlayer(actor, $"{target.FirstName.FirstCharToUpper()}'s mouth vanishes!", cancellationToken);
                        await this.Communicator.SendToPlayer(target, $"{actor.FirstName.FirstCharToUpper()} has silences you!", cancellationToken);
                        await this.Communicator.SendToRoom(actor.Location, actor, target, $"{target?.FirstName.FirstCharToUpper()} has been silenced by {actor.FirstName}!", cancellationToken);

                        target?.AffectedBy.AddIfNotAffected(effect);

                        if (target != null)
                        {
                            await this.Combat.StartFighting(actor, target, cancellationToken);
                        }
                    }
                }
            }
        }
    }
}
