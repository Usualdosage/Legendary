// <copyright file="Message.cs" company="Legendary™">
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
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    /// <summary>
    /// Message object.
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.Int64)]
        [BsonElement("_id")]
        public long MessageId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the player the message is to.
        /// </summary>
        public long? To { get; set; }

        /// <summary>
        /// Gets or sets the ID of the player the message is from.
        /// </summary>
        public long? From { get; set; }

        /// <summary>
        /// Gets or sets the name of the player the message is to.
        /// </summary>
        public string? ToName { get; set; }

        /// <summary>
        /// Gets or sets the name of the player the message is from.
        /// </summary>
        public string? FromName { get; set; }

        /// <summary>
        /// Gets or sets the subject of the player message.
        /// </summary>
        public string? Subject { get; set; }

        /// <summary>
        /// Gets or sets the content of the player message.
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the email was read.
        /// </summary>
        public bool? IsRead { get; set; }

        /// <summary>
        /// Gets or sets the sent date.
        /// </summary>
        public DateTime? SentDate { get; set; }

        /// <summary>
        /// Gets or sets the read date.
        /// </summary>
        public DateTime? ReadDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the email has been deleted.
        /// </summary>
        public bool? IsDeleted { get; set; }
    }
}