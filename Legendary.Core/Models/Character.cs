// <copyright file="Character.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Core.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Legendary.Core.Contracts;
    using Legendary.Core.Types;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    /// <summary>
    /// Represents a single character.
    /// </summary>
    [BsonIgnoreExtraElements]
    public class Character
    {
        private MaxCurrent str = new MaxCurrent(10, 10);
        private MaxCurrent intg = new MaxCurrent(10, 10);
        private MaxCurrent wis = new MaxCurrent(10, 10);
        private MaxCurrent dex = new MaxCurrent(10, 10);
        private MaxCurrent con = new MaxCurrent(10, 10);

        private int saveDeath = 8;
        private int saveSpell = 8;
        private int saveMaledictive = 8;
        private int saveNegative = 8;
        private int saveAfflictive = 8;

        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.Int64)]
        [BsonElement("_id")]
        public long CharacterId { get; set; }

        /// <summary>
        /// Gets the unique ID of this character for generating a unique character ID.
        /// </summary>
        public Guid UniqueId { get => Guid.NewGuid(); }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the middle name.
        /// </summary>
        public string? MiddleName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        public string? LastName { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the player's age.
        /// </summary>
        public int Age { get; set; } = 18;

        /// <summary>
        /// Gets or sets the level.
        /// </summary>
        public int Level { get; set; } = 1;

        /// <summary>
        /// Gets or sets the experience.
        /// </summary>
        public long Experience { get; set; } = 0;

        /// <summary>
        /// Gets or sets the currency.
        /// </summary>
        public long Currency { get; set; } = 0;

        /// <summary>
        /// Gets or sets the player's divine favor.
        /// </summary>
        public int DivineFavor { get; set; }

        /// <summary>
        /// Gets or sets the password (hash).
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Gets or sets the short description.
        /// </summary>
        public string? ShortDescription { get; set; }

        /// <summary>
        /// Gets or sets the long description.
        /// </summary>
        public string? LongDescription { get; set; }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        public KeyValuePair<long, long> Location { get; set; } = new KeyValuePair<long, long>(1, 1);

        /// <summary>
        /// Gets or sets a value indicating whether this is an NPC.
        /// </summary>
        public virtual bool IsNPC { get; set; } = false;

        /// <summary>
        /// Gets or sets the number of practice sessions.
        /// </summary>
        public int Practices { get; set; } = 0;

        /// <summary>
        /// Gets or sets number of training sessions.
        /// </summary>
        public int Trains { get; set; } = 0;

        /// <summary>
        /// Gets or sets the character flags.
        /// </summary>
        public IList<CharacterFlags> CharacterFlags { get; set; } = new List<CharacterFlags>();

        /// <summary>
        /// Gets or sets the chararacter the player last spoke to.
        /// </summary>
        public string? LastComm { get; set; }

        /// <summary>
        /// Gets or sets the health.
        /// </summary>
        public MaxCurrent Health { get; set; } = new MaxCurrent(0, 0);

        /// <summary>
        /// Gets or sets the mana.
        /// </summary>
        public MaxCurrent Mana { get; set; } = new MaxCurrent(0, 0);

        /// <summary>
        /// Gets or sets the movement.
        /// </summary>
        public MaxCurrent Movement { get; set; } = new MaxCurrent(0, 0);

        /// <summary>
        /// Gets or sets the carry weight of the player.
        /// </summary>
        public MaxCurrent CarryWeight { get; set; } = new MaxCurrent(120, 120);

        /// <summary>
        /// Gets or sets the player's hunger counter.
        /// </summary>
        public MaxCurrent Hunger { get; set; } = new MaxCurrent(24, 0);

        /// <summary>
        /// Gets or sets the player's thirst counter.
        /// </summary>
        public MaxCurrent Thirst { get; set; } = new MaxCurrent(24, 0);

        /// <summary>
        /// Gets or sets the character's home room (recall point).
        /// </summary>
        public KeyValuePair<long, long> Home { get; set; } = new KeyValuePair<long, long>(1, 1);

        /// <summary>
        /// Gets or sets the inventory.
        /// </summary>
        public List<Item> Inventory { get; set; } = new List<Item>();

        /// <summary>
        /// Gets or sets the player's strength.
        /// </summary>
        public MaxCurrent Str
        {
            get
            {
                return new MaxCurrent(this.str.Max, this.str.Current + this.AffectedBy.Sum(a => a.Str ?? 0));
            }

            set
            {
                this.str = value;
            }
        }

        /// <summary>
        /// Gets or sets the player's strength.
        /// </summary>
        public MaxCurrent Int
        {
            get
            {
                return new MaxCurrent(this.intg.Max, this.intg.Current + this.AffectedBy.Sum(a => a.Int ?? 0));
            }

            set
            {
                this.intg = value;
            }
        }

        /// <summary>
        /// Gets or sets the player's wisdom.
        /// </summary>
        public MaxCurrent Wis
        {
            get
            {
                return new MaxCurrent(this.wis.Max, this.wis.Current + this.AffectedBy.Sum(a => a.Wis ?? 0));
            }

            set
            {
                this.wis = value;
            }
        }

        /// <summary>
        /// Gets or sets the player's dexterity.
        /// </summary>
        public MaxCurrent Dex
        {
            get
            {
                return new MaxCurrent(this.dex.Max, this.dex.Current + this.AffectedBy.Sum(a => a.Dex ?? 0));
            }

            set
            {
                this.dex = value;
            }
        }

        /// <summary>
        /// Gets or sets the player's constitution.
        /// </summary>
        public MaxCurrent Con
        {
            get
            {
                return new MaxCurrent(this.con.Max, this.con.Current + this.AffectedBy.Sum(a => a.Con ?? 0));
            }

            set
            {
                this.con = value;
            }
        }

        /// <summary>
        /// Gets or sets the alignment.
        /// </summary>
        public Alignment Alignment { get; set; } = Alignment.Neutral;

        /// <summary>
        /// Gets the hit dice.
        /// </summary>
        public int HitDice
        {
            get
            {
                double hitDice = Constants.STANDARD_PLAYER_HITDICE;

                // Players get an additional .5 hit dice, rounded up, for each dexterity over 12.
                hitDice += Math.Round((this.Dex.Current - 12) * .5, 0);

                // Sum up all hit dice modifiers for their equipment they are wearing.
                hitDice += this.Equipment.Sum(s => s.HitDice);

                // Sum up any hit modifiers the user has as an effect.
                hitDice += this.AffectedBy.Sum(s => s.HitDice ?? 0);

                return (int)hitDice;
            }
        }

        /// <summary>
        /// Gets the damage dice.
        /// </summary>
        public int DamageDice
        {
            get
            {
                double damDice = Constants.STANDARD_PLAYER_DAMDICE;

                // Players get an additional .5 damage dice, rounded up, for each strength over 12.
                damDice += Math.Round((this.Str.Current - 12) * .5, 0);

                // Sum up all damage dice modifiers for their equipment they are wearing.
                damDice += this.Equipment.Sum(s => s.DamageDice);

                // Sum up any damage modifiers the user has as an effect.
                damDice += this.AffectedBy.Sum(s => s.DamageDice ?? 0);

                return (int)damDice;
            }
        }

        /// <summary>
        /// Gets or sets the ethos.
        /// </summary>
        public Ethos Ethos { get; set; } = Ethos.Neutral;

        /// <summary>
        /// Gets or sets the gender.
        /// </summary>
        public Gender Gender { get; set; } = Gender.Other;

        /// <summary>
        /// Gets the pronoun associated with the character's gender.
        /// </summary>
        public string Pronoun
        {
            get
            {
                switch (this.Gender)
                {
                    default:
                    {
                        return "their";
                    }

                    case Legendary.Core.Types.Gender.Female:
                    {
                        return "her";
                    }

                    case Legendary.Core.Types.Gender.Male:
                    {
                        return "his";
                    }
                }
            }
        }

        /// <summary>
        /// Gets the pronoun associated with the character's gender.
        /// </summary>
        public string PronounSubjective
        {
            get
            {
                switch (this.Gender)
                {
                    default:
                        {
                            return "they";
                        }

                    case Legendary.Core.Types.Gender.Female:
                        {
                            return "she";
                        }

                    case Legendary.Core.Types.Gender.Male:
                        {
                            return "he";
                        }
                }
            }
        }

        /// <summary>
        /// Gets or sets the race.
        /// </summary>
        public Race Race { get; set; } = Race.Human;

        /// <summary>
        /// Gets or sets the player's skills.
        /// </summary>
        public IList<SkillProficiency> Skills { get; set; } = new List<SkillProficiency>();

        /// <summary>
        /// Gets or sets the player's spells.
        /// </summary>
        public IList<SpellProficiency> Spells { get; set; } = new List<SpellProficiency>();

        /// <summary>
        /// Gets or sets the player's metrics.
        /// </summary>
        public Metrics Metrics { get; set; } = new Metrics();

        /// <summary>
        /// Gets or sets the player's equipment.
        /// </summary>
        public List<Item> Equipment { get; set; } = new List<Item>();

        /// <summary>
        /// Gets or sets the player's followers.
        /// </summary>
        public IList<long> Followers { get; set; } = new List<long>();

        /// <summary>
        /// Gets or sets the character the player is following.
        /// </summary>
        public Character? Following { get; set; }

        /// <summary>
        /// Gets or sets the skills or spells the player is affected by, and the remaining duration.
        /// </summary>
        public List<Effect> AffectedBy { get; set; } = new List<Effect>();

        /// <summary>
        /// Gets or sets the save vs. spell.
        /// </summary>
        public int SaveSpell
        {
            get
            {
                return this.saveSpell + this.AffectedBy.Sum(a => a.Spell ?? 0);
            }

            set
            {
                this.saveSpell = value;
            }
        }

        /// <summary>
        /// Gets or sets the save vs. negative.
        /// </summary>
        public int SaveNegative
        {
            get
            {
                return this.saveNegative + this.AffectedBy.Sum(a => a.Negative ?? 0);
            }

            set
            {
                this.saveNegative = value;
            }
        }

        /// <summary>
        /// Gets or sets the save vs. maledictive.
        /// </summary>
        public int SaveMaledictive
        {
            get
            {
                return this.saveMaledictive + this.AffectedBy.Sum(a => a.Maledictive ?? 0);
            }

            set
            {
                this.saveMaledictive = value;
            }
        }

        /// <summary>
        /// Gets or sets the save vs. afflictive.
        /// </summary>
        public int SaveAfflictive
        {
            get
            {
                return this.saveAfflictive + this.AffectedBy.Sum(a => a.Afflictive ?? 0);
            }

            set
            {
                this.saveAfflictive = value;
            }
        }

        /// <summary>
        /// Gets or sets the save vs. death.
        /// </summary>
        public int SaveDeath
        {
            get
            {
                return this.saveDeath + this.AffectedBy.Sum(a => a.Death ?? 0);
            }

            set
            {
                this.saveDeath = value;
            }
        }

        /// <summary>
        /// Gets or sets the character this character (or mob) is currently fighting.
        /// </summary>
        public long? Fighting { get; set; }

        /// <summary>
        /// Gets or sets the base-64 image for this character.
        /// </summary>
        public string? Image { get; set; }

        /// <summary>
        /// Indicates whether the player has a given skill.
        /// </summary>
        /// <param name="name">The name of the skill.</param>
        /// <returns>True if they have it.</returns>
        public bool HasSkill(string name)
        {
            return this.Skills.Any(sk => sk.SkillName.ToLower().StartsWith(name.ToLower()));
        }

        /// <summary>
        /// Gets the skill proficiency by name.
        /// </summary>
        /// <param name="name">The skill name.</param>
        /// <returns>The skill proficiency, if exists.</returns>
        public SkillProficiency? GetSkillProficiency(string name)
        {
            return this.Skills.FirstOrDefault(sk => sk.SkillName.ToLower().StartsWith(name.ToLower()));
        }

        /// <summary>
        /// Indicates whether the player has a given spell.
        /// </summary>
        /// <param name="name">The name of the spell.</param>
        /// <returns>True if they have it.</returns>
        public bool HasSpell(string name)
        {
            return this.Spells.Any(sp => sp.SpellName.ToLower().StartsWith(name.ToLower()));
        }

        /// <summary>
        /// Gets the spell proficiency by name.
        /// </summary>
        /// <param name="name">The spell name.</param>
        /// <returns>The spell proficiency, if exists.</returns>
        public SpellProficiency? GetSpellProficiency(string name)
        {
            return this.Spells.FirstOrDefault(sk => sk.SpellName.ToLower().StartsWith(name.ToLower()));
        }

        /// <summary>
        /// Gets whether this character is affected by an action.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>True if affected.</returns>
        public bool IsAffectedBy(IAction action)
        {
            return this.AffectedBy.Any(a => a.Name == action.Name);
        }
    }
}