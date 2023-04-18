// <copyright file="MongoService.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.AreaBuilder.Services
{
    using Legendary.Core.Models;
    using MongoDB.Driver;
    using Item = Legendary.AreaBuilder.Types.Item;
    using Mobile = Legendary.AreaBuilder.Types.Mobile;

    /// <summary>
    /// Reads and writes objects to Mongo.
    /// </summary>
    public class MongoService
    {
        private readonly string connectionString = "mongodb+srv://legacyadmin:8PoB23w2iMiMGyK5@legacy.uigz5.mongodb.net/Legacy?authSource=admin&replicaSet=atlas-nq55dl-shard-0&readPreference=primary&appname=MongoDB%20Compass&ssl=true";

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoService"/> class.
        /// </summary>
        public MongoService()
        {
            try
            {
                MongoClient dbClient = new (this.connectionString);
                var database = dbClient.GetDatabase("Legacy");
                this.Mobiles = database.GetCollection<Mobile>("Mobiles");
                this.Areas = database.GetCollection<Area>("Areas");
                this.Items = database.GetCollection<Item>("Items");
                this.Memory = database.GetCollection<Memory>("Memory");
                this.Personas = database.GetCollection<Persona>("Personas");
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Gets the mobiles collection.
        /// </summary>
        public IMongoCollection<Mobile> Mobiles { get; private set; }

        /// <summary>
        /// Gets the areas collection.
        /// </summary>
        public IMongoCollection<Area> Areas { get; private set; }

        /// <summary>
        /// Gets the items collection.
        /// </summary>
        public IMongoCollection<Item> Items { get; private set; }

        /// <summary>
        /// Gets the memory collection.
        /// </summary>
        public IMongoCollection<Memory> Memory { get; private set; }

        /// <summary>
        /// Gets the persona collection.
        /// </summary>
        public IMongoCollection<Persona> Personas { get; private set; }
    }
}
