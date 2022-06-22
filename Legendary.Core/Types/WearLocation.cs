// <copyright file="WearLocation.cs" company="Legendary">
//  Copyright © 2021-2022 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
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

