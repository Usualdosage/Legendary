// <copyright file="JSONExtensions.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Core.Extensions
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Extensions for handling JSON objects.
    /// </summary>
    public static class JSONExtensions
    {
        /// <summary>
        /// Strips out circular references before BSON serializing to Mongo.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="obj">The object.</param>
        /// <returns>The generic type.</returns>
        public static T RemoveCircularReferences<T>(this T obj)
        {
            var stringContent = JsonConvert.SerializeObject(obj, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }).ToString();
            return JsonConvert.DeserializeObject<T>(stringContent);
        }
    }
}