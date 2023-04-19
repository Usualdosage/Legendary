// <copyright file="CompanionModel.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Web.Models
{
    /// <summary>
    /// Model for logging into the companion app.
    /// </summary>
    public class CompanionModel
    {
        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the persona name to chat with.
        /// </summary>
        public string Persona { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the URL to the persona's avatar.
        /// </summary>
        public string AvatarUrl { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the input message.
        /// </summary>
        public string Input { get; set; } = string.Empty;
    }
}
