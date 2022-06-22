// <copyright file="DatabaseSettings.cs" company="Legendary">
//  Copyright © 2021-2022 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Data
{
    using Legendary.Data.Contracts;

    /// <summary>
    /// Database settings class.
    /// </summary>
    public class DatabaseSettings : IDatabaseSettings
    {
        /// <summary>
        /// Gets or sets conversation log collection name.
        /// </summary>
        public string? CollectionName { get; set; }

        /// <summary>
        /// Gets or sets connection string.
        /// </summary>
        public string? ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets database name.
        /// </summary>
        public string? DatabaseName { get; set; }
    }
}