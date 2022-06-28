// <copyright file="MaxCurrent.cs" company="Legendary™">
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
    /// Represents a max/current object for things like health.
    /// </summary>
    public class MaxCurrent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaxCurrent"/> class.
        /// </summary>
        /// <param name="max">The max value.</param>
        /// <param name="current">The current value.</param>
        public MaxCurrent(double max, double current)
        {
            this.Max = max;
            this.Current = current;
        }

        /// <summary>
        /// Gets or sets the Max.
        /// </summary>
        public double Max { get; set; }

        /// <summary>
        /// Gets or sets the current.
        /// </summary>
        public double Current { get; set; }
    }
}