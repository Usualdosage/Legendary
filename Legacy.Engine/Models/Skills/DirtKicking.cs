// <copyright file="DirtKicking.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Models.Skills
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Extensions;

    /// <summary>
    /// Dirt kicking skill.
    /// </summary>
    public class DirtKicking : Skill
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DirtKicking"/> class.
        /// </summary>
        /// <param name="communicator">The communicator.</param>
        /// <param name="random">The random generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat engine.</param>
        public DirtKicking(ICommunicator communicator, IRandom random, IWorld world, ILogger logger, Combat combat)
            : base(communicator, random, world, logger, combat)
        {
            this.Name = "Dirt Kicking";
            this.ManaCost = 0;
            this.CanInvoke = true;
            this.IsAffect = true;
            this.AffectDuration = 0;
            this.DamageModifier = 0;
            this.HitDice = 1;
            this.DamageDice = 6;
            this.DamageType = Core.Types.DamageType.Afflictive;
            this.DamageNoun = "kicked dirt";
        }

        /// <inheritdoc/>
        public override async Task Act(Character actor, Character? target, Item? itemTarget, CancellationToken cancellationToken)
        {
            if (target == null)
            {
                await this.Communicator.SendToPlayer(actor, "Kick dirt at whom?", cancellationToken);
            }
            else
            {
                if (target.Location.Value != actor.Location.Value)
                {
                    await this.Communicator.SendToPlayer(actor, "They aren't here.", cancellationToken);
                }
                else
                {
                    if (target.IsAffectedBy(this) || target.IsAffectedBy(EffectName.BLINDNESS))
                    {
                        await this.Communicator.SendToPlayer(actor, $"{target.FirstName} is already blinded.", cancellationToken);
                        return;
                    }

                    if (this.Combat.DidSave(target, this))
                    {
                        if (target != null)
                        {
                            await base.Act(actor, target, itemTarget, cancellationToken);

                            await this.Communicator.SendToPlayer(actor, $"{target.FirstName.FirstCharToUpper()} avoids your kicked dirt.", cancellationToken);
                            await this.Communicator.SendToPlayer(target, $"You avoid {actor.FirstName.FirstCharToUpper()}'s kicked dirt.", cancellationToken);

                            await this.Combat.StartFighting(actor, target, cancellationToken);
                        }
                    }
                    else
                    {
                        if (target != null)
                        {
                            var effect = new Effect()
                            {
                                Effector = actor,
                                Action = this,
                                Name = this.Name,
                                Duration = Math.Max(1, actor.Level / 8) / 2,
                            };

                            await base.Act(actor, target, itemTarget, cancellationToken);

                            await this.Communicator.SendToPlayer(actor, $"You kick dirt into {target.FirstName.FirstCharToUpper()}'s eyes!", cancellationToken);
                            await this.Communicator.SendToPlayer(target, $"{actor.FirstName.FirstCharToUpper()} kicks dirt into your eyes!", cancellationToken);
                            await this.Communicator.SendToRoom(actor.Location, actor, target, $"{target.FirstName.FirstCharToUpper()} his blinded by {actor.FirstName} kicked dirt!", cancellationToken);

                            target.AffectedBy.AddIfNotAffected(effect);

                            await this.Combat.StartFighting(actor, target, cancellationToken);
                        }
                    }
                }
            }
        }
    }
}