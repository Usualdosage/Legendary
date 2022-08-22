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
    using Legendary.Core.Attributes;

    /// <summary>
    /// Defines the possible wear location on a player for items.
    /// </summary>
    public enum WearLocation : short
    {
        /// <summary>
        /// Cannot be worn.
        /// </summary>
        None = 0,

        /// <summary>
        /// Inventory.
        /// </summary>
        InventoryOnly = 1,

        /// <summary>
        /// Head.
        /// </summary>
        [WearDescription("Worn on head")]
        Head = 2,

        /// <summary>
        /// Torso.
        /// </summary>
        [WearDescription("Worn on body")]
        Torso = 3,

        /// <summary>
        /// Arms.
        /// </summary>
        [WearDescription("Worn on arms")]
        Arms = 4,

        /// <summary>
        /// Legs.
        /// </summary>
        [WearDescription("Worn on legs")]
        Legs = 5,

        /// <summary>
        /// Waist.
        /// </summary>
        [WearDescription("Worn around waist")]
        Waist = 6,

        /// <summary>
        /// Neck.
        /// </summary>
        [WearDescription("Worn around neck")]
        Neck = 7,

        /// <summary>
        /// Left wrist.
        /// </summary>
        [WearDescription("Worn on left wrist")]
        LWrist = 8,

        /// <summary>
        /// Right wrist.
        /// </summary>
        [WearDescription("Worn on right wrist")]
        RWist = 9,

        /// <summary>
        /// Feet.
        /// </summary>
        [WearDescription("Worn on feet")]
        Feet = 10,

        /// <summary>
        /// Face.
        /// </summary>
        [WearDescription("Worn on face")]
        Face = 11,

        /// <summary>
        /// Left ear.
        /// </summary>
        [WearDescription("Worn in left ear")]
        LEar = 12,

        /// <summary>
        /// Right ear.
        /// </summary>
        [WearDescription("Worn in right ear")]
        REar = 13,

        /// <summary>
        /// Hands.
        /// </summary>
        [WearDescription("Worn on hands")]
        Hands = 14,

        /// <summary>
        /// Right finger.
        /// </summary>
        [WearDescription("Worn on right finger")]
        RFinger = 15,

        /// <summary>
        /// Left finger.
        /// </summary>
        [WearDescription("Worn on left finger")]
        LFinder = 16,

        /// <summary>
        /// Floating.
        /// </summary>
        [WearDescription("Floating nearby")]
        Floating = 17,

        /// <summary>
        /// Held.
        /// </summary>
        [WearDescription("Held in hand")]
        Held = 18,

        /// <summary>
        /// Primary.
        /// </summary>
        [WearDescription("Wielded")]
        Wielded = 19,

        /// <summary>
        /// Secondary.
        /// </summary>
        [WearDescription("Wielded in off hand")]
        DualWielded = 20,

        /// <summary>
        /// Shield.
        /// </summary>
        [WearDescription("Worn as a shield")]
        Shield = 21,

        /// <summary>
        /// Shield.
        /// </summary>
        [WearDescription("Worn about the body")]
        Body = 22,

        /// <summary>
        /// Shield.
        /// </summary>
        [WearDescription("Used as a light")]
        Light = 23,
    }
}
