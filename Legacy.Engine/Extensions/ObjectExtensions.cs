// <copyright file="ObjectExtensions.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Extensions
{
    using Legendary.Core.Models;
    using Newtonsoft.Json;

    /// <summary>
    /// Extensions of various objects.
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Makes a clone (deep copy) of the mobile.
        /// </summary>
        /// <param name="mobile">The mobile.</param>
        /// <returns>Mobile.</returns>
        public static Mobile DeepCopy(this Mobile mobile)
        {
            var json = JsonConvert.SerializeObject(mobile);
            return JsonConvert.DeserializeObject<Mobile>(json);
        }

        /// <summary>
        /// Makes a clone (deep copy) of the item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>Item.</returns>
        public static Item DeepCopy(this Item item)
        {
            var json = JsonConvert.SerializeObject(item);
            return JsonConvert.DeserializeObject<Item>(json);
        }
    }
}