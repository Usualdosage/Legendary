// <copyright file="CharacterFlags.cs" company="Legendary">
//  Copyright © 2021-2022 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
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
    }
}