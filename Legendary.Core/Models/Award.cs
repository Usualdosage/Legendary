// <copyright file="Award.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Core.Models
{
    /// <summary>
    /// Represents an award that a player can earn.
    /// </summary>
    public class Award
    {
        /// <summary>
        /// Gets or sets the name of the award.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the award level.
        /// </summary>
        public int AwardLevel { get; set; } = 0;

        /// <summary>
        /// Gets or sets the image.
        /// </summary>
        public string? Image { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the experience per level.
        /// </summary>
        public int ExperiencePerLevel { get; set; } = 0;

        /// <summary>
        /// Gets or sets the rank or order of the award.
        /// </summary>
        public int Order { get; set; } = 0;
    }
}