// <copyright file="ApiClient.cs" company="Legendary">
//  Copyright © 2021 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Networking
{
    using System;
    using System.Threading.Tasks;
    using Legendary.Core.Contracts;
    using Legendary.Engine.Contracts;
    using Legendary.Networking.Contracts;
    using RestSharp;

    /// <summary>
    /// Handles calls to the we server API to fetch and post content.
    /// </summary>
    public class ApiClient : IApiClient
    {
        private readonly IServerSettings settings;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiClient"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="settings">The server settings.</param>
        public ApiClient(ILogger logger, IServerSettings settings)
        {
            this.settings = settings;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves dynamic content by calling the web server API.
        /// </summary>
        /// <param name="endpoint">The API method to call.</param>
        /// <returns>HTML string.</returns>
        public async Task<string?> GetContent(string endpoint)
        {
            try
            {
                var client = new RestClient($"{this.settings.ApiUrl}/api/content/");
                var request = new RestRequest($"{endpoint}", Method.Get);
                var result = await client.ExecuteAsync(request);

                var html = System.Web.HttpUtility.HtmlDecode(result.Content);

                return html?.Replace("\\n", string.Empty).Replace("\\r", string.Empty).Replace("\"", string.Empty);
            }
            catch (Exception exc)
            {
                await this.logger.Error(exc);
                return $"<h3>An Exception Occurred</h3><p>{exc}</p>";
            }
        }

        /// <summary>
        /// Retrieves dynamic content (raw) by calling the web server API. Used for fetching base-64 encoded images.
        /// </summary>
        /// <param name="endpoint">The API method to call.</param>
        /// <returns>Base-64 string.</returns>
        public async Task<string?> GetRawContent(string endpoint)
        {
            try
            {
                var client = new RestClient($"{this.settings.ApiUrl}/api/content/");
                var request = new RestRequest($"{endpoint}", Method.Get);
                var result = await client.ExecuteAsync(request);
                return result?.Content?.Replace("\\n", string.Empty).Replace("\\r", string.Empty).Replace("\"", string.Empty);
            }
            catch (Exception exc)
            {
                await this.logger.Error(exc);
                return string.Empty;
            }
        }
    }
}