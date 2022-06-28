// <copyright file="MongoConnection.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Data
{
    using System;
    using Legendary.Data.Contracts;
    using MongoDB.Driver;

    /// <summary>
    /// Concrete implementation of IDBConnection for accessing data within a Mongo Database.
    /// </summary>
    public class MongoConnection : IDBConnection
    {
        private readonly IDatabaseSettings databaseSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoConnection"/> class.
        /// </summary>
        /// <param name="databaseSettings">The database settings.</param>
        public MongoConnection(IDatabaseSettings databaseSettings)
        {
            if (databaseSettings == null)
            {
                throw new ArgumentNullException(nameof(databaseSettings));
            }

            this.databaseSettings = databaseSettings;
        }

        /// <summary>
        /// Gets or sets the database.
        /// </summary>
        public IMongoDatabase? Database { get; set; }

        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        public MongoClient? Client { get; set; }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Client = null;
            this.Database = null;
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public bool TestConnection()
        {
            try
            {
                var settings = MongoClientSettings.FromConnectionString(this.databaseSettings.ConnectionString);
                this.Client = new MongoClient(settings);
                this.Database = this.Client.GetDatabase(this.databaseSettings.DatabaseName);
                return this.Database != null;
            }
            catch
            {
                throw;
            }
        }
    }
}