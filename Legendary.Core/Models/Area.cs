﻿// <copyright file="Area.cs" company="Legendary™">
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
    using System.ComponentModel;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    /// <summary>
    /// Represents an area which contains rooms.
    /// </summary>
    [BsonIgnoreExtraElements]
    public class Area
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Area"/> class.
        /// </summary>
        /// <param name="areaId">The area id.</param>
        /// <param name="name">The area name.</param>
        /// <param name="author">The author.</param>
        /// <param name="description">The description.</param>
        /// <param name="rooms">The room list.</param>
        public Area(int areaId, string? name, string? author, string? description, List<Room> rooms)
        {
            this.AreaId = areaId;
            this.Name = name;
            this.Author = author;
            this.Description = description;
            this.Rooms = rooms;
        }

        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.Int32)]
        [BsonElement("_id")]
        public int AreaId { get; set; }

        /// <summary>
        /// Gets or sets the area name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the area author.
        /// </summary>
        public string? Author { get; set; }

        /// <summary>
        /// Gets or sets the area description.
        /// </summary>
        public virtual string? Description { get; set; }

        /// <summary>
        /// Gets or sets rooms within the area.
        /// </summary>
        [Browsable(false)]
        public virtual List<Room> Rooms { get; set; } = new List<Room>();
    }
}