// <copyright file="Fireball.cs" company="Legendary™">
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
    /// Casts the fireball spell.
    /// </summary>
    public class Fireball : Spell
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Fireball"/> class.
        /// </summary>
        /// <param name="communicator">ICommunicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat generator.</param>
        public Fireball(ICommunicator communicator, IRandom random, IWorld world, ILogger logger, Combat combat)
            : base(communicator, random, world, logger, combat)
        {
            this.Name = "Fireball";
            this.ManaCost = 50;
            this.CanInvoke = true;
            this.DamageType = Core.Types.DamageType.Fire;
            this.IsAffect = false;
            this.AffectDuration = 0;
            this.HitDice = 3;
            this.DamageDice = 6;
            this.DamageModifier = 200;
            this.DamageNoun = "fiery blast";
        }

        /// <inheritdoc/>
        public override async Task Act(Character actor, Character? target, CancellationToken cancellationToken)
        {
            await this.Communicator.PlaySound(actor, Core.Types.AudioChannel.Spell, Sounds.FIREBALL, cancellationToken);

            if (target == null)
            {
                await this.DamageToRoom(actor, cancellationToken);
            }
            else
            {
                if (target.Location.Value != actor.Location.Value)
                {
                    await this.Communicator.SendToPlayer(actor, "They aren't here.", cancellationToken);
                }
                else
                {
                    await this.Communicator.PlaySound(target, Core.Types.AudioChannel.Spell, Sounds.FIREBALL, cancellationToken);
                    await this.DamageToTarget(actor, target, cancellationToken);
                }
            }

            await this.Communicator.PlaySoundToRoom(actor, target, Sounds.FIREBALL, cancellationToken);
        }
    }
}
