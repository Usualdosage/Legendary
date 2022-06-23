// <copyright file="Room.cs" company="Legendary">
//  Copyright © 2021-2022 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Core.Models
{
    using System.Collections.Generic;
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
        public static Room Default
        {
            get
            {
                return new Room() { RoomId = 1, AreaId = 1 };
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
        /// Gets or sets available exits.
        /// </summary>
        public IList<Exit> Exits { get; set; } = new List<Exit>();

        // TO-DO: FIX THIS PROBLEM

        /// <summary>
        /// Gets or sets the room flags.
        /// </summary>
        public object? Flags { get; set; }

        /// <summary>
        /// Gets or sets the room's terrain.
        /// </summary>
        public Terrain? Terrain { get; set; }

        /// <summary>
        /// Gets or sets the items in the room.
        /// </summary>
        public IList<Item> Items { get; set; } = new List<Item>();

        /// <summary>
        /// Gets or sets the mobiles in the room.
        /// </summary>
        public IList<Mobile> Mobiles { get; set; } = new List<Mobile>();

        /// <summary>
        /// Gets or sets the items reset in the room.
        /// </summary>
        public IList<long> ItemResets { get; set; } = new List<long>();

        /// <summary>
        /// Gets or sets the mobiles reset in the room.
        /// </summary>
        public IList<long> MobileResets { get; set; } = new List<long>();

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



