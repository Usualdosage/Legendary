// <copyright file="Character.cs" company="Legendary">
//  Copyright © 2021 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Core.Models
{
    using System.Collections.Generic;
    using Legendary.Core.Types;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    /// <summary>
    /// Represents a single character.
    /// </summary>
    [BsonIgnoreExtraElements]
    public class Character
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.Int64)]
        [BsonElement("_id")]
        public long CharacterId { get; set; }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// Gets or sets the middle name.
        /// </summary>
        public string? MiddleName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        public string? LastName { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the level.
        /// </summary>
        public int Level { get; set; } = 1;

        /// <summary>
        /// Gets or sets the experience.
        /// </summary>
        public long Experience { get; set; } = 0;

        /// <summary>
        /// Gets or sets the currency.
        /// </summary>
        public long Currency { get; set; } = 0;

        /// <summary>
        /// Gets or sets the password (hash).
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        public Room Location { get; set; } = new Room() { AreaId = 1, RoomId = 1 };

        /// <summary>
        /// Gets or sets a value indicating whether this is an NPC.
        /// </summary>
        public virtual bool IsNPC { get; set; } = false;

        /// <summary>
        /// Gets or sets the character flags.
        /// </summary>
        public IList<CharacterFlags>? CharacterFlags { get; set; }

        /// <summary>
        /// Gets or sets the health.
        /// </summary>
        public MaxCurrent Health { get; set; } = new MaxCurrent(0, 0);

        /// <summary>
        /// Gets or sets the mana.
        /// </summary>
        public MaxCurrent Mana { get; set; } = new MaxCurrent(0, 0);

        /// <summary>
        /// Gets or sets the movement.
        /// </summary>
        public MaxCurrent Movement { get; set; } = new MaxCurrent(0, 0);

        /// <summary>
        /// Gets or sets the inventory.
        /// </summary>
        public IList<Item> Inventory { get; set; } = new List<Item>();

        /// <summary>
        /// Gets or sets the player's strength.
        /// </summary>
        public int Str { get; set; } = 12;

        /// <summary>
        /// Gets or sets the player's intelligence.
        /// </summary>
        public int Int { get; set; } = 12;

        /// <summary>
        /// Gets or sets the player's wisdom.
        /// </summary>
        public int Wis { get; set; } = 12;

        /// <summary>
        /// Gets or sets the player's dexterity.
        /// </summary>
        public int Dex { get; set; } = 12;

        /// <summary>
        /// Gets or sets the player's constitution.
        /// </summary>
        public int Con { get; set; } = 12;

        /// <summary>
        /// Gets or sets the inventory.
        /// </summary>
        public IList<Skill> Skills { get; set; } = new List<Skill>();

        /// <summary>
        /// Gets or sets the inventory.
        /// </summary>
        public IList<Spell> Spells { get; set; } = new List<Spell>();
    }
}