// <copyright file="Memory.cs" company="Legendary™">
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
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;
    using MongoDB.Bson.Serialization.IdGenerators;

    /// <summary>
    /// Stores ineraction memories between a character an an AI mob.
    /// </summary>
    public class Memory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Memory"/> class.
        /// </summary>
        /// <param name="characterId">The character ID.</param>
        /// <param name="mobileId">The moile ID.</param>
        public Memory(long characterId, long mobileId)
        {
            this.CharacterId = characterId;
            this.MobileId = mobileId;
        }

        /// <summary>
        /// Gets or sets the ID of the memory for Mongo.
        /// </summary>
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        [BsonId(IdGenerator = typeof(ObjectIdGenerator))]
        [BsonElement("_id")]
        public ObjectId MemoryId { get; set; }

        /// <summary>
        /// Gets or sets the character ID.
        /// </summary>
        public long CharacterId { get; set; }

        /// <summary>
        /// Gets or sets the mobile ID.
        /// </summary>
        public long MobileId { get; set; }

        /// <summary>
        /// Gets or sets the last time the player and mob interacted.
        /// </summary>
        public DateTime? LastInteraction { get; set; }

        /// <summary>
        /// Gets or sets the list of memories.
        /// </summary>
        public List<string> Memories { get; set; } = new List<string>();
    }
}