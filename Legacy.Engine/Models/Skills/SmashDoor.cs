// <copyright file="SmashDoor.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Models.Skills
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Core.Types;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Extensions;
    using Legendary.Engine.Processors;

    /// <summary>
    /// Allows a player to smash open a door.
    /// </summary>
    public class SmashDoor : Skill
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmashDoor"/> class.
        /// </summary>
        /// <param name="communicator">ICommunicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat generator.</param>
        public SmashDoor(ICommunicator communicator, IRandom random, IWorld world, ILogger logger, CombatProcessor combat)
            : base(communicator, random, world, logger, combat)
        {
            this.Name = "Smash Door";
            this.ManaCost = 0;
            this.CanInvoke = true;
            this.IsAffect = false;
            this.AffectDuration = 0;
            this.DamageModifier = 0;
            this.HitDice = 0;
            this.DamageDice = 0;
        }

        /// <inheritdoc/>
        public override async Task Act(Character actor, Character? target, Item? itemTarget, CancellationToken cancellationToken)
        {
            if (actor.CharacterFlags.Contains(CharacterFlags.Resting))
            {
                await this.Communicator.SendToPlayer(actor, "You're far too relaxed.", cancellationToken);
                return;
            }

            var room = this.Communicator.ResolveRoom(actor.Location);

            if (room != null)
            {
                var exits = room.Exits.Where(e => e.IsClosed).ToList();

                if (exits != null)
                {
                    // Is there a locked exit?
                    var lockedExit = exits.FirstOrDefault(e => e.IsLocked);

                    if (lockedExit != null)
                    {
                        var race = Races.RaceData.First(r => r.Key == actor.Race);
                        var chance = 0;

                        switch (race.Value.Size)
                        {
                            case Size.Tiny:
                                chance = 2;
                                break;
                            case Size.Small:
                                chance = 5;
                                break;
                            case Size.Medium:
                                chance = 10;
                                break;
                            case Size.Large:
                                chance = 20;
                                break;
                            case Size.ExtraLarge:
                                chance = 25;
                                break;
                            case Size.Giant:
                                chance = 35;
                                break;
                        }

                        chance += (int)actor.Str.Current;

                        if (this.Random.Next(0, 100) < chance)
                        {
                            await this.Communicator.SendToPlayer(actor, $"You throw yourself against the {lockedExit.DoorName ?? "door"}, and smash it off its hinges!", cancellationToken);
                            var damage = this.Random.Next(10, 50);
                            lockedExit.IsLocked = false;
                            lockedExit.IsClosed = false;
                            var damageVerb = CombatProcessor.CalculateDamageVerb(damage);
                            await this.Communicator.SendToRoom(actor.Location, actor, target, $"{actor.FirstName.FirstCharToUpper()} smashes into the {lockedExit.DoorName ?? "door"} and knocks it off the hinges!", cancellationToken);
                            await this.Communicator.SendToPlayer(actor, $"Smashing open the {lockedExit.DoorName ?? "door"} {damageVerb} you!", cancellationToken);
                            await this.Communicator.SendToRoom(actor.Location, actor, target, $"{actor.FirstName.FirstCharToUpper()}'s smash door {damageVerb} {actor.Pronoun}!", cancellationToken);

                            actor.Health.Current -= damage;

                            // Need to open the door on BOTH sides
                            var oppRoom = this.Communicator.ResolveRoom(new KeyValuePair<long, long>(lockedExit.ToArea, lockedExit.ToRoom));

                            var exitToThisRoom = oppRoom?.Exits.FirstOrDefault(e => e.ToArea == room?.AreaId && e.ToRoom == room?.RoomId);

                            if (exitToThisRoom != null)
                            {
                                exitToThisRoom.IsClosed = false;
                                exitToThisRoom.IsLocked = false;
                                await this.Communicator.SendToRoom(new KeyValuePair<long, long>(lockedExit.ToArea, lockedExit.ToRoom), actor, target, $"{actor.FirstName.FirstCharToUpper()} obliterates the {exitToThisRoom.DoorName ?? "door"}!", cancellationToken);
                            }
                        }
                        else
                        {
                            await this.Communicator.SendToPlayer(actor, "You smash yourself against the door, and bounce off completely. Ouch, that HURT!", cancellationToken);
                            var damage = this.Random.Next(40, 150);

                            await this.Communicator.SendToRoom(actor.Location, actor, target, $"{actor.FirstName.FirstCharToUpper()} smashes into the {lockedExit.DoorName ?? "door"} and bounces completely off!", cancellationToken);

                            var damageVerb = CombatProcessor.CalculateDamageVerb(damage);

                            await this.Communicator.SendToPlayer(actor, $"Your smash attempt {damageVerb} you!", cancellationToken);
                            await this.Communicator.SendToRoom(actor.Location, actor, target, $"{actor.FirstName.FirstCharToUpper()}'s smash door {damageVerb} {actor.Pronoun}!", cancellationToken);

                            actor.Health.Current -= damage;
                        }
                    }
                    else
                    {
                        await this.Communicator.SendToPlayer(actor, "You might want to just try to open it first.", cancellationToken);
                    }
                }
                else
                {
                    await this.Communicator.SendToPlayer(actor, "There are no closed doors here, you wild animal.", cancellationToken);
                }
            }
        }
    }
}
