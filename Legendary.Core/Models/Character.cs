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
    using System.ComponentModel;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Legendary.Core.Contracts;
    using Legendary.Core.Types;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;
    using MongoDB.Bson.Serialization.Options;
    using static MongoDB.Driver.WriteConcern;

    /// <summary>
    /// Represents a single character.
    /// </summary>
    [BsonIgnoreExtraElements]
    public partial class Character
    {
        private MaxCurrent str = new (10, 10);
        private MaxCurrent intg = new (10, 10);
        private MaxCurrent wis = new (10, 10);
        private MaxCurrent dex = new (10, 10);
        private MaxCurrent con = new (10, 10);
        private MaxCurrent carryWeight = new (80, 80);

        private int defaultAge = 18;
        private int saveDeath = 8;
        private int saveSpell = 8;
        private int saveMaledictive = 8;
        private int saveNegative = 8;
        private int saveAfflictive = 8;
        private int hitDice = 0;
        private int damDice = 0;

        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.Int64)]
        [BsonElement("_id")]
        [Browsable(false)]
        public long CharacterId { get; set; }

        /// <summary>
        /// Gets the unique ID of this character for generating a unique character ID.
        /// </summary>
        [Browsable(false)]
        public Guid UniqueId { get => Guid.NewGuid(); }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        public virtual string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the middle name.
        /// </summary>
        public virtual string? MiddleName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        public virtual string? LastName { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public virtual string? Title { get; set; }

        /// <summary>
        /// Gets the player's age.
        /// </summary>
        [Browsable(false)]
        public virtual int Age => this.defaultAge + (this.Metrics.GameHoursPlayed / 8760);

        /// <summary>
        /// Gets or sets the level.
        /// </summary>
        public virtual int Level { get; set; } = 1;

        /// <summary>
        /// Gets or sets the experience.
        /// </summary>
        [Browsable(false)]
        public long Experience { get; set; } = 0;

        /// <summary>
        /// Gets or sets the currency.
        /// </summary>
        public virtual decimal Currency { get; set; } = 0m;

        /// <summary>
        /// Gets or sets the player's divine favor.
        /// </summary>
        [Browsable(false)]
        public int DivineFavor { get; set; }

        /// <summary>
        /// Gets or sets the most recent image the player observed.
        /// </summary>
        [Browsable(false)]
        public string? LastImage { get; set; }

        /// <summary>
        /// Gets or sets the password (hash).
        /// </summary>
        [Browsable(false)]
        public string? Password { get; set; }

        /// <summary>
        /// Gets or sets the short description.
        /// </summary>
        public virtual string? ShortDescription { get; set; }

        /// <summary>
        /// Gets or sets the long description.
        /// </summary>
        public virtual string? LongDescription { get; set; }

        /// <summary>
        /// Gets or sets the language the character is currently speaking.
        /// </summary>
        [Browsable(false)]
        public string Speaking { get; set; } = "Common";

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        [Browsable(false)]
        public KeyValuePair<long, long> Location { get; set; } = new KeyValuePair<long, long>(1, 1);

        /// <summary>
        /// Gets or sets a value indicating whether this is an NPC.
        /// </summary>
        [Browsable(false)]
        public virtual bool IsNPC { get; set; } = false;

        /// <summary>
        /// Gets or sets the number of practice sessions.
        /// </summary>
        [Browsable(false)]
        public int Practices { get; set; } = 0;

        /// <summary>
        /// Gets or sets number of training sessions.
        /// </summary>
        [Browsable(false)]
        public int Trains { get; set; } = 0;

        /// <summary>
        /// Gets or sets number of learning sessions.
        /// </summary>
        [Browsable(false)]
        public int Learns { get; set; } = 0;

        /// <summary>
        /// Gets or sets the character flags.
        /// </summary>
        [Browsable(false)]
        public IList<CharacterFlags> CharacterFlags { get; set; } = new List<CharacterFlags>();

        /// <summary>
        /// Gets or sets the chararacter the player last spoke to.
        /// </summary>
        [Browsable(false)]
        public string? LastComm { get; set; }

        /// <summary>
        /// Gets or sets the health.
        /// </summary>
        public virtual MaxCurrent Health { get; set; } = new MaxCurrent(0, 0);

        /// <summary>
        /// Gets or sets the mana.
        /// </summary>
        [Browsable(false)]
        public virtual MaxCurrent Mana { get; set; } = new MaxCurrent(0, 0);

        /// <summary>
        /// Gets or sets the movement.
        /// </summary>
        [Browsable(false)]
        public virtual MaxCurrent Movement { get; set; } = new MaxCurrent(0, 0);

        /// <summary>
        /// Gets or sets the carry weight of the player.
        /// </summary>
        [Browsable(false)]
        public MaxCurrent CarryWeight
        {
            get
            {
                var current = this.Equipment.Sum(e => e.Value.Weight) + this.Inventory.Sum(i => i.Weight);
                var size = Races.RaceData.FirstOrDefault(r => r.Key == this.Race);
                if (size.Value != null)
                {
                    var max = size.Value.BaseCarryWeight + (this.Str.Max * 2);
                    return new MaxCurrent(max, (int)current);
                }
                else
                {
                    return new MaxCurrent(100 + (this.Str.Max * 2), (int)current);
                }
            }

            set
            {
                this.carryWeight = value;
            }
        }

        /// <summary>
        /// Gets or sets the player's hunger counter.
        /// </summary>
        [Browsable(false)]
        public MaxCurrent Hunger { get; set; } = new MaxCurrent(96, 0);

        /// <summary>
        /// Gets or sets the player's thirst counter.
        /// </summary>
        [Browsable(false)]
        public MaxCurrent Thirst { get; set; } = new MaxCurrent(72, 0);

        /// <summary>
        /// Gets or sets the character's home room (recall point).
        /// </summary>
        [Browsable(false)]
        public KeyValuePair<long, long> Home { get; set; } = new KeyValuePair<long, long>(1, 1);

        /// <summary>
        /// Gets or sets the inventory.
        /// </summary>
        public List<Item> Inventory { get; set; } = new List<Item>();

        /// <summary>
        /// Gets or sets the player's strength.
        /// </summary>
        public virtual MaxCurrent Str
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
        public virtual MaxCurrent Int
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
        public virtual MaxCurrent Wis
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
        public virtual MaxCurrent Dex
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
        public virtual MaxCurrent Con
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
        public virtual Alignment Alignment { get; set; } = Alignment.Neutral;

        /// <summary>
        /// Gets or sets the deity.
        /// </summary>
        public Deities Deity { get; set; } = Deities.Khoda;

        /// <summary>
        /// Gets or sets the hit dice.
        /// </summary>
        public virtual int HitDice
        {
            get
            {
                double hitDice = Constants.STANDARD_PLAYER_HITDICE;

                // Players get an additional .5 hit dice, rounded up, for each dexterity over 12.
                hitDice += Math.Round((this.Dex.Current - 12) * .5, 0);

                // Sum up all hit dice modifiers for their equipment they are wearing.
                hitDice += this.Equipment.Sum(s => s.Value.HitDice);

                // Sum up any hit modifiers the user has as an effect.
                hitDice += this.AffectedBy.Sum(s => s.HitDice ?? 0);

                return (int)hitDice;
            }

            set
            {
                this.hitDice = value;
            }
        }

        /// <summary>
        /// Gets or sets the damage dice.
        /// </summary>
        public virtual int DamageDice
        {
            get
            {
                double damDice = Constants.STANDARD_PLAYER_DAMDICE;

                // Players get an additional .5 damage dice, rounded up, for each strength over 12.
                damDice += Math.Round((this.Str.Current - 12) * .5, 0);

                // Sum up all damage dice modifiers for their equipment they are wearing.
                damDice += this.Equipment.Sum(s => s.Value.DamageDice);

                // Sum up any damage modifiers the user has as an effect.
                damDice += this.AffectedBy.Sum(s => s.DamageDice ?? 0);

                return (int)damDice;
            }

            set
            {
                this.damDice = value;
            }
        }

        /// <summary>
        /// Gets or sets the ethos.
        /// </summary>
        public virtual Ethos Ethos { get; set; } = Ethos.Neutral;

        /// <summary>
        /// Gets or sets the gender.
        /// </summary>
        public virtual Gender Gender { get; set; } = Gender.Other;

        /// <summary>
        /// Gets the pronoun associated with the character's gender.
        /// </summary>
        [Browsable(false)]
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

                    case Gender.Female:
                        {
                            return "her";
                        }

                    case Gender.Male:
                        {
                            return "his";
                        }
                }
            }
        }

        /// <summary>
        /// Gets the pronoun associated with the character's gender.
        /// </summary>
        [Browsable(false)]
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

                    case Gender.Female:
                        {
                            return "she";
                        }

                    case Gender.Male:
                        {
                            return "he";
                        }
                }
            }
        }

        /// <summary>
        /// Gets or sets the race.
        /// </summary>
        public virtual Race Race { get; set; } = Race.Human;

        /// <summary>
        /// Gets or sets the player's skills.
        /// </summary>
        [Browsable(false)]
        public IList<SkillProficiency> Skills { get; set; } = new List<SkillProficiency>();

        /// <summary>
        /// Gets or sets the player's spells.
        /// </summary>
        [Browsable(false)]
        public IList<SpellProficiency> Spells { get; set; } = new List<SpellProficiency>();

        /// <summary>
        /// Gets or sets the player's metrics.
        /// </summary>
        [Browsable(false)]
        public Metrics Metrics { get; set; } = new Metrics();

        /// <summary>
        /// Gets or sets the player's equipment.
        /// </summary>
        /// <remarks>
        /// This is an irritating thing about Mongo's C# driver.
        /// </remarks>
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<WearLocation, Item> Equipment { get; set; } = new Dictionary<WearLocation, Item>();

        /// <summary>
        /// Gets or sets the player's followers.
        /// </summary>
        [Browsable(false)]
        public IList<long> Followers { get; set; } = new List<long>();

        /// <summary>
        /// Gets or sets the character the player is following.
        /// </summary>
        [Browsable(false)]
        public long? Following { get; set; }

        /// <summary>
        /// Gets or sets the skills or spells the player is affected by, and the remaining duration.
        /// </summary>
        [Browsable(false)]
        public List<Effect> AffectedBy { get; set; } = new List<Effect>();

        /// <summary>
        /// Gets or sets the save vs. spell.
        /// </summary>
        public virtual int SaveSpell
        {
            get
            {
                return this.saveSpell + this.AffectedBy.Sum(a => a.Spell ?? 0) + this.Equipment.Sum(s => s.Value.SaveSpell ?? 0);
            }

            set
            {
                this.saveSpell = value;
            }
        }

        /// <summary>
        /// Gets or sets the save vs. negative.
        /// </summary>
        public virtual int SaveNegative
        {
            get
            {
                return this.saveNegative + this.AffectedBy.Sum(a => a.Negative ?? 0) + this.Equipment.Sum(s => s.Value.SaveNegative ?? 0);
            }

            set
            {
                this.saveNegative = value;
            }
        }

        /// <summary>
        /// Gets or sets the save vs. maledictive.
        /// </summary>
        public virtual int SaveMaledictive
        {
            get
            {
                return this.saveMaledictive + this.AffectedBy.Sum(a => a.Maledictive ?? 0) + this.Equipment.Sum(s => s.Value.SaveMaledictive ?? 0);
            }

            set
            {
                this.saveMaledictive = value;
            }
        }

        /// <summary>
        /// Gets or sets the save vs. afflictive.
        /// </summary>
        public virtual int SaveAfflictive
        {
            get
            {
                return this.saveAfflictive + this.AffectedBy.Sum(a => a.Afflictive ?? 0) + this.Equipment.Sum(s => s.Value.SaveAfflictive ?? 0);
            }

            set
            {
                this.saveAfflictive = value;
            }
        }

        /// <summary>
        /// Gets or sets the save vs. death.
        /// </summary>
        public virtual int SaveDeath
        {
            get
            {
                return this.saveDeath + this.AffectedBy.Sum(a => a.Death ?? 0) + this.Equipment.Sum(s => s.Value.SaveDeath ?? 0);
            }

            set
            {
                this.saveDeath = value;
            }
        }

        /// <summary>
        /// Gets or sets the character this character (or mob) is currently fighting.
        /// </summary>
        [Browsable(false)]
        public long? Fighting { get; set; }

        /// <summary>
        /// Gets or sets the images for this mobile.
        /// </summary>
        public virtual List<string>? Images { get; set; }

        /// <summary>
        /// Gets or sets the player's rewards.
        /// </summary>
        [Browsable(false)]
        public List<Award> Awards { get; set; } = new List<Award>();

        /// <summary>
        /// Gets or sets the player's group.
        /// </summary>
        [Browsable(false)]
        public long? GroupId { get; set; }

        /// <summary>
        /// Gets or sets the player's wimpy setting.
        /// </summary>
        public int Wimpy { get; set; }

        /// <summary>
        /// Gets the race abbreviation.
        /// </summary>
        [Browsable(false)]
        public string RaceAbbrev
        {
            get => this.Race.ToString()[..3];
        }

        /// <summary>
        /// Indicates whether the player has a given skill.
        /// </summary>
        /// <param name="name">The name of the skill.</param>
        /// <returns>True if they have it.</returns>
        public bool HasSkill(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            // This will prevent skills being resolved for directions (e, ne, sw, etc).
            if (name.Length <= 2)
            {
                return false;
            }

            return this.Skills.Any(sk => sk.SkillName.ToLower().StartsWith(name.ToLower()));
        }

        /// <summary>
        /// Gets the skill proficiency by name.
        /// </summary>
        /// <param name="name">The skill name.</param>
        /// <returns>The skill proficiency, if exists.</returns>
        public SkillProficiency? GetSkillProficiency(string? name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                return this.Skills.FirstOrDefault(sp => sp.SkillName.ToLower().StartsWith(name.ToLower()));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Indicates whether the player has a given spell.
        /// </summary>
        /// <param name="name">The name of the spell.</param>
        /// <returns>True if they have it.</returns>
        public bool HasSpell(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            return this.Spells.Any(sp => sp.SpellName.ToLower().StartsWith(name.ToLower()));
        }

        /// <summary>
        /// Gets the spell proficiency by name.
        /// </summary>
        /// <param name="name">The spell name.</param>
        /// <returns>The spell proficiency, if exists.</returns>
        public SpellProficiency? GetSpellProficiency(string? name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                return this.Spells.FirstOrDefault(sp => sp.SpellName.ToLower().StartsWith(name.ToLower()));
            }
            else
            {
                return null;
            }
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

        /// <summary>
        /// Gets whether this character is affected by an action.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>True if affected.</returns>
        public bool IsAffectedBy(string action)
        {
            string effectName = EffectRegex().Replace(action, "$1 $2");
            return this.AffectedBy.Any(a => a.Name?.ToLower() == effectName.ToLower());
        }

        [GeneratedRegex("([a-z])([A-Z])")]
        private static partial Regex EffectRegex();
    }
}