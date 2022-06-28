// <copyright file="IApiClient.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
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