// <copyright file="Persona.cs" company="Legendary™">
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
    using System.IO;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a character persona that can be modified.
    /// </summary>
    [BsonIgnoreExtraElements]
    public class Persona
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.Int32)]
        [BsonElement("_id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of this persona.
        /// </summary>
        [BsonElement("name")]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the prompt of this persona.
        /// </summary>
        [BsonElement("prompt")]
        public string? Prompt { get; set; }

        /// <summary>
        /// Gets or sets the attitude of this persona.
        /// </summary>
        [BsonElement("attitude")]
        public string? Attitude { get; set; }

        /// <summary>
        /// Gets or sets the age of this persona.
        /// </summary>
        [BsonElement("age")]
        public string? Age { get; set; }

        /// <summary>
        /// Gets or sets the race of this persona.
        /// </summary>
        [BsonElement("race")]
        public string? Race { get; set; }

        /// <summary>
        /// Gets or sets the class of this persona.
        /// </summary>
        [BsonElement("class")]
        public string? Class { get; set; }

        /// <summary>
        /// Gets or sets information about this persona in sentences.
        /// </summary>
        [BsonElement("background")]
        public List<string> Background { get; set; } = new List<string>();
    }
}