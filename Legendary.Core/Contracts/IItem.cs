// <copyright file="IItem.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Core.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Legendary.Core.Types;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    /// <summary>
    /// Implementation contract for an item.
    /// </summary>
    public interface IItem
    {
        /// <summary>
        /// Gets the unique ID of this item for generating a unique item ID.
        /// </summary>
        Guid UniqueId { get; }

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
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the level.
        /// </summary>
        int Level { get; set; }

        /// <summary>
        /// Gets or sets the short description.
        /// </summary>
        string? ShortDescription { get; set; }

        /// <summary>
        /// Gets or sets the long description.
        /// </summary>
        string? LongDescription { get; set; }

        /// <summary>
        /// Gets or sets the weight of the item.
        /// </summary>
        decimal Weight { get; set; }

        /// <summary>
        ///  Gets or sets the durability of the armor.
        /// </summary>
        MaxCurrent Durability { get; set; }

        /// <summary>
        /// Gets or sets the percent chance to block piercing attacks.
        /// </summary>
        int Pierce { get; set; }

        /// <summary>
        /// Gets or sets the percent chance to block blunt attacks.
        /// </summary>
        int Blunt { get; set; }

        /// <summary>
        /// Gets or sets the percent chance to block edged attacks.
        /// </summary>
        int Edged { get; set; }

        /// <summary>
        /// Gets or sets the percent chance to block magic attacks.
        /// </summary>
        int Magic { get; set; }

        /// <summary>
        /// Gets or sets the item type.
        /// </summary>
        ItemType ItemType { get; set; }

        /// <summary>
        /// Gets or sets the hit dice.
        /// </summary>
        int HitDice { get; set; }

        /// <summary>
        /// Gets or sets the damage dice.
        /// </summary>
        int DamageDice { get; set; }

        /// <summary>
        /// Gets or sets the damage modifier.
        /// </summary>
        int Modifier { get; set; }

        /// <summary>
        /// Gets or sets the damage type.
        /// </summary>
        DamageType DamageType { get; set; }

        /// <summary>
        /// Gets or sets the armor flags.
        /// </summary>
        IList<ItemFlags> ItemFlags { get; set; }

        /// <summary>
        /// Gets or sets the wear location(s) of this equipment.
        /// </summary>
        IList<WearLocation> WearLocation { get; set; }

        /// <summary>
        /// Gets or sets the location of the item.
        /// </summary>
        KeyValuePair<long, long> Location { get; set; }

        /// <summary>
        /// Gets or sets the value of the item.
        /// </summary>
        decimal Value { get; set; }

        /// <summary>
        /// Gets or sets the time until this item rots into dust (in ticks).
        /// </summary>
        int RotTimer { get; set; }

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
        /// Gets or sets the key id of the item for the lock.
        /// </summary>
        public long? KeyId { get; set; }

        /// <summary>
        /// Gets or sets the kind of item.
        /// </summary>
        public ItemKind ItemKind { get; set; }

        /// <summary>
        /// Gets or sets the liquid type in a drink or spring.
        /// </summary>
        public LiquidType LiquidType { get; set; }

        /// <summary>
        /// Gets or sets the current and max carry weight of the container.
        /// </summary>
        public MaxCurrent? CarryWeight { get; set; }

        /// <summary>
        /// Gets or sets the item resets for a container.
        /// </summary>
        public List<long>? ItemResets { get; set; }

        /// <summary>
        /// Gets or sets the current and max carry food value of the item.
        /// </summary>
        public MaxCurrent? Food { get; set; }

        /// <summary>
        /// Gets or sets the current and max number of drinks of the drink.
        /// </summary>
        public MaxCurrent? Drinks { get; set; }

        /// <summary>
        /// Gets or sets the program file.
        /// </summary>
        public string? Program { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not the container is trapped.
        /// </summary>
        public bool IsTrapped { get; set; }
    }
}