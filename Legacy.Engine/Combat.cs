﻿// <copyright file="Combat.cs" company="Legendary™">
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Channels;
    using System.Threading.Tasks;
    using Legendary.Core;
    using Legendary.Core.Contracts;
    using Legendary.Core.Extensions;
    using Legendary.Core.Models;
    using Legendary.Core.Types;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Extensions;
    using Legendary.Engine.Models.Skills;
    using Legendary.Engine.Processors;
    using MongoDB.Driver;

    /// <summary>
    /// Handles actions in combat related to skill and spell usage.
    /// </summary>
    public class Combat
    {
        private readonly IRandom random;
        private readonly ICommunicator communicator;
        private readonly ILogger logger;
        private readonly IWorld world;
        private readonly AwardProcessor awardProcessor;
        private readonly ActionProcessor actionProcessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="Combat"/> class.
        /// </summary>
        /// <param name="communicator">The communicator.</param>
        /// <param name="world">The world.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="logger">The logger.</param>
        public Combat(ICommunicator communicator, IWorld world, IRandom random, ILogger logger)
        {
            this.random = random;
            this.communicator = communicator;
            this.logger = logger;
            this.world = world;

            this.awardProcessor = new AwardProcessor(communicator, world, logger, random, this);
            this.actionProcessor = new ActionProcessor(communicator, world, logger, random, this);
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
        /// Shows the player's physical condition.
        /// </summary>
        /// <param name="target">The player.</param>
        /// <returns>String.</returns>
        public static string? GetPlayerCondition(Character? target)
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

            return $"{target.FirstName.FirstCharToUpper()} {message}";
        }

        /// <summary>
        /// Applies the damage to the target. If damage brings them to below 0, return true.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="damage">The damage.</param>
        /// <returns>True if the target is killed.</returns>
        public static bool ApplyDamage(Character? target, int damage)
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
        /// Calculates the damage verb messages based on the raw damage.
        /// </summary>
        /// <param name="damage">The damage as a total.</param>
        /// <param name="blocked">Whether or not the attack was blocked.</param>
        /// <returns>String.</returns>
        public static string CalculateDamageVerb(int damage, bool blocked)
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
                > 1100 and <= 100000 => "<span class='damage damage_13'>does UNSPEAKABLE things</span>", // Over 1100
                _ => "<span class='damage damage_0'>has no effect on</span>",
            };

            return message;
        }

        /// <summary>
        /// Starts combat between two characters.
        /// </summary>
        /// <param name="actor">The first character.</param>
        /// <param name="target">The second character.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task StartFighting(Character actor, Character target, CancellationToken cancellationToken)
        {
            if (!actor.IsNPC && !target.IsNPC && actor.CharacterFlags.Contains(CharacterFlags.Ghost))
            {
                // PVP, where actor is a ghost.
                await this.communicator.SendToPlayer(actor, $"You can't attack {target.FirstName} while you're a ghost.", cancellationToken);
            }
            else if (!target.IsNPC && target.CharacterFlags.Contains(CharacterFlags.Ghost))
            {
                // PVP, where target is a ghost.
                await this.communicator.SendToPlayer(actor, $"You can't attack {target.FirstName} because they are a ghost.", cancellationToken);
            }
            else
            {
                await this.communicator.SendToPlayer(target, $"[NOTIFICATION]|../img/notifications/attack.png|{actor.FirstName} has attacked you!", cancellationToken);

                actor.CharacterFlags.AddIfNotExists(CharacterFlags.Fighting);
                actor.Fighting = target.CharacterId;
                target.CharacterFlags.AddIfNotExists(CharacterFlags.Fighting);
                target.Fighting = actor.CharacterId;
            }
        }

        /// <summary>
        /// Gets the default combat action (martial) for the fighting character.
        /// </summary>
        /// <remarks>If they are wielding a weapon, gets that weapon type, and returns the skill for it. Othwerise, returns hand to hand.</remarks>
        /// <param name="actor">The actor.</param>
        /// <returns>IAction.</returns>
        public IAction GetCombatAction(Character actor)
        {
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

                    case DamageType.Blunt:
                        {
                            return new BluntWeapons(this.communicator, this.random, this);
                        }

                    case DamageType.Pierce:
                        {
                            return new PiercingWeapons(this.communicator, this.random, this);
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
        public async Task DoDamage(Character actor, Character target, IAction? action, CancellationToken cancellationToken = default)
        {
            // Get the action the character is using to fight.
            IAction combatAction = action ?? this.GetCombatAction(actor);

            // Assume no block unless otherwise checked.
            bool blocked = false;

            // This is an automatic martial skill.
            if (combatAction.ActionType == ActionType.Skill && !combatAction.CanInvoke)
            {
                SkillProficiency? proficiency;

                proficiency = actor.GetSkillProficiency(combatAction.Name);

                if (proficiency != null && proficiency.Proficiency > 0)
                {
                    if (await combatAction.IsSuccess(proficiency.Proficiency, cancellationToken))
                    {
                        // This was a hit, check armor block.
                        blocked = await this.CheckArmorBlock(actor, target, combatAction, cancellationToken);

                        await this.communicator.PlaySound(actor, AudioChannel.Martial, GetSoundEffect(combatAction.DamageNoun), cancellationToken);
                        await this.communicator.PlaySound(target, AudioChannel.Martial, GetSoundEffect(combatAction.DamageNoun), cancellationToken);
                        await combatAction.PreAction(actor, target, cancellationToken);
                        await combatAction.Act(actor, target, cancellationToken);
                    }
                    else
                    {
                        // This was a total martial combat miss. Show the miss and exit.
                        await this.communicator.SendToPlayer(actor, $"Your {combatAction.DamageNoun} misses {target.FirstName}.", cancellationToken);
                        await this.communicator.SendToPlayer(target, $"{actor.FirstName.FirstCharToUpper()}'s {combatAction.DamageNoun} misses you.", cancellationToken);
                        await this.communicator.SendToRoom(actor.Location, actor, target, $"{actor.FirstName.FirstCharToUpper()}'s {combatAction.DamageNoun} misses {target.FirstName}.", cancellationToken);
                    }

                    // Run post action to check if the skill improved.
                    await combatAction.PostAction(actor, target, cancellationToken);
                }
                else
                {
                    // This was a total miss because the character is not proficient. Don't allow an increase.
                    await this.communicator.SendToPlayer(actor, $"Your {combatAction.DamageNoun} misses {target.FirstName}.", cancellationToken);
                    await this.communicator.SendToPlayer(target, $"{actor.FirstName.FirstCharToUpper()}'s {combatAction.DamageNoun} misses you.", cancellationToken);
                    await this.communicator.SendToRoom(actor.Location, actor, target, $"{actor.FirstName.FirstCharToUpper()}'s {combatAction.DamageNoun} misses {target.FirstName}.", cancellationToken);
                    return;
                }
            }
            else
            {
                // This is a spell that automatically hits, check armor block.
                blocked = await this.CheckArmorBlock(actor, target, combatAction, cancellationToken);
            }

            // Calculate damage FROM character TO target.
            var damage = this.CalculateDamage(actor, target, combatAction);

            // Calculate the damage verb.
            var damFromVerb = CalculateDamageVerb(damage, blocked);

            await this.communicator.SendToPlayer(actor, $"Your {combatAction.DamageNoun} {damFromVerb} {target.FirstName}!", cancellationToken);
            await this.communicator.SendToPlayer(target, $"{actor.FirstName.FirstCharToUpper()}'s {combatAction.DamageNoun} {damFromVerb} you!", cancellationToken);
            await this.communicator.SendToRoom(actor.Location, actor, target, $"{actor.FirstName.FirstCharToUpper()}'s {combatAction.DamageNoun} {damFromVerb} {target.FirstName}!", cancellationToken);

            bool isDead = ApplyDamage(target, damage);

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
                    await this.KillPlayer(target, actor, cancellationToken);
                }

                // Add the experience to the player.
                var experience = this.CalculateExperience(actor, target);
                await this.communicator.SendToPlayer(actor, $"You gain {experience} experience points.", cancellationToken);
                actor.Experience += experience;

                // See if the player advanced a level.
                var advance = this.communicator.CheckLevelAdvance(actor, cancellationToken);
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
                    var target = this.communicator.ResolveFightingCharacter(character);

                    if (character != null && character.CharacterFlags.Contains(CharacterFlags.Fighting) && target != null)
                    {
                        await this.DoDamage(character, target, this.GetCombatAction(character), cancellationToken);

                        // If the target is an NPC, do damage from it to the player (unless it's dead). Otherwise, for PvP, the loop will just pick up the next fighter.
                        if (target.CharacterFlags.Contains(CharacterFlags.Fighting) && target.IsNPC)
                        {
                            await this.DoDamage(target, character, this.GetCombatAction(target), cancellationToken);
                        }

                        // Update the player info.
                        await this.communicator.SendGameUpdate(user.Value.Character, null, null, cancellationToken);

                        // Show the opponent's condition.
                        string? condition = GetPlayerCondition(target);

                        if (!string.IsNullOrWhiteSpace(condition))
                        {
                            await this.communicator.SendToPlayer(user.Value.Character, condition, cancellationToken);
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

            int hitDice = 0;
            int damDice = 0;

            // If it's a spell, use the spell's hit/dam, otherwise, use the hit/dam of the character.
            if (action.ActionType == ActionType.Spell)
            {
                hitDice = action.HitDice;
                damDice = action.DamageDice;
            }
            else
            {
                hitDice = Math.Max(1, actor.HitDice);
                damDice = Math.Max(4, actor.DamageDice);
            }

            // Reduce the damage inversely by level. So if the player is 10, target is 10, damage modifier is normal.
            // If the player is 20, target is 10, damage modifier is doubled.
            // If the player is 10, target is 20, damage modifier is halved.
            double adjust = (actor.Level / target.Level) * action.DamageModifier;

            var damage = 0;
            for (var x = 0; x < hitDice; x++)
            {
                damage += this.random.Next(1, damDice);
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
            int baseExperience = (target.Level * 8) + this.random.Next(1, 199);

            if (actor.Level <= target.Level)
            {
                double levelDiff = target.Level - actor.Level;

                double experienceResult = (double)baseExperience * Math.Max(1, levelDiff);

                var modified = experienceResult * this.GetModifier(actor, target);

                return (int)modified;
            }
            else
            {
                double levelDiff = actor.Level - target.Level;

                // If more than ten levels higher than target, no experience.
                if (levelDiff > 10)
                {
                    return 0;
                }
                else
                {
                    double levelModifier = 1d / (double)(actor.Level - target.Level);

                    double experienceResult = (double)baseExperience * levelModifier;

                    var modified = experienceResult * this.GetModifier(actor, target);

                    return (int)modified;
                }
            }
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
            await this.communicator.SendToRoom(target.Location, target, killer, $"{target.FirstName.FirstCharToUpper()} is DEAD!", cancellationToken);

            killer.Metrics.MobKills += 1;

            var mobKills = killer.Metrics.MobKills;

            switch (mobKills)
            {
                default:
                    break;
                case 10:
                    await this.awardProcessor.GrantAward(3, killer, $"killed {mobKills} creatures", cancellationToken);
                    break;
                case 100:
                    await this.awardProcessor.GrantAward(3, killer, $"killed {mobKills} creatures", cancellationToken);
                    break;
                case 500:
                    await this.awardProcessor.GrantAward(3, killer, $"killed {mobKills} creatures", cancellationToken);
                    break;
                case 1000:
                    await this.awardProcessor.GrantAward(3, killer, $"killed {mobKills} creatures", cancellationToken);
                    break;
            }

            var room = this.communicator.ResolveRoom(killer.Location);

            if (room != null)
            {
                var mobile = (Mobile)target;

                // Let's just do this once.
                if (room.Mobiles.Contains(mobile))
                {
                    room.Mobiles.Remove(mobile);
                    var corpse = this.GenerateCorpse(killer.Location, target);

                    if (killer.CharacterFlags.Contains(CharacterFlags.Autoloot))
                    {
                        await this.actionProcessor.ItemsFromContainer(killer, corpse, cancellationToken);
                    }

                    if (killer.CharacterFlags.Contains(CharacterFlags.Autosac))
                    {
                        if (corpse != null)
                        {
                            killer.DivineFavor += 1;
                            await this.communicator.SendToPlayer(killer, $"You sacrifice {corpse.Name} to your deity for some divine favor.", cancellationToken);
                            await this.communicator.SendToRoom(killer, killer.Location, $"{killer.FirstName.FirstCharToUpper()} sacrifices {corpse.Name} to their deity.", cancellationToken);
                            room.Items.Remove(corpse);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Kills a player.
        /// </summary>
        /// <param name="actor">The player about to die.</param>
        /// <param name="killer">The killer of the player.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task KillPlayer(Character? actor, Character killer, CancellationToken cancellationToken = default)
        {
            if (actor != null)
            {
                StopFighting(actor, killer);

                await this.communicator.SendToPlayer(killer, $"You have KILLED {actor.FirstName}!", cancellationToken);
                await this.communicator.SendToPlayer(actor, $"{killer.FirstName.FirstCharToUpper()} has KILLED you! You are now dead.", cancellationToken);

                await this.communicator.SendToPlayer(actor, $"[NOTIFICATION]|../img/notifications/death.png|{killer.FirstName} has killed you.", cancellationToken);

                await this.communicator.SendToRoom(killer.Location, killer, actor, $"{actor.FirstName.FirstCharToUpper()} is DEAD!");

                await this.communicator.PlaySound(actor, AudioChannel.Actor, Sounds.DEATH, cancellationToken);

                killer.Metrics.PlayerKills += 1;

                var playerKills = killer.Metrics.PlayerKills;

                switch (playerKills)
                {
                    default:
                        break;
                    case 1:
                        await this.awardProcessor.GrantAward(5, killer, $"killed {playerKills} people", cancellationToken);
                        break;
                    case 10:
                        await this.awardProcessor.GrantAward(5, killer, $"killed {playerKills} people", cancellationToken);
                        break;
                    case 50:
                        await this.awardProcessor.GrantAward(5, killer, $"killed {playerKills} people", cancellationToken);
                        break;
                    case 100:
                        await this.awardProcessor.GrantAward(5, killer, $"killed {playerKills} people", cancellationToken);
                        break;
                }

                // Make a ghost and add effects
                actor.CharacterFlags?.AddIfNotExists(CharacterFlags.Ghost);
                actor.AffectedBy.Add(new Effect() { Name = "Ghost", Duration = 6 });
                await this.communicator.SendToPlayer(actor, $"You have been turned into a ghost for a few hours, unless you attack something.", cancellationToken);

                var room = this.communicator.ResolveRoom(killer.Location);

                if (room != null)
                {
                    this.logger.Info($"{killer.FirstName.FirstCharToUpper()} has killed {actor.FirstName} in room {room.RoomId}, area {room.AreaId}!", this.communicator);

                    // Generate the corpse.
                    this.GenerateCorpse(killer.Location, actor);
                }

                // Remove all equipment, currency, and inventory.
                actor.Inventory = new List<Item>();
                actor.Equipment = new List<Item>();
                actor.Currency = 0m;

                // Send the character to their home.
                actor.Location = actor.Home;

                // Increment deaths.
                if (killer.IsNPC)
                {
                    actor.Metrics.MobDeaths += 1;
                }
                else
                {
                    actor.Metrics.PlayerDeaths += 1;
                }

                // Update con loss.
                if (actor.Metrics.TotalDeaths % 4 == 0)
                {
                    actor.Con.Max -= 1;
                    actor.Con.Current = Math.Min(actor.Con.Current, actor.Con.Max);
                    await this.communicator.SendToPlayer(actor, "You feel less healthy.", cancellationToken);
                }

                // Set health to 1.
                actor.Health.Current = 1;

                // Save changes.
                await this.communicator.SaveCharacter(actor);

                // Show player info.
                await this.communicator.SendGameUpdate(actor, null, null, cancellationToken);

                // Show the player their new surroundings.
                await this.communicator.ShowRoomToPlayer(actor, cancellationToken);
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
                    targetMagicPct += target.AffectedBy.Sum(a => a.Magic ?? 0);
                    if (armorSavePct < targetMagicPct)
                    {
                        blocked = true;
                    }

                    break;
                case DamageType.Pierce:
                    var targetPiercePct = target.Equipment.Where(e => e.ItemType == ItemType.Armor).Sum(s => s.Pierce);
                    targetPiercePct += target.AffectedBy.Sum(a => a.Pierce ?? 0);
                    if (armorSavePct < targetPiercePct)
                    {
                        blocked = true;
                    }

                    break;
                case DamageType.Slash:
                    var targetSlashPct = target.Equipment.Where(e => e.ItemType == ItemType.Armor).Sum(s => s.Edged);
                    targetSlashPct += target.AffectedBy.Sum(a => a.Slash ?? 0);
                    if (armorSavePct < targetSlashPct)
                    {
                        blocked = true;
                    }

                    break;
                case DamageType.Blunt:
                    var targetBluntPct = target.Equipment.Where(e => e.ItemType == ItemType.Armor).Sum(s => s.Blunt);
                    targetBluntPct += target.AffectedBy.Sum(a => a.Blunt ?? 0);
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
                            await this.communicator.SendToPlayer(actor, $"{target.FirstName.FirstCharToUpper()} blocked your attack with their armor!", cancellationToken);
                            await this.communicator.SendToPlayer(target, $"You absorbed {actor.FirstName}'s attack with {randomGear.Name}!", cancellationToken);

                            randomGear.Durability.Current -= 1;

                            if (randomGear.Durability.Current <= 0)
                            {
                                // It's destroyed.
                                await this.communicator.SendToPlayer(actor, $"You destroyed {randomGear.Name}!", cancellationToken);
                                await this.communicator.SendToPlayer(target, $"{actor.FirstName.FirstCharToUpper()} destroyed {randomGear.Name}.", cancellationToken);

                                target.Equipment.Remove(randomGear);
                            }
                        }
                    }
                }
                catch (Exception exc)
                {
                    this.logger.Error(exc, this.communicator);
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
        public bool DidSave(Character target, IAction action)
        {
            var saveThrow = this.random.Next(1, 20);

            // Critical failure.
            if (saveThrow == 1)
            {
                return false;
            }

            int saves = 0;

            switch (action.DamageType)
            {
                default:
                    {
                        saves = target.SaveSpell;
                        break;
                    }

                case Core.Types.DamageType.Energy:
                case Core.Types.DamageType.Negative:
                    {
                        saves = target.SaveNegative;
                        break;
                    }

                case Core.Types.DamageType.Afflictive:
                    {
                        saves = target.SaveAfflictive;
                        break;
                    }

                case Core.Types.DamageType.Maledictive:
                    {
                        saves = target.SaveMaledictive;
                        break;
                    }
            }

            return saveThrow < saves;
        }

        /// <summary>
        /// Gets the hit sound effect based on the damage noun.
        /// </summary>
        /// <param name="damageNoun">The damage noun.</param>
        /// <returns>URL to audio.</returns>
        private static string GetSoundEffect(string? damageNoun)
        {
            switch (damageNoun?.ToLower())
            {
                default:
                case "punch":
                    return Sounds.PUNCH;
                case "slash":
                    return Sounds.SLASH;
                case "pound":
                    return Sounds.BLUNT;
                case "pierce":
                    return Sounds.PIERCE;
            }
        }

        /// <summary>
        /// Gets the modifier applied based on the difference of alignments of the target and actor.
        /// </summary>
        /// <param name="actor">The actor.</param>
        /// <param name="target">The target.</param>
        /// <returns>decimal.</returns>
        private double GetModifier(Character actor, Character target)
        {
            switch (actor.Alignment)
            {
                case Alignment.Good:
                    {
                        switch (target.Alignment)
                        {
                            case Alignment.Good:
                                {
                                    return .75d;
                                }

                            case Alignment.Neutral:
                                {
                                    return 1d;
                                }

                            case Alignment.Evil:
                                {
                                    return 2d;
                                }
                        }
                    }

                    break;
                case Alignment.Neutral:
                    {
                        switch (target.Alignment)
                        {
                            case Alignment.Good:
                                {
                                    return 1.25d;
                                }

                            case Alignment.Neutral:
                                {
                                    return 1d;
                                }

                            case Alignment.Evil:
                                {
                                    return 1.25d;
                                }
                        }
                    }

                    break;
                case Alignment.Evil:
                    {
                        switch (target.Alignment)
                        {
                            case Alignment.Good:
                                {
                                    return 2d;
                                }

                            case Alignment.Neutral:
                                {
                                    return 1d;
                                }

                            case Alignment.Evil:
                                {
                                    return .75d;
                                }
                        }
                    }

                    break;
            }

            return 1d;
        }

        /// <summary>
        /// Gets the skill proficiency of basic hand to hand based on the mob and the target.
        /// </summary>
        /// <param name="mobile">The mobile.</param>
        /// <param name="target">The target.</param>
        /// <returns>Skill proficiency.</returns>
        private SkillProficiency GetDefaultMobileProficiency(Character mobile, Character target)
        {
            // TODO: Adjust this.
            var skillProf = new SkillProficiency("hand to hand", 50);
            return skillProf;
        }

        /// <summary>
        /// Generates a corpse of the victim and places it in the room.
        /// </summary>
        /// <param name="location">The room to generate the corpse in.</param>
        /// <param name="victim">The victim to generate the corpse from.</param>
        private Item? GenerateCorpse(KeyValuePair<long, long> location, Character victim)
        {
            try
            {
                // Create a corpse with their stuff in the room.
                var corpse = new Item()
                {
                    ItemType = ItemType.Container,
                    Location = location,
                    Level = victim.Level,
                    Name = $"the corpse of {victim.FirstName}",
                    ShortDescription = $"The corpse of {victim.FirstName} is decomposing here.",
                    LongDescription = $"The corpse of {victim.FirstName} is decomposing here.",
                    WearLocation = new List<WearLocation>() { WearLocation.None },
                    Weight = 200,
                    RotTimer = 2,
                    Contains = new List<IItem>(),
                    IsPlayerCorpse = !victim.IsNPC,
                    IsNPCCorpse = victim.IsNPC,
                };

                // If the victim was a mob, randomize the currency a little bit. If not, just use the full value.
                var currency = (victim.IsNPC && victim.Currency > 0) ? this.random.Next(victim.Currency - 1, victim.Currency + 1) : victim.Currency;

                // If the player had any money, add it to the corpse.
                if (currency > 0)
                {
                    var currencyItems = currency.ToCurrencyItems();
                    corpse.Contains.AddRange(currencyItems);
                }

                // Add any equipment.
                foreach (var eq in victim.Equipment)
                {
                    corpse.Contains.Add(eq.Clone());
                }

                // Add any inventory.
                foreach (var inv in victim.Inventory)
                {
                    corpse.Contains.Add(inv.Clone());
                }

                // Add the corpse to the room.
                var room = this.communicator.ResolveRoom(location);

                if (room != null)
                {
                    room.Items.Add(corpse);
                }

                return corpse;
            }
            catch (Exception exc)
            {
                this.logger.Error(exc, this.communicator);
                return null;
            }
        }
    }
}
