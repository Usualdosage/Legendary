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
    public class Constants
    {
        /// <summary>
        /// Time of a tick in milliseconds.
        /// </summary>
        public const int TICK = 60000;

        /// <summary>
        /// Time of a viotick in milliseconds.
        /// </summary>
        public const int VIOTICK = 1000;

        /// <summary>
        /// Amount of health restored each tick.
        /// </summary>
        public const int STANDARD_HP_RECOVERY = 20;

        /// <summary>
        /// Amount of mana restored each tick.
        /// </summary>
        public const int STANDARD_MANA_RECOVERY = 20;

        /// <summary>
        /// Amount of movement restored each tick.
        /// </summary>
        public const int STANDARD_MOVE_RECOVERY = 20;

        /// <summary>
        /// The multiplier to standard recovery while resting.
        /// </summary>
        public const int REST_RECOVERY_MULTIPLIER = 2;

        /// <summary>
        /// The multiplier to standard recovery while sleeping.
        /// </summary>
        public const int SLEEP_RECOVERY_MULTIPLIER = 5;

        /// <summary>
        /// What mobiles will say when they have a connection error to the AI server.
        /// </summary>
        public static readonly List<string> CONNECTION_ERROR = new List<string>()
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
        public static readonly List<string> IGNORE_MESSAGE = new List<string>()
        {
            "I don't feel like talking right now.",
            "Ahem.",
            "Pardon?",
            "Did you say something to me?",
            "What?",
        };

        /// <summary>
        /// What mobiles will do when they are not talking to a user. {0} is name, {1} is pronoun.
        /// </summary>
        public static readonly List<string> EMOTE_ACTION = new List<string>()
        {
            "{0} looks around idly.",
            "{0} shrugs to {1}self.",
            "{0} smoothes the wrinkles out of {1} clothes.",
            "{0} gazes solemnly at the sky.",
            "{0} runs {1} fingers through {1} hair.",
            "{0} bites {1} lip contemplatively.",
            "{0} picks some dirt out of {1} fingernails.",
            "{0} hums a little tune.",
            "{0} looks at you curiously.",
            "{0} whistles a little tune.",
            "{0} says '<span class='say'>Looks like weather is coming.</span>'.",
        };
    }
}