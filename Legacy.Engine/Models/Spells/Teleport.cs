// <copyright file="Teleport.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Models.Spells
{
    using System.Collections.Generic;
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
    /// Casts the teleport spell.
    /// </summary>
    public class Teleport : Spell
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Teleport"/> class.
        /// </summary>
        /// <param name="communicator">ICommunicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat generator.</param>
        public Teleport(ICommunicator communicator, IRandom random, IWorld world, ILogger logger, CombatProcessor combat)
            : base(communicator, random, world, logger, combat)
        {
            this.Name = "Teleport";
            this.ManaCost = 50;
            this.CanInvoke = true;
            this.IsAffect = false;
            this.AffectDuration = 0;
        }

        /// <inheritdoc/>
        public override async Task Act(Character actor, Character? target, Item? itemTarget, CancellationToken cancellationToken)
        {
            if (target == null)
            {
                var rooms = new List<Room>();

                if (actor.Fighting.HasValue)
                {
                    // Only teleport in the same area if they are in combat.
                    var area = this.Communicator.ResolveArea(actor.Location.Key);

                    if (area != null && area.Rooms != null)
                    {
                        rooms.AddRange(area.Rooms.ToList());
                    }
                    else
                    {
                        await this.Communicator.SendToPlayer(actor, $"You were unable to teleport.", cancellationToken);
                    }
                }
                else
                {
                    rooms.AddRange(this.World.Areas.SelectMany(a => a.Rooms != null ? a.Rooms.ToList() : new List<Room>()).ToList());
                }

                var randomRoomIndex = this.Random.Next(0, rooms.Count - 1);
                var randomRoom = rooms[randomRoomIndex];

                if (randomRoom != null)
                {
                    await this.Communicator.PlaySound(actor, Core.Types.AudioChannel.Spell, Sounds.TELEPORT, cancellationToken);
                    await base.Act(actor, target, itemTarget, cancellationToken);
                    await this.Communicator.SendToPlayer(actor, $"Your close your eyes and teleport.", cancellationToken);
                    await this.Communicator.SendToRoom(actor.Location, actor, target, $"{actor.FirstName.FirstCharToUpper()} closes {actor.Pronoun} eyes and vanishes!", cancellationToken);
                    await this.Communicator.PlaySoundToRoom(actor, target, Sounds.TELEPORT, cancellationToken);
                    actor.Location = new KeyValuePair<long, long>(randomRoom.AreaId, randomRoom.RoomId);

                    await this.Communicator.ShowRoomToPlayer(actor, cancellationToken);
                }
                else
                {
                    await this.Communicator.SendToPlayer(actor, $"You were unable to teleport.", cancellationToken);
                }
            }
            else
            {
                await this.Communicator.SendToPlayer(actor, $"You can't cast this spell on others.", cancellationToken);
            }
        }
    }
}
