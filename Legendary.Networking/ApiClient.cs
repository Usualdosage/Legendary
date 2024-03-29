﻿// <copyright file="ApiClient.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Networking
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Engine.Contracts;
    using Legendary.Networking.Models;
    using Newtonsoft.Json;
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
                this.logger.Error(exc, null);
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
                this.logger.Error(exc, null);
                return string.Empty;
            }
        }
    }
}