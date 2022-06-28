﻿// <copyright file="ServerSettings.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Networking.Models
{
    using Legendary.Networking.Contracts;

    /// <summary>
    /// Used to house server settings loaded from a config file.
    /// </summary>
    public class ServerSettings : IServerSettings
    {
        /// <inheritdoc/>
        public string? ApiUrl { get; set; }

        /// <inheritdoc/>
        public int? ApiPort { get; set; }
    }
}