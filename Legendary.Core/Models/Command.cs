// <copyright file="Command.cs" company="Legendary™">
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
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a JSON command that gets send to the server for processing from the client.
    /// </summary>
    public class Command
    {
        /// <summary>
        /// Gets or sets the JSON action.
        /// </summary>
        [JsonProperty("action")]
        public string? Action { get; set; }

        /// <summary>
        /// Gets or sets the JSON context.
        /// </summary>
        [JsonProperty("context")]
        public string? Context { get; set; }
    }
}