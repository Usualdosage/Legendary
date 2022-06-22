// <copyright file="IDatabaseSettings.cs" company="Legendary">
//  Copyright © 2021-2022 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Data.Contracts
{
    /// <summary>
    /// Database settings interface to support configurability and multi-tenant db use.
    /// </summary>
    public interface IDatabaseSettings
    {
        /// <summary>
        /// Gets or sets name for the collection.
        /// </summary>
        string? CollectionName { get; set; }

        /// <summary>
        /// Gets or sets database connection string.
        /// </summary>
        string? ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets database Name.
        /// </summary>
        string? DatabaseName { get; set; }
    }
}