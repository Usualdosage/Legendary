// <copyright file="Parser.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Handles processing of the Parser.json file.
    /// </summary>
    public class Parser
    {
        /// <summary>
        /// Gets or sets the exclude words.
        /// </summary>
        [JsonProperty("exclude")]
        public List<string>? Exclude { get; set; }

        /// <summary>
        /// Gets or sets the replace words.
        /// </summary>
        [JsonProperty("replace")]
        public Dictionary<string, string>? Replace { get; set; }
    }
}