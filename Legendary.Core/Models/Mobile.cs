// <copyright file="Mobile.cs" company="Legendary">
//  Copyright © 2021 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Core.Models
{
    using Legendary.Core.Types;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    /// <summary>
    /// Represents a single mobile (NPC).
    /// </summary>
    [BsonIgnoreExtraElements]
    public class Mobile : Character
    {
        /// <summary>
        /// Gets or sets the ID of the mobile.
        /// </summary>
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.Int64)]
        [BsonElement("_id")]
        public long MobileId { get; set; }

        /// <summary>
        /// Gets or sets flags applied to the mobile.
        /// </summary>
        public new MobileFlags Flags { get; set; } = MobileFlags.None;

        /// <summary>
        /// Gets a value indicating whether this is an NPC. Always returns true.
        /// </summary>
        public override bool IsNPC { get => true; }
    }
}



