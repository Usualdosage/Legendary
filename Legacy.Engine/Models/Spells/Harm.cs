// <copyright file="Harm.cs" company="Legendary™">
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

    /// <summary>
    /// Casts the cause serious spell.
    /// </summary>
    public class Harm : Spell
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Harm"/> class.
        /// </summary>
        /// <param name="communicator">ICommunicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat generator.</param>
        public Harm(ICommunicator communicator, IRandom random, IWorld world, ILogger logger, Combat combat)
            : base(communicator, random, world, logger, combat)
        {
            this.Name = "Harm";
            this.ManaCost = 30;
            this.CanInvoke = true;
            this.IsAffect = false;
            this.HitDice = 3;
            this.DamageDice = 8;
            this.AffectDuration = 0;
            this.DamageModifier = 50;
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
