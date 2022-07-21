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
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    /// <summary>
    /// Metrics that are tracked and updated.
    /// </summary>
    public class GameMetrics
    {
        /// <summary>
        /// Gets or sets the object Id.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.Int64)]
        [BsonElement("_id")]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the startup date time (used to calculate uptime).
        /// </summary>
        public DateTime? LastStartupDateTime { get; set; }

        /// <summary>
        /// Gets or sets the current game day.
        /// </summary>
        public int CurrentDay { get; set; } = 1;

        /// <summary>
        /// Gets or sets the current game hour.
        /// </summary>
        public int CurrentHour { get; set; } = 0;

        /// <summary>
        /// Gets or sets the current game year.
        /// </summary>
        public int CurrentYear { get; set; } = 100;

        /// <summary>
        /// Gets or sets the current game month.
        /// </summary>
        public int CurrentMonth { get; set; } = 1;

        /// <summary>
        /// Gets or sets the last error encountered.
        /// </summary>
        public Exception? LastError { get; set; }

        /// <summary>
        /// Gets or sets the max players all time since start.
        /// </summary>
        public long MaxPlayers { get; set; } = 0;

        /// <summary>
        /// Gets or sets the total area count.
        /// </summary>
        public long TotalAreas { get; set; } = 0;

        /// <summary>
        /// Gets or sets the total room count.
        /// </summary>
        public long TotalRooms { get; set; } = 0;

        /// <summary>
        /// Gets or sets the total mobile count.
        /// </summary>
        public long TotalMobiles { get; set; } = 0;

        /// <summary>
        /// Gets or sets the total item count.
        /// </summary>
        public long TotalItems { get; set; } = 0;

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
        [BsonIgnore]
        public TimeSpan Uptime
        {
            get
            {
                if (this.LastStartupDateTime.HasValue)
                {
                    return DateTime.UtcNow.Subtract(this.LastStartupDateTime.Value);
                }
                else
                {
                    return TimeSpan.Zero;
                }
            }
        }
    }
}