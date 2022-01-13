// <copyright file="Emote.cs" company="Legendary">
//  Copyright © 2021 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
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