// <copyright file="Poison.cs" company="Legendary™">
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
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Extensions;

    /// <summary>
    /// Casts the poison spell.
    /// </summary>
    public class Poison : Spell
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Poison"/> class.
        /// </summary>
        /// <param name="communicator">ICommunicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat generator.</param>
        public Poison(ICommunicator communicator, IRandom random, IWorld world, ILogger logger, Combat combat)
            : base(communicator, random, world, logger, combat)
        {
            this.Name = "Poison";
            this.ManaCost = 20;
            this.CanInvoke = true;
            this.DamageType = Core.Types.DamageType.Maledictive;
            this.IsAffect = true;
            this.AffectDuration = 3;
            this.HitDice = 1;
            this.DamageDice = 6;
            this.DamageModifier = 10;
            this.DamageNoun = "poison";
        }

        /// <inheritdoc/>
        public override async Task Act(Character actor, Character? target, CancellationToken cancellationToken)
        {
            if (target == null)
            {
                await this.Communicator.SendToPlayer(actor, "Poison whom?", cancellationToken);
            }
            else
            {
                if (target.IsAffectedBy(this))
                {
                    await this.Communicator.SendToPlayer(actor, $"{target.FirstName} is already poisoned.", cancellationToken);
                    return;
                }

                if (this.Combat.DidSave(target, this))
                {
                    await this.Communicator.SendToPlayer(actor, $"{target.FirstName.FirstCharToUpper()} looks queasy for a moment, but it passes.", cancellationToken);
                    await this.Communicator.SendToPlayer(target, $"You feel queasy for a moment, but it passes.", cancellationToken);
                }
                else
                {
                    var effect = new Effect()
                    {
                        Effector = actor,
                        Action = this,
                        Name = this.Name,
                        Duration = actor.Level / 10,
                    };

                    await this.Communicator.SendToPlayer(actor, $"{target.FirstName.FirstCharToUpper()} suddenly looks very ill.", cancellationToken);
                    await this.Communicator.SendToPlayer(target, $"{actor.FirstName.FirstCharToUpper()} has poisoned you!", cancellationToken);
                    await this.Communicator.SendToRoom(actor.Location, actor, target, $"{target?.FirstName.FirstCharToUpper()} has been poisoned by {actor.FirstName}!", cancellationToken);

                    target?.AffectedBy.Add(effect);

                    if (target != null)
                    {
                        await this.Combat.StartFighting(actor, target, cancellationToken);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public override async Task OnTick(Character actor, Effect effect, CancellationToken cancellationToken = default)
        {
            if (effect.Action != null)
            {
                if (effect.Effector != null)
                {
                    var damage = this.Combat.CalculateDamage(effect.Effector, actor, effect.Action);
                    var damageVerb = Combat.CalculateDamageVerb(damage);

                    await this.Communicator.SendToPlayer(actor, $"{effect.Effector.FirstName.FirstCharToUpper()}'s {effect.Action.DamageNoun} {damageVerb} you.", cancellationToken);
                    await this.Communicator.SendToPlayer(effect.Effector, $"Your {effect.Action.DamageNoun} {damageVerb} {actor.FirstName}.", cancellationToken);
                    await this.Communicator.SendToRoom(actor.Location, actor, effect.Effector, $"{effect.Effector.FirstName.FirstCharToUpper()}'s {effect.Action.DamageNoun} {damageVerb} {actor.FirstName}.", cancellationToken);

                    if (Combat.ApplyDamage(actor, damage))
                    {
                        if (actor.IsNPC)
                        {
                            await this.Combat.KillMobile(actor, effect.Effector, cancellationToken);
                        }
                        else
                        {
                            await this.Combat.KillPlayer(actor, effect.Effector, cancellationToken);
                        }
                    }
                }
            }
        }
    }
}
