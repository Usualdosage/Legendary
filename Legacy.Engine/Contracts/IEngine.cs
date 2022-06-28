// <copyright file="IEngine.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Contracts
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Implementation contract for a server engine.
    /// </summary>
    public interface IEngine
    {
        /// <summary>
        /// Raises an event every second.
        /// </summary>
        event EventHandler? VioTick;

        /// <summary>
        /// Raises an event every 30 seconds.
        /// </summary>
        event EventHandler? Tick;

        /// <summary>
        /// Raises an event when the engine is updated.
        /// </summary>
        event EventHandler? EngineUpdate;

        /// <summary>
        /// Starts the engine.
        /// </summary>
        /// <returns>Task.</returns>
        Task Initialize();
    }
}
