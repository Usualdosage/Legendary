// <copyright file="BuildSettings.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Legendary.Web.Models
{
    using System;
    using Legendary.Web.Contracts;

    /// <summary>
    /// Displayed to the end user on the web interface.
    /// </summary>
    public class BuildSettings : IBuildSettings
    {
        /// <summary>
        /// Gets or sets the build version.
        /// </summary>
        public string? Version { get; set; }

        /// <summary>
        /// Gets or sets the last release date.
        /// </summary>
        public DateTime? ReleaseDate { get; set; }
    }
}
