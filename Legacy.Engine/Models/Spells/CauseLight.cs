﻿// <copyright file="CauseLight.cs" company="Legendary™">
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
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Processors;

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
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat generator.</param>
        public CauseLight(ICommunicator communicator, IRandom random, IWorld world, ILogger logger, CombatProcessor combat)
            : base(communicator, random, world, logger, combat)
        {
            this.Name = "Cause Light";
            this.ManaCost = 10;
            this.CanInvoke = true;
            this.IsAffect = false;
            this.HitDice = 1;
            this.DamageDice = 8;
            this.AffectDuration = 0;
            this.DamageType = Core.Types.DamageType.Negative;
            this.DamageNoun = "spell";
        }

        /// <inheritdoc/>
        public override async Task Act(Character actor, Character? target, Item? itemTarget, CancellationToken cancellationToken)
        {
            if (target == null)
            {
                if (actor.Fighting.HasValue)
                {
                    var player = this.Communicator.ResolveCharacter(actor.Fighting.Value);

                    if (player == null)
                    {
                        var mobile = this.Communicator.ResolveMobile(actor.Fighting.Value);

                        if (mobile != null)
                        {
                            await base.Act(actor, target, itemTarget, cancellationToken);

                            await this.Communicator.PlaySound(actor, Core.Types.AudioChannel.Spell, Sounds.HARM, cancellationToken);
                            await this.Communicator.PlaySoundToRoom(actor, target, Sounds.HARM, cancellationToken);

                            await this.DamageToTarget(actor, mobile, cancellationToken);
                        }
                        else
                        {
                            await this.Communicator.SendToPlayer(actor, "They aren't here.", cancellationToken);
                        }
                    }
                    else
                    {
                        await base.Act(actor, target, itemTarget, cancellationToken);

                        await this.Communicator.PlaySound(actor, Core.Types.AudioChannel.Spell, Sounds.HARM, cancellationToken);
                        await this.Communicator.PlaySound(player.Character, Core.Types.AudioChannel.Spell, Sounds.HARM, cancellationToken);
                        await this.Communicator.PlaySoundToRoom(actor, target, Sounds.HARM, cancellationToken);

                        await this.DamageToTarget(actor, player.Character, cancellationToken);
                    }
                }
                else
                {
                    await this.Communicator.SendToPlayer(actor, "Cast the spell on whom?", cancellationToken);
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
                    await base.Act(actor, target, itemTarget, cancellationToken);

                    await this.Communicator.PlaySound(target, Core.Types.AudioChannel.Spell, Sounds.HARM, cancellationToken);
                    await this.Communicator.PlaySound(actor, Core.Types.AudioChannel.Spell, Sounds.HARM, cancellationToken);
                    await this.Communicator.PlaySoundToRoom(actor, target, Sounds.HARM, cancellationToken);
                    await this.DamageToTarget(actor, target, cancellationToken);
                }
            }
        }
    }
}
