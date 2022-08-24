// <copyright file="Exit.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Core.Models
{
    using Legendary.Core.Types;

    /// <summary>
    /// Represents an exit in a room.
    /// </summary>
    public class Exit
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Exit"/> class.
        /// </summary>
        /// <param name="direction">The direction.</param>
        /// <param name="toArea">To area.</param>
        /// <param name="toRoom">To room.</param>
        public Exit(Direction direction, int toArea, long toRoom)
        {
            this.Direction = direction;
            this.ToArea = toArea;
            this.ToRoom = toRoom;
        }

        /// <summary>
        /// Gets the direction of this exit.
        /// </summary>
        public Direction Direction { get; private set; }

        /// <summary>
        /// Gets the area this exit goes to.
        /// </summary>
        public int ToArea { get; private set; }

        /// <summary>
        /// Gets the room this exit goes to.
        /// </summary>
        public long ToRoom { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is a door.
        /// </summary>
        public bool IsDoor { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether this door is closed.
        /// </summary>
        public bool IsClosed { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether this door is trapped.
        /// </summary>
        public bool IsTrapped { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether this door is hidden.
        /// </summary>
        public bool IsHidden { get; set; } = false;

        /// <summary>
        /// Gets or sets the trap type.
        /// </summary>
        public TrapType? TrapType { get; set; }

        /// <summary>
        /// Gets or sets the name of the door.
        /// </summary>
        public string? DoorName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this door is locked.
        /// </summary>
        public bool IsLocked { get; set; } = false;

        /// <summary>
        /// Gets or sets the id of the key that will open this door.
        /// </summary>
        public long? KeyId { get; set; }
    }
}