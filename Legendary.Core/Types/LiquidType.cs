// <copyright file="LiquidType.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Core.Types
{
    using Legendary.Core.Attributes;

    /// <summary>
    /// The type of liquid in a container or a spring.
    /// </summary>
    public enum LiquidType
    {
        /// <summary>
        /// None.
        /// </summary>
        None = 0,

        /// <summary>
        /// Water.
        /// </summary>
        [LiquidDescription("water")]
        Water = 1,

        /// <summary>
        /// Poison.
        /// </summary>
        [LiquidDescription("poison")]
        Poison = 2,

        /// <summary>
        /// Beer.
        /// </summary>
        [LiquidDescription("beer")]
        Beer = 3,

        /// <summary>
        /// Milk.
        /// </summary>
        [LiquidDescription("milk")]
        Milk = 4,

        /// <summary>
        /// Holy water.
        /// </summary>
        [LiquidDescription("holy water")]
        HolyWater = 5,

        /// <summary>
        /// Salt water.
        /// </summary>
        [LiquidDescription("salt water")]
        SaltWater = 6,

        /// <summary>
        /// Lamp oil.
        /// </summary>
        [LiquidDescription("lamp oil")]
        LampOil = 7,

        /// <summary>
        /// Olive oil.
        /// </summary>
        [LiquidDescription("olive oil")]
        OliveOil = 8,

        /// <summary>
        /// Slime.
        /// </summary>
        [LiquidDescription("slime")]
        Slime = 9,

        /// <summary>
        /// Red wine.
        /// </summary>
        [LiquidDescription("red wine")]
        RedWine = 10,

        /// <summary>
        /// Whiskey.
        /// </summary>
        [LiquidDescription("whiskey")]
        Whiskey = 11,

        /// <summary>
        /// Vodka.
        /// </summary>
        [LiquidDescription("vodka")]
        Vodka = 12,

        /// <summary>
        /// Gin.
        /// </summary>
        [LiquidDescription("gin")]
        Gin = 13,

        /// <summary>
        /// Mead.
        /// </summary>
        [LiquidDescription("mead")]
        Mead = 14,

        /// <summary>
        /// White wine.
        /// </summary>
        [LiquidDescription("white wine")]
        WhiteWine = 15,

        /// <summary>
        /// Champagne.
        /// </summary>
        [LiquidDescription("champagne")]
        Champagne = 16,

        /// <summary>
        /// Blood.
        /// </summary>
        [LiquidDescription("blood")]
        Blood = 17,
    }
}