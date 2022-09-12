// <copyright file="Infravision.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Models.Spells
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Extensions;

    /// <summary>
    /// Casts the infravision spell.
    /// </summary>
    public class Infravision : Spell
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Infravision"/> class.
        /// </summary>
        /// <param name="communicator">ICommunicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat generator.</param>
        public Infravision(ICommunicator communicator, IRandom random, IWorld world, ILogger logger, Combat combat)
            : base(communicator, random, world, logger, combat)
        {
            this.Name = "Infravision";
            this.ManaCost = 20;
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
                Duration = Math.Max(1, actor.Level / 3),
            };

            if (target == null)
            {
                if (actor.IsAffectedBy(this) || actor.Race == Core.Types.Race.Avian || actor.Race == Core.Types.Race.Faerie)
                {
                    await this.Communicator.SendToPlayer(actor, $"You are can already see in the dark.", cancellationToken);
                }
                else
                {
                    await base.Act(actor, target, itemTarget, cancellationToken);

                    actor.AffectedBy.AddIfNotAffected(effect);

                    await this.Communicator.SendToPlayer(actor, $"Your eyes glow red.", cancellationToken);
                    await this.Communicator.SendToRoom(actor.Location, actor, target, $"{actor.FirstName.FirstCharToUpper()}'s eyes glow red.", cancellationToken);
                }
            }
            else
            {
                await this.Communicator.SendToPlayer(actor, $"You can't cast this spell on others.", cancellationToken);
            }
        }
    }
}
