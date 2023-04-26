// <copyright file="CombatProcessor.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Processors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core;
    using Legendary.Core.Contracts;
    using Legendary.Core.Extensions;
    using Legendary.Core.Models;
    using Legendary.Core.Types;
    using Legendary.Data.Contracts;
    using Legendary.Engine;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Extensions;
    using Legendary.Engine.Helpers;
    using Legendary.Engine.Models;
    using Legendary.Engine.Models.Skills;
    using MongoDB.Driver;

    /// <summary>
    /// Handles actions in combat related to skill and spell usage.
    /// </summary>
    public class CombatProcessor
    {
        private readonly IRandom random;
        private readonly ICommunicator communicator;
        private readonly ILogger logger;
        private readonly IWorld world;
        private readonly AwardProcessor awardProcessor;
        private readonly ActionProcessor actionProcessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="CombatProcessor"/> class.
        /// </summary>
        /// <param name="communicator">The communicator.</param>
        /// <param name="world">The world.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="messageProcessor">The message processor.</param>
        /// <param name="dataService">The data service.</param>
        public CombatProcessor(ICommunicator communicator, IWorld world, IEnvironment environment, IRandom random, ILogger logger, IMessageProcessor messageProcessor, IDataService dataService)
        {
            this.random = random;
            this.communicator = communicator;
            this.logger = logger;
            this.world = world;

            this.awardProcessor = new AwardProcessor(communicator, world, logger, random, this);
            this.actionProcessor = new ActionProcessor(communicator, environment, world, logger, random, this, messageProcessor, dataService);
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
        /// Calculates the damage verb messages based on the raw damage.
        /// </summary>
        /// <param name="damage">The damage as a total.</param>
        /// <returns>String.</returns>
        public static string CalculateDamageVerb(int damage)
        {
            var message = damage switch
            {
                <= 0 => "<span class='damage damage_0'>has no real effect on</span>",
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
        /// Absolutely determines if a player is dead.
        /// </summary>
        /// <param name="character">The character or mob.</param>
        /// <returns>True, if dead.</returns>
        public static bool IsDead(Character character)
        {
            if (character.CharacterFlags != null && character.CharacterFlags.Contains(CharacterFlags.Ghost))
            {
                return true;
            }

            if (character.Health.Current <= 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Stops combat between two characters.
        /// </summary>
        /// <param name="victim">The victim.</param>
        /// <param name="killer">The killer.</param>
        /// <returns>True when the fighting stops.</returns>
        public bool StopFighting(Character victim, Character? killer)
        {
            victim.CharacterFlags.RemoveIfExists(CharacterFlags.Fighting);
            victim.Fighting = null;

            if (killer != null)
            {
                killer.CharacterFlags.RemoveIfExists(CharacterFlags.Fighting);
                killer.Fighting = null;

                // See if the dead victim had a group. If so, the opposing group will be fighting the killer still.
                if (GroupHelper.IsInGroup(victim.CharacterId))
                {
                    var groupMembers = GroupHelper.GetAllGroupMembers(victim.CharacterId);

                    if (groupMembers != null)
                    {
                        var remainingMembers = groupMembers.Where(g => g != victim.CharacterId).ToList();

                        if (remainingMembers.Count > 0)
                        {
                            var nextOnDeck = this.random.Next(0, remainingMembers.Count);

                            // Retarget the attacks of the killer and their group.
                            var killerGroup = GroupHelper.GetAllGroupMembers(killer.CharacterId);

                            if (killerGroup != null)
                            {
                                foreach (var attacker in killerGroup)
                                {
                                    var charInGroup = this.communicator.ResolveCharacter(attacker);

                                    if (charInGroup != null)
                                    {
                                        // Retarget the attacks.
                                        charInGroup.Character.CharacterFlags.AddIfNotExists(CharacterFlags.Fighting);
                                        charInGroup.Character.Fighting = remainingMembers[nextOnDeck];
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Everyone is dead. End all fighting.
                            var killerGroup = GroupHelper.GetAllGroupMembers(killer.CharacterId);

                            if (killerGroup != null)
                            {
                                foreach (var attacker in killerGroup)
                                {
                                    var charInGroup = this.communicator.ResolveCharacter(attacker);

                                    if (charInGroup != null)
                                    {
                                        charInGroup.Character.CharacterFlags.RemoveIfExists(CharacterFlags.Fighting);
                                        charInGroup.Character.Fighting = null;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return true;
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

            // If they fall below wimpy, haul ass.
            if (target.Health.Current <= target.Wimpy)
            {
                if (!target.IsNPC)
                {
                    if (Communicator.Users != null)
                    {
                        var userData = Communicator.Users.FirstOrDefault(u => u.Value.Character.CharacterId == target.CharacterId);
                        var commandArgs = new CommandArgs("flee", null, null, 0);
                        this.actionProcessor.DoAction(userData.Value, commandArgs).Wait();
                    }
                }
                else
                {
                    // TODO: Implement me.
                }
            }

            return false;
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
                if (!target.IsNPC)
                {
                    await this.communicator.SendToPlayer(target, $"[NOTIFICATION]|../img/notifications/attack.png|{actor.FirstName} has attacked you!", cancellationToken);
                }

                // Start the fight between the two characters.
                actor.CharacterFlags.AddIfNotExists(CharacterFlags.Fighting);
                actor.Fighting = target.CharacterId;
                target.CharacterFlags.AddIfNotExists(CharacterFlags.Fighting);
                target.Fighting = actor.CharacterId;

                // If the attacker is in a group, engage all other group members.
                if (GroupHelper.IsInGroup(actor.CharacterId))
                {
                    var groupMembers = GroupHelper.GetAllGroupMembers(actor.CharacterId);

                    if (groupMembers != null)
                    {
                        // Get everyone except the one who is already fighting.
                        var otherMembers = groupMembers.Where(m => m != actor.CharacterId);

                        foreach (var other in otherMembers)
                        {
                            var member = this.communicator.ResolveCharacter(other);

                            if (member != null)
                            {
                                // If they're a ghost, not anymore.
                                member.Character.CharacterFlags.RemoveIfExists(CharacterFlags.Ghost);

                                // Set them to be fighting the target.
                                member.Character.CharacterFlags.AddIfNotExists(CharacterFlags.Fighting);
                                member.Character.Fighting = target.CharacterId;
                            }
                        }
                    }
                }

                // If the target is a PC, and they are in a group, engage those members.
                if (!target.IsNPC && GroupHelper.IsInGroup(target.CharacterId))
                {
                    var groupMembers = GroupHelper.GetAllGroupMembers(target.CharacterId);

                    if (groupMembers != null)
                    {
                        // Get everyone except the one who is already fighting.
                        var otherMembers = groupMembers.Where(m => m != target.CharacterId);

                        foreach (var other in otherMembers)
                        {
                            var member = this.communicator.ResolveCharacter(other);

                            if (member != null)
                            {
                                // If they're a ghost, not anymore.
                                member.Character.CharacterFlags.RemoveIfExists(CharacterFlags.Ghost);

                                // Set them to be fighting the target.
                                member.Character.CharacterFlags.AddIfNotExists(CharacterFlags.Fighting);
                                member.Character.Fighting = actor.CharacterId;
                            }
                        }
                    }
                }
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
            var wielded = actor.Equipment.FirstOrDefault(e => e.Key == WearLocation.Wielded);

            if (wielded.Value != null)
            {
                switch (wielded.Value.WeaponType)
                {
                    default:
                        {
                            return new HandToHand(this.communicator, this.random, this.world, this.logger, this);
                        }

                    case WeaponType.Axe:
                    case WeaponType.Sword:
                        {
                            return new EdgedWeapons(this.communicator, this.random, this.world, this.logger, this);
                        }

                    case WeaponType.Spear:
                    case WeaponType.Dagger:
                        {
                            return new PiercingWeapons(this.communicator, this.random, this.world, this.logger, this);
                        }

                    case WeaponType.Mace:
                    case WeaponType.Club:
                        {
                            return new BluntWeapons(this.communicator, this.random, this.world, this.logger, this);
                        }

                    case WeaponType.Flail:
                        {
                            return new Flails(this.communicator, this.random, this.world, this.logger, this);
                        }

                    case WeaponType.Whip:
                        {
                            return new Whips(this.communicator, this.random, this.world, this.logger, this);
                        }

                    case WeaponType.Polearm:
                        {
                            return new Polearms(this.communicator, this.random, this.world, this.logger, this);
                        }

                    case WeaponType.Staff:
                        {
                            return new Staffs(this.communicator, this.random, this.world, this.logger, this);
                        }

                    case WeaponType.Exotic:
                        {
                            return new Exotics(this.communicator, this.random, this.world, this.logger, this);
                        }

                    case WeaponType.TwoHanded:
                        {
                            return new TwoHandedWeapons(this.communicator, this.random, this.world, this.logger, this);
                        }
                }
            }

            return new HandToHand(this.communicator, this.random, this.world, this.logger, this);
        }

        /// <summary>
        /// Does damage from the actor to the target.
        /// </summary>
        /// <param name="actor">The actor.</param>
        /// <param name="target">The target.</param>
        /// <param name="action">The action.</param>
        /// <param name="isCritical">Whether or not this was a critical hit.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if the target was killed.</returns>
        public async Task<bool> DoDamage(Character actor, Character target, IAction? action, bool isCritical = false, CancellationToken cancellationToken = default)
        {
            if (!PlayerHelper.IsInPK(actor, target))
            {
                // Safety valve. Can't do damage to any player not in PK range.
                await this.communicator.SendToPlayer(actor, $"{target.FirstName} is protected from you by the Gods.", cancellationToken);
                return false;
            }
            else
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
                            // This should be a hit, so check ability to dodge/parry/evade.
                            blocked = await this.CheckDefensiveSkills(actor, target, combatAction, cancellationToken);

                            // This was a hit, check armor block.
                            if (!blocked)
                            {
                                blocked = await this.CheckArmorBlock(actor, target, combatAction, cancellationToken);
                            }

                            await this.communicator.PlaySound(actor, AudioChannel.Martial, GetSoundEffect(combatAction.DamageNoun), cancellationToken);
                            await this.communicator.PlaySound(target, AudioChannel.Martial, GetSoundEffect(combatAction.DamageNoun), cancellationToken);
                            await combatAction.PreAction(actor, target, null, cancellationToken);
                            await combatAction.Act(actor, target, null, cancellationToken);
                        }
                        else
                        {
                            // This was a total martial combat miss. Show the miss and exit.
                            await this.communicator.SendToPlayer(actor, $"Your {combatAction.DamageNoun} misses {target.FirstName}.", cancellationToken);
                            await this.communicator.SendToPlayer(target, $"{actor.FirstName.FirstCharToUpper()}'s {combatAction.DamageNoun} misses you.", cancellationToken);
                            await this.communicator.SendToRoom(actor.Location, actor, target, $"{actor.FirstName.FirstCharToUpper()}'s {combatAction.DamageNoun} misses {target.FirstName}.", cancellationToken);
                        }

                        // Run post action to check if the skill improved.
                        await combatAction.PostAction(actor, target, null, cancellationToken);
                    }
                    else
                    {
                        // This was a total miss because the character is not proficient. Don't allow an increase.
                        await this.communicator.SendToPlayer(actor, $"Your {combatAction.DamageNoun} misses {target.FirstName}.", cancellationToken);
                        await this.communicator.SendToPlayer(target, $"{actor.FirstName.FirstCharToUpper()}'s {combatAction.DamageNoun} misses you.", cancellationToken);
                        await this.communicator.SendToRoom(actor.Location, actor, target, $"{actor.FirstName.FirstCharToUpper()}'s {combatAction.DamageNoun} misses {target.FirstName}.", cancellationToken);
                    }
                }
                else
                {
                    // This is a spell that automatically hits, but certain spells can be evaded.
                    blocked = await this.CheckDefensiveSkills(actor, target, combatAction, cancellationToken);

                    // Check if the armor takes the hit.
                    if (!blocked)
                    {
                        blocked = await this.CheckArmorBlock(actor, target, combatAction, cancellationToken);
                    }
                }

                if (!blocked)
                {
                    // Calculate damage FROM character TO target.
                    var damage = this.CalculateDamage(actor, target, combatAction);

                    // Double damage for critical strikes if they have the skill, otherwise, max damage.
                    if (isCritical)
                    {
                        var criticalStrikes = actor.GetSkillProficiency("critical strikes");

                        if (criticalStrikes != null)
                        {
                            var result = this.random.Next(1, 101);

                            if (result < criticalStrikes.Proficiency && result != 1)
                            {
                                damage *= 3;
                                await this.communicator.SendToPlayer(actor, $"You land a CRITICAL STRIKE on {target.FirstName}!", cancellationToken);
                                await this.communicator.SendToPlayer(target, $"{actor.FirstName.FirstCharToUpper()} lands a CRITICAL STRIKE on you!", cancellationToken);
                                await this.communicator.SendToRoom(actor.Location, actor, target, $"{actor.FirstName.FirstCharToUpper()} lands a CRITICAL STRIKE on {target.FirstName}!", cancellationToken);
                            }
                            else
                            {
                                await this.communicator.SendToPlayer(actor, $"You nearly land a critical strike on {target.FirstName}, but miss.", cancellationToken);
                            }
                        }
                        else
                        {
                            await this.communicator.SendToPlayer(actor, $"You critically hit {target.FirstName}!", cancellationToken);
                            await this.communicator.SendToPlayer(target, $"{actor.FirstName.FirstCharToUpper()} critically hits!", cancellationToken);
                            await this.communicator.SendToRoom(actor.Location, actor, target, $"{actor.FirstName.FirstCharToUpper()} criticially hits {target.FirstName}!", cancellationToken);
                            damage *= 2;
                        }
                    }

                    // Calculate the damage verb.
                    var damFromVerb = CalculateDamageVerb(damage);

                    await this.communicator.SendToPlayer(actor, $"Your {combatAction.DamageNoun} {damFromVerb} {target.FirstName}!", cancellationToken);
                    await this.communicator.SendToPlayer(target, $"{actor.FirstName.FirstCharToUpper()}'s {combatAction.DamageNoun} {damFromVerb} you!", cancellationToken);
                    await this.communicator.SendToRoom(actor.Location, actor, target, $"{actor.FirstName.FirstCharToUpper()}'s {combatAction.DamageNoun} {damFromVerb} {target.FirstName}!", cancellationToken);

                    bool isDead = this.ApplyDamage(target, damage);

                    if (isDead)
                    {
                        // Target is dead.
                        while (!this.StopFighting(target, actor))
                        {
                            this.logger.Debug($"Ending combat between {target.FirstName} and {actor.FirstName}...", this.communicator);
                        }

                        if (actor.IsNPC && target.IsNPC)
                        {
                            // Mob killed mob.
                            await this.KillMobile(target, actor, cancellationToken);
                        }
                        else if (target.IsNPC)
                        {
                            // Player killed mobile.
                            await this.KillMobile(target, actor, cancellationToken);
                        }
                        else
                        {
                            // Player killed player.
                            await this.KillPlayer(target, actor, cancellationToken);
                        }

                        if (GroupHelper.IsInGroup(actor.CharacterId))
                        {
                            // Calculate the group's experience.
                            await this.ApplyGroupExperience(actor, target, cancellationToken);
                        }
                        else
                        {
                            // Add the experience to the player.
                            await this.ApplyPlayerExperience(actor, target, cancellationToken);
                        }

                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Executes all available martial attacks on a target.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="target">The victim.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task ExecuteAttacks(Character character, Character target, CancellationToken cancellationToken = default)
        {
            bool dead = false;

            // First attack
            var primaryAction = this.GetCombatAction(character);
            SkillProficiency? primaryWeapon = character.GetSkillProficiency(primaryAction.Name);

            try
            {
                if (character.IsNPC || (primaryWeapon != null && primaryWeapon.Proficiency > 1))
                {
                    var result = this.random.Next(1, 101);
                    dead = await this.DoDamage(character, target, primaryAction, result >= 100, cancellationToken);
                }
                else
                {
                    await this.communicator.SendToPlayer(character, $"Your {primaryAction.DamageNoun} misses {target.FirstName}.", cancellationToken);
                    await this.communicator.SendToRoom(character.Location, character, target, $"{character.FirstName.FirstCharToUpper()}'s {primaryAction.DamageNoun} misses {target.FirstName}.", cancellationToken);

                    if (this.random.Next(1, 100) < 20)
                    {
                        await this.communicator.SendToPlayer(character, $"You're not likely to do much to {target.FirstName} without a weapon or training.", cancellationToken);
                    }
                }
            }
            catch (Exception exc)
            {
                this.logger.Error($"ExecuteAttacks->First: {exc}", this.communicator);
                throw;
            }

            // Second attack
            if (!dead)
            {
                try
                {
                    SkillProficiency? secondAttack = character.GetSkillProficiency("second attack");

                    if (secondAttack != null && secondAttack.Proficiency > 1)
                    {
                        var result = this.random.Next(1, 101);

                        if (result < secondAttack.Proficiency && result != 1)
                        {
                            dead = await this.DoDamage(character, target, this.GetCombatAction(character), result >= 100, cancellationToken);
                        }

                        SecondAttack skill = new (this.communicator, this.random, this.world, this.logger, this);
                        await skill.CheckImprove(character, cancellationToken);
                    }
                }
                catch (Exception exc)
                {
                    this.logger.Error($"ExecuteAttacks->Second: {exc}", this.communicator);
                    throw;
                }
            }

            // Third attack
            if (!dead)
            {
                try
                {
                    SkillProficiency? thirdAttack = character.GetSkillProficiency("third attack");

                    if (thirdAttack != null && thirdAttack.Proficiency > 1)
                    {
                        var result = this.random.Next(1, 101);
                        if (result < thirdAttack.Proficiency && result != 1)
                        {
                            dead = await this.DoDamage(character, target, this.GetCombatAction(character), result >= 100, cancellationToken);
                        }

                        ThirdAttack skill = new (this.communicator, this.random, this.world, this.logger, this);
                        await skill.CheckImprove(character, cancellationToken);
                    }
                }
                catch (Exception exc)
                {
                    this.logger.Error($"ExecuteAttacks->Third: {exc}", this.communicator);
                    throw;
                }
            }

            // Fourth attack
            if (!dead)
            {
                try
                {
                    SkillProficiency? fourthAttack = character.GetSkillProficiency("fourth attack");

                    if (fourthAttack != null && fourthAttack.Proficiency > 1)
                    {
                        var result = this.random.Next(1, 101);
                        if (result < fourthAttack.Proficiency && result != 1)
                        {
                            dead = await this.DoDamage(character, target, this.GetCombatAction(character), result >= 100, cancellationToken);
                        }

                        FourthAttack skill = new (this.communicator, this.random, this.world, this.logger, this);
                        await skill.CheckImprove(character, cancellationToken);
                    }
                }
                catch (Exception exc)
                {
                    this.logger.Error($"ExecuteAttacks->Fourth: {exc}", this.communicator);
                    throw;
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
                    var target = this.communicator.ResolveFightingCharacter(character);

                    if (IsDead(character))
                    {
                        return;
                    }

                    if (target != null && IsDead(target))
                    {
                        return;
                    }

                    if (character != null && character.CharacterFlags.Contains(CharacterFlags.Fighting) && target != null)
                    {
                        try
                        {
                            await this.ExecuteAttacks(character, target, cancellationToken);

                            // If the target is an NPC, do damage from it to the player (unless it's dead). Otherwise, for PvP, the loop will just pick up the next fighter.
                            if (target.CharacterFlags.Contains(CharacterFlags.Fighting) && target.IsNPC)
                            {
                                // NPC should only engage the player who is fighting it (e.g. the tank, not the entire group).
                                if (target.Fighting != null && target.Fighting == character.CharacterId)
                                {
                                    await this.ExecuteAttacks(target, character, cancellationToken);
                                }
                                else if (target.Fighting == null)
                                {
                                    // Need to pick a target, because a target is currently kicking the NPC's ass.
                                    target.Fighting = character.CharacterId;
                                }
                                else
                                {
                                    continue;
                                }
                            }

                            // Show the opponent's condition.
                            string? condition = GetPlayerCondition(target);

                            if (!string.IsNullOrWhiteSpace(condition) && !IsDead(user.Value.Character) && !IsDead(target))
                            {
                                await this.communicator.SendToPlayer(user.Value.Character, condition, cancellationToken);
                            }
                        }
                        catch (Exception exc)
                        {
                            this.logger.Error($"HandleCombatTick: {exc}", this.communicator);
                        }
                    }

                    // Update the player info.
                    await this.communicator.SendGameUpdate(user.Value.Character, null, null, cancellationToken);
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

            int hitDice;
            int damDice;

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
            double adjust = actor.Level / (double)target.Level * action.DamageModifier;

            var damage = 0;
            for (var x = 0; x < hitDice; x++)
            {
                damage += this.random.Next(1, damDice + 1);
            }

            if (target.IsAffectedBy(EffectName.SANCTUARY))
            {
                // Reduce by half.
                return (int)((damage + adjust) / 2);
            }
            else if (this.DidSave(target, action))
            {
                // Save for half damage (spells only).
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
            double experience;
            int actorLevel = actor.Level;
            int targetLevel = target.Level;
            int maxExperience = 1500 + this.random.Next(1, 100);
            int baseIncrementalExperience = 200 + this.random.Next(1, 20);

            int levelDifference = Math.Max(0, targetLevel - actorLevel);

            // Get the modifier for the alignment difference.
            double modifier = GetModifier(actor, target);

            // If the actor's level is lower than or equal to the target's level or if the actor's level is more than 10 levels above the target,
            // return the minimum experience.
            if (levelDifference <= 0 || levelDifference > 10)
            {
                if (!target.IsNPC)
                {
                    // We'll give the player a little experience for a PK.
                    return (int)(this.random.Next(1, 100) * modifier);
                }
                else
                {
                    // Mob was same level, no experience.
                    return 0;
                }
            }

            // Calculate the base experience gained based on the level difference.
            int baseExperience = (int)Math.Round(maxExperience * Math.Pow(0.5, levelDifference));

            // Calculate the incremental experience gained for every level higher the target is than the actor.
            double incrementalExperienceMultiplier = 1 + ((levelDifference - 1) * 0.1);
            int incrementalExperience = (int)Math.Round(baseIncrementalExperience * incrementalExperienceMultiplier);
            int incrementalExperienceGained = 0;
            for (int i = 0; i < levelDifference; i++)
            {
                incrementalExperienceGained += incrementalExperience;
                incrementalExperience = (int)Math.Round(incrementalExperience * 1.1);
            }

            // Calculate the total experience gained.
            experience = baseExperience + incrementalExperienceGained;

            // Ensure that the experience gained is within the range of minExperience and maxExperience.
            experience = Math.Max(1, Math.Min(maxExperience, experience));

            // Apply the modifier.
            experience *= modifier;

            return (int)Math.Round(experience);
        }

        /// <summary>
        /// Apply the experience the player gets from killing the target.
        /// </summary>
        /// <param name="actor">The actor.</param>
        /// <param name="target">The target.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task ApplyPlayerExperience(Character actor, Character target, CancellationToken cancellationToken = default)
        {
            var experience = this.CalculateExperience(actor, target);

            await this.communicator.SendToPlayer(actor, $"You gain {experience} experience points.", cancellationToken);
            actor.Experience += experience;

            // See if the player advanced a level.
            if (experience > 0)
            {
                await this.communicator.CheckLevelAdvance(actor, cancellationToken);
            }
        }

        /// <summary>
        /// Apply the experience the group gets from killing the target.
        /// </summary>
        /// <param name="actor">The actor.</param>
        /// <param name="target">The target.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task ApplyGroupExperience(Character actor, Character target, CancellationToken cancellationToken = default)
        {
            if (actor.GroupId.HasValue)
            {
                var group = GroupHelper.GetAllGroupMembers(actor.CharacterId);

                if (group != null)
                {
                    // If there are 2 in the group, reduce by .85.
                    // Favor 3 in a group, so give them a bonus.
                    // Reduce dramatically for 4 or more in a group
                    double modifier = (group.Count == 2) ? 0.85 : (group.Count == 3) ? 1.5 : (group.Count >= 4) ? 0.75 - ((group.Count - 4) * 0.2) : 0;

                    foreach (var member in group)
                    {
                        var player = this.communicator.ResolveCharacter(member);

                        if (player != null)
                        {
                            int experience = (int)(this.CalculateExperience(player.Character, target) * modifier);

                            await this.communicator.SendToPlayer(player.Character, $"You gain {experience} experience points.", cancellationToken);
                            player.Character.Experience += experience;

                            // See if the player advanced a level.
                            if (experience > 0)
                            {
                                await this.communicator.CheckLevelAdvance(player.Character, cancellationToken);
                            }
                        }
                    }
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
            while (!this.StopFighting(target, killer))
            {
                this.logger.Debug($"Ending combat between {target.FirstName} and {killer.FirstName}...", this.communicator);
            }

            await this.communicator.SendToPlayer(killer, $"You have KILLED {target.FirstName}!", cancellationToken);
            await this.communicator.SendToRoom(target.Location, target, killer, $"{target.FirstName.FirstCharToUpper()} is DEAD!", cancellationToken);

            // See if they get an award for killing it.
            await this.awardProcessor.CheckSlayerAward((Mobile)target, killer, cancellationToken);

            killer.Metrics.MobKills += 1;

            var mobKills = killer.Metrics.MobKills;

            switch (mobKills)
            {
                default:
                    break;
                case 10:
                    await this.awardProcessor.GrantAward((int)AwardType.CombatProcessor, killer, $"killed {mobKills} creatures", cancellationToken);
                    break;
                case 100:
                    await this.awardProcessor.GrantAward((int)AwardType.CombatProcessor, killer, $"killed {mobKills} creatures", cancellationToken);
                    break;
                case 500:
                    await this.awardProcessor.GrantAward((int)AwardType.CombatProcessor, killer, $"killed {mobKills} creatures", cancellationToken);
                    break;
                case 1000:
                    await this.awardProcessor.GrantAward((int)AwardType.CombatProcessor, killer, $"killed {mobKills} creatures", cancellationToken);
                    break;
                case 2000:
                    await this.awardProcessor.GrantAward((int)AwardType.CombatProcessor, killer, $"killed {mobKills} creatures", cancellationToken);
                    break;
                case 5000:
                    await this.awardProcessor.GrantAward((int)AwardType.CombatProcessor, killer, $"killed {mobKills} creatures", cancellationToken);
                    break;
                case 10000:
                    await this.awardProcessor.GrantAward((int)AwardType.CombatProcessor, killer, $"killed {mobKills} creatures", cancellationToken);
                    break;
            }

            var room = this.communicator.ResolveRoom(killer.Location);

            if (room != null)
            {
                var mobile = room.Mobiles.FirstOrDefault(m => m.CharacterId == target.CharacterId);

                if (mobile != null)
                {
                    if (room.Mobiles.Contains(mobile))
                    {
                        room.Mobiles.Remove(mobile);
                    }

                    var corpse = this.GenerateCorpse(killer.Location, target, killer.Level);

                    if (killer.CharacterFlags.Contains(CharacterFlags.Autoloot))
                    {
                        await this.actionProcessor.ItemsFromContainer(killer, corpse, cancellationToken);
                    }

                    if (killer.CharacterFlags.Contains(CharacterFlags.Autosac))
                    {
                        if (corpse != null && corpse.IsPlayerCorpse)
                        {
                            await this.communicator.SendToPlayer(killer, $"You can't sacrifice {corpse.Name} to {killer.Deity}.", cancellationToken);
                        }
                        else if (corpse != null && corpse.IsNPCCorpse)
                        {
                            this.actionProcessor.ItemsFromCorpseToRoom(killer, corpse);

                            killer.DivineFavor += 1;
                            await this.communicator.SendToPlayer(killer, $"You sacrifice {corpse.Name} to {killer.Deity} for some divine favor.", cancellationToken);
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
                while (!this.StopFighting(actor, killer))
                {
                    this.logger.Debug($"Ending combat between {actor.FirstName} and {killer.FirstName}...", this.communicator);
                }

                await this.communicator.SendToPlayer(killer, $"You have KILLED {actor.FirstName}!", cancellationToken);
                await this.communicator.SendToPlayer(actor, $"{killer.FirstName.FirstCharToUpper()} has KILLED you! You are now dead.", cancellationToken);

                await this.communicator.SendToPlayer(actor, $"[NOTIFICATION]|../img/notifications/death.png|{killer.FirstName} has killed you.", cancellationToken);

                await this.communicator.SendToRoom(killer.Location, killer, actor, $"{actor.FirstName.FirstCharToUpper()} is DEAD!", cancellationToken);

                await this.communicator.PlaySound(actor, AudioChannel.Actor, Sounds.DEATH, cancellationToken);

                killer.Metrics.PlayerKills += 1;

                var playerKills = killer.Metrics.PlayerKills;

                switch (playerKills)
                {
                    default:
                        break;
                    case 1:
                        await this.awardProcessor.GrantAward((int)AwardType.Hunter, killer, $"killed {playerKills} people", cancellationToken);
                        break;
                    case 5:
                        await this.awardProcessor.GrantAward((int)AwardType.Hunter, killer, $"killed {playerKills} people", cancellationToken);
                        break;
                    case 10:
                        await this.awardProcessor.GrantAward((int)AwardType.Hunter, killer, $"killed {playerKills} people", cancellationToken);
                        break;
                    case 25:
                        await this.awardProcessor.GrantAward((int)AwardType.Hunter, killer, $"killed {playerKills} people", cancellationToken);
                        break;
                    case 50:
                        await this.awardProcessor.GrantAward((int)AwardType.Hunter, killer, $"killed {playerKills} people", cancellationToken);
                        break;
                    case 100:
                        await this.awardProcessor.GrantAward((int)AwardType.Hunter, killer, $"killed {playerKills} people", cancellationToken);
                        break;
                    case 250:
                        await this.awardProcessor.GrantAward((int)AwardType.Hunter, killer, $"killed {playerKills} people", cancellationToken);
                        break;
                    case 500:
                        await this.awardProcessor.GrantAward((int)AwardType.Hunter, killer, $"killed {playerKills} people", cancellationToken);
                        break;
                    case 1000:
                        await this.awardProcessor.GrantAward((int)AwardType.Hunter, killer, $"killed {playerKills} people", cancellationToken);
                        break;
                    case 2000:
                        await this.awardProcessor.GrantAward((int)AwardType.Hunter, killer, $"killed {playerKills} people", cancellationToken);
                        break;
                    case 5000:
                        await this.awardProcessor.GrantAward(5, killer, $"killed {playerKills} people", cancellationToken);
                        break;
                }

                // Make a ghost and add effects
                actor.CharacterFlags?.AddIfNotExists(CharacterFlags.Ghost);
                actor.AffectedBy.Add(new Effect() { Name = "Ghost", Duration = 6 });
                await this.communicator.SendToPlayer(actor, $"You have been turned into a ghost for a few hours, unless you attack something.", cancellationToken);

                var room = this.communicator.ResolveRoom(killer.Location);
                Item? corpse = null;

                if (room != null)
                {
                    this.logger.Info($"{killer.FirstName.FirstCharToUpper()} has killed {actor.FirstName} in room {room.RoomId}, area {room.AreaId}!", this.communicator);

                    // Generate the corpse.
                    corpse = this.GenerateCorpse(killer.Location, actor, killer.Level);
                }

                // Remove all equipment, currency, and inventory.
                actor.Inventory = new List<Item>();
                actor.Equipment = new Dictionary<WearLocation, Item>();
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

                if (killer.CharacterFlags.Contains(CharacterFlags.Autoloot))
                {
                    await this.actionProcessor.ItemsFromContainer(killer, corpse, cancellationToken);
                }

                if (killer.CharacterFlags.Contains(CharacterFlags.Autosac))
                {
                    if (corpse != null && corpse.IsPlayerCorpse)
                    {
                        await this.communicator.SendToPlayer(killer, $"You can't sacrifice {corpse.Name} to {killer.Deity}.", cancellationToken);
                    }
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
        /// Checks all possible defensive skills to see if a player was able to get out of the way of a hit.
        /// </summary>
        /// <param name="actor">The attacker.</param>
        /// <param name="target">The victim.</param>
        /// <param name="action">The action being performed.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if blocked.</returns>
        public async Task<bool> CheckDefensiveSkills(Character actor, Character target, IAction action, CancellationToken cancellationToken)
        {
            SkillProficiency? dodge = target.GetSkillProficiency("dodge");
            SkillProficiency? parry = target.GetSkillProficiency("parry");
            SkillProficiency? evasive = target.GetSkillProficiency("evasive");

            // If blinded, severely reduce the effectiveness of defensive skills.
            if (target.IsAffectedBy(EffectName.BLINDNESS) || target.IsAffectedBy(EffectName.DIRTKICKING))
            {
                if (dodge != null)
                {
                    dodge.Proficiency = this.random.Next(1, 10);
                }

                if (parry != null)
                {
                    parry.Proficiency = this.random.Next(1, 10);
                }

                if (evasive != null)
                {
                    evasive.Proficiency = this.random.Next(1, 10);
                }
            }

            switch (action.DamageType)
            {
                default:
                    break;
                case DamageType.Slash:
                case DamageType.Pierce:
                case DamageType.Blunt:
                    if (dodge != null && dodge.Proficiency > 1)
                    {
                        var dodged = false;
                        var dodgeResult = this.random.Next(1, 101);
                        if (dodgeResult < dodge.Proficiency)
                        {
                            await this.communicator.SendToPlayer(actor, $"{target.FirstName.FirstCharToUpper()} dodges your attack!", cancellationToken);
                            await this.communicator.SendToPlayer(target, $"You dodge {actor.FirstName}'s attack!", cancellationToken);
                            await this.communicator.SendToRoom(actor.Location, actor, target, $"{target.FirstName.FirstCharToUpper()} dodges {actor.FirstName}'s attack!", cancellationToken);
                            dodged = true;
                        }

                        Dodge skill = new (this.communicator, this.random, this.world, this.logger, this);
                        await skill.CheckImprove(target, cancellationToken);

                        return dodged;
                    }

                    // Must be wielding a weapon in order to parry.
                    if (parry != null && parry.Proficiency > 1 && target.Equipment.Any(e => e.Key == WearLocation.Wielded))
                    {
                        var parried = false;
                        var parryResult = this.random.Next(1, 101);
                        if (parryResult < parry.Proficiency)
                        {
                            await this.communicator.SendToPlayer(actor, $"{target.FirstName.FirstCharToUpper()} parries your attack!", cancellationToken);
                            await this.communicator.SendToPlayer(target, $"You parry {actor.FirstName}'s attack!", cancellationToken);
                            await this.communicator.SendToRoom(actor.Location, actor, target, $"{target.FirstName.FirstCharToUpper()} parries {actor.FirstName}'s attack!", cancellationToken);
                            parried = true;
                        }

                        Parry skill = new (this.communicator, this.random, this.world, this.logger, this);
                        await skill.CheckImprove(target, cancellationToken);

                        return parried;
                    }

                    if (evasive != null && evasive.Proficiency > 1)
                    {
                        bool evaded = false;
                        var evadeResult = this.random.Next(1, 101);
                        if (evadeResult < evasive.Proficiency)
                        {
                            await this.communicator.SendToPlayer(actor, $"{target.FirstName.FirstCharToUpper()} cleverly evades your attack!", cancellationToken);
                            await this.communicator.SendToPlayer(target, $"You cleverly evade {actor.FirstName}'s attack!", cancellationToken);
                            evaded = true;
                        }

                        EvasiveManeuvers skill = new (this.communicator, this.random, this.world, this.logger, this);
                        await skill.CheckImprove(target, cancellationToken);

                        return evaded;
                    }

                    break;
                case DamageType.Lightning:
                case DamageType.Energy:
                    if (evasive != null && evasive.Proficiency > 1)
                    {
                        bool evaded = false;
                        var evadeResult = this.random.Next(1, 101);
                        if (evadeResult < evasive.Proficiency)
                        {
                            await this.communicator.SendToPlayer(actor, $"{target.FirstName.FirstCharToUpper()} deftly evades your attack!", cancellationToken);
                            await this.communicator.SendToPlayer(target, $"You deftly evade {actor.FirstName}'s attack!", cancellationToken);
                            await this.communicator.SendToRoom(actor.Location, actor, target, $"{target.FirstName.FirstCharToUpper()} deftly evades {actor.FirstName}'s attack!", cancellationToken);
                            evaded = true;
                        }

                        EvasiveManeuvers skill = new (this.communicator, this.random, this.world, this.logger, this);
                        await skill.CheckImprove(target, cancellationToken);

                        return evaded;
                    }

                    break;
            }

            return false;
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
            var armorSavePct = this.random.Next(1, 101);

            switch (action.DamageType)
            {
                default:
                    var targetMagicPct = target.Equipment.Where(e => e.Value.ItemType == ItemType.Armor).Sum(s => s.Value.Magic);
                    targetMagicPct += target.AffectedBy.Sum(a => a.Magic ?? 0);
                    if (armorSavePct < targetMagicPct)
                    {
                        blocked = true;
                    }

                    break;
                case DamageType.Pierce:
                    var targetPiercePct = target.Equipment.Where(e => e.Value.ItemType == ItemType.Armor).Sum(s => s.Value.Pierce);
                    targetPiercePct += target.AffectedBy.Sum(a => a.Pierce ?? 0);
                    if (armorSavePct < targetPiercePct)
                    {
                        blocked = true;
                    }

                    break;
                case DamageType.Slash:
                    var targetSlashPct = target.Equipment.Where(e => e.Value.ItemType == ItemType.Armor).Sum(s => s.Value.Edged);
                    targetSlashPct += target.AffectedBy.Sum(a => a.Slash ?? 0);
                    if (armorSavePct < targetSlashPct)
                    {
                        blocked = true;
                    }

                    break;
                case DamageType.Blunt:
                    var targetBluntPct = target.Equipment.Where(e => e.Value.ItemType == ItemType.Armor).Sum(s => s.Value.Blunt);
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
                    var allArmor = target.Equipment.Where(e => e.Value.ItemType == ItemType.Armor && e.Value.Durability.Current > 0).ToList();

                    if (allArmor.Count > 0)
                    {
                        var armorIndex = this.random.Next(0, allArmor.Count);
                        var randomGear = allArmor[armorIndex];

                        if (randomGear.Value != null)
                        {
                            await this.communicator.SendToPlayer(actor, $"{target.FirstName.FirstCharToUpper()} blocks your attack with their armor!", cancellationToken);
                            await this.communicator.SendToPlayer(target, $"You absorb {actor.FirstName}'s attack with {randomGear.Value.Name}!", cancellationToken);

                            randomGear.Value.Durability.Current -= 1;

                            if (randomGear.Value.Durability.Current <= 0)
                            {
                                // It's destroyed.
                                await this.communicator.SendToPlayer(actor, $"You destroy {randomGear.Value.Name}!", cancellationToken);
                                await this.communicator.SendToPlayer(target, $"{actor.FirstName.FirstCharToUpper()} destroys {randomGear.Value.Name}!", cancellationToken);

                                target.Equipment.Remove(randomGear.Key);
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
            // Saves only work against spells.
            if (action.ActionType == ActionType.Skill)
            {
                return false;
            }

            var saveThrow = this.random.Next(1, 101);

            // Critical failure.
            if (saveThrow == 1)
            {
                return false;
            }

            int saves;

            switch (action.DamageType)
            {
                default:
                    {
                        saves = target.SaveSpell;
                        break;
                    }

                case DamageType.Energy:
                case DamageType.Negative:
                    {
                        saves = target.SaveNegative;
                        break;
                    }

                case DamageType.Afflictive:
                    {
                        saves = target.SaveAfflictive;
                        break;
                    }

                case DamageType.Maledictive:
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
            return damageNoun?.ToLower() switch
            {
                "slash" => Sounds.SLASH,
                "pound" => Sounds.BLUNT,
                "pierce" => Sounds.PIERCE,
                _ => Sounds.PUNCH,
            };
        }

        /// <summary>
        /// Gets the modifier applied based on the difference of alignments of the target and actor.
        /// </summary>
        /// <param name="actor">The actor.</param>
        /// <param name="target">The target.</param>
        /// <returns>decimal.</returns>
        private static double GetModifier(Character actor, Character target)
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
        /// Generates a corpse of the victim and places it in the room.
        /// </summary>
        /// <param name="location">The room to generate the corpse in.</param>
        /// <param name="victim">The victim to generate the corpse from.</param>
        /// <param name="actorLevel">The level of the actor (killer).</param>
        private Item? GenerateCorpse(KeyValuePair<long, long> location, Character victim, int actorLevel)
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
                    RotTimer = 12,
                    Contains = new List<IItem>(),
                    IsPlayerCorpse = !victim.IsNPC,
                    IsNPCCorpse = victim.IsNPC,
                    ItemId = Constants.ITEM_CORPSE,
                };

                // If the victim was a mob, randomize the currency a little bit. If not, just use the full value.
                var currency = victim.IsNPC && victim.Currency > 0 ? this.random.Next(victim.Currency - 1, victim.Currency + 1) : victim.Currency;

                // If the player had any money, add it to the corpse.
                if (currency > 0)
                {
                    var currencyItems = currency.ToCurrencyItems();
                    corpse.Contains.AddRange(currencyItems);
                }

                // Add any equipment.
                foreach (var eq in victim.Equipment)
                {
                    corpse.Contains.Add(eq.Value.Clone());
                }

                // Add any inventory.
                foreach (var inv in victim.Inventory)
                {
                    corpse.Contains.Add(inv.Clone());
                }

                // Add any random loot drops
                if (victim.IsNPC && this.random.Next(1, 101) < 50)
                {
                    var item = ItemHelper.CreateRandomArmor(victim.Level, actorLevel, this.random);

                    if (item != null)
                    {
                        corpse.Contains.Add(item);
                    }
                }

                // Add the corpse to the room.
                var room = this.communicator.ResolveRoom(location);

                room?.Items.Add(corpse);

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
