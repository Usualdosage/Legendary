// <copyright file="LanguageModel.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Models
{
    using Legendary.Engine.Types;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a language model which is populated from a set of JSON training data.
    /// </summary>
    public class LanguageModel
    {
        /// <summary>
        /// Gets or sets the conversations.
        /// </summary>
        [JsonProperty("greeting")]
        public ConversationType? Greeting { get; set; }

        /// <summary>
        /// Gets or sets the conversations.
        /// </summary>
        [JsonProperty("departure")]
        public ConversationType? Departure { get; set; }

        /// <summary>
        /// Gets or sets the conversations.
        /// </summary>
        [JsonProperty("conversation")]
        public ConversationType? Conversation { get; set; }
    }
}