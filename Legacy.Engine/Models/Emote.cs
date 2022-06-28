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
        public Emote(string toSelf, string toRoom)
        {
            this.ToSelf = toSelf;
            this.ToRoom = toRoom;
        }

        /// <summary>
        /// Gets the message a player sees when emoting.
        /// </summary>
        public string ToSelf { get; private set; }

        /// <summary>
        /// Gets the message the room sees when a player emotes.
        /// </summary>
        public string ToRoom { get; private set; }
    }
}