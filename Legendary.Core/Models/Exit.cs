// <copyright file="Exit.cs" company="Legendary">
//  Copyright © 2021-2022 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
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
    }
}