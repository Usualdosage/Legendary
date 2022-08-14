// <copyright file="Award.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Core.Types
{
    /// <summary>
    /// Represents an award a player can earn.
    /// </summary>
    public class Award
    {
        /// <summary>
        /// Gets or sets the award level.
        /// </summary>
        public AwardLevel AwardLevel { get; set; }

        /// <summary>
        /// Gets or sets the award title.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the award content.
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// Gets or sets the experience per award level.
        /// </summary>
        public int ExperiencePerLevel { get; set; }

        /// <summary>
        /// Gets or sets the award image.
        /// </summary>
        public string? AwardImage { get; set; }
    }
}