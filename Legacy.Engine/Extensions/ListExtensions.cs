// <copyright file="ListExtensions.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;

    /// <summary>
    /// Extensions for lists.
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Resolves a list of long (item IDs) to the actual Item objects.
        /// </summary>
        /// <param name="input">The items.</param>
        /// <param name="communicator">The communicator.</param>
        /// <returns>List of resolved items.</returns>
        public static List<Item> ResolveItems(this List<long> input, ICommunicator communicator)
        {
            List<Item> items = new List<Item>();

            input.ForEach(i =>
            {
                items.Add(communicator.ResolveItem(i));
            });

            return items;
        }
    }
}