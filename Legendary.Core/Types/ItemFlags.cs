// <copyright file="ItemFlags.cs" company="Legendary">
//  Copyright © 2021-2022 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Core.Types
{
    /// <summary>
    /// Flags that can be applied to items.
    /// </summary>
    public enum ItemFlags : short
    {
        /// <summary>
        /// No flags applied.
        /// </summary>
        None = 0,

        /// <summary>
        /// Glowing.
        /// </summary>
        Glowing = 1,

        /// <summary>
        /// Humming.
        /// </summary>
        Humming = 2,

        /// <summary>
        /// Sharp.
        /// </summary>
        Sharp = 3,
    }
}