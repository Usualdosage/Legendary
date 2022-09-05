// <copyright file="ContentController.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Web.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Legendary.Engine.Helpers;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Serves up content from the content folder to be consumed by the server.
    /// </summary>
    [Route("api/content")]
    [ApiController]
    public class ContentController : Controller
    {
        private readonly IWebHostEnvironment webHostEnvironment;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentController"/> class.
        /// </summary>
        /// <param name="webHostEnvironment">The hosting environment.</param>
        public ContentController(IWebHostEnvironment webHostEnvironment)
        {
            this.webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// Displays the welcome message.
        /// </summary>
        /// <param name="playerName">The name of the player.</param>
        /// <returns>HTML string.</returns>
        [HttpGet]
        [Route("welcome")]
        public async Task<string> Welcome(string playerName)
        {
            var content = await this.RenderViewAsync<string>("Welcome", playerName, true);
            return content;
        }
    }
}
