﻿// <copyright file="Room.cs" company="Legendary™">
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
    using Legendary.Core.Types;
    using MongoDB.Bson.Serialization.Attributes;

    /// <summary>
    /// Represents a single room within an area.
    /// </summary>
    [BsonIgnoreExtraElements]
    public class Room
    {
        /// <summary>
        /// Gets the default room (for new characters).
        /// </summary>
        public static KeyValuePair<long, long> Default
        {
            get
            {
                return new KeyValuePair<long, long>(1, 1);
            }
        }

        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        /// <summary>
        /// Gets or sets loan number. As this is the primary key, it will never be null.
        /// </summary>
        public long RoomId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the area this room belongs to.
        /// </summary>
        public int AreaId { get; set; }

        /// <summary>
        /// Gets or sets the name of the room.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the room.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the image for the room.
        /// </summary>
        public string? Image { get; set; }

        /// <summary>
        /// Gets or sets the max number of mobs that will autospawn in this room.
        /// </summary>
        public int? MaxAutospawn { get; set; }

        /// <summary>
        /// Gets or sets the video for the room.
        /// </summary>
        public string? Video { get; set; }

        /// <summary>
        /// Gets or sets the watch keyword for the video.
        /// </summary>
        public string? WatchKeyword { get; set; }

        /// <summary>
        /// Gets or sets available exits.
        /// </summary>
        public List<Exit> Exits { get; set; } = new List<Exit>();

        /// <summary>
        /// Gets or sets the room flags.
        /// </summary>
        public List<RoomFlags>? Flags { get; set; } = new List<RoomFlags>();

        /// <summary>
        /// Gets or sets the room's terrain.
        /// </summary>
        public Terrain Terrain { get; set; } = Terrain.City;

        /// <summary>
        /// Gets or sets the items in the room.
        /// </summary>
        public List<Item> Items { get; set; } = new List<Item>();

        /// <summary>
        /// Gets or sets the mobiles in the room.
        /// </summary>
        public List<Mobile> Mobiles { get; set; } = new List<Mobile>();

        /// <summary>
        /// Gets or sets the items reset in the room.
        /// </summary>
        public List<long> ItemResets { get; set; } = new List<long>();

        /// <summary>
        /// Gets or sets the mobiles reset in the room.
        /// </summary>
        public List<long> MobileResets { get; set; } = new List<long>();

        /// <summary>
        /// Gets or sets the program.
        /// </summary>
        public string? Program { get; set; }

        /// <summary>
        /// Determines whether one room is equal to another matching on area and room IDs.
        /// </summary>
        /// <param name="obj">The room.</param>
        /// <returns>True if the same.</returns>
        public override bool Equals(object? obj)
        {
            if (obj == null)
            {
                return false;
            }
            else if (obj is Room room)
            {
                return room.RoomId == this.RoomId && room.AreaId == this.AreaId;
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
