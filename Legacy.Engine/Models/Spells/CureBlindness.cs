// <copyright file="CureBlindness.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Models.Spells
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;

    /// <summary>
    /// Casts the cure blindness spell.
    /// </summary>
    public class CureBlindness : Spell
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CureBlindness"/> class.
        /// </summary>
        /// <param name="communicator">ICommunicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="combat">The combat generator.</param>
        public CureBlindness(ICommunicator communicator, IRandom random, Combat combat)
            : base(communicator, random, combat)
        {
            this.Name = "Cure Blindness";
            this.ManaCost = 50;
            this.CanInvoke = true;
            this.IsAffect = false;
            this.AffectDuration = 0;
        }

        /// <inheritdoc/>
        public override async Task Act(Character actor, Character? target, CancellationToken cancellationToken)
        {
            await this.Communicator.PlaySound(actor, Core.Types.AudioChannel.Spell, Sounds.CURELIGHT, cancellationToken);

            var result = this.Random.Next(3, 24) + (actor.Level / 10);

            if (target == null)
            {
                if (!actor.AffectedBy.Any(a => a.Name?.ToLower() == EffectName.BLINDNESS))
                {
                    await this.Communicator.SendToPlayer(actor, "You are not blinded.", cancellationToken);
                }
                else
                {
                    actor.AffectedBy.RemoveAll(r => r.Name == EffectName.BLINDNESS);
                    await this.Communicator.SendToPlayer(actor, "You can see again!", cancellationToken);
                }
            }
            else
            {
                if (!target.AffectedBy.Any(a => a.Name?.ToLower() == EffectName.BLINDNESS))
                {
                    await this.Communicator.SendToPlayer(actor, "They are not blinded.", cancellationToken);
                }
                else
                {
                    actor.AffectedBy.RemoveAll(r => r.Name == EffectName.BLINDNESS);
                    await this.Communicator.SendToPlayer(target, "You can see again!", cancellationToken);
                }
            }
        }
    }
}
