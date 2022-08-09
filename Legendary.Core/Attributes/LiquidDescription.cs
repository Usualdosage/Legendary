// <copyright file="LiquidDescription.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Core.Attributes
{
    using System;

    /// <summary>
    /// Describes the liquid contained inside of an item.
    /// </summary>
    public class LiquidDescription : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LiquidDescription"/> class.
        /// </summary>
        /// <param name="description">The description.</param>
        public LiquidDescription(string description)
        {
            this.Description = description;
        }

        /// <summary>
        /// Gets or sets the liquid description.
        /// </summary>
        public string Description { get; set; }
    }
}