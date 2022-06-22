// <copyright file="ItemType.cs" company="Legendary">
//  Copyright © 2021-2022 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
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