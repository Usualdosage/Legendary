// <copyright file="Conversation.cs" company="Legendary™">
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
    using Legendary.Core.Types;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Represents a conversation within a language processor type.
    /// </summary>
    public class Conversation
    {
        /// <summary>
        /// Gets or sets the emotion.
        /// </summary>
        [JsonProperty("emotion", ItemConverterType = typeof(StringEnumConverter))]
        public IList<Emotion?> Emotion { get; set; } = new List<Emotion?>();

        /// <summary>
        /// Gets or sets the available messages.
        /// </summary>
        [JsonProperty("messages")]
        public IList<string> Messages { get; set; } = new List<string>();
    }
}