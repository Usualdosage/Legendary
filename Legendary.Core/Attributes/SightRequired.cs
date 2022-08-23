// <copyright file="SightRequired.cs" company="Legendary™">
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
    /// Attribute to indicate the player must be able to see to use this action.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class SightRequired : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SightRequired"/> class.
        /// </summary>
        public SightRequired()
        {
        }
    }
}