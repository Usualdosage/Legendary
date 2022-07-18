// <copyright file="CureLight.cs" company="Legendary™">
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
    /// Casts the cure light spell.
    /// </summary>
    public class CureLight : Spell
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CureLight"/> class.
        /// </summary>
        /// <param name="communicator">ICommunicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="combat">The combat generator.</param>
        public CureLight(ICommunicator communicator, IRandom random, Combat combat)
            : base(communicator, random, combat)
        {
            this.Name = "CureLight";
            this.ManaCost = 10;
            this.CanInvoke = true;
            this.IsAffect = false;
            this.AffectDuration = 0;
        }

        /// <inheritdoc/>
        public override async Task Act(UserData actor, UserData? target, CancellationToken cancellationToken)
        {
            await this.Communicator.PlaySound(actor.Character, Core.Types.AudioChannel.Spell, Sounds.CURELIGHT, cancellationToken);

            var result = this.Random.Next(1, 8) + (actor.Character.Level / 10);

            if (target == null)
            {
                await this.Communicator.SendToPlayer(actor.Character, "You feel a little better.", cancellationToken);
                await this.Communicator.PlaySound(actor.Character, Core.Types.AudioChannel.Spell, Sounds.CURELIGHT, cancellationToken);
                actor.Character.Health.Current += result;
            }
            else
            {
                await this.Communicator.SendToPlayer(actor.Character, "You can't cast this spell on others.", cancellationToken);
            }
        }
    }
}
