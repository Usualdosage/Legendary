// <copyright file="MinimumLevelAttribute.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Attributes
{
    using System;

    /// <summary>
    /// Used to mark methods that require a level requirement to use.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class MinimumLevelAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MinimumLevelAttribute"/> class.
        /// </summary>
        /// <param name="level">The level.</param>
        public MinimumLevelAttribute(int level)
        {
            this.Level = level;
        }

        /// <summary>
        /// Gets or sets the wear description.
        /// </summary>
        public int Level { get; set; }
    }
}