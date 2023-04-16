// <copyright file="Item.cs" company="Legendary™">
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
    using Legendary.Core.Contracts;
    using Legendary.Core.Types;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    /// <summary>
    /// Represents a single item.
    /// </summary>
    [BsonIgnoreExtraElements]
    public class Item : IItem
    {
        /// <summary>
        /// Gets the unique ID of this character for generating a unique character ID.
        /// </summary>
        public Guid UniqueId { get => Guid.NewGuid(); }

        /// <summary>
        /// Gets or sets the ID of the item.
        /// </summary>
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.Int64)]
        [BsonElement("_id")]
        public long ItemId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the level.
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Gets or sets the short description.
        /// </summary>
        public string? ShortDescription { get; set; }

        /// <summary>
        /// Gets or sets the long description.
        /// </summary>
        public string? LongDescription { get; set; }

        /// <summary>
        /// Gets or sets the weight of the item.
        /// </summary>
        public decimal Weight { get; set; }

        /// <summary>
        ///  Gets or sets the durability of the armor.
        /// </summary>
        public MaxCurrent Durability { get; set; } = new MaxCurrent(0, 0);

        /// <summary>
        /// Gets or sets the percent chance to block piercing attacks.
        /// </summary>
        public int Pierce { get; set; } = 0;

        /// <summary>
        /// Gets or sets the percent chance to block blunt attacks.
        /// </summary>
        public int Blunt { get; set; } = 0;

        /// <summary>
        /// Gets or sets the percent chance to block edged attacks.
        /// </summary>
        public int Edged { get; set; } = 0;

        /// <summary>
        /// Gets or sets the percent chance to block magic attacks.
        /// </summary>
        public int Magic { get; set; } = 0;

        /// <summary>
        /// Gets or sets the item type.
        /// </summary>
        public ItemType ItemType { get; set; }

        /// <summary>
        /// Gets or sets the hit dice.
        /// </summary>
        public int HitDice { get; set; } = 0;

        /// <summary>
        /// Gets or sets the damage dice.
        /// </summary>
        public int DamageDice { get; set; } = 0;

        /// <summary>
        /// Gets or sets the damage modifier.
        /// </summary>
        public int Modifier { get; set; } = 0;

        /// <summary>
        /// Gets or sets the damage type.
        /// </summary>
        public DamageType DamageType { get; set; }

        /// <summary>
        /// Gets or sets the weapon type.
        /// </summary>
        public WeaponType WeaponType { get; set; } = WeaponType.Sword;

        /// <summary>
        /// Gets or sets the armor flags.
        /// </summary>
        public IList<ItemFlags> ItemFlags { get; set; } = new List<ItemFlags>();

        /// <summary>
        /// Gets or sets the wear location(s) of this equipment.
        /// </summary>
        public IList<WearLocation> WearLocation { get; set; } = new List<WearLocation>();

        /// <summary>
        /// Gets or sets the location of the item.
        /// </summary>
        public KeyValuePair<long, long> Location { get; set; }

        /// <summary>
        /// Gets or sets the value of the item.
        /// </summary>
        public decimal Value { get; set; } = 0;

        /// <summary>
        /// Gets or sets the time until this item rots into dust (in ticks).
        /// </summary>
        public int RotTimer { get; set; } = -1;

        /// <summary>
        /// Gets or sets a list of things this item contains (if it's a container).
        /// </summary>
        public List<IItem>? Contains { get; set; }

        /// <summary>
        /// Gets or sets the item image.
        /// </summary>
        public string? Image { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the item is closed (if container).
        /// </summary>
        public bool IsClosed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the item is locked (if container).
        /// </summary>
        [Category("Containers")]
        [Description("Indicates if the container is locked.")]
        public bool IsLocked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the item is trapped (if container).
        /// </summary>
        public bool IsTrapped { get; set; }

        /// <summary>
        /// Gets or sets the trap type.
        /// </summary>
        public TrapType? TrapType { get; set; }

        /// <summary>
        /// Gets or sets the keyId for the lock.
        /// </summary>
        public long? KeyId { get; set; }

        /// <summary>
        /// Gets or sets the kind of item.
        /// </summary>
        public ItemKind ItemKind { get; set; } = ItemKind.Common;

        /// <summary>
        /// Gets or sets the liquid type in a drink or spring.
        /// </summary>
        public LiquidType LiquidType { get; set; } = LiquidType.None;

        /// <summary>
        /// Gets or sets the max and current carry weight of the container.
        /// </summary>
        public MaxCurrent? CarryWeight { get; set; }

        /// <summary>
        /// Gets or sets the item resets for a container.
        /// </summary>
        public List<long>? ItemResets { get; set; }

        /// <summary>
        /// Gets or sets the food value for an item.
        /// </summary>
        public MaxCurrent? Food { get; set; }

        /// <summary>
        /// Gets or sets the number of drinks in a container (drink).
        /// </summary>
        public MaxCurrent? Drinks { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not this is a mob corpse.
        /// </summary>
        public bool IsNPCCorpse { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not this is a player corpse.
        /// </summary>
        public bool IsPlayerCorpse { get; set; }

        /// <summary>
        /// Gets or sets what the item casts when used.
        /// </summary>
        public string? SpellName { get; set; }

        /// <summary>
        /// Gets or sets the level the spell is cast at.
        /// </summary>
        public int? CastLevel { get; set; }

        /// <summary>
        /// Gets or sets the frequency with which the action is cast.
        /// </summary>
        public int? CastFrequency { get; set; }

        /// <summary>
        /// Gets or sets the save maledictive modifier.
        /// </summary>
        public int? SaveMaledictive { get; set; }

        /// <summary>
        /// Gets or sets the save spell modifier.
        /// </summary>
        public int? SaveSpell { get; set; }

        /// <summary>
        /// Gets or sets the save negative modifier.
        /// </summary>
        public int? SaveNegative { get; set; }

        /// <summary>
        /// Gets or sets the save death modifier.
        /// </summary>
        public int? SaveDeath { get; set; }

        /// <summary>
        /// Gets or sets the save afflictive modifier.
        /// </summary>
        public int? SaveAfflictive { get; set; }

        /// <summary>
        /// Gets or sets the program.
        /// </summary>
        public string? Program { get; set; }
    }
}