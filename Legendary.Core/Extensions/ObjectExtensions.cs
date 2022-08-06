// <copyright file="ObjectExtensions.cs" company="Legendary™">
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
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Object extensions.
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Deep clones an object, preserving reference loops.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="obj">The object.</param>
        /// <returns>Clone of object.</returns>
        public static T Clone<T>(this T obj)
        {
            string serialized = JsonConvert.SerializeObject(obj);
            return JsonConvert.DeserializeObject<T>(serialized, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }

        /// <summary>
        /// Determines if two players are in the same room (and in the same area).
        /// </summary>
        /// <param name="current">The current player.</param>
        /// <param name="target">The target player.</param>
        /// <returns>True if in same room.</returns>
        public static bool InSamePlace(this KeyValuePair<long, long> current, KeyValuePair<long, long> target)
        {
            return current.Key == target.Key && current.Value == target.Value;
        }
    }
}