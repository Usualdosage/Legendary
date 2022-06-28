// <copyright file="WearLocation.cs" company="Legendary™">
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
    /// Defines the possible wear location on a player for items.
    /// </summary>
    public enum WearLocation : short
    {
        /// <summary>
        /// Inventory.
        /// </summary>
        Inventory = 0,

        /// <summary>
        /// Head.
        /// </summary>
        Head = 1,

        /// <summary>
        /// Torso.
        /// </summary>
        Torso = 2,

        /// <summary>
        /// Arms.
        /// </summary>
        Arms = 3,

        /// <summary>
        /// Legs.
        /// </summary>
        Legs = 4,

        /// <summary>
        /// Waist.
        /// </summary>
        Waist = 5,

        /// <summary>
        /// Neck.
        /// </summary>
        Neck = 6,

        /// <summary>
        /// Left wrist.
        /// </summary>
        LWrist = 7,

        /// <summary>
        /// Right wrist.
        /// </summary>
        RWist = 8,

        /// <summary>
        /// Feet.
        /// </summary>
        Feet = 9,

        /// <summary>
        /// Face.
        /// </summary>
        Face = 10,

        /// <summary>
        /// Left ear.
        /// </summary>
        LEar = 11,

        /// <summary>
        /// Right ear.
        /// </summary>
        REar = 12,

        /// <summary>
        /// Hands.
        /// </summary>
        Hands = 13,

        /// <summary>
        /// Right finger.
        /// </summary>
        RFinger = 14,

        /// <summary>
        /// Left finger.
        /// </summary>
        LFinder = 15,

        /// <summary>
        /// Floating.
        /// </summary>
        Floating = 16,

        /// <summary>
        /// Held.
        /// </summary>
        Held = 17,

        /// <summary>
        /// Primary.
        /// </summary>
        Wielded = 18,

        /// <summary>
        /// Secondary.
        /// </summary>
        DualWielded = 19,

        /// <summary>
        /// Shield.
        /// </summary>
        Shield = 20,
    }
}
