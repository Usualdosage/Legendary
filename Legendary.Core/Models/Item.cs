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
        /// Gets or sets the name of the item.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the short description of the item.
        /// </summary>
        public string? ShortDescription { get; set; }

        /// <summary>
        /// Gets or sets the long description of the item.
        /// </summary>
        public string? LongDescription { get; set; }

        /// <summary>
        /// Gets or sets the weight.
        /// </summary>
        public int Weight { get; set; } = 0;

        /// <summary>
        /// Gets or sets the type of the item.
        /// </summary>
        public ItemType ItemType { get; set; }

        /// <summary>
        /// Gets or sets where the item can be worn/carried.
        /// </summary>
        public IList<WearLocation>? WearLocation { get; set; }

        /// <summary>
        /// Gets or sets the flags on the item.
        /// </summary>
        public IList<ItemFlags>? Flags { get; set; }
    }
}