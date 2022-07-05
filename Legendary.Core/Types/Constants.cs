// <copyright file="Constants.cs" company="Legendary™">
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
    }
}