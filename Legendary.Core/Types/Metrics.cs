// <copyright file="Metrics.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Core.Types
{
    using System;
    using System.Collections.Generic;
    using MongoDB.Bson.Serialization.Attributes;

    /// <summary>
    /// Tracks various metrics for a character.
    /// </summary>
    [BsonIgnoreExtraElements]
    public class Metrics
    {
        /// <summary>
        /// Gets the total number of deaths the player has had.
        /// </summary>
        [BsonIgnore]
        public int TotalDeaths { get => this.MobDeaths + this.PlayerDeaths; }

        /// <summary>
        /// Gets or sets the total number of times the player has been killed by a mob.
        /// </summary>
        public int MobDeaths { get; set; } = 0;

        /// <summary>
        /// Gets or sets the total number of times the player has been killed by a player.
        /// </summary>
        public int PlayerDeaths { get; set; } = 0;

        /// <summary>
        /// Gets or sets the total number of players the character has killed.
        /// </summary>
        public int PlayerKills { get; set; } = 0;

        /// <summary>
        /// Gets or sets the total number of mobs the player has killed.
        /// </summary>
        public int MobKills { get; set; } = 0;

        /// <summary>
        /// Gets or sets the last login date.
        /// </summary>
        public DateTime LastLogin { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the first login date.
        /// </summary>
        public DateTime FirstLogin { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the the IP addresses the player has logged in from.
        /// </summary>
        public List<string> IPAddresses { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the total kills.
        /// </summary>
        public Dictionary<string, int> TotalKills { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Gets or sets the total game hours (ticks) played, to calculate age.
        /// </summary>
        public int GameHoursPlayed { get; set; }

        /// <summary>
        /// Gets or sets the rooms explored for each area.
        /// </summary>
        public Dictionary<long, List<long>> RoomsExplored { get; set; } = new Dictionary<long, List<long>>();
    }
}
