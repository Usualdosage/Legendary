// <copyright file="NumericExtensions.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Extensions
{
    using System.Collections.Generic;
    using Legendary.Core.Models;
    using Legendary.Core.Types;

    /// <summary>
    /// Extensions for numeric data types.
    /// </summary>
    public static class NumericExtensions
    {
        /// <summary>
        /// Converts a player's currency to a string description.
        /// </summary>
        /// <param name="currency">The player's currency.</param>
        /// <returns>String.</returns>
        public static string ToCurrencyDescription(this decimal currency)
        {
            if (currency == 0)
            {
                return "You are flat broke.";
            }
            else
            {
                // Currency is a decimal, so like, 23.49. This would represent
                // 23 gold, 4 silver, and 9 copper.
                var currencyParts = currency.ToString().Split('.');
                int gold = 0;
                int silver = 0;
                int copper = 0;

                if (currencyParts.Length > 0)
                {
                    gold = int.Parse(currencyParts[0]);
                }

                if (currencyParts.Length > 1)
                {
                    // Section segment will be something like 4 or 49
                    if (currencyParts[1].Length == 1)
                    {
                        silver = int.Parse(currencyParts[1][0].ToString());
                    }

                    if (currencyParts[1].Length == 2)
                    {
                        silver = int.Parse(currencyParts[1][0].ToString());
                        copper = int.Parse(currencyParts[1][1].ToString());
                    }
                }

                return $"<span class='currency-gold'>{gold}</span> gold<br/><span class='currency-silver'>{silver}</span> silver<br/><span class='currency-copper'>{copper}</span> copper";
            }
        }

        /// <summary>
        /// Converts a a decimal of currency into items that can be placed in a room or corpse.
        /// </summary>
        /// <param name="currency">The currency.</param>
        /// <returns>String.</returns>
        public static List<Item> ToCurrencyItems(this decimal currency)
        {
            // Currency is a decimal, so like, 23.49. This would represent
            // 23 gold, 4 silver, and 9 copper.
            var currencyParts = currency.ToString().Split('.');
            int gold = 0;
            int silver = 0;
            int copper = 0;

            if (currencyParts.Length > 0)
            {
                gold = int.Parse(currencyParts[0]);
            }

            if (currencyParts.Length > 1)
            {
                // Section segment will be something like 4 or 49
                if (currencyParts[1].Length == 1)
                {
                    silver = int.Parse(currencyParts[1][0].ToString());
                }

                if (currencyParts[1].Length == 2)
                {
                    silver = int.Parse(currencyParts[1][0].ToString());
                    copper = int.Parse(currencyParts[1][1].ToString());
                }
            }

            List<Item> items = new List<Item>();

            if (gold > 0)
            {
                var goldCurrency = new Item()
                {
                    ItemType = ItemType.Currency,
                    Value = gold,
                    WearLocation = new List<WearLocation>() { WearLocation.None },
                    Name = $"{gold} gold coins",
                    ShortDescription = $"{gold} gold coins",
                    LongDescription = $"{gold} gold coins are lying here.",
                    Level = 0,
                };

                items.Add(goldCurrency);
            }

            if (silver > 0)
            {
                var silverCurrency = new Item()
                {
                    ItemType = ItemType.Currency,
                    Value = (decimal)silver / 10m,
                    WearLocation = new List<WearLocation>() { WearLocation.None },
                    Name = $"{silver} silver coins",
                    ShortDescription = $"{silver} silver coins",
                    LongDescription = $"{silver} silver coins are lying here.",
                    Level = 0,
                };

                items.Add(silverCurrency);
            }

            if (copper > 0)
            {
                var copperCurrency = new Item()
                {
                    ItemType = ItemType.Currency,
                    Value = (decimal)copper / 100m,
                    WearLocation = new List<WearLocation>() { WearLocation.None },
                    Name = $"{copper} copper coins",
                    ShortDescription = $"{copper} copper coins",
                    LongDescription = $"{copper} copper coins are lying here.",
                    Level = 0,
                };

                items.Add(copperCurrency);
            }

            return items;
        }
    }
}