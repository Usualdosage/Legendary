// <copyright file="EquipmentReset.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Core.Types
{
    using System;

    /// <summary>
    ///  Resets for mobile equipment.
    /// </summary>
    public class EquipmentReset
    {
        /// <summary>
        /// Gets or sets the item Id.
        /// </summary>
        public long ItemId { get; set; }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        public WearLocation WearLocation { get; set; }
    }
}