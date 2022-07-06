// <copyright file="StatModel.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Web.Models
{
    using System;

    /// <summary>
    /// Model for generating character stats.
    /// </summary>
    [Serializable]
    public class StatModel
    {
        /// <summary>
        /// Gets or sets the strength.
        /// </summary>
        public int Str { get; set; } = 12;

        /// <summary>
        /// Gets or sets the wisdom.
        /// </summary>
        public int Wis { get; set; } = 12;

        /// <summary>
        /// Gets or sets the intelligence.
        /// </summary>
        public int Int { get; set; } = 12;

        /// <summary>
        /// Gets or sets the dexterity.
        /// </summary>
        public int Dex { get; set; } = 12;

        /// <summary>
        /// Gets or sets the constitution.
        /// </summary>
        public int Con { get; set; } = 12;

        /// <summary>
        /// Gets or sets the model message.
        /// </summary>
        public string? Message { get; set; }
    }
}