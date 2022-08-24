// <copyright file="Emote.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Models
{
    /// <summary>
    /// Represents a player action.
    /// </summary>
    public class Emote
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Emote"/> class.
        /// </summary>
        /// <param name="toSelf">Message to self.</param>
        /// <param name="toRoom">Message to room.</param>
        /// <param name="toChar">Message with target.</param>
        /// <param name="selfToTarget">Message from self to target.</param>
        public Emote(string toSelf, string toRoom, string toChar, string selfToTarget)
        {
            this.ToSelf = toSelf;
            this.SelfToTarget = selfToTarget;
            this.ToRoom = toRoom;
            this.ToChar = toChar;
        }

        /// <summary>
        /// Gets the message a player sees when emoting.
        /// </summary>
        public string ToSelf { get; private set; }

        /// <summary>
        /// Gets the message the room sees when a player emotes.
        /// </summary>
        public string ToRoom { get; private set; }

        /// <summary>
        /// Gets the message everyone sees if there's a target.
        /// </summary>
        public string ToChar { get; private set; }

        /// <summary>
        /// Gets the message a player sees when emoting at a target.
        /// </summary>
        public string SelfToTarget { get; private set; }
    }
}