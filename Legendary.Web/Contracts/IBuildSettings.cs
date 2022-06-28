// <copyright file="IBuildSettings.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Legendary.Web.Contracts
{
    using System;

    /// <summary>
    /// Displayed to the end user on the web interface.
    /// </summary>
    public interface IBuildSettings
    {
        /// <summary>
        /// Gets or sets the build version.
        /// </summary>
        string? Version { get; set; }

        /// <summary>
        /// Gets or sets the last release date.
        /// </summary>
        DateTime? ReleaseDate { get; set; }
    }
}
