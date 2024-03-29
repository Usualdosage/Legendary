﻿// <copyright file="ItemType.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Core.Types
{
    /// <summary>
    /// Various types that an item can be.
    /// </summary>
    public enum ItemType : short
    {
        /// <summary>
        /// Weapon.
        /// </summary>
        Weapon = 0,

        /// <summary>
        /// Armor.
        /// </summary>
        Armor = 1,

        /// <summary>
        /// Food.
        /// </summary>
        Food = 3,

        /// <summary>
        /// Light.
        /// </summary>
        Light = 4,

        /// <summary>
        /// Drink.
        /// </summary>
        Drink = 5,

        /// <summary>
        /// Container.
        /// </summary>
        Container = 6,

        /// <summary>
        /// Currency.
        /// </summary>
        Currency = 7,

        /// <summary>
        /// Spring.
        /// </summary>
        Spring = 8,

        /// <summary>
        /// Key.
        /// </summary>
        Key = 9,

        /// <summary>
        /// Gem.
        /// </summary>
        Gem = 10,

        /// <summary>
        /// Map.
        /// </summary>
        Map = 11,

        /// <summary>
        /// Potion.
        /// </summary>
        Potion = 12,

        /// <summary>
        /// Pill.
        /// </summary>
        Pill = 13,

        /// <summary>
        /// Scroll.
        /// </summary>
        Scroll = 14,

        /// <summary>
        /// Herb.
        /// </summary>
        Herb = 15,

        /// <summary>
        /// Boat.
        /// </summary>
        Boat = 16,
    }
}