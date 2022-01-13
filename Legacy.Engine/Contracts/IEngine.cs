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
    using Legendary.Core.Contracts;
    using Legendary.Data.Contracts;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Implementation contract for a server engine.
    /// </summary>
    public interface IEngine : IDisposable
    {
        /// <summary>
        /// Gets the World.
        /// </summary>
        IWorld? World { get; }

        /// <summary>
        /// Tests the database connection.
        /// </summary>
        void TestConnection();

        /// <summary>
        /// Invokes HTTP handlers.
        /// </summary>
        /// <param name="context">The HttpContext.</param>
        /// <returns>Task.</returns>
        Task Invoke(HttpContext context);

        /// <summary>
        /// Terminates the engine.
        /// </summary>
        void Terminate();

        /// <summary>
        /// Starts or restarts the engine.
        /// </summary>
        /// <returns>Task.</returns>
        Task Start();
    }
}



