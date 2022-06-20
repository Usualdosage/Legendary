// <copyright file="IEngine.cs" company="Legendary">
//  Copyright © 2021 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
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
        /// Starts the engine.
        /// </summary>
        /// <returns>Task.</returns>
        Task Initialize();

        /// <summary>
        /// Raises an event every second.
        /// </summary>
        event EventHandler? VioTick;

        /// <summary>
        /// Raises an event every 30 seconds.
        /// </summary>
        event EventHandler? Tick;
    }
}



