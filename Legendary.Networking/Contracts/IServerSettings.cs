// <copyright file="IServerSettings.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Networking.Contracts
{
    /// <summary>
    /// Implementation contract for server settings.
    /// </summary>
    public interface IServerSettings
    {
        /// <summary>
        /// Gets or sets the URL of the content API.
        /// </summary>
        string? ApiUrl { get; set; }

        /// <summary>
        /// Gets or sets the port of the content API.
        /// </summary>
        int? ApiPort { get; set; }
    }
}