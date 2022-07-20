// <copyright file="GameMetrics.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Core.Models
{
    using System;

    /// <summary>
    /// Metrics that are tracked and updated.
    /// </summary>
    public class GameMetrics
    {
        /// <summary>
        /// Gets or sets the startup date time (used to calculate uptime).
        /// </summary>
        public DateTime LastStartupDateTime { get; set; }

        /// <summary>
        /// Gets or sets the current game day.
        /// </summary>
        public int CurrentDay { get; set; } = 1;

        /// <summary>
        /// Gets or sets the current game hour.
        /// </summary>
        public int CurrentHour { get; set; } = 1;

        /// <summary>
        /// Gets or sets the current game year.
        /// </summary>
        public int CurrentYear { get; set; } = 1;

        /// <summary>
        /// Gets or sets the last error encountered.
        /// </summary>
        public Exception? LastError { get; set; }

        /// <summary>
        /// Gets or sets the max players all time since start.
        /// </summary>
        public int MaxPlayers { get; set; } = 0;

        /// <summary>
        /// Gets or sets the total area count.
        /// </summary>
        public int TotalAreas { get; set; } = 0;

        /// <summary>
        /// Gets or sets the total room count.
        /// </summary>
        public int TotalRooms { get; set; } = 0;

        /// <summary>
        /// Gets or sets the total mobile count.
        /// </summary>
        public int TotalMobiles { get; set; } = 0;

        /// <summary>
        /// Gets or sets the total item count.
        /// </summary>
        public int TotalItems { get; set; } = 0;

        /// <summary>
        /// Gets or sets the player with the most player kills.
        /// </summary>
        public string? MostKills { get; set; }

        /// <summary>
        /// Gets or sets the host URL.
        /// </summary>
        public string? HostURL { get; set; }

        /// <summary>
        /// Gets the total uptime.
        /// </summary>
        public TimeSpan Uptime
        {
            get
            {
                return DateTime.UtcNow.Subtract(this.LastStartupDateTime);
            }
        }
    }
}