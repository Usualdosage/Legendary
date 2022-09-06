// <copyright file="PickLock.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Models.Skills
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Extensions;
    using Legendary.Engine.Processors;

    /// <summary>
    /// Allows a player to pick a lock.
    /// </summary>
    public class PickLock : Skill
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PickLock"/> class.
        /// </summary>
        /// <param name="communicator">ICommunicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat generator.</param>
        public PickLock(ICommunicator communicator, IRandom random, IWorld world, ILogger logger, Combat combat)
            : base(communicator, random, world, logger, combat)
        {
            this.Name = "Pick Lock";
            this.ManaCost = 0;
            this.CanInvoke = true;
            this.IsAffect = false;
            this.AffectDuration = 0;
            this.DamageModifier = 0;
            this.HitDice = 0;
            this.DamageDice = 0;
        }

        /// <inheritdoc/>
        public override async Task Act(Character actor, Character? target, CancellationToken cancellationToken)
        {
            // First, see if it's a door.
            var room = this.Communicator.ResolveRoom(actor.Location);

            if (room != null)
            {
                foreach (var exit in room.Exits)
                {
                    if (exit.IsDoor && exit.IsClosed && exit.IsLocked)
                    {
                        exit.IsLocked = false;
                        var dir = ActionProcessor.ParseFriendlyDirection(exit.Direction);
                        await this.Communicator.SendToPlayer(actor, $"You pick the lock on the door {dir} you.", cancellationToken);
                        await this.Communicator.SendToRoom(actor, actor.Location, $"{actor.FirstName} picks the lock on the door {dir} you.", cancellationToken);
                        return;
                    }
                }

                var lockedItem = room.Items.FirstOrDefault(i => i.IsClosed == true && i.IsLocked == true);

                if (lockedItem != null)
                {
                    lockedItem.IsLocked = false;
                    await this.Communicator.SendToPlayer(actor, $"You pick the lock on {lockedItem.Name}.", cancellationToken);
                    await this.Communicator.SendToRoom(actor, actor.Location, $"{actor.FirstName} picks the lock on {lockedItem.Name}.", cancellationToken);
                }
            }
        }

        /// <inheritdoc/>
        public override async Task PostAction(Character actor, Character? target, CancellationToken cancellationToken = default)
        {
            await this.CheckImprove(actor, cancellationToken);
        }
    }
}
