// <copyright file="Area.cs" company="Legendary">
//  Copyright © 2021-2022 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Core.Models
{
    using System.Collections.Generic;
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
        public Area(int areaId, string name, string author, string description, IList<Room> rooms)
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
        public string Author { get; set; }

        /// <summary>
        /// Gets or sets the area description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets rooms within the area.
        /// </summary>
        public IList<Room> Rooms { get; set; }
    }
}