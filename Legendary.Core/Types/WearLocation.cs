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
        [WearDescription("Worn on head", "on your head", "from your head")]
        Head = 2,

        /// <summary>
        /// Torso.
        /// </summary>
        [WearDescription("Worn on body", "around your body", "from around your body")]
        Torso = 3,

        /// <summary>
        /// Arms.
        /// </summary>
        [WearDescription("Worn on arms", "on your arms", "from your arms")]
        Arms = 4,

        /// <summary>
        /// Legs.
        /// </summary>
        [WearDescription("Worn on legs", "on your legs", "from your legs")]
        Legs = 5,

        /// <summary>
        /// Waist.
        /// </summary>
        [WearDescription("Worn around waist", "around your waist", "from around your waist")]
        Waist = 6,

        /// <summary>
        /// Neck.
        /// </summary>
        [WearDescription("Worn around neck", "around your neck", "from your around your neck")]
        Neck = 7,

        /// <summary>
        /// Left wrist.
        /// </summary>
        [WearDescription("Worn on left wrist", "on your left wrist", "from your left wrist")]
        LWrist = 8,

        /// <summary>
        /// Right wrist.
        /// </summary>
        [WearDescription("Worn on right wrist", "on your right wrist", "from your right wrist")]
        RWrist = 9,

        /// <summary>
        /// Feet.
        /// </summary>
        [WearDescription("Worn on feet", "on your feet", "from your feet")]
        Feet = 10,

        /// <summary>
        /// Face.
        /// </summary>
        [WearDescription("Worn on face", "on your face", "from your face")]
        Face = 11,

        /// <summary>
        /// Left ear.
        /// </summary>
        [WearDescription("Worn in left ear", "in your left ear", "from your left ear")]
        LEar = 12,

        /// <summary>
        /// Right ear.
        /// </summary>
        [WearDescription("Worn in right ear", "in your right ear", "from your right ear")]
        REar = 13,

        /// <summary>
        /// Hands.
        /// </summary>
        [WearDescription("Worn on hands", "on your hands", "from your hands")]
        Hands = 14,

        /// <summary>
        /// Right finger.
        /// </summary>
        [WearDescription("Worn on right finger", "on your right finger", "from your right finger")]
        RFinger = 15,

        /// <summary>
        /// Left finger.
        /// </summary>
        [WearDescription("Worn on left finger", "on your left finger", "from your left finger")]
        LFinger = 16,

        /// <summary>
        /// Floating.
        /// </summary>
        [WearDescription("Floating nearby", "and it floats beside you", "from floating beside you")]
        Floating = 17,

        /// <summary>
        /// Held.
        /// </summary>
        [WearDescription("Held in hand", "in your hand", "from your hand")]
        Held = 18,

        /// <summary>
        /// Primary.
        /// </summary>
        [WearDescription("Wielded", "as your primary weapon", "as your primary weapon")]
        Wielded = 19,

        /// <summary>
        /// Secondary.
        /// </summary>
        [WearDescription("Wielded in off hand", "as your secondary weapon", "as your secondary weapon")]
        DualWielded = 20,

        /// <summary>
        /// Shield.
        /// </summary>
        [WearDescription("Worn as a shield", "as a shield", "from your shield arm")]
        Shield = 21,

        /// <summary>
        /// Body.
        /// </summary>
        [WearDescription("Worn about the body", "about your body", "from about your body")]
        Body = 22,

        /// <summary>
        /// Light.
        /// </summary>
        [WearDescription("Used as a light", "as a light", "as your light")]
        Light = 23,

        /// <summary>
        /// Tattoo (special).
        /// </summary>
        [WearDescription("Tattooed", "as a tattoo", "from your body")]
        Tattoo = 24,
    }
}
