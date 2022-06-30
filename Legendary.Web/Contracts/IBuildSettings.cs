// <copyright file="IBuildSettings.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
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
