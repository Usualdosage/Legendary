// <copyright file="Persona.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Web.Models
{
    using System.Collections.Generic;
    using Legendary.Web.Types;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    /// <summary>
    /// Represents a companion persona.
    /// </summary>
    [BsonIgnoreExtraElements]
    public class Persona
    {
        /// <summary>
        /// Gets or sets the persona id.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.Int32)]
        [BsonElement("_id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the persona name.
        /// </summary>
        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the training data.
        /// </summary>
        [BsonElement("training")]
        public List<string> Training { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the URL of the avatar.
        /// </summary>
        [BsonElement("avatar")]
        public string AvatarUrl { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets all of the available images for this persona.
        /// </summary>
        [BsonElement("images")]
        public List<string> Images { get; set; } = new List<string>();
    }
}
