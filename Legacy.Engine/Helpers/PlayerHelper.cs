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
        /// Determines whether or not the target is within the actor's PK range.
        /// </summary>
        /// <param name="actor">The actor.</param>
        /// <param name="target">The target.</param>
        /// <returns>True if the target is in the actor's PK range.</returns>
        public static bool IsInPK(Character actor, Character? target)
        {
            if (target == null)
            {
                return false;
            }
            else
            {
                if (target.IsNPC)
                {
                    return true;
                }

                if (actor.Level >= 90)
                {
                    return true;
                }
                else if (target.Level < 10 || actor.Level < 10)
                {
                    return false;
                }
                else
                {
                    double percentVariance = actor.Experience * 0.5;

                    // Calculate the upper and lower bounds based on 5% of the first number
                    double upperBound = actor.Experience + percentVariance;
                    double lowerBound = actor.Experience - percentVariance;

                    // Check if the second number is within the upper and lower bounds
                    return target.Experience >= lowerBound && target.Experience <= upperBound;
                }
            }
        }

        /// <summary>
        /// Determines if a player can see the room or not.
        /// </summary>
        /// <param name="environment">The environment.</param>
        /// <param name="communicator">The communicator.</param>
        /// <param name="actor">The actor.</param>
        /// <returns>True if the player can see.</returns>
        public static bool CanPlayerSee(IEnvironment environment, ICommunicator communicator, Character actor)
        {
            if (actor.AffectedBy.Any(e => e.Name == EffectName.BLINDNESS || e.Name == EffectName.DIRTKICKING))
            {
                return false;
            }

            var room = communicator.ResolveRoom(actor.Location);

            if (room != null && room.Flags != null)
            {
                if (room.Flags.Contains(RoomFlags.Bright))
                {
                    return true;
                }

                if (environment.IsNight || room.Flags.Contains(RoomFlags.Dark))
                {
                    if (actor.IsAffectedBy(nameof(Infravision)))
                    {
                        return true;
                    }
                    else
                    {
                        var light = actor.Equipment.FirstOrDefault(e => e.Key == WearLocation.Light).Value;

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
                            return true;
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

            if (actor.AffectedBy.Any(e => e.Name == EffectName.BLINDNESS || e.Name == EffectName.DIRTKICKING))
            {
                return false;
            }

            var room = communicator.ResolveRoom(actor.Location);

            if (room != null)
            {
                if (room.Flags != null)
                {
                    if (room.Flags.Contains(RoomFlags.Bright))
                    {
                        return true;
                    }

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
                            var light = actor.Equipment.FirstOrDefault(e => e.Key == WearLocation.Light).Value;

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

        /// <summary>
        /// Given the level, this method will calculate the total experience required for the player to reach it.
        /// </summary>
        /// <remarks>This method was created with assistance by ChatGPT-4.</remarks>
        /// <param name="startingLevel">The starting level.</param>
        /// <param name="targetLevel">The target level.</param>
        /// <param name="experiencePenalty">The experience penalty.</param>
        /// <returns>long.</returns>
        public static long GetTotalExperienceRequired(int startingLevel, int targetLevel, double experiencePenalty = 0)
        {
            int baseExperience = 1500;
            long totalExperienceRequired = 0;

            for (int i = startingLevel; i < targetLevel; i++)
            {
                totalExperienceRequired += (long)((baseExperience * (1 + Math.Log10(i)) * Math.Pow(1.2, i - 1)) + experiencePenalty);
            }

            return totalExperienceRequired;
        }
    }
}