// <copyright file="Alignment.cs" company="Legendary">
//  Copyright © 2021-2022 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Core.Types
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Trancs various metrics for a character.
    /// </summary>
    public class Metrics
	{
        /// <summary>
        /// Total number of deaths the player has had.
        /// </summary>
        public int TotalDeaths { get; set; } = 0;

        /// <summary>
        /// Total number of times the player has been killed by a mob.
        /// </summary>
        public int MobDeaths { get; set; } = 0;

        /// <summary>
        /// Total number of times the player has been killed by a player.
        /// </summary>
        public int PlayerDeaths { get; set; } = 0;

        /// <summary>
        /// Total number of players the character has killed.
        /// </summary>
        public int PlayerKills { get; set; } = 0;

        /// <summary>
        /// Total number of mobs the player has killed.
        /// </summary>
        public int MobKills { get; set; } = 0;

        /// <summary>
        /// Last login date.
        /// </summary>
        public DateTime LastLogin { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// First login date.
        /// </summary>
        public DateTime FirstLogin { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// IP addresses the player has logged in from.
        /// </summary>
        public List<string> IPAddresses { get; set; } = new List<string>();

        /// <summary>
        /// Tracks the player killed and number of times.
        /// </summary>
        public Dictionary<string, int> TotalKills { get; set; } = new Dictionary<string, int>();
    }
}

