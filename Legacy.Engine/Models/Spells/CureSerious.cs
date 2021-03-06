// <copyright file="CureSerious.cs" company="Legendary™">
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
    /// Casts the cure serious spell.
    /// </summary>
    public class CureSerious : Spell
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CureSerious"/> class.
        /// </summary>
        /// <param name="communicator">ICommunicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="combat">The combat generator.</param>
        public CureSerious(ICommunicator communicator, IRandom random, Combat combat)
            : base(communicator, random, combat)
        {
            this.Name = "CureSerious";
            this.ManaCost = 20;
            this.CanInvoke = true;
            this.IsAffect = false;
            this.AffectDuration = 0;
        }

        /// <inheritdoc/>
        public override async Task Act(UserData actor, UserData? target, CancellationToken cancellationToken)
        {
            await this.Communicator.PlaySound(actor.Character, Core.Types.AudioChannel.Spell, Sounds.CURELIGHT, cancellationToken);

            var result = this.Random.Next(2, 16) + (actor.Character.Level / 10);

            if (target == null)
            {
                await this.Communicator.SendToPlayer(actor.Character, "You feel much better!", cancellationToken);
                await this.Communicator.PlaySound(actor.Character, Core.Types.AudioChannel.Spell, Sounds.CURELIGHT, cancellationToken);
                actor.Character.Health.Current += result;
            }
            else
            {
                await this.Communicator.SendToPlayer(target.Character, "You feel much better!", cancellationToken);
                await this.Communicator.PlaySound(target.Character, Core.Types.AudioChannel.Spell, Sounds.CURELIGHT, cancellationToken);
                actor.Character.Health.Current += result;
            }
        }
    }
}
