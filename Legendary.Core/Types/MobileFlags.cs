// <copyright file="MobileFlags.cs" company="Legendary">
//  Copyright © 2021 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Core.Types
{
    /// <summary>
    /// Flags that can be applied to a mobile.
    /// </summary>
    public enum MobileFlags : short
    {
        /// <summary>
        /// No flags applied.
        /// </summary>
        None = 0,

        /// <summary>
        /// Whether the mob is aggressive or not.
        /// </summary>
        Aggressive = 1,
    }
}