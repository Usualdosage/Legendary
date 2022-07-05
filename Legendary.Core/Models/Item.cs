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
    using Legendary.Core.Types;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    /// <summary>
    /// Represents a single item.
    /// </summary>
    [BsonIgnoreExtraElements]
    public class Item
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
        /// Gets or sets the weight of the armor.
        /// </summary>
        public int Weight { get; set; }

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
        public Room? Location { get; set; }
    }
}