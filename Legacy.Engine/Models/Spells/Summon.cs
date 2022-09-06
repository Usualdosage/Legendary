// <copyright file="Summon.cs" company="Legendary™">
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

    /// <summary>
    /// Casts the summon spell.
    /// </summary>
    public class Summon : Spell
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Summon"/> class.
        /// </summary>
        /// <param name="communicator">ICommunicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat generator.</param>
        public Summon(ICommunicator communicator, IRandom random, IWorld world, ILogger logger, Combat combat)
            : base(communicator, random, world, logger, combat)
        {
            this.Name = "Summon";
            this.ManaCost = 50;
            this.CanInvoke = true;
            this.IsAffect = false;
        }

        /// <inheritdoc/>
        public override async Task Act(Character actor, Character? target, Item? itemTarget, CancellationToken cancellationToken)
        {
            if (target == null)
            {
               await this.Communicator.SendToPlayer(actor, "Summon whom?", cancellationToken);
            }
            else
            {
                var player = this.Communicator.ResolveCharacter(target);

                if (player != null)
                {
                    // Must be in the same area to summon a player.
                    if (player.Character.Location.Key == actor.Location.Key)
                    {
                        if (player.Character.Level > actor.Level + 6)
                        {
                            await this.Communicator.SendToPlayer(actor, $"You failed to summon {target.FirstName}.", cancellationToken);
                        }
                        else if (!this.Combat.DidSave(target, this))
                        {
                            // Player gets a save vs. spell
                            player.Character.Location = actor.Location;

                            // Track exploration for award purposes.
                            if (player.Character.Metrics.RoomsExplored.ContainsKey(actor.Location.Key))
                            {
                                var roomList = player.Character.Metrics.RoomsExplored[actor.Location.Key];

                                if (!roomList.Contains(actor.Location.Value))
                                {
                                    player.Character.Metrics.RoomsExplored[actor.Location.Key].Add(actor.Location.Value);
                                    await this.AwardProcessor.CheckVoyagerAward(actor.Location.Key, actor, cancellationToken);
                                }
                            }
                            else
                            {
                                player.Character.Metrics.RoomsExplored.Add(actor.Location.Key, new List<long>() { actor.Location.Value });
                                await this.AwardProcessor.CheckVoyagerAward(actor.Location.Key, actor, cancellationToken);
                            }

                            await this.Communicator.SendToPlayer(actor, $"You have summoned {player.Character.FirstName} here!", cancellationToken);
                            await this.Communicator.SendToRoom(actor.Location, actor, player.Character, $"{player.Character.FirstName.FirstCharToUpper()} arrives in a puff of smoke.", cancellationToken);
                            await this.Communicator.SendToPlayer(player.Connection, $"{actor.FirstName.FirstCharToUpper()} has summoned you!", cancellationToken);
                            await this.Communicator.SendToRoom(player.Character.Location, actor, player.Character, $"{player.Character.FirstName.FirstCharToUpper()} vanishes in a flash of light.", cancellationToken);

                            await this.Communicator.PlaySound(target, Core.Types.AudioChannel.Spell, Sounds.SUMMON, cancellationToken);
                            await this.Communicator.PlaySound(actor, Core.Types.AudioChannel.Spell, Sounds.SUMMON, cancellationToken);
                            await this.Communicator.PlaySoundToRoom(actor, target, Sounds.SUMMON, cancellationToken);
                        }
                        else
                        {
                            await this.Communicator.SendToPlayer(actor, $"You failed to summon {player.Character.FirstName}.", cancellationToken);
                        }
                    }
                    else
                    {
                        await this.Communicator.SendToPlayer(actor, $"They aren't here.", cancellationToken);
                    }
                }
                else
                {
                    var mobile = (Mobile)target;

                    if (mobile != null)
                    {
                        if (mobile.Level > actor.Level + 6)
                        {
                            await this.Communicator.SendToPlayer(actor, $"You failed to summon {target.FirstName}.", cancellationToken);
                        }
                        else if (!this.Combat.DidSave(mobile, this))
                        {
                            var oldRoom = this.Communicator.ResolveRoom(mobile.Location);
                            var newRoom = this.Communicator.ResolveRoom(actor.Location);

                            var oldMob = oldRoom != null ? oldRoom.Mobiles?.FirstOrDefault(m => m.CharacterId == mobile.CharacterId) : null;

                            if (oldMob != null)
                            {
                                oldRoom?.Mobiles?.Remove(oldMob);
                            }

                            newRoom?.Mobiles?.Add(mobile);

                            mobile.Location = actor.Location;

                            await this.Communicator.SendToPlayer(actor, $"You have summoned {mobile.FirstName} here!", cancellationToken);
                            await this.Communicator.SendToRoom(actor.Location, actor, null, $"{mobile.FirstName.FirstCharToUpper()} arrives in a puff of smoke.", cancellationToken);
                            await this.Communicator.SendToRoom(mobile.Location, actor, null, $"{mobile.FirstName.FirstCharToUpper()} vanishes in a flash of light.", cancellationToken);

                            await this.Communicator.PlaySound(target, Core.Types.AudioChannel.Spell, Sounds.SUMMON, cancellationToken);
                            await this.Communicator.PlaySound(actor, Core.Types.AudioChannel.Spell, Sounds.SUMMON, cancellationToken);
                            await this.Communicator.PlaySoundToRoom(actor, target, Sounds.SUMMON, cancellationToken);
                        }
                        else
                        {
                            await this.Communicator.SendToPlayer(actor, $"You failed to summon {target.FirstName}.", cancellationToken);
                        }
                    }
                    else
                    {
                        await this.Communicator.SendToPlayer(actor, $"They aren't here.", cancellationToken);
                    }
                }
            }
        }
    }
}
