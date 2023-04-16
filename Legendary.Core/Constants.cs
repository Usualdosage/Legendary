// <copyright file="Constants.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Core
{
    using System.Collections.Generic;

    /// <summary>
    /// Game constants.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// This is where all new players begin.
        /// </summary>
        public const long START_AREA = 31866;

        /// <summary>
        /// This is where all new players begin.
        /// </summary>
        public const long START_ROOM = 32075;

        /// <summary>
        /// The Area ID for Griffonshire, the hometown.
        /// </summary>
        public const long GRIFFONSHIREAREA = 6828;

        /// <summary>
        /// The light temple in Griffonshire for recall purposes.
        /// </summary>
        public const long GRIFFONSHIRE_LIGHT_TEMPLE = 6956;

        /// <summary>
        /// The dark temple in Griffonshire for recall purposes.
        /// </summary>
        public const long GRIFFONSHIRE_DARK_TEMPLE = 7055;

        /// <summary>
        /// The neutral temple in Griffonshire for recall purposes.
        /// </summary>
        public const long GRIFFONSHIRE_NEUTRAL_TEMPLE = 7017;

        /// <summary>
        /// Time of a tick in seconds * 2. (15 = 30 seconds).
        /// </summary>
        public const int TICK = 15;

        /// <summary>
        /// Time of a viotick in milliseconds.
        /// </summary>
        public const int VIOTICK = 2000;

        /// <summary>
        /// Food items that come from create food.
        /// </summary>
        public const long ITEM_FOOD = 9000000;

        /// <summary>
        /// Spring items that come from create spring.
        /// </summary>
        public const long ITEM_SPRING = 9000001;

        /// <summary>
        /// Mobile corpse.
        /// </summary>
        public const long ITEM_CORPSE = 9000002;

        /// <summary>
        /// Mobile corpse.
        /// </summary>
        public const long ITEM_LIGHT = 9000003;

        /// <summary>
        /// Basic armor item (from outfit).
        /// </summary>
        public const long ITEM_BASIC_ARMOR = 9000004;

        /// <summary>
        /// Basic weapon item (from outfit).
        /// </summary>
        public const long ITEM_BASIC_WEAPON = 9000005;

        /// <summary>
        /// Loot armor item (from mob loot drop).
        /// </summary>
        public const long ITEM_LOOT_ARMOR = 9000006;

        /// <summary>
        /// Loot weapon item (from mob loot drop).
        /// </summary>
        public const long ITEM_LOOT_WEAPON = 9000007;

        /// <summary>
        /// Random (autospawn) mobile.
        /// </summary>
        public const long RANDOM_MOBILE = 9000008;

        /// <summary>
        /// Amount of health restored each tick.
        /// </summary>
        public const int STANDARD_HP_RECOVERY = 50;

        /// <summary>
        /// Amount of mana restored each tick.
        /// </summary>
        public const int STANDARD_MANA_RECOVERY = 50;

        /// <summary>
        /// Amount of movement restored each tick.
        /// </summary>
        public const int STANDARD_MOVE_RECOVERY = 50;

        /// <summary>
        /// The multiplier to standard recovery while resting.
        /// </summary>
        public const int REST_RECOVERY_MULTIPLIER = 2;

        /// <summary>
        /// The multiplier to standard recovery while sleeping.
        /// </summary>
        public const int SLEEP_RECOVERY_MULTIPLIER = 5;

        /// <summary>
        /// The standard hit dice for any player.
        /// </summary>
        public const int STANDARD_PLAYER_HITDICE = 1;

        /// <summary>
        /// The standard damage dice for any player.
        /// </summary>
        public const int STANDARD_PLAYER_DAMDICE = 4;

        /// <summary>
        /// Gets the level at which a player is considered immortal (staff).
        /// </summary>
        public const int WIZLEVEL = 90;

        /// <summary>
        /// What mobiles will say when they have a connection error to the AI server.
        /// </summary>
        public static readonly List<string> CONNECTION_ERROR = new ()
        {
            "I'm sorry, I didn't understand that.",
            "I'm not sure what to say.",
            "Hm. I can't think of a response to that.",
            "Sorry, I'm not thinking straight right now.",
            "I'm a little confused.",
            "Maybe ask me that again? I'm not sure I heard you right.",
            "I'm a little tired and not thinking straight right now.",
        };

        /// <summary>
        /// What mobiles will say when they are ignoring a user.
        /// </summary>
        public static readonly List<string> IGNORE_MESSAGE = new ()
        {
            "I don't feel like talking right now.",
            "Ahem.",
            "Pardon?",
            "Did you say something to me?",
            "What?",
        };

        /// <summary>
        /// What happy mobiles will do when they are not talking to a user. {0} is name, {1} is pronoun.
        /// </summary>
        public static readonly List<string> EMOTE_ACTION_HAPPY = new ()
        {
            "{0} looks around with a smile.",
            "{0} smiles happily.",
            "{0} shrugs innocently.",
            "{0} gazes deeply at the sky in awe.",
            "{0} runs {1} fingers through {1} hair.",
            "{0} bites {1} lip contemplatively.",
            "{0} picks some dirt out of {1} fingernails.",
            "{0} hums a little tune.",
            "{0} laughs a little bit.",
            "{0} looks at you curiously.",
            "{0} whistles a little tune.",
            "{0} says '<span class='say'>I love this kind of day!</span>'.",
            "{0} smiles and says '<span class='say'>Hi there!</span>'.",
        };

        /// <summary>
        /// What angry mobiles will do when they are not talking to a user. {0} is name, {1} is pronoun.
        /// </summary>
        public static readonly List<string> EMOTE_ACTION_ANGRY = new ()
        {
            "{0} looks around angrily.",
            "{0} shrugs, shaking {1} head.",
            "{0} grumbles idly.",
            "{0} growls under {1} breath.",
            "{0} looks at you with a snarl.",
            "{0} glares at you.",
            "{0} says '<span class='say'>Looks like crappy weather is coming.</span>'.",
            "{0} says '<span class='say'>Ugh, I hate it here.</span>'.",
            "{0} mutters something under {1} breath.",
            "{0} says '<span class='say'>What? What are you looking at?</span>'.",
            "{0} says '<span class='say'>Another day where eveything sucks.</span>'.",
        };

        /// <summary>
        /// What neutral mobiles will do when they are not talking to a user. {0} is name, {1} is pronoun.
        /// </summary>
        public static readonly List<string> EMOTE_ACTION_NEUTRAL = new ()
        {
            "{0} looks around idly.",
            "{0} shrugs.",
            "{0} yawns out of boredom.",
            "{0} gazes around, a bored expression on {1} face.",
            "{0} runs {1} fingers through {1} hair.",
            "{0} bites {1} lip contemplatively.",
            "{0} picks some dirt out of {1} fingernails.",
            "{0} looks at you ambivalently.",
            "{0} shrugs apathetically.",
        };

        /// <summary>
        /// What sad mobiles will do when they are not talking to a user. {0} is name, {1} is pronoun.
        /// </summary>
        public static readonly List<string> EMOTE_ACTION_SAD = new ()
        {
            "{0} looks around in sorrow.",
            "{0} wipes some tears from {1} eyes.",
            "{0} shrugs, shaking {1} head.",
            "{0} gazes solemnly at the ground.",
            "{0} nervously twists {1} hair.",
            "{0} sighs, releasing {1} breath shakily.",
            "{0} bites {1} lip sullenly.",
            "{0} bows {1} head sadly.",
            "{0} says '<span class='say'>Today is so depressing.</span>'.",
            "{0} says '<span class='say'>I wish it would rain.</span>'.",
            "{0} says '<span class='say'>I love the color gray.</span>'.",
        };

        /// <summary>
        /// What animal mobiles will do. {0} is name, {1} is pronoun.
        /// </summary>
        public static readonly List<string> EMOTE_ANIMAL_ACTION = new ()
        {
            "{0} scurries about.",
            "{0} sniffs the air.",
            "{0} begins to clean {1}self.",
            "{0} darts away quickly.",
            "{0} peers at you curiously.",
            "{0} sniffs the ground.",
            "{0} makes some subtle little noises.",
            "{0} scratches {1}self.",
            "{0} whines pitifully.",
            "{0} nibbles on some food.",
        };
    }
}