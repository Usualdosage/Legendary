// <copyright file="Protection.cs" company="Legendary™">
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
    using Legendary.Engine.Extensions;

    /// <summary>
    /// Casts the armor spell.
    /// </summary>
    public class Protection : Spell
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Protection"/> class.
        /// </summary>
        /// <param name="communicator">ICommunicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat generator.</param>
        public Protection(ICommunicator communicator, IRandom random, IWorld world, ILogger logger, Combat combat)
            : base(communicator, random, world, logger, combat)
        {
            this.Name = "Protection";
            this.ManaCost = 40;
            this.CanInvoke = true;
            this.IsAffect = true;
            this.AffectDuration = 0;
        }

        /// <inheritdoc/>
        public override async Task Act(Character actor, Character? target, Item? itemTarget, CancellationToken cancellationToken)
        {
            var effect = new Effect()
            {
                Name = this.Name,
                Duration = actor.Level / 10,
                Spell = 20,
            };

            if (target == null)
            {
                if (actor.IsAffectedBy(this))
                {
                    await this.Communicator.SendToPlayer(actor, $"You are already protected.", cancellationToken);
                }
                else
                {
                    await this.Communicator.PlaySound(actor, Core.Types.AudioChannel.Spell, Sounds.ARMOR, cancellationToken);
                    await this.Communicator.PlaySoundToRoom(actor, target, Sounds.ARMOR, cancellationToken);

                    actor.AffectedBy.Add(effect);
                    await this.Communicator.SendToPlayer(actor, $"You feel protected by the Gods.", cancellationToken);
                    await this.Communicator.SendToRoom(actor.Location, actor, target, $"{actor.FirstName.FirstCharToUpper()} is surrounded by an aura of protection.", cancellationToken);
                }
            }
            else
            {
                await this.Communicator.SendToPlayer(actor, $"You can't cast this on another person.", cancellationToken);
            }
        }
    }
}
