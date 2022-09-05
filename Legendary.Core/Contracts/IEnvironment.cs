// <copyright file="IEnvironment.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Core.Contracts
{
    using System.Threading.Tasks;

    /// <summary>
    /// Contract for a player environment.
    /// </summary>
    public interface IEnvironment
    {
        /// <summary>
        /// Gets or sets a value indicating whether it is nighttime.
        /// </summary>
        public bool IsNight { get; set; }

        /// <summary>
        /// Processes changes to the connected user's environment each game hour.
        /// </summary>
        /// <param name="gameTicks">The game ticks.</param>
        /// <param name="gameHour">The hour of the game.</param>
        /// <returns>Task.</returns>
        Task ProcessEnvironmentChanges(int gameTicks, int gameHour);

        /// <summary>
        /// Randomly generates the weather for the world.
        /// </summary>
        void GenerateWeather();
    }
}
