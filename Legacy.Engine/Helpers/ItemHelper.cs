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
    using System.Reflection.PortableExecutable;
    using Legendary.Core;
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
            List<string> weaponAdjectives = new ()
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

            List<Tuple<string, DamageType>> weaponNouns = new ()
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
                ItemId = Constants.ITEM_BASIC_WEAPON,
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
        /// Gets a valud indicating whether or not the player can carry the current item. Applies the weight.
        /// </summary>
        /// <param name="actor">The actor.</param>
        /// <param name="item">The item.</param>
        /// <returns>True if they can carry it.</returns>
        public static bool CanCarry(Character actor, IItem item)
        {
            if (actor.CarryWeight.Current + (double)item.Weight > actor.CarryWeight.Max)
            {
                return false;
            }
            else
            {
                actor.CarryWeight.Current += (double)item.Weight;
                return true;
            }
        }

        /// <summary>
        /// Creates a very basic piece of practice armor.
        /// </summary>
        /// <param name="random">The random generator.</param>
        /// <param name="wearLocation">The wear location.</param>
        /// <returns>Item.</returns>
        public static Item CreatePracticeGear(IRandom random, WearLocation wearLocation)
        {
            List<string> gearAdjectives = new ()
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

            List<string> gearNouns = new ();

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
                ItemId = Constants.ITEM_BASIC_ARMOR,
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
            List<string> lampAdjectives = new ()
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

            List<string> lampNouns = new ()
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

        /// <summary>
        /// Random loot drop from killing a mob.
        /// </summary>
        /// <param name="mobLevel">The mob level.</param>
        /// <param name="characterLevel">The character level.</param>
        /// <param name="random">The random generator.</param>
        /// <returns>Item.</returns>
        public static Item? CreateRandomArmor(int mobLevel, int characterLevel, IRandom random)
        {
            if (mobLevel > 75)
            {
                return null;
            }

            Array values = Enum.GetValues(typeof(WearLocation));
            object? randomWearLoc = values.GetValue(random.Next(2, values.Length - 1));

            WearLocation wearLocation = WearLocation.None;

            if (randomWearLoc != null)
            {
                wearLocation = (WearLocation)randomWearLoc;
            }
            else
            {
                return null;
            }

            List<string> gearAdjectives = new ()
            {
                "Leather",
                "Padded",
                "Steel",
                "Iron",
                "Deerhide",
                "Buckskin",
                "Velvet",
                "Canvas",
                "Felt",
                "Fur-lined",
                "Copper",
                "Brass",
                "Platinum",
                "Golden",
                "Silvery",
                "Wooden",
                "Bone",
                "Ivory",
                "Ebony",
                "Bright Steel",
                "Dark Steel",
                "Blackened",
                "Shining",
            };

            var adj = gearAdjectives[random.Next(0, gearAdjectives.Count - 1)];

            List<string> gearNouns = new ();

            switch (wearLocation)
            {
                default:
                    {
                        return null;
                    }

                case WearLocation.Arms:
                    {
                        gearNouns = new List<string>()
                        {
                            $"some {adj} Sleeves",
                            $"some {adj} Bracers",
                            $"some {adj} Vambraces",
                            $"some {adj} Arm guards",
                            $"some {adj} Pauldrons",
                        };
                        break;
                    }

                case WearLocation.Head:
                    {
                        gearNouns = new List<string>()
                        {
                            $"a {adj} Hat",
                            $"a {adj} Cap",
                            $"a {adj} Hood",
                            $"a {adj} Helm",
                            $"a {adj} Helmet",
                        };
                        break;
                    }

                case WearLocation.Legs:
                    {
                        gearNouns = new List<string>()
                        {
                            $"some {adj} Pants",
                            $"some {adj} Chaps",
                            $"some {adj} Shorts",
                            $"some {adj} Greaves",
                            $"some {adj} Leggings",
                        };
                        break;
                    }

                case WearLocation.Torso:
                    {
                        gearNouns = new List<string>()
                        {
                            $"a {adj} Vest",
                            $"a {adj} Jacket",
                            $"a {adj} Doublet",
                            $"a {adj} Shirt",
                            $"a {adj} Tunic",
                            $"a {adj} Cuirass",
                        };
                        break;
                    }

                case WearLocation.Feet:
                    {
                        gearNouns = new List<string>()
                        {
                            $"some {adj} Sandals",
                            $"a Pair of {adj} Boots",
                            $"some {adj} Shoes",
                            $"some {adj} Sollerettes",
                        };
                        break;
                    }

                case WearLocation.Hands:
                    {
                        gearNouns = new List<string>()
                        {
                            $"some {adj} Gloves",
                            $"a pair of {adj} Mittens",
                            $"some {adj} Fingerless Gloves",
                            $"some {adj} Hand Wraps",
                            $"some {adj} Claws",
                        };
                        break;
                    }

                case WearLocation.RWrist:
                case WearLocation.LWrist:
                    {
                        gearNouns = new List<string>()
                        {
                            $"a {adj} Bracelet",
                            $"a {adj} Bracer",
                            $"a {adj} Wrist Guard",
                            $"a {adj} Gauntlet",
                        };
                        break;
                    }
            }

            var gear = gearNouns[random.Next(0, gearNouns.Count - 1)];

            string title = $"{gear}";
            string shortDesc = $"You see {gear.ToLower()} lying here.";
            var durability = Math.Min(1, random.Next(mobLevel / 10, mobLevel / 3));

            var item = new Item()
            {
                ItemId = Constants.ITEM_LOOT_ARMOR,
                ItemType = ItemType.Armor,
                WearLocation = new List<WearLocation>() { wearLocation },
                Name = title,
                ShortDescription = shortDesc,
                LongDescription = shortDesc,
                Weight = random.Next(1m, 10m),
                Value = random.Next(.5m, 2m * mobLevel),
                Durability = new MaxCurrent(durability, durability),
                ItemKind = ItemKind.Practice,
                Level = random.Next(mobLevel - 5, mobLevel + 5),
            };

            switch (random.Next(0, 4))
            {
                default:
                case 0:
                    {
                        item.Edged = random.Next(1, characterLevel / 5);
                        break;
                    }

                case 1:
                    {
                        item.Pierce = random.Next(0, characterLevel / 5);
                        item.Blunt = random.Next(0, characterLevel / 5);
                        break;
                    }

                case 2:
                    {
                        item.Pierce = random.Next(0, characterLevel / 5);
                        item.Blunt = random.Next(0, characterLevel / 5);
                        item.Edged = random.Next(0, characterLevel / 5);
                        break;
                    }

                case 3:
                    {
                        item.Pierce = random.Next(0, characterLevel / 5);
                        item.Blunt = random.Next(0, characterLevel / 5);
                        item.Edged = random.Next(0, characterLevel / 5);
                        item.Magic = random.Next(0, characterLevel / 5);
                        break;
                    }
            }

            if (mobLevel > 20 && mobLevel < 40)
            {
                // 25% chance to have one effect
                if (random.Next(0, 100) <= 25)
                {
                    Array flags = Enum.GetValues(typeof(ItemFlags));
                    object? randomFlags = values.GetValue(random.Next(0, values.Length - 1));

                    ItemFlags itemFlag = ItemFlags.None;

                    if (itemFlag != ItemFlags.None)
                    {
                        item.ItemFlags.Add(itemFlag);

                        switch (random.Next(0, 7))
                        {
                            case 0:
                                {
                                    item.SaveAfflictive = random.Next(0, mobLevel / 10);
                                    break;
                                }

                            case 1:
                                {
                                    item.SaveDeath = random.Next(0, mobLevel / 10);
                                    break;
                                }

                            case 2:
                                {
                                    item.SaveMaledictive = random.Next(0, mobLevel / 10);
                                    break;
                                }

                            case 3:
                                {
                                    item.SaveNegative = random.Next(0, mobLevel / 10);
                                    break;
                                }

                            case 4:
                                {
                                    item.SaveSpell = random.Next(0, mobLevel / 10);
                                    break;
                                }

                            default:
                            case 5:
                                {
                                    item.HitDice = random.Next(0, mobLevel / 10);
                                    break;
                                }

                            case 6:
                                {
                                    item.DamageDice = random.Next(0, mobLevel / 10);
                                    break;
                                }
                        }
                    }
                }
            }
            else if (mobLevel >= 40 && mobLevel <= 60)
            {
                // 25% chance to have one effect
                if (random.Next(0, 100) <= 25)
                {
                    Array flags = Enum.GetValues(typeof(ItemFlags));
                    object? randomFlags = values.GetValue(random.Next(0, values.Length - 1));

                    ItemFlags itemFlag = ItemFlags.None;

                    if (itemFlag != ItemFlags.None)
                    {
                        item.ItemFlags.Add(itemFlag);

                        switch (random.Next(0, 7))
                        {
                            case 0:
                                {
                                    item.SaveAfflictive = random.Next(0, mobLevel / 6);
                                    break;
                                }

                            case 1:
                                {
                                    item.SaveDeath = random.Next(0, mobLevel / 6);
                                    break;
                                }

                            case 2:
                                {
                                    item.SaveMaledictive = random.Next(0, mobLevel / 6);
                                    break;
                                }

                            case 3:
                                {
                                    item.SaveNegative = random.Next(0, mobLevel / 6);
                                    break;
                                }

                            case 4:
                                {
                                    item.SaveSpell = random.Next(0, mobLevel / 6);
                                    break;
                                }

                            default:
                            case 5:
                                {
                                    item.HitDice = random.Next(0, mobLevel / 6);
                                    break;
                                }

                            case 6:
                                {
                                    item.DamageDice = random.Next(0, mobLevel / 6);
                                    break;
                                }
                        }
                    }
                }

                // 25% chance to have second effect
                if (random.Next(0, 100) <= 25)
                {
                    Array flags = Enum.GetValues(typeof(ItemFlags));
                    object? randomFlags = values.GetValue(random.Next(0, values.Length - 1));

                    ItemFlags itemFlag = ItemFlags.None;

                    if (itemFlag != ItemFlags.None)
                    {
                        item.ItemFlags.Add(itemFlag);

                        switch (random.Next(0, 7))
                        {
                            case 0:
                                {
                                    item.SaveAfflictive = random.Next(0, mobLevel / 6);
                                    break;
                                }

                            case 1:
                                {
                                    item.SaveDeath = random.Next(0, mobLevel / 6);
                                    break;
                                }

                            case 2:
                                {
                                    item.SaveMaledictive = random.Next(0, mobLevel / 6);
                                    break;
                                }

                            case 3:
                                {
                                    item.SaveNegative = random.Next(0, mobLevel / 6);
                                    break;
                                }

                            case 4:
                                {
                                    item.SaveSpell = random.Next(0, mobLevel / 6);
                                    break;
                                }

                            default:
                            case 5:
                                {
                                    item.HitDice = random.Next(0, mobLevel / 6);
                                    break;
                                }

                            case 6:
                                {
                                    item.DamageDice = random.Next(0, mobLevel / 6);
                                    break;
                                }
                        }
                    }
                }
            }
            else if (mobLevel >= 60 && mobLevel <= 75)
            {
                // 25% chance to have one effect
                if (random.Next(0, 100) <= 25)
                {
                    Array flags = Enum.GetValues(typeof(ItemFlags));
                    object? randomFlags = values.GetValue(random.Next(0, values.Length - 1));

                    ItemFlags itemFlag = ItemFlags.None;

                    if (itemFlag != ItemFlags.None)
                    {
                        item.ItemFlags.Add(itemFlag);

                        switch (random.Next(0, 7))
                        {
                            case 0:
                                {
                                    item.SaveAfflictive = random.Next(0, mobLevel / 6);
                                    break;
                                }

                            case 1:
                                {
                                    item.SaveDeath = random.Next(0, mobLevel / 6);
                                    break;
                                }

                            case 2:
                                {
                                    item.SaveMaledictive = random.Next(0, mobLevel / 6);
                                    break;
                                }

                            case 3:
                                {
                                    item.SaveNegative = random.Next(0, mobLevel / 6);
                                    break;
                                }

                            case 4:
                                {
                                    item.SaveSpell = random.Next(0, mobLevel / 6);
                                    break;
                                }

                            default:
                            case 5:
                                {
                                    item.HitDice = random.Next(0, mobLevel / 6);
                                    break;
                                }

                            case 6:
                                {
                                    item.DamageDice = random.Next(0, mobLevel / 6);
                                    break;
                                }
                        }
                    }
                }

                // 25% chance to have second effect
                if (random.Next(0, 100) <= 25)
                {
                    Array flags = Enum.GetValues(typeof(ItemFlags));
                    object? randomFlags = values.GetValue(random.Next(0, values.Length - 1));

                    ItemFlags itemFlag = ItemFlags.None;

                    if (itemFlag != ItemFlags.None)
                    {
                        item.ItemFlags.Add(itemFlag);

                        switch (random.Next(0, 7))
                        {
                            case 0:
                                {
                                    item.SaveAfflictive = random.Next(0, mobLevel / 6);
                                    break;
                                }

                            case 1:
                                {
                                    item.SaveDeath = random.Next(0, mobLevel / 6);
                                    break;
                                }

                            case 2:
                                {
                                    item.SaveMaledictive = random.Next(0, mobLevel / 6);
                                    break;
                                }

                            case 3:
                                {
                                    item.SaveNegative = random.Next(0, mobLevel / 6);
                                    break;
                                }

                            case 4:
                                {
                                    item.SaveSpell = random.Next(0, mobLevel / 6);
                                    break;
                                }

                            default:
                            case 5:
                                {
                                    item.HitDice = random.Next(0, mobLevel / 6);
                                    break;
                                }

                            case 6:
                                {
                                    item.DamageDice = random.Next(0, mobLevel / 6);
                                    break;
                                }
                        }
                    }
                }

                // 25% chance to have third effect
                if (random.Next(0, 100) <= 25)
                {
                    Array flags = Enum.GetValues(typeof(ItemFlags));
                    object? randomFlags = values.GetValue(random.Next(0, values.Length - 1));

                    ItemFlags itemFlag = ItemFlags.None;

                    if (itemFlag != ItemFlags.None)
                    {
                        item.ItemFlags.Add(itemFlag);

                        switch (random.Next(0, 7))
                        {
                            case 0:
                                {
                                    item.SaveAfflictive = random.Next(0, mobLevel / 6);
                                    break;
                                }

                            case 1:
                                {
                                    item.SaveDeath = random.Next(0, mobLevel / 6);
                                    break;
                                }

                            case 2:
                                {
                                    item.SaveMaledictive = random.Next(0, mobLevel / 6);
                                    break;
                                }

                            case 3:
                                {
                                    item.SaveNegative = random.Next(0, mobLevel / 6);
                                    break;
                                }

                            case 4:
                                {
                                    item.SaveSpell = random.Next(0, mobLevel / 6);
                                    break;
                                }

                            default:
                            case 5:
                                {
                                    item.HitDice = random.Next(0, mobLevel / 6);
                                    break;
                                }

                            case 6:
                                {
                                    item.DamageDice = random.Next(0, mobLevel / 6);
                                    break;
                                }
                        }
                    }
                }
            }

            var suffix = string.Empty;

            if (item.Pierce > 10)
            {
                suffix = " of the Boar";
            }
            else if (item.Blunt > 10)
            {
                suffix = " of the Badger";
            }
            else if (item.Edged > 10)
            {
                suffix = " of the Warlord";
            }
            else if (item.Magic > 10)
            {
                suffix = " of the Magi";
            }
            else if (item.DamageDice > 0)
            {
                suffix = " of the Lion";
            }
            else if (item.HitDice > 0)
            {
                suffix = " of the Mongoose";
            }
            else if (item.SaveAfflictive > 0)
            {
                suffix = " of the Hawk";
            }
            else if (item.SaveDeath > 0)
            {
                suffix = " of the Gods";
            }
            else if (item.SaveMaledictive > 0)
            {
                suffix = " of the Raven";
            }
            else if (item.SaveNegative > 0)
            {
                suffix = " of the Bear";
            }
            else if (item.SaveSpell > 0)
            {
                suffix = " of the Hobbit";
            }

            item.Name += suffix;

            return item;
        }
    }
}