// <copyright file="IApiClient.cs" company="Legendary">
//  Copyright © 2021-2022 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Core.Contracts
{
    using System.Threading.Tasks;

    /// <summary>
    /// Implemenation contract for an Api Client.
    /// </summary>
    public interface IApiClient
    {
        /// <summary>
        /// Retrieves dynamic content by calling the web server API.
        /// </summary>
        /// <param name="endpoint">The API method to call.</param>
        /// <returns>HTML string.</returns>
        Task<string?> GetContent(string endpoint);

        /// <summary>
        /// Retrieves dynamic content (raw) by calling the web server API. Used for fetching base-64 encoded images.
        /// </summary>
        /// <param name="endpoint">The API method to call.</param>
        /// <returns>Base-64 string.</returns>
        Task<string?> GetRawContent(string endpoint);
    }
}