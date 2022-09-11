// <copyright file="PlayerHelper.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection.PortableExecutable;
    using Legendary.Core;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Core.Types;
    using Legendary.Engine.Extensions;
    using Legendary.Engine.Models.Skills;
    using Legendary.Engine.Models.Spells;

    /// <summary>
    /// Helper for players.
    /// </summary>
    public class PlayerHelper
    {
        /// <summary>
        /// Determines if a player can see the room or not.
        /// </summary>
        /// <param name="environment">The environment.</param>
        /// <param name="communicator">The communicator.</param>
        /// <param name="actor">The actor.</param>
        /// <returns>True if the player can see.</returns>
        public static bool CanPlayerSee(IEnvironment environment, ICommunicator communicator, Character actor)
        {
            if (actor.AffectedBy.Any(e => e.Name == EffectName.BLINDNESS))
            {
                return false;
            }

            var room = communicator.ResolveRoom(actor.Location);

            if (room != null && room.Flags != null)
            {
                if (environment.IsNight || room.Flags.Contains(RoomFlags.Dark))
                {
                    if (actor.IsAffectedBy(nameof(Infravision)))
                    {
                        return true;
                    }
                    else
                    {
                        var light = actor.Equipment.FirstOrDefault(e => e.WearLocation.Contains(WearLocation.Light));
                        if (light == null)
                        {
                            // See if there is a light in the room
                            var lightInRoom = room.Items.FirstOrDefault(e => e.WearLocation.Contains(WearLocation.Light));

                            if (lightInRoom == null)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Determines if a player can see another player.
        /// </summary>
        /// <param name="environment">The environment.</param>
        /// <param name="communicator">The communicator.</param>
        /// <param name="actor">The actor.</param>
        /// <param name="target">The target.</param>
        /// <returns>True if the actor can see the target.</returns>
        public static bool CanPlayerSeePlayer(IEnvironment environment, ICommunicator communicator, Character actor, Character? target)
        {
            if (target == null)
            {
                return false;
            }

            if (actor.CharacterId == target.CharacterId)
            {
                return true;
            }

            if (actor.AffectedBy.Any(e => e.Name == EffectName.BLINDNESS))
            {
                return false;
            }

            var room = communicator.ResolveRoom(actor.Location);

            if (room != null)
            {
                if (room.Flags != null)
                {
                    if (environment.IsNight || room.Flags.Contains(RoomFlags.Dark))
                    {
                        if (actor.IsAffectedBy(nameof(Infravision)))
                        {
                            if (target.IsAffectedBy(nameof(Hide)))
                            {
                                return false;
                            }
                            else if (target.IsAffectedBy(nameof(Invisibility)))
                            {
                                if (actor.IsAffectedBy(nameof(DetectInvisibility)))
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                return true;
                            }
                        }
                        else
                        {
                            var light = actor.Equipment.FirstOrDefault(e => e.WearLocation.Contains(WearLocation.Light));
                            if (light == null)
                            {
                                // See if there is a light in the room
                                var lightInRoom = room.Items.FirstOrDefault(e => e.WearLocation.Contains(WearLocation.Light));

                                if (lightInRoom == null)
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                if (target.IsAffectedBy(nameof(Hide)))
                                {
                                    return false;
                                }
                                else if (target.IsAffectedBy(nameof(Invisibility)))
                                {
                                    if (actor.IsAffectedBy(nameof(DetectInvisibility)))
                                    {
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                                else
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (target.IsAffectedBy(nameof(Hide)))
                        {
                            return false;
                        }
                        else if (target.IsAffectedBy(nameof(Invisibility)))
                        {
                            if (actor.IsAffectedBy(nameof(DetectInvisibility)))
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    if (target.IsAffectedBy(nameof(Hide)))
                    {
                        return false;
                    }
                    else if (target.IsAffectedBy(nameof(Invisibility)))
                    {
                        if (actor.IsAffectedBy(nameof(DetectInvisibility)))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            else
            {
                return false;
            }

            return true;
        }
    }
}