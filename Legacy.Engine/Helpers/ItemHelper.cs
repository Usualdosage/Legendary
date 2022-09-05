// <copyright file="ItemHelper.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Helpers
{
    using System;
    using System.Collections.Generic;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Core.Types;
    using Legendary.Engine.Extensions;

    /// <summary>
    /// Helper for items.
    /// </summary>
    public class ItemHelper
    {
        /// <summary>
        /// Creates a very basic practice weapon.
        /// </summary>
        /// <param name="random">The random generator.</param>
        /// <returns>Item.</returns>
        public static Item CreatePracticeWeapon(IRandom random)
        {
            List<string> weaponAdjectives = new List<string>()
            {
                "steel",
                "wooden",
                "practice",
                "black iron",
                "worn",
                "well-used",
                "tarnished",
                "rusty",
                "battered",
                "metal",
                "cracked",
            };

            var adj = weaponAdjectives[random.Next(0, weaponAdjectives.Count - 1)];

            List<Tuple<string, DamageType>> weaponNouns = new List<Tuple<string, DamageType>>()
            {
                new Tuple<string, DamageType>($"a {adj} mace", DamageType.Blunt),
                new Tuple<string, DamageType>($"a {adj} sword", DamageType.Slash),
                new Tuple<string, DamageType>($"a {adj} hatchet", DamageType.Slash),
                new Tuple<string, DamageType>($"a {adj} dagger", DamageType.Pierce),
                new Tuple<string, DamageType>($"a {adj} knife", DamageType.Pierce),
                new Tuple<string, DamageType>($"a {adj} mace", DamageType.Blunt),
                new Tuple<string, DamageType>($"a {adj} club", DamageType.Blunt),
                new Tuple<string, DamageType>($"a {adj} sword", DamageType.Slash),
                new Tuple<string, DamageType>($"a {adj} short spear", DamageType.Pierce),
            };

            var weaponTuple = weaponNouns[random.Next(0, weaponNouns.Count - 1)];

            string title = $"{weaponTuple.Item1}";
            string shortDesc = $"{weaponTuple.Item1.FirstCharToUpper()} is lying here.";
            var durability = random.Next(2, 7);

            var item = new Item()
            {
                ItemType = ItemType.Weapon,
                DamageType = weaponTuple.Item2,
                WearLocation = new List<WearLocation>() { WearLocation.Wielded },
                Name = title,
                ShortDescription = shortDesc,
                LongDescription = shortDesc,
                Weight = random.Next(.5m, 2m),
                Value = random.Next(0m, .1m),
                Durability = new MaxCurrent(durability, durability),
                ItemKind = ItemKind.Practice,
                Level = random.Next(1, 9),
                HitDice = random.Next(1, 2),
                DamageDice = random.Next(4, 8),
            };

            return item;
        }

        /// <summary>
        /// Creates a very basic piece of practice armor.
        /// </summary>
        /// <param name="random">The random generator.</param>
        /// <param name="wearLocation">The wear location.</param>
        /// <returns>Item.</returns>
        public static Item CreatePracticeGear(IRandom random, WearLocation wearLocation)
        {
            List<string> gearAdjectives = new List<string>()
            {
                "leather",
                "padded",
                "worn",
                "cloth",
                "deerhide",
                "buckskin",
                "velvet",
                "canvas",
                "felt",
                "fur-lined",
                "silky",
                "rugged",
                "torn",
                "second-hand",
                "well-used",
            };

            var adj = gearAdjectives[random.Next(0, gearAdjectives.Count - 1)];

            List<string> gearNouns = new List<string>();

            switch (wearLocation)
            {
                case WearLocation.Arms:
                    {
                        gearNouns = new List<string>()
                        {
                            $"some {adj} sleeves",
                            $"some {adj} bracers",
                            $"some {adj} vambraces",
                            $"some {adj} arm guards",
                        };
                        break;
                    }

                case WearLocation.Head:
                    {
                        gearNouns = new List<string>()
                        {
                            $"a {adj} hat",
                            $"a {adj} cap",
                            $"a {adj} hood",
                            $"a {adj} helm",
                        };
                        break;
                    }

                case WearLocation.Legs:
                    {
                        gearNouns = new List<string>()
                        {
                            $"a {adj} pants",
                            $"a {adj} chaps",
                            $"a {adj} shorts",
                            $"a {adj} greaves",
                        };
                        break;
                    }

                default:
                case WearLocation.Torso:
                    {
                        gearNouns = new List<string>()
                        {
                            $"a {adj} vest",
                            $"a {adj} jacket",
                            $"a {adj} doublet",
                            $"a {adj} shirt",
                        };
                        break;
                    }

                case WearLocation.Feet:
                    {
                        gearNouns = new List<string>()
                        {
                            $"some {adj} sandals",
                            $"a pair of {adj} boots",
                            $"some {adj} shoes",
                            $"some {adj} sollerettes",
                        };
                        break;
                    }

                case WearLocation.Hands:
                    {
                        gearNouns = new List<string>()
                        {
                            $"some {adj} gloves",
                            $"a pair of {adj} mittens",
                            $"some {adj} fingerless gloves",
                            $"some {adj} hand wraps",
                        };
                        break;
                    }
            }

            var gear = gearNouns[random.Next(0, gearNouns.Count - 1)];

            string title = $"{gear}";
            string shortDesc = $"You see {gear} lying here.";
            var durability = random.Next(2, 7);

            var item = new Item()
            {
                ItemType = ItemType.Armor,
                WearLocation = new List<WearLocation>() { wearLocation },
                Name = title,
                ShortDescription = shortDesc,
                LongDescription = shortDesc,
                Weight = random.Next(.5m, 2m),
                Value = random.Next(0m, .1m),
                Pierce = random.Next(0, 2),
                Blunt = random.Next(0, 2),
                Edged = random.Next(0, 2),
                Magic = random.Next(0, 2),
                Durability = new MaxCurrent(durability, durability),
                ItemKind = ItemKind.Practice,
                Level = random.Next(1, 9),
            };

            return item;
        }

        /// <summary>
        /// Creates a generic light for the newbie.
        /// </summary>
        /// <param name="random">The randomizer.</param>
        /// <returns>Item.</returns>
        public static Item CreateLight(IRandom random)
        {
            List<string> lampAdjectives = new List<string>()
            {
                "copper",
                "brass",
                "wooden",
                "magical",
                "glowing",
                "tarnished",
                "steel",
            };

            var adj = lampAdjectives[random.Next(0, lampAdjectives.Count - 1)];

            List<string> lampNouns = new List<string>()
            {
                $"a {adj} lamp",
                $"a {adj} lantern",
                $"a {adj} light",
                $"a {adj} torch",
            };

            var lamp = lampNouns[random.Next(0, lampNouns.Count - 1)];

            string title = $"{lamp}";
            string shortDesc = $"You see {lamp} lying here.";

            var item = new Item()
            {
                ItemType = ItemType.Light,
                WearLocation = new List<WearLocation>() { WearLocation.Light },
                Name = title,
                ShortDescription = shortDesc,
                LongDescription = shortDesc,
                Weight = random.Next(.5m, 2m),
                Value = random.Next(0m, .1m),
                Durability = new MaxCurrent(1, 1),
                ItemKind = ItemKind.Practice,
                Level = random.Next(1, 9),
            };

            return item;
        }
    }
}