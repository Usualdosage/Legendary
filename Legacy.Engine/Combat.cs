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
    using Legendary.Engine.Extensions;
    using Legendary.Engine.Models;
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
        /// Stops combat between two characters.
        /// </summary>
        /// <param name="actor">The first character.</param>
        /// <param name="target">The second character.</param>
        public static void StopFighting(Character actor, Character? target)
        {
            actor.CharacterFlags.RemoveIfExists(CharacterFlags.Fighting);
            actor.Fighting = null;

            if (target != null)
            {
                target.CharacterFlags.RemoveIfExists(CharacterFlags.Fighting);
                target.Fighting = null;
            }
        }

        /// <summary>
        /// Starts combat between two characters.
        /// </summary>
        /// <param name="actor">The first character.</param>
        /// <param name="target">The second character.</param>
        public static void StartFighting(Character actor, Character target)
        {
            actor.CharacterFlags.AddIfNotExists(CharacterFlags.Fighting);
            actor.Fighting = target;
            target.CharacterFlags.AddIfNotExists(CharacterFlags.Fighting);
            target.Fighting = actor;
        }

        /// <summary>
        /// Gets the default combat action (martial) for the fighting character.
        /// </summary>
        /// <remarks>If they are wielding a weapon, gets that weapon type, and returns the skill for it. Othwerise, returns hand to hand.</remarks>
        /// <param name="actor">The actor.</param>
        /// <returns>IAction.</returns>
        public IAction GetCombatAction(Character actor)
        {
            // TODO: This needs work insofar as we can't return a skill if the player doesn't have it. Also, we need to return the %age chance so
            // we can randomize it.
            var wielded = actor.Equipment.FirstOrDefault(e => e.WearLocation.Contains(WearLocation.Wielded));

            if (wielded != null)
            {
                switch (wielded.DamageType)
                {
                    default:
                        {
                            return new HandToHand(this.communicator, this.random, this);
                        }

                    case DamageType.Slash:
                        {
                            return new EdgedWeapons(this.communicator, this.random, this);
                        }
                }
            }

            return new HandToHand(this.communicator, this.random, this);
        }

        /// <summary>
        /// Does damage from the actor to the target.
        /// </summary>
        /// <param name="actor">The actor.</param>
        /// <param name="target">The target.</param>
        /// <param name="action">The action.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task DoDamage(Character actor, Character target, IAction? action, CancellationToken cancellationToken)
        {
            // Get the action the character is using to fight.
            IAction combatAction = action ?? this.GetCombatAction(actor);

            // Calculate damage FROM character TO target.
            var damage = this.CalculateDamage(actor, target, combatAction);

            // Check if the armor blocked anything.
            bool blocked = await this.CheckArmorBlock(actor, target, combatAction, cancellationToken);

            // Armor blocked it, so no damage taken.
            if (blocked)
            {
                damage = 0;
            }
            else
            {
                // Calculate the damage verb.
                var damFromVerb = this.CalculateDamageVerb(damage, blocked);

                await this.communicator.SendToPlayer(actor, $"Your {combatAction.DamageNoun} {damFromVerb} {target.FirstName}!", cancellationToken);
                await this.communicator.SendToPlayer(target, $"{actor.FirstName}'s {combatAction.DamageNoun} {damFromVerb} you!", cancellationToken);
                await this.communicator.SendToRoom(actor.Location, actor, target, $"{actor.FirstName}'s {combatAction.DamageNoun} {damFromVerb} {actor.LastName}!", cancellationToken);

                bool isDead = this.ApplyDamage(target, damage);

                if (isDead)
                {
                    // Target is dead.
                    StopFighting(actor, target);

                    if (actor.IsNPC && target.IsNPC)
                    {
                        // Mob killed mob.
                        await this.KillMobile(target, actor);
                    }
                    else if (target.IsNPC)
                    {
                        // Player killed mobile.
                        await this.KillMobile(target, actor);
                    }
                    else
                    {
                        // Player killed player.
                        var deadPlayer = this.communicator.ResolveCharacter(target);
                        if (deadPlayer != null)
                        {
                            await this.KillPlayer(deadPlayer, actor, cancellationToken);
                        }
                    }

                    // Add the experience to the player.
                    var experience = this.CalculateExperience(actor, target);
                    await this.communicator.SendToPlayer(actor, $"You gain {experience} experience points.", cancellationToken);
                }
                else
                {
                    // Show the opponent's condition.
                    string? condition = this.GetPlayerCondition(target);

                    if (!string.IsNullOrWhiteSpace(condition))
                    {
                        await this.communicator.SendToPlayer(actor, condition);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the combat tick globally for all users engaged in combat.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task HandleCombatTick(CancellationToken cancellationToken = default)
        {
            if (Communicator.Users != null)
            {
                foreach (var user in Communicator.Users)
                {
                    var character = user.Value.Character;
                    var target = user.Value.Character.Fighting;

                    if (character != null && character.CharacterFlags.Contains(CharacterFlags.Fighting) && target != null)
                    {
                        await this.DoDamage(character, target, this.GetCombatAction(character), cancellationToken);

                        // If the target is an NPC, do damage from it to the player (unless it's dead). Otherwise, for PvP, the loop will just pick up the next fighter.
                        if (target.CharacterFlags.Contains(CharacterFlags.Fighting) && target.IsNPC)
                        {
                            await this.DoDamage(target, character, this.GetCombatAction(character), cancellationToken);
                        }

                        // Update the player info.
                        await this.communicator.ShowPlayerInfo(user.Value);
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

            // TODO: We need to calculate the hitdice and damdice based on the WEAPON if this isn't hand to hand.

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
        /// Calculate the experience the actor gets from killing the target.
        /// </summary>
        /// <param name="actor">The actor.</param>
        /// <param name="target">The target.</param>
        /// <returns>Int.</returns>
        public int CalculateExperience(Character actor, Character target)
        {
            // The greater the level difference, the more experience, and vice versa.

            // Start with the base experience per kill.
            int baseExperience = this.random.Next(200, 400);

            // e.g. 10, 50 = -40
            int levelOffset = actor.Level - target.Level;

            if (levelOffset < 0)
            {
                levelOffset = Math.Abs(levelOffset * 10); // e.g. 400
            }

            // Calculate the modifier. Need at least 1 to produce a non-zero result.
            double expModifier = Math.Max(1, levelOffset / 100); // e.g. 4

            // Bonus if char is evil vs good, vice versa.
            double bonus = 1;

            switch (actor.Alignment)
            {
                case Alignment.Good:
                    {
                        switch (target.Alignment)
                        {
                            case Alignment.Good:
                                {
                                    bonus = .5;
                                    break;
                                }

                            case Alignment.Evil:
                                {
                                    bonus = 2;
                                    break;
                                }
                        }

                        break;
                    }

                case Alignment.Evil:
                    {
                        switch (target.Alignment)
                        {
                            case Alignment.Good:
                                {
                                    bonus = 2;
                                    break;
                                }

                            case Alignment.Evil:
                                {
                                    bonus = .75;
                                    break;
                                }
                        }

                        break;
                    }
            }

            double expResult = (baseExperience * expModifier) * bonus;

            actor.Experience += (int)expResult;

            return (int)expResult;
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
        /// Kills a mobile.
        /// </summary>
        /// <param name="target">The mobile.</param>
        /// <param name="killer">The killer of the mobile.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task KillMobile(Character target, Character killer, CancellationToken cancellationToken = default)
        {
            StopFighting(target, killer);

            await this.communicator.SendToPlayer(killer, $"You have KILLED {target.FirstName}!", cancellationToken);
            await this.communicator.SendToRoom(target.Location, target, killer, $"{target.FirstName} is DEAD!", cancellationToken);

            var room = this.communicator.ResolveRoom(killer.Location);

            if (room != null)
            {
                var mobile = (Mobile)target;

                // Let's just do this once.
                if (room.Mobiles.Contains(mobile))
                {
                    room.Mobiles.Remove(mobile);
                    this.GenerateCorpse(killer.Location, target);
                }
            }
        }

        /// <summary>
        /// Kills a player.
        /// </summary>
        /// <param name="userData">The user data.</param>
        /// <param name="killer">The killer of the player.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task KillPlayer(UserData? userData, Character killer, CancellationToken cancellationToken = default)
        {
            if (userData != null)
            {
                StopFighting(userData.Character, killer);

                await this.communicator.SendToPlayer(killer, $"You have KILLED {userData.Character.FirstName}!", cancellationToken);
                await this.communicator.SendToPlayer(userData.Connection, $"{killer.FirstName} has KILLED you! You are now dead.", cancellationToken);
                await this.communicator.SendToRoom(killer.Location, userData.ConnectionId, $"{userData.Character.FirstName} is DEAD!");

                // Make dead and ghost.
                userData.Character.CharacterFlags?.AddIfNotExists(Core.Types.CharacterFlags.Dead);
                userData.Character.CharacterFlags?.AddIfNotExists(Core.Types.CharacterFlags.Ghost);

                var room = this.communicator.ResolveRoom(killer.Location);

                if (room != null)
                {
                    this.logger.Debug($"{killer.FirstName} has killed {userData.Character.FirstName} in room {room.RoomId}, area {room.AreaId}!");

                    // Generate the corpse.
                    this.GenerateCorpse(killer.Location, userData.Character);
                }

                // Remove all equipment and inventory.
                userData.Character.Inventory = new List<Item>();
                userData.Character.Equipment = new List<Item>();
                userData.Character.Currency = 0;

                // Send the character to their home.
                userData.Character.Location = userData.Character.Home;

                // Increment deaths.
                if (killer.IsNPC)
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

                // Show player info.
                await this.communicator.ShowPlayerInfo(userData, cancellationToken);

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
        /// Checks to see if a player's armor blocks a particular attack. If it blocks, apply damage to the armor.
        /// </summary>
        /// <param name="actor">The actor.</param>
        /// <param name="target">The target.</param>
        /// <param name="action">The action.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if blocked.</returns>
        public async Task<bool> CheckArmorBlock(Character actor, Character target, IAction action, CancellationToken cancellationToken)
        {
            if (target.Equipment.Count == 0)
            {
                return false;
            }

            bool blocked = false;
            var armorSavePct = this.random.Next(1, 100);

            switch (action.DamageType)
            {
                default:
                    var targetMagicPct = target.Equipment.Where(e => e.ItemType == ItemType.Armor).Sum(s => s.Magic);
                    if (armorSavePct < targetMagicPct)
                    {
                        blocked = true;
                    }

                    break;
                case DamageType.Pierce:
                    var targetPiercePct = target.Equipment.Where(e => e.ItemType == ItemType.Armor).Sum(s => s.Pierce);
                    if (armorSavePct < targetPiercePct)
                    {
                        blocked = true;
                    }

                    break;
                case DamageType.Slash:
                    var targetSlashPct = target.Equipment.Where(e => e.ItemType == ItemType.Armor).Sum(s => s.Edged);
                    if (armorSavePct < targetSlashPct)
                    {
                        blocked = true;
                    }

                    break;
                case DamageType.Blunt:
                    var targetBluntPct = target.Equipment.Where(e => e.ItemType == ItemType.Armor).Sum(s => s.Blunt);
                    if (armorSavePct < targetBluntPct)
                    {
                        blocked = true;
                    }

                    break;
            }

            if (blocked)
            {
                try
                {
                    // Get the random piece of player's armor that performed the block.
                    var allArmor = target.Equipment.Where(e => e.ItemType == ItemType.Armor).ToList();

                    if (allArmor.Count > 0)
                    {
                        var armorIndex = this.random.Next(0, allArmor.Count - 1);
                        var randomGear = allArmor[armorIndex];

                        if (randomGear != null)
                        {
                            await this.communicator.SendToPlayer(actor, $"{target.FirstName} blocked your attack with their armor!", cancellationToken);
                            await this.communicator.SendToPlayer(target, $"You absorbed {actor.FirstName}'s attack with {randomGear.Name}!", cancellationToken);

                            randomGear.Durability.Current -= 1;

                            if (randomGear.Durability.Current <= 0)
                            {
                                // It's destroyed.
                                await this.communicator.SendToPlayer(actor, $"You destroyed {randomGear.Name}!", cancellationToken);
                                await this.communicator.SendToPlayer(target, $"{actor.FirstName} destroyed {randomGear.Name}.", cancellationToken);

                                target.Equipment.Remove(randomGear);
                            }

                            if (!target.IsNPC)
                            {
                                await this.communicator.SaveCharacter(target);
                            }
                        }
                    }
                }
                catch (Exception exc)
                {
                    this.logger.Error(exc);
                }
            }

            return blocked;
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
        /// <param name="blocked">Whether or not the attack was blocked.</param>
        /// <returns>String.</returns>
        public string CalculateDamageVerb(int damage, bool blocked)
        {
            if (blocked)
            {
                return "<span class='damage damage_0'>was blocked by</span>";
            }

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

        /// <summary>
        /// Generates a corpse of the victim and places it in the room.
        /// </summary>
        /// <param name="location">The room to generate the corpse in.</param>
        /// <param name="victim">The victim to generate the corpse from.</param>
        private void GenerateCorpse(KeyValuePair<long, long> location, Character victim)
        {
            try
            {
                var corpse = new Item()
                {
                    ItemType = ItemType.Container,
                    Location = location,
                    Level = victim.Level,
                    Name = $"the corpse of {victim.FirstName}",
                    ShortDescription = $"The corpse of {victim.FirstName} is rotting here.",
                    LongDescription = $"The corpse of {victim.FirstName} is rotting here.",
                    WearLocation = new List<WearLocation>() { WearLocation.None },
                    Weight = 200,
                    RotTimer = 2,
                };

                // Create a corpse with their stuff in the room.
                var playerInventory = victim.Inventory;
                var equipment = victim.Equipment;
                var currency = victim.Currency;

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

                    // corpse.Contains.Add(currencyObj);
                }

                // Add the gear to the corpse.
                // corpse.Contains.AddRange(equipment);
                // corpse.Contains.AddRange(playerInventory);

                // Add the corpse to the room.
                var room = this.communicator.ResolveRoom(location);
                room.Items.Add(corpse);
            }
            catch (Exception exc)
            {
                this.logger.Error(exc);
            }
        }
    }
}
