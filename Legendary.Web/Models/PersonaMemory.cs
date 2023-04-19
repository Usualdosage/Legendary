// <copyright file="PersonaMemory.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Web.Models
{
    using System;
    using System.Collections.Generic;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    /// <summary>
    /// Represents a memory set of a conversaion between a user and a persona.
    /// </summary>
    public class PersonaMemory
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.Int32)]
        [BsonElement("_id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the id of this memory set.
        /// </summary>
        [BsonElement("personaid")]
        public int PersonaId { get; set; }

        /// <summary>
        /// Gets or sets the username these memories were made with.
        /// </summary>
        [BsonElement("username")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets last interaction datetime.
        /// </summary>
        [BsonElement("lastinteraction")]
        public DateTime? LastInteraction { get; set; }

        /// <summary>
        /// Gets or sets the memories for this person with this persona.
        /// </summary>
        [BsonElement("memories")]
        public List<string> Memories { get; set; } = new List<string>();
    }
}
