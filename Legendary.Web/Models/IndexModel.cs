// <copyright file="IndexModel.cs" company="Legendary™">
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
    using Legendary.Core.Models;
    using Legendary.Web.Contracts;

    /// <summary>
    /// Contains the needed properties to render on the Index view.
    /// </summary>
    public class IndexModel
    {
        /// <summary>
        /// Gets or sets the game metrics.
        /// </summary>
        public GameMetrics? GameMetrics { get; set; }

        /// <summary>
        /// Gets or sets the build settings.
        /// </summary>
        public IBuildSettings? BuildSettings { get; set; }
    }
}