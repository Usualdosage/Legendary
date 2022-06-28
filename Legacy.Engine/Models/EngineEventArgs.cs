// <copyright file="EngineEventArgs.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Models
{
    using System;

    /// <summary>
    /// Event args that are passed when an event occurs in the engine.
    /// </summary>
    public class EngineEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EngineEventArgs"/> class.
        /// </summary>
        /// <param name="gameHour">The game hour.</param>
        /// <param name="gameTicks">The game ticks.</param>
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
