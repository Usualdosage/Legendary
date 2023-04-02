// <copyright file="OutputMessage.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Models.Output
{
    using System;
    using Legendary.Core.Models;
    using Newtonsoft.Json;

    /// <summary>
    /// This message is sent to the client each time there is an update to the controls on the page.
    /// </summary>
    public class OutputMessage
    {
        /// <summary>
        /// Gets or sets the message body.
        /// </summary>
        [JsonProperty("m")]
        public Message? Message { get; set; }

        /// <summary>
        /// Gets or sets the message type.
        /// </summary>
        [JsonProperty("type")]
        public string? Type { get; set; } = "update";
    }

    /// <summary>
    /// Main message body.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Compiled together in single JSON object.")]
    public class Message
    {
        /// <summary>
        /// Gets or sets the first name of the player.
        /// </summary>
        [JsonProperty("f")]
        public string? FirstName { get; set; }

        /// <summary>
        /// Gets or sets the title of the player.
        /// </summary>
        [JsonProperty("t")]
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the alignment of the player.
        /// </summary>
        [JsonProperty("a")]
        public string? Alignment { get; set; }

        /// <summary>
        /// Gets or sets the level of the player.
        /// </summary>
        [JsonProperty("l")]
        public int Level { get; set; }

        /// <summary>
        /// Gets or sets the condition of the player.
        /// </summary>
        [JsonProperty("c")]
        public string? Condition { get; set; }

        /// <summary>
        /// Gets or sets vital statistics.
        /// </summary>
        [JsonProperty("s")]
        public StatMessage? Stats { get; set; }

        /// <summary>
        /// Gets or sets the weather.
        /// </summary>
        [JsonProperty("w")]
        public Weather? Weather { get; set; }

        /// <summary>
        /// Gets or sets the current image info.
        /// </summary>
        [JsonProperty("i")]
        public ImageInfo? ImageInfo { get; set; }

        /// <summary>
        /// Gets or sets the map data.
        /// </summary>
        [JsonProperty("m")]
        public Map? Map { get; set; }
    }

    /// <summary>
    /// Messages that contains current user stats.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Compiled together in single JSON object.")]
    public class StatMessage
    {
        /// <summary>
        /// Gets or sets health.
        /// </summary>
        [JsonProperty("hp")]
        public Status? Health { get; set; }

        /// <summary>
        /// Gets or sets mana.
        /// </summary>
        [JsonProperty("mm")]
        public Status? Mana { get; set; }

        /// <summary>
        /// Gets or sets movement.
        /// </summary>
        [JsonProperty("mv")]
        public Status? Movement { get; set; }

        /// <summary>
        /// Gets or sets experience.
        /// </summary>
        [JsonProperty("exp")]
        public Status? Experience { get; set; }
    }

    /// <summary>
    /// Message that contains current weather.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Compiled together in single JSON object.")]
    public class Weather
    {
        /// <summary>
        /// Gets or sets time.
        /// </summary>
        [JsonProperty("tm")]
        public string? Time { get; set; }

        /// <summary>
        /// Gets or sets the temp.
        /// </summary>
        [JsonProperty("tp")]
        public string? Temp { get; set; }

        /// <summary>
        /// Gets or sets the weather image.
        /// </summary>
        [JsonProperty("img")]
        public string? Image { get; set; }
    }

    /// <summary>
    /// Message that contains area and room info.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Compiled together in single JSON object.")]
    public class ImageInfo
    {
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        [JsonProperty("c")]
        public string? Caption { get; set; }

        /// <summary>
        /// Gets or sets the area image.
        /// </summary>
        [JsonProperty("img")]
        public string? Image { get; set; }
    }

    /// <summary>
    /// Status container.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Compiled together in single JSON object.")]
    public class Status
    {
        /// <summary>
        /// Gets or sets the max.
        /// </summary>
        [JsonProperty("m")]
        public double Max { get; set; }

        /// <summary>
        /// Gets or sets the current.
        /// </summary>
        [JsonProperty("c")]
        public double Current { get; set; }

        /// <summary>
        /// Gets the percentage.
        /// </summary>
        [JsonProperty("p")]
        public int Percentage => (int)Math.Round((this.Current / this.Max) * 100);
    }

    /// <summary>
    /// Map container.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Compiled together in single JSON object.")]
    public class Map
    {
        /// <summary>
        /// Gets or sets the current room.
        /// </summary>
        [JsonProperty("c")]
        public long Current { get; set; }

        /// <summary>
        /// Gets or sets the map context.
        /// </summary>
        [JsonProperty("r")]
        public dynamic[]? Rooms { get; set; }

        /// <summary>
        /// Gets or sets the mobs in the area.
        /// </summary>
        [JsonProperty("m")]
        public dynamic[]? MobsInArea { get; set; }

        /// <summary>
        /// Gets or sets the players in the area.
        /// </summary>
        [JsonProperty("p")]
        public dynamic[]? PlayersInArea { get; set; }

        /// <summary>
        /// Gets or sets the exploration percentage of the area.
        /// </summary>
        [JsonProperty("x")]
        public double PercentageExplored { get; set; }
    }
}