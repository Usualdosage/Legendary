// <copyright file="EngineEventArgs.cs" company="Legendary">
//  Copyright © 2022 Legendary
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
        /// <param name="socketId">The socket Id.</param>
        /// <param name="message">The message.</param>
        public EngineEventArgs(int gameTicks, int gameHour)
        {
            this.GameTicks = gameTicks;
            this.GameHour = gameHour;
        }

        /// <summary>
        /// Gets the message.
        /// </summary>
        public int GameHour { get; private set; }

        /// <summary>
        /// Gets the message.
        /// </summary>
        public int GameTicks { get; private set; }
    }
}

