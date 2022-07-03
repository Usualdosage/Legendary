// <copyright file="Armor.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Core.Types
{
    using System.Collections.Generic;

    /// <summary>
    /// Properties for a piece of armor.
    /// </summary>
    public class Armor
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the short description.
        /// </summary>
        public string? ShortDescription { get; set; }

        /// <summary>
        /// Gets or sets the long description.
        /// </summary>
        public string? LongDescription { get; set; }

        /// <summary>
        /// Gets or sets the weight of the armor.
        /// </summary>
        public int Weight { get; set; }

        /// <summary>
        ///  Gets or sets the durability of the armor.
        /// </summary>
        public MaxCurrent Durability { get; set; } = new MaxCurrent(0, 0);

        /// <summary>
        /// Gets or sets the percent chance to block piercing attacks.
        /// </summary>
        public int Pierce { get; set; } = 0;

        /// <summary>
        /// Gets or sets the percent chance to block blunt attacks.
        /// </summary>
        public int Blunt { get; set; } = 0;

        /// <summary>
        /// Gets or sets the percent chance to block edged attacks.
        /// </summary>
        public int Edged { get; set; } = 0;

        /// <summary>
        /// Gets or sets the percent chance to block magic attacks.
        /// </summary>
        public int Magic { get; set; } = 0;

        /// <summary>
        /// Gets or sets the armor flags.
        /// </summary>
        public IList<ItemFlags> ItemFlags { get; set; } = new List<ItemFlags>();

        /// <summary>
        /// Gets or sets the wear location of this armor.
        /// </summary>
        public WearLocation WearLocation { get; set; } = WearLocation.None;
    }
}
