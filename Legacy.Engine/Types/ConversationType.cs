// <copyright file="ConversationType.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Types
{
    using System.Collections.Generic;
    using Legendary.Engine.Models;

    /// <summary>
    /// Represents the type of conversation triggered by an input word.
    /// </summary>
    public class ConversationType
    {
        /// <summary>
        /// Gets or sets the input words.
        /// </summary>
        public IList<string> Input { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the output words.
        /// </summary>
        public IList<Conversation> Output { get; set; } = new List<Conversation>();
    }
}