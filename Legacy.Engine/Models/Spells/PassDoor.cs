// <copyright file="PassDoor.cs" company="Legendary™">
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
    /// Casts the pass door spell.
    /// </summary>
    public class PassDoor : Spell
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PassDoor"/> class.
        /// </summary>
        /// <param name="communicator">ICommunicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat generator.</param>
        public PassDoor(ICommunicator communicator, IRandom random, IWorld world, ILogger logger, Combat combat)
            : base(communicator, random, world, logger, combat)
        {
            this.Name = "Pass Door";
            this.ManaCost = 50;
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
            };

            if (target == null)
            {
                if (actor.IsAffectedBy(this))
                {
                    await this.Communicator.SendToPlayer(actor, $"You are already translucent.", cancellationToken);
                }
                else
                {
                    await base.Act(actor, target, itemTarget, cancellationToken);
                    actor.AffectedBy.Add(effect);
                    await this.Communicator.SendToPlayer(actor, $"You become translucent.", cancellationToken);
                    await this.Communicator.SendToRoom(actor.Location, actor, target, $"{actor.FirstName.FirstCharToUpper()} suddently becomes translucent.", cancellationToken);
                }
            }
            else
            {
                await this.Communicator.SendToPlayer(actor, $"You can't cast this spell on others.", cancellationToken);
            }
        }
    }
}
