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
    using MongoDB.Bson.Serialization.Attributes;

    /// <summary>
    /// Represents a single mobile (NPC).
    /// </summary>
    [BsonIgnoreExtraElements]
    public class Mobile : Character
    {
        /// <inheritdoc/>
        public override bool IsNPC { get => true; set => base.IsNPC = true; }

        /// <summary>
        /// Gets or sets flags applied to the mobile.
        /// </summary>
        public IList<MobileFlags>? MobileFlags { get; set; }

        /// <summary>
        /// Gets or sets the personality of the mobile.
        /// </summary>
        public Emotion? Emotion { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not this mobile should use the AI language processor.
        /// </summary>
        public bool UseAI { get; set; } = true;

        /// <summary>
        /// Gets or sets the persona file to consume if this mbile uses AI.
        /// </summary>
        public string? PersonaFile { get; set; }

        /// <summary>
        /// Gets or sets the handle which player the mob is speaking with, tracking, and fighting.
        /// </summary>
        public string? PlayerTarget { get; set; }

        /// <summary>
        /// Gets or sets the equipment resets for the mobile.
        /// </summary>
        public List<EquipmentReset> EquipmentResets { get; set; } = new List<EquipmentReset>();

        /// <summary>
        /// Gets or sets the school type if this mob is a teacher.
        /// </summary>
        public SchoolType? SchoolType { get; set; }

        /// <summary>
        /// Gets or sets the hit dice.
        /// </summary>
        public override int HitDice { get; set; }

        /// <summary>
        /// Gets or sets the damage dice.
        /// </summary>
        public override int DamageDice { get; set; }

        /// <summary>
        /// Gets or sets the XImage.
        /// </summary>
        public string? XImage { get; set; }

        /// <summary>
        /// Gets or sets the XActive.
        /// </summary>
        public bool? XActive { get; set; }
    }
}
