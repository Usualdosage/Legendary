// <copyright file="ItemType.cs" company="Legendary™">
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
    }
}