// <copyright file="EngineEventArgs.cs" company="Legendary">
//  Copyright © 2021-2022 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Engine.Models
{
    using System;

    public class EngineEventArgs : EventArgs
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationEventArgs"/> class.
        /// </summary>
        /// <param name="gameHour">The game hour</param>
        /// <param name="gameTicks">The game ticks</param>
        /// <param name="message">The message.</param>
        public EngineEventArgs(int gameTicks, int gameHour, string? message)
        {
            this.GameTicks = gameTicks;
            this.GameHour = gameHour;
            this.GameMessage = message;
        }

        /// <summary>
        /// Gets the game hour.
        /// </summary>
        public int GameHour { get; private set; }

        /// <summary>
        /// Gets the game ticks.
        /// </summary>
        public int GameTicks { get; private set; }

        /// <summary>
        /// Gets the message.
        /// </summary>
        public string? GameMessage { get; private set; }
    }
}

