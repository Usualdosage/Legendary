// <copyright file="Fly.cs" company="Legendary™">
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
    /// Casts the fly spell.
    /// </summary>
    public class Fly : Spell
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Fly"/> class.
        /// </summary>
        /// <param name="communicator">ICommunicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat generator.</param>
        public Fly(ICommunicator communicator, IRandom random, IWorld world, ILogger logger, Combat combat)
            : base(communicator, random, world, logger, combat)
        {
            this.Name = "Fly";
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
                Duration = actor.Level / 5,
            };

            if (target == null)
            {
                if (actor.IsAffectedBy(this) || actor.Race == Core.Types.Race.Avian || actor.Race == Core.Types.Race.Faerie)
                {
                    await this.Communicator.SendToPlayer(actor, $"You are already flying.", cancellationToken);
                }
                else
                {
                    await base.Act(actor, target, itemTarget, cancellationToken);

                    actor.AffectedBy.Add(effect);

                    await this.Communicator.SendToPlayer(actor, $"Your feet rise off the ground.", cancellationToken);
                    await this.Communicator.SendToRoom(actor.Location, actor, target, $"{actor.FirstName.FirstCharToUpper()}'s feet rise off the ground.", cancellationToken);
                }
            }
            else
            {
                if (target.IsAffectedBy(this) || target.Race == Core.Types.Race.Avian || target.Race == Core.Types.Race.Faerie)
                {
                    await this.Communicator.SendToPlayer(actor, $"{target?.FirstName.FirstCharToUpper()} is already flying.", cancellationToken);
                }
                else
                {
                    await base.Act(actor, target, itemTarget, cancellationToken);

                    target?.AffectedBy.Add(effect);

                    await this.Communicator.SendToPlayer(actor, $"{target?.FirstName.FirstCharToUpper()}'s feet rise off the ground.", cancellationToken);
                    await this.Communicator.SendToRoom(actor.Location, actor, target, $"{target?.FirstName.FirstCharToUpper()}'s feet rise off the ground.", cancellationToken);
                }
            }
        }
    }
}
