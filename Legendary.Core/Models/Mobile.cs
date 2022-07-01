// <copyright file="Mobile.cs" company="Legendary™">
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
    using Legendary.Core.Types;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    /// <summary>
    /// Represents a single mobile (NPC).
    /// </summary>
    [BsonIgnoreExtraElements]
    public class Mobile : Character
    {
        ///// <summary>
        ///// Gets or sets the ID of the mobile.
        ///// </summary>
        ///// <summary>
        ///// Gets or sets the Id.
        ///// </summary>
        //[BsonId]
        //[BsonRepresentation(BsonType.Int64)]
        //[BsonElement("_id")]
        //public override CharacterId{ get; set; }

        /// <inheritdoc/>
        public override bool IsNPC { get => true; set => base.IsNPC = true; }

        /// <summary>
        /// Gets or sets flags applied to the mobile.
        /// </summary>
        public IList<MobileFlags>? MobileFlags { get; set; }
    }
}
