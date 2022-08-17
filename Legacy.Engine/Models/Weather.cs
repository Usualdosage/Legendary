// <copyright file="Weather.cs" company="Legendary™">
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
    /// Class to handle weather expressions.
    /// </summary>
    public class Weather
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Weather"/> class.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <param name="message">The message.</param>
        /// <param name="sound">The sound.</param>
        /// <param name="status">The status.</param>
        /// <param name="temp">The temperature.</param>
        public Weather(int order, string message, string sound, string status, int temp)
        {
            this.Order = order;
            this.Message = message;
            this.Sound = sound;
            this.Status = status;
            this.Temp = temp;
        }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the sound.
        /// </summary>
        public string Sound { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the temp.
        /// </summary>
        public int Temp { get; set; }
    }
}