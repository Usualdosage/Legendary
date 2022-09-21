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
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Legendary.Core.Models;
    using Legendary.Core.Types;

    /// <summary>
    /// Extensions for numeric data types.
    /// </summary>
    public static class NumericExtensions
    {
        /// <summary>
        /// Returns a roman numeral for an integer between 1 and 10.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <returns>string.</returns>
        public static string ToRomanNumeral(this int number)
        {
            return number switch
            {
                2 => "II",
                3 => "III",
                4 => "IV",
                5 => "V",
                6 => "VI",
                7 => "VII",
                8 => "VIII",
                9 => "IX",
                10 => "X",
                _ => "I",
            };
        }

        /// <summary>
        /// Returns an integer for a roman numeral between I and X.
        /// </summary>
        /// <param name="numeral">The numeral.</param>
        /// <returns>string.</returns>
        public static int FromRomanNumeral(this string numeral)
        {
            return numeral.ToUpper() switch
            {
                "I" => 1,
                "II" => 2,
                "III" => 3,
                "IV" => 4,
                "V" => 5,
                "VI" => 6,
                "VII" => 7,
                "VIII" => 8,
                "IX" => 9,
                "X" => 10,
                _ => 0,
            };
        }

        /// <summary>
        /// Converts a decimal currency to language.
        /// </summary>
        /// <param name="currency">The currency.</param>
        /// <returns>String.</returns>
        public static string CurrencyToWords(this decimal currency)
        {
            if (currency == 0)
            {
                return "nothing";
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

                    if (currencyParts[1].Length >= 2)
                    {
                        silver = int.Parse(currencyParts[1][0].ToString());
                        copper = int.Parse(currencyParts[1][1].ToString());
                    }
                }

                StringBuilder sb = new StringBuilder();

                if (gold > 0)
                {
                    sb.Append($"{gold} gold");
                }

                if (silver > 0)
                {
                    sb.Append($" {silver} silver");
                }

                if (copper > 0)
                {
                    sb.Append($" {copper} copper");
                }

                return sb.ToString();
            }
        }

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

                    if (currencyParts[1].Length >= 2)
                    {
                        silver = int.Parse(currencyParts[1][0].ToString());
                        copper = int.Parse(currencyParts[1][1].ToString());
                    }
                }

                return $"<span class='currency-gold'>{gold}</span> gold<br/><span class='currency-silver'>{silver}</span> silver<br/><span class='currency-copper'>{copper}</span> copper";
            }
        }

        /// <summary>
        /// Converts a value of an item to the correct currency.
        /// </summary>
        /// <param name="price">The the value of the item.</param>
        /// <param name="actor">The player.</param>
        /// <param name="merchant">The merchant.</param>
        /// <returns>String.</returns>
        public static string ToMerchantSellPrice(this decimal price, Character actor, Mobile merchant)
        {
            if (price == 0)
            {
                return "not currently for sale.";
            }
            else
            {
                price = price.AdjustSellPrice(actor, merchant);

                // Currency is a decimal, so like, 23.49. This would represent
                // 23 gold, 4 silver, and 9 copper.
                var currencyParts = price.ToString().Split('.');
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

                    if (currencyParts[1].Length >= 2)
                    {
                        silver = int.Parse(currencyParts[1][0].ToString());
                        copper = int.Parse(currencyParts[1][1].ToString());
                    }
                }

                StringBuilder sb = new StringBuilder();

                if (gold > 0)
                {
                    sb.Append($"{gold} gold");
                }

                if (silver > 0)
                {
                    if (gold > 0)
                    {
                        sb.Append($", {silver} silver");
                    }
                    else
                    {
                        sb.Append($"{silver} silver");
                    }
                }

                if (copper > 0)
                {
                    if (gold > 0 || silver > 0)
                    {
                        sb.Append($", and {copper} copper");
                    }
                    else
                    {
                        sb.Append($"{copper} copper");
                    }
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Converts a value of an item to the correct currency.
        /// </summary>
        /// <param name="price">The the value of the item.</param>
        /// <param name="actor">The player.</param>
        /// <param name="merchant">The merchant.</param>
        /// <returns>String.</returns>
        public static string ToMerchantBuyPrice(this decimal price, Character actor, Mobile merchant)
        {
            if (price == 0)
            {
                return "nothing at all";
            }
            else
            {
                price = price.AdjustBuyPrice(actor, merchant);

                // Currency is a decimal, so like, 23.49. This would represent
                // 23 gold, 4 silver, and 9 copper.
                var currencyParts = price.ToString().Split('.');
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

                    if (currencyParts[1].Length >= 2)
                    {
                        silver = int.Parse(currencyParts[1][0].ToString());
                        copper = int.Parse(currencyParts[1][1].ToString());
                    }
                }

                StringBuilder sb = new StringBuilder();

                if (gold > 0)
                {
                    sb.Append($"{gold} gold");
                }

                if (silver > 0)
                {
                    if (gold > 0)
                    {
                        sb.Append($", {silver} silver");
                    }
                    else
                    {
                        sb.Append($"{silver} silver");
                    }
                }

                if (copper > 0)
                {
                    if (gold > 0 || silver > 0)
                    {
                        sb.Append($", and {copper} copper");
                    }
                    else
                    {
                        sb.Append($"{copper} copper");
                    }
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Adjust the selling price of an item based on character alignment and race.
        /// </summary>
        /// <param name="price">The price.</param>
        /// <param name="actor">The actor.</param>
        /// <param name="merchant">The merchant.</param>
        /// <returns>decimal.</returns>
        public static decimal AdjustSellPrice(this decimal price, Character actor, Mobile merchant)
        {
            decimal modifier = 0;

            switch (merchant.Alignment)
            {
                case Alignment.Good:
                    {
                        switch (actor.Alignment)
                        {
                            case Alignment.Good:
                                {
                                    modifier = 1.3m;
                                    break;
                                }

                            case Alignment.Neutral:
                                {
                                    modifier = 1.8m;
                                    break;
                                }

                            case Alignment.Evil:
                                {
                                    modifier = 2.6m;
                                    break;
                                }
                        }

                        break;
                    }

                case Alignment.Neutral:
                    {
                        switch (actor.Alignment)
                        {
                            case Alignment.Good:
                                {
                                    modifier = 1.75m;
                                    break;
                                }

                            case Alignment.Neutral:
                                {
                                    modifier = 1.25m;
                                    break;
                                }

                            case Alignment.Evil:
                                {
                                    modifier = 1.75m;
                                    break;
                                }
                        }

                        break;
                    }

                case Alignment.Evil:
                    {
                        switch (actor.Alignment)
                        {
                            case Alignment.Good:
                                {
                                    modifier = 2.8m;
                                    break;
                                }

                            case Alignment.Neutral:
                                {
                                    modifier = 2.1m;
                                    break;
                                }

                            case Alignment.Evil:
                                {
                                    modifier = 1.5m;
                                    break;
                                }
                        }

                        break;
                    }
            }

            switch (merchant.Race)
            {
                default:
                    break;
                case Race.Elf:
                    {
                        switch (actor.Race)
                        {
                            case Race.Dwarf:
                                {
                                    modifier += .15m;
                                    break;
                                }

                            case Race.HalfOrc:
                                {
                                    modifier += .75m;
                                    break;
                                }

                            case Race.Drow:
                                {
                                    modifier += 3.76m;
                                    break;
                                }
                        }

                        break;
                    }

                case Race.Duergar:
                    {
                        switch (actor.Race)
                        {
                            case Race.Dwarf:
                                {
                                    modifier += 3.76m;
                                    break;
                                }
                        }

                        break;
                    }

                case Race.Dwarf:
                    {
                        switch (actor.Race)
                        {
                            case Race.Duergar:
                                {
                                    modifier += 4.23m;
                                    break;
                                }
                        }

                        break;
                    }

                case Race.Drow:
                    {
                        switch (actor.Race)
                        {
                            case Race.Elf:
                                {
                                    modifier += 4.23m;
                                    break;
                                }
                        }

                        break;
                    }
            }

            price *= modifier;

            return price;
        }

        /// <summary>
        /// Adjust the purchase price of an item based on character alignment and race.
        /// </summary>
        /// <param name="price">The price.</param>
        /// <param name="actor">The actor.</param>
        /// <param name="merchant">The merchant.</param>
        /// <returns>decimal.</returns>
        public static decimal AdjustBuyPrice(this decimal price, Character actor, Mobile merchant)
        {
            decimal modifier = 0;

            switch (merchant.Alignment)
            {
                case Alignment.Good:
                    {
                        switch (actor.Alignment)
                        {
                            case Alignment.Good:
                                {
                                    modifier = .4m;
                                    break;
                                }

                            case Alignment.Neutral:
                                {
                                    modifier = .5m;
                                    break;
                                }

                            case Alignment.Evil:
                                {
                                    modifier = .6m;
                                    break;
                                }
                        }

                        break;
                    }

                case Alignment.Neutral:
                    {
                        switch (actor.Alignment)
                        {
                            case Alignment.Good:
                                {
                                    modifier = .5m;
                                    break;
                                }

                            case Alignment.Neutral:
                                {
                                    modifier = .6m;
                                    break;
                                }

                            case Alignment.Evil:
                                {
                                    modifier = .5m;
                                    break;
                                }
                        }

                        break;
                    }

                case Alignment.Evil:
                    {
                        switch (actor.Alignment)
                        {
                            case Alignment.Good:
                                {
                                    modifier = .4m;
                                    break;
                                }

                            case Alignment.Neutral:
                                {
                                    modifier = .5m;
                                    break;
                                }

                            case Alignment.Evil:
                                {
                                    modifier = .6m;
                                    break;
                                }
                        }

                        break;
                    }
            }

            switch (merchant.Race)
            {
                default:
                    break;
                case Race.Elf:
                    {
                        switch (actor.Race)
                        {
                            case Race.Dwarf:
                                {
                                    modifier -= .01m;
                                    break;
                                }

                            case Race.HalfOrc:
                                {
                                    modifier -= .08m;
                                    break;
                                }

                            case Race.Drow:
                                {
                                    modifier -= .15m;
                                    break;
                                }
                        }

                        break;
                    }

                case Race.Duergar:
                    {
                        switch (actor.Race)
                        {
                            case Race.Dwarf:
                                {
                                    modifier -= .15m;
                                    break;
                                }
                        }

                        break;
                    }

                case Race.Dwarf:
                    {
                        switch (actor.Race)
                        {
                            case Race.Duergar:
                                {
                                    modifier -= .15m;
                                    break;
                                }
                        }

                        break;
                    }

                case Race.Drow:
                    {
                        switch (actor.Race)
                        {
                            case Race.Elf:
                                {
                                    modifier -= .15m;
                                    break;
                                }
                        }

                        break;
                    }
            }

            price *= modifier;

            return price;
        }

        /// <summary>
        /// Breaks the currency down into gold, silver, and copper.
        /// </summary>
        /// <param name="currency">The decimal currency.</param>
        /// <returns>Tuple.</returns>
        public static Tuple<int, int, int> GetCurrency(this decimal currency)
        {
            if (currency == 0)
            {
                return new Tuple<int, int, int>(0, 0, 0);
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

                return new Tuple<int, int, int>(gold, silver, copper);
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
                    ShortDescription = $"{gold} gold coins are lying here.",
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
                    ShortDescription = $"{silver} silver coins are lying here.",
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
                    ShortDescription = $"{copper} copper coins are lying here.",
                    LongDescription = $"{copper} copper coins are lying here.",
                    Level = 0,
                };

                items.Add(copperCurrency);
            }

            return items;
        }
    }
}