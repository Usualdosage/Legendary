// <copyright file="MIRPEventArgs.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Types
{
    using System;
    using Legendary.Core.Models;

    /// <summary>
    /// Mob-Item-Room Program Event Arguments.
    /// </summary>
    public class MIRPEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the player.
        /// </summary>
        public Character? Player { get; set; }

        /// <summary>
        /// Gets or sets the mobile.
        /// </summary>
        public Mobile? Mobile { get; set; }

        /// <summary>
        /// Gets or sets the item.
        /// </summary>
        public Item? Item { get; set; }

        /// <summary>
        /// Gets or sets the room.
        /// </summary>
        public Room? Room { get; set; }

        /// <summary>
        /// Gets or sets the area.
        /// </summary>
        public Area? Area { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        public string? Message { get; set; }
    }
}
