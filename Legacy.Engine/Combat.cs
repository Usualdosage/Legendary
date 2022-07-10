// <copyright file="Combat.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core.Contracts;
    using Legendary.Core.Extensions;
    using Legendary.Core.Models;
    using Legendary.Core.Types;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Models.Skills;

    /// <summary>
    /// Handles actions in combat related to skill and spell usage.
    /// </summary>
    public class Combat
    {
        private readonly IRandom random;
        private readonly ICommunicator communicator;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="Combat"/> class.
        /// </summary>
        /// <param name="communicator">The communicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="logger">The logger.</param>
        public Combat(ICommunicator communicator, IRandom random, ILogger logger)
        {
            this.random = random;
            this.communicator = communicator;
            this.logger = logger;
        }

        /// <summary>
        /// Handles the combat tick globally for all users engaged in combat.
        /// </summary>
        /// <param name="users">The global list of users.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task HandleCombatTick(ConcurrentDictionary<string, UserData>? users, CancellationToken cancellationToken = default)
        {
            if (users != null)
            {
                foreach (var user in users)
                {
                    var character = user.Value.Character;

                    if (character.CharacterFlags.Contains(CharacterFlags.Fighting) && character.Fighting != null)
                    {
                        this.logger.Debug($"{character.FirstName} is fighting {character.Fighting?.FirstName}.");

                        // Calculate damage FROM character TO target
                        var damFrom = this.CalculateDamage(user.Value.Character, character.Fighting, new HandToHand(this.communicator, this.random, this));

                        var damFromVerb = this.CalculateDamageVerb(damFrom);

                        await this.communicator.SendToPlayer(user.Value.Connection, $"Your punch {damFromVerb} {character.Fighting?.FirstName}!");

                        bool charIsDead = this.ApplyDamage(character.Fighting, damFrom);

                        // Calculate damage FROM target TO character
                        var damTo = this.CalculateDamage(character.Fighting, user.Value.Character, new HandToHand(this.communicator, this.random, this));

                        var damToVerb = this.CalculateDamageVerb(damTo);

                        await this.communicator.SendToPlayer(user.Value.Connection, $"{character.Fighting?.FirstName}'s punch {damToVerb} you!");

                        bool targetIsDead = this.ApplyDamage(character, damTo);

                        // Update the player info
                        await this.communicator.ShowPlayerInfo(user.Value);

                        // Show the opponent's condition.
                        string? condition = this.GetPlayerCondition(character.Fighting);
                        if (!string.IsNullOrWhiteSpace(condition))
                        {
                            await this.communicator.SendToPlayer(user.Value.Connection, condition);
                        }

                        // Check dead
                        if (charIsDead || targetIsDead)
                        {
                            // Send the death message to the room, players, and area
                            if (charIsDead)
                            {
                                await this.communicator.SendToPlayer(user.Value.Connection, $"{character.Fighting?.FirstName} has KILLED you!");

                                if (character.Fighting != null)
                                {
                                    await this.communicator.SendToRoom(character.Fighting.Location, string.Empty, $"{character.Fighting?.FirstName} has KILLED {user.Value.Character.FirstName}!");
                                }

                                await this.KillPlayer(user.Value, character.Fighting?.IsNPC ?? false);
                            }

                            if (targetIsDead)
                            {
                                await this.communicator.SendToPlayer(user.Value.Connection, $"{character.Fighting?.FirstName} is DEAD!");
                                await this.communicator.SendToRoom(user.Value.Character.Location, string.Empty, $"{character.Fighting?.FirstName} is DEAD!");

                                // TODO: Generate corpse

                                // TODO: Check experience
                            }

                            character.Fighting?.CharacterFlags.Remove(CharacterFlags.Fighting);

                            if (character.Fighting?.Fighting != null)
                            {
                                character.Fighting.Fighting = null;
                            }

                            character.CharacterFlags.Remove(CharacterFlags.Fighting);
                            character.Fighting = null;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the damage for an action by an actor against a target.
        /// </summary>
        /// <param name="actor">The actor.</param>
        /// <param name="target">The target.</param>
        /// <param name="action">The action.</param>
        /// <returns>Damage.</returns>
        public int CalculateDamage(Character? actor, Character? target, IAction action)
        {
            if (actor == null || target == null)
            {
                return 0;
            }

            // Reduce the damage inversely by level. So if the player is 10, target is 10, damage modifier is normal.
            // If the player is 20, target is 10, damage modifier is doubled.
            // If the player is 10, target is 20, damage modifier is halved.
            double adjust = (actor.Level / target.Level) * action.DamageModifier;

            var damage = 0;
            for (var x = 0; x < action.HitDice; x++)
            {
                damage += this.random.Next(1, action.DamageDice);
            }

            if (this.DidSave(target, action))
            {
                // Save for half damage.
                return (int)((damage + adjust) / 2);
            }
            else
            {
                // Whole numbers only.
                return (int)(damage + adjust);
            }
        }

        /// <summary>
        /// Applies the damage to the target. If damage brings them to below 0, return true.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="damage">The damage.</param>
        /// <returns>True if the target is killed.</returns>
        public bool ApplyDamage(Character? target, int damage)
        {
            if (target == null)
            {
                return false;
            }

            target.Health.Current -= damage;

            // If below zero, character is dead.
            if (target.Health.Current < 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Kills a player.
        /// </summary>
        /// <param name="userData">The user data.</param>
        /// <param name="isMobDeath">True if killed by mob.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task KillPlayer(UserData? userData, bool isMobDeath, CancellationToken cancellationToken = default)
        {
            if (userData != null)
            {
                // Remove fighting flags.
                userData.Character.CharacterFlags?.RemoveIfExists(Core.Types.CharacterFlags.Fighting);

                // Make dead and ghost.
                userData.Character.CharacterFlags?.AddIfNotExists(Core.Types.CharacterFlags.Dead);
                userData.Character.CharacterFlags?.AddIfNotExists(Core.Types.CharacterFlags.Ghost);

                // Create a corpse with their stuff in the room.
                var playerInventory = userData.Character.Inventory;
                var equipment = userData.Character.Equipment;
                var currency = userData.Character.Currency;

                var corpse = new Item()
                {
                    ItemType = ItemType.Container,
                    Location = userData.Character.Location,
                    Level = userData.Character.Level,
                    ShortDescription = $"The corpse of {userData.Character.FirstName}",
                    LongDescription = $"The corpse of {userData.Character.FirstName} is lying here.",
                    WearLocation = new List<WearLocation>() { WearLocation.None },
                    Weight = 200,
                    RotTimer = 24,
                };

                // If the player had any money, add it to the corpse.
                if (currency > 0)
                {
                    var currencyObj = new Item()
                    {
                        ItemType = ItemType.Currency,
                        Value = currency,
                        WearLocation = new List<WearLocation>() { WearLocation.None },
                        Weight = currency / 10,
                        ShortDescription = $"{currency} gold coins",
                        LongDescription = $"{currency} gold coins are lying here.",
                        Level = 0,
                    };

                    corpse.Contains.Add(currencyObj);
                }

                // Add the gear to the corpse.
                corpse.Contains.AddRange(equipment);
                corpse.Contains.AddRange(playerInventory);

                // Add the corpse to the room.
                Room? room = this.communicator.GetRoom(userData.Character.Location);
                if (room != null)
                {
                    room.Items.Add(corpse);
                }

                // Remove all equipment and inventory.
                userData.Character.Inventory = new List<Item>();
                userData.Character.Equipment = new List<Item>();
                userData.Character.Currency = 0;

                // Send the character to their home.
                userData.Character.Location = userData.Character.Home ?? Room.Default;

                // Increment deaths.
                if (isMobDeath)
                {
                    userData.Character.Metrics.MobDeaths += 1;
                }
                else
                {
                    userData.Character.Metrics.PlayerDeaths += 1;
                }

                // Update con loss.
                if (userData.Character.Metrics.TotalDeaths % 4 == 0)
                {
                    userData.Character.Con.Max -= 1;
                    userData.Character.Con.Current = Math.Min(userData.Character.Con.Current, userData.Character.Con.Max);
                    await this.communicator.SendToPlayer(userData.Connection, "You feel less healthy.", cancellationToken);
                }

                // Save changes.
                await this.communicator.SaveCharacter(userData);

                // Show the player their new surroundings.
                await this.communicator.ShowRoomToPlayer(userData, cancellationToken);
            }
        }

        /// <summary>
        /// Shows the player's physical condition.
        /// </summary>
        /// <param name="target">The player.</param>
        /// <returns>String.</returns>
        public string? GetPlayerCondition(Character? target)
        {
            if (target == null)
            {
                return null;
            }

            // Get the target's health as a percentage of their total health.
            var percentage = target.Health.GetPercentage();

            var message = percentage switch
            {
                <= 0 => "<span class='player-health'>is DEAD.</span>", // 0
                > 0 and <= 10 => $"<span class='player-health'>is on {target.Pronoun} death bed.</span>", // 1-10
                > 11 and <= 20 => "<span class='player-health'>is mortally wounded.</span>", // 11-20
                > 21 and <= 30 => "<span class='player-health'>is seriously hurt.</span>", // 21-30
                > 31 and <= 40 => "<span class='player-health'>has taken some severe damage.</span>", // 31-40
                > 41 and <= 50 => "<span class='player-health'>is covered in major wounds.</span>", // 41-50
                > 51 and <= 60 => "<span class='player-health'>is bleeding profusely from many wounds.</span>", // 51-60
                > 61 and <= 70 => "<span class='player-health'>has some big wounds and nasty scratches.</span>", // 61-70
                > 71 and <= 80 => "<span class='player-health'>has some abrasions and lacerations.</span>", // 71-80
                > 81 and <= 90 => "<span class='player-health'>has some small wounds and bruises.</span>", // 81-90
                > 91 and <= 99 => "<span class='player-health'>has some tiny scrapes.</span>", // 91-99
                _ => "<span class='player-health'>is in perfect health.</span>" // 100
            };

            return $"{target.FirstName} {message}.";
        }

        /// <summary>
        /// Determines whether the target saved vs the attack type.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="action">The action to save against.</param>
        /// <returns>True if the target saved.</returns>
        public bool DidSave(Character target, IAction action)
        {
            var saveThrow = this.random.Next(1, 20);

            // Critical failure.
            if (saveThrow == 1)
            {
                return false;
            }

            switch (action.DamageType)
            {
                default:
                    return saveThrow < target.Saves.Spell;
                case Core.Types.DamageType.Energy:
                case Core.Types.DamageType.Negative:
                    return saveThrow < target.Saves.Negative;
            }
        }

        /// <summary>
        /// Applies the damage to the target. If damage brings them to below 0, sets the "dead" flags.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="damage">The damage.</param>
        public void ApplyDamage(Mobile target, int damage)
        {
            target.Health.Current -= damage;

            // If below zero, character is dead. Set the appropriate flags.
            if (target.Health.Current < 0)
            {
                target.CharacterFlags?.RemoveIfExists(Core.Types.CharacterFlags.Fighting);
                target.CharacterFlags?.AddIfNotExists(Core.Types.CharacterFlags.Dead);
                target.CharacterFlags?.AddIfNotExists(Core.Types.CharacterFlags.Ghost);
                target.Location = target.Home ?? Room.Default;
            }
        }

        /// <summary>
        /// Determines whether the target saved vs the attack type.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="action">The action to save against.</param>
        /// <returns>True if the target saved.</returns>
        public bool DidSave(Mobile target, IAction action)
        {
            var saveThrow = this.random.Next(1, 20);

            // Critical failure.
            if (saveThrow == 1)
            {
                return false;
            }

            switch (action.DamageType)
            {
                default:
                    return saveThrow < target.Saves.Spell;
                case Core.Types.DamageType.Energy:
                case Core.Types.DamageType.Negative:
                    return saveThrow < target.Saves.Negative;
            }
        }

        /// <summary>
        /// Calculates the damage verb messages based on the raw damage.
        /// </summary>
        /// <param name="damage">The damage as a total.</param>
        /// <returns>String.</returns>
        public string CalculateDamageVerb(int damage)
        {
            var message = damage switch
            {
                <= 0 => "<span class='damage damage_0'>has no effect on</span>",
                > 0 and <= 10 => "<span class='damage damage_1'>scratches</span>", // 1-10
                > 11 and <= 20 => "<span class='damage damage_2'>injures</span>", // 11-20
                > 21 and <= 30 => "<span class='damage damage_3'>wounds</span>", // 21-30
                > 31 and <= 40 => "<span class='damage damage_4'>mauls</span>", // 31-40
                > 41 and <= 50 => "<span class='damage damage_5'>maims</span>", // 41-50
                > 51 and <= 100 => "<span class='damage damage_6'>MUTILATES</span>", // 51-100
                > 101 and <= 200 => "<span class='damage damage_7'>MASSACRES</span>", // 101-200
                > 201 and <= 300 => "<span class='damage damage_8'>MANGLES</span>", // 201-300
                > 301 and <= 500 => "<span class='damage damage_9'>*** OBLITERATES ***</span>", // 301-500
                > 501 and <= 700 => "<span class='damage damage_10'>*** DISINTEGRATES ***</span>", // 501-700
                > 701 and <= 900 => "<span class='damage damage_11'>*** ANNIHILIATES ***</span>", // 701-900
                > 901 and <= 1100 => "<span class='damage damage_12'>=== EVISCERATES ===</span>", // 901-1100
                _ => "<span class='damage damage_13'>does UNSPEAKABLE things</span>" // Over 1100
            };

            return message;
        }

        /// <summary>
        /// If the action has an affect, applies that affect to the target.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="action">The action.</param>
        public void ApplyAffect(Character target, IAction action)
        {
            bool hasEffect = target.AffectedBy.Any(a => a.Key == action);

            if (!hasEffect && action.AffectDuration.HasValue)
            {
                target.AffectedBy.Add(action, action.AffectDuration.Value);
            }
        }
    }
}
