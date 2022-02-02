// <copyright file="RoomFlags.cs" company="Legendary">
//  Copyright © 2021 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Core.Types
{
    /// <summary>
    /// Flags applied to rooms.
    /// </summary>
    public enum RoomFlags : int
    {
        /// <summary>
        /// Room is always dark.
        /// </summary>
        Dark = 0,

        /// <summary>
        /// Unaffected by weather.
        /// </summary>
        Indoors = 1,
    }
}