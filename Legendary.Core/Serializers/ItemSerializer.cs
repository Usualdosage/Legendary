// <copyright file="ItemSerializer.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Core.Serializers
{
    using System;
    using System.Collections.Generic;
    using Legendary.Core.Models;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization;
    using MongoDB.Bson.Serialization.Serializers;
    using Newtonsoft.Json;

    /// <summary>
    /// Custom serializer for items that have circular references (e.g. an Item containing a list of Items).
    /// </summary>
    public class ItemSerializer : SerializerBase<Item>
    {
        /// <inheritdoc/>
        public override Item Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            try
            {
                var serializer = BsonSerializer.LookupSerializer(typeof(BsonDocument));
                var document = serializer.Deserialize(context, args);

                var bsonDocument = document.ToBsonDocument();

                var result = BsonExtensionMethods.ToJson(bsonDocument, new MongoDB.Bson.IO.JsonWriterSettings() { OutputMode = MongoDB.Bson.IO.JsonOutputMode.Strict });
                return JsonConvert.DeserializeObject<Item>(result, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Serialize });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <inheritdoc/>
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Item value)
        {
            try
            {
                var jsonDocument = JsonConvert.SerializeObject(value, Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Serialize });
                var bsonDocument = BsonSerializer.Deserialize<BsonDocument>(jsonDocument);

                var serializer = BsonSerializer.LookupSerializer(typeof(BsonDocument));
                serializer.Serialize(context, bsonDocument.AsBsonValue);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}