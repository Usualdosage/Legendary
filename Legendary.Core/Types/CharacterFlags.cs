// <copyright file="CharacterFlags.cs" company="Legendary™">
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
    /// Flags that can be applied to a character (or an NPC).
    /// </summary>
    public enum CharacterFlags
    {
        /// <summary>
        /// No flags applied.
        /// </summary>
        None = 0,

        /// <summary>
        /// Character is fighting.
        /// </summary>
        Fighting = 1,

        /// <summary>
        /// Character is charmed.
        /// </summary>
        Charmed = 2,

        /// <summary>
        /// Character is resting.
        /// </summary>
        Resting = 3,

        /// <summary>
        /// Character is sleeping.
        /// </summary>
        Sleeping = 4,

        /// <summary>
        /// Character is dead.
        /// </summary>
        Dead = 5,

        /// <summary>
        /// Character is a ghost.
        /// </summary>
        Ghost = 6,
    }
}