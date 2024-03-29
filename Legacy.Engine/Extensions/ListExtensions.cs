﻿// <copyright file="ListExtensions.cs" company="Legendary™">
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
            List<Item> items = new ();

            input.ForEach(i =>
            {
                items.Add(communicator.ResolveItem(i));
            });

            return items;
        }

        /// <summary>
        /// Adds an effect to a player if they are not already affected by it.
        /// </summary>
        /// <param name="input">The effects.</param>
        /// <param name="effect">The effect.</param>
        public static void AddIfNotAffected(this List<Effect> input, Effect effect)
        {
            if (input.Any(e => e.Name?.ToLower() == effect.Name?.ToLower()))
            {
                return;
            }
            else
            {
                input.Add(effect);
            }
        }

        /// <summary>
        /// Adds a room to the game map as long as it isn't already there.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="room">The room.</param>
        public static void AddIfNotExists(this List<Room> list, Room room)
        {
            if (list.Any(l => l == room))
            {
                return;
            }
            else
            {
                list.Add(room);
            }
        }
    }
}