// <copyright file="LightningBolt.cs" company="Legendary™">
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

    /// <summary>
    /// Casts the fireball spell.
    /// </summary>
    public class LightningBolt : Spell
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LightningBolt"/> class.
        /// </summary>
        /// <param name="communicator">ICommunicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="combat">The combat generator.</param>
        public LightningBolt(ICommunicator communicator, IRandom random, Combat combat)
            : base(communicator, random, combat)
        {
            this.Name = "LightningBolt";
            this.ManaCost = 50;
            this.CanInvoke = true;
            this.DamageType = Core.Types.DamageType.Energy;
            this.IsAffect = false;
            this.AffectDuration = 0;
            this.HitDice = 3;
            this.DamageDice = 8;
            this.DamageModifier = 100;
            this.DamageNoun = "lightning bolt";
        }

        /// <inheritdoc/>
        public override async Task Act(UserData actor, UserData? target, CancellationToken cancellationToken)
        {
            await this.Communicator.PlaySound(actor.Character, Core.Types.AudioChannel.Spell, Sounds.LIGHTNINGBOLT, cancellationToken);

            if (target == null)
            {
                await this.DamageToRoom(actor, cancellationToken);
            }
            else
            {
                await this.Communicator.PlaySound(target.Character, Core.Types.AudioChannel.Spell, Sounds.LIGHTNINGBOLT, cancellationToken);

                await this.DamageToTarget(actor, target, cancellationToken);
            }

            await this.Communicator.PlaySoundToRoom(actor.Character, target?.Character, Sounds.LIGHTNINGBOLT, cancellationToken);
        }
    }
}
