// <copyright file="DataService.cs" company="Legendary">
//  Copyright © 2021 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Data
{
    using System;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Data.Contracts;

    /// <summary>
    /// Concrete implementation of an IDataService.
    /// </summary>
    public class DataService : IDataService
    {
        private readonly IDBConnection dbConnection;
        private readonly IApiClient apiClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataService"/> class.
        /// </summary>
        /// <param name="dbConnection">The database connection.</param>
        /// <param name="apiClient">The API client.</param>
        public DataService(IDBConnection dbConnection, IApiClient apiClient)
        {
            this.apiClient = apiClient;
            this.dbConnection = dbConnection;
        }

        /// <inheritdoc/>
        public World? LoadWorld()
        {
            var characters = this.dbConnection.Database?.GetCollection<Character>("Characters");
            var areas = this.dbConnection.Database?.GetCollection<Area>("Areas");
            var items = this.dbConnection.Database?.GetCollection<Item>("Items");

            if (areas != null && characters != null && items != null)
            {
                return new World(areas, characters, items, this.apiClient);
            }
            else
            {
                throw new Exception("Error loading world. No areas found.");
            }
        }
    }
}