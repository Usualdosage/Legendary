// <copyright file="Server.cs" company="Legendary">
//  Copyright © 2021 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Networking
{
    using System.Threading.Tasks;
    using Legendary.Core.Contracts;
    using Legendary.Data.Contracts;
    using Legendary.Engine.Contracts;
    using Legendary.Networking.Contracts;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Server concrete implementation.
    /// </summary>
    public class Server : IServer
    {
        private readonly Engine.Engine engine;

        /// <summary>
        /// Initializes a new instance of the <see cref="Server"/> class.
        /// </summary>
        /// <param name="requestDelegate">The request delegate.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="connection">The database connection.</param>
        /// <param name="dataService">The data service.</param>
        /// <param name="apiClient">The API client.</param>
        public Server(RequestDelegate requestDelegate, ILogger logger, IDBConnection connection, IDataService dataService, IApiClient apiClient)
        {
            logger.Info("Legendary server is starting up...");
            this.engine = new Engine.Engine(requestDelegate, logger, connection, dataService, apiClient);
            this.engine.Start();
        }

        /// <inheritdoc/>
        public async Task Invoke(HttpContext context)
        {
            // TO-DO: Handle IP ban list
            await this.engine.Invoke(context);
        }
    }
}


