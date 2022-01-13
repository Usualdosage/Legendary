// <copyright file="IDataService.cs" company="Legendary">
//  Copyright © 2021 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Data.Contracts
{
    using Legendary.Core.Models;

    /// <summary>
    /// Implementation contract for interacting with a database.
    /// </summary>
    public interface IDataService
    {
        /// <summary>
        /// Loads the world into memory.
        /// </summary>
        /// <returns>The current world.</returns>
        public World? LoadWorld();
    }
}