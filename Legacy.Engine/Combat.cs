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
    using System.Linq;
    using Legendary.Core.Contracts;
    using Legendary.Core.Extensions;
    using Legendary.Core.Models;
    using Legendary.Engine.Contracts;

    /// <summary>
    /// Handles actions in combat related to skill and spell usage.
    /// </summary>
    public class Combat
    {
        private readonly IRandom random;

        /// <summary>
        /// Initializes a new instance of the <see cref="Combat"/> class.
        /// </summary>
        /// <param name="random">The random number generator.</param>
        public Combat(IRandom random)
        {
            this.random = random;
        }

        /// <summary>
        /// Calculates the damage for an action by an actor against a target.
        /// </summary>
        /// <param name="actor">The actor.</param>
        /// <param name="target">The target.</param>
        /// <param name="action">The action.</param>
        /// <returns>Damage.</returns>
        public int CalculateDamage(UserData actor, UserData target, IAction action)
        {
            // Reduce the damage inversely by level. So if the player is 10, target is 10, damage modifier is normal.
            // If the player is 20, target is 10, damage modifier is doubled.
            // If the player is 10, target is 20, damage modifier is halved.
            double adjust = (actor.Character.Level / target.Character.Level) * action.DamageModifier;

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
        /// Calculates the damage for an action by an actor against a mobile.
        /// </summary>
        /// <param name="actor">The actor.</param>
        /// <param name="target">The target.</param>
        /// <param name="action">The action.</param>
        /// <returns>Damage.</returns>
        public int CalculateDamage(UserData actor, Mobile target, IAction action)
        {
            // Reduce the damage inversely by level. So if the player is 10, target is 10, damage modifier is normal.
            // If the player is 20, target is 10, damage modifier is doubled.
            // If the player is 10, target is 20, damage modifier is halved.
            double adjust = (actor.Character.Level / target.Level) * action.DamageModifier;

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
        /// Applies the damage to the target. If damage brings them to below 0, sets the "dead" flags.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="damage">The damage.</param>
        public void ApplyDamage(UserData target, int damage)
        {
            target.Character.Health.Current -= damage;

            // If below zero, character is dead. Set the appropriate flags.
            if (target.Character.Health.Current < 0)
            {
                target.Character.CharacterFlags?.RemoveIfExists(Core.Types.CharacterFlags.Fighting);
                target.Character.CharacterFlags?.AddIfNotExists(Core.Types.CharacterFlags.Dead);
                target.Character.CharacterFlags?.AddIfNotExists(Core.Types.CharacterFlags.Ghost);
                target.Character.Location = target.Character.Home ?? Room.Default;
            }
        }

        /// <summary>
        /// Determines whether the target saved vs the attack type.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="action">The action to save against.</param>
        /// <returns>True if the target saved.</returns>
        public bool DidSave(UserData target, IAction action)
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
                    return saveThrow < target.Character.Saves.Spell;
                case Core.Types.DamageType.Energy:
                case Core.Types.DamageType.Negative:
                    return saveThrow < target.Character.Saves.Negative;
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
        public void ApplyAffect(UserData target, IAction action)
        {
            bool hasEffect = target.Character.AffectedBy.Any(a => a.Key == action);

            if (!hasEffect && action.AffectDuration.HasValue)
            {
                target.Character.AffectedBy.Add(action, action.AffectDuration.Value);
            }
        }
    }
}
