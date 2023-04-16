// <copyright file="Invisibility.cs" company="Legendary™">
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
    using Legendary.Engine.Processors;

    /// <summary>
    /// Casts the invisibility spell.
    /// </summary>
    public class Invisibility : Spell
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Invisibility"/> class.
        /// </summary>
        /// <param name="communicator">ICommunicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat generator.</param>
        public Invisibility(ICommunicator communicator, IRandom random, IWorld world, ILogger logger, CombatProcessor combat)
            : base(communicator, random, world, logger, combat)
        {
            this.Name = "Invisibility";
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
                if (actor.IsAffectedBy(this))
                {
                    await this.Communicator.SendToPlayer(actor, $"You are already invisible.", cancellationToken);
                }
                else
                {
                    await base.Act(actor, target, itemTarget, cancellationToken);

                    actor.AffectedBy.AddIfNotAffected(effect);

                    await this.Communicator.SendToPlayer(actor, $"Your fade out of existence.", cancellationToken);
                    await this.Communicator.SendToRoom(actor.Location, actor, target, $"{actor.FirstName.FirstCharToUpper()} fades out of existence.", cancellationToken);
                }
            }
            else
            {
                if (target.IsAffectedBy(this) || target.Race == Core.Types.Race.Avian || target.Race == Core.Types.Race.Faerie)
                {
                    await this.Communicator.SendToPlayer(actor, $"{target?.FirstName.FirstCharToUpper()} is already invisible.", cancellationToken);
                }
                else
                {
                    await base.Act(actor, target, itemTarget, cancellationToken);

                    target?.AffectedBy.AddIfNotAffected(effect);

                    await this.Communicator.SendToPlayer(actor, $"{target?.FirstName.FirstCharToUpper()} fades out of existence.", cancellationToken);
                    await this.Communicator.SendToRoom(actor.Location, actor, target, $"{target?.FirstName.FirstCharToUpper()} fades out of existence.", cancellationToken);
                }
            }
        }
    }
}
