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
    using System.Threading.Tasks;
    using Legendary.Engine.Helpers;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Serves up content from the content folder to be consumed by the server.
    /// </summary>
    [Route("api/content")]
    [ApiController]
    public class ContentController : Controller
    {
        /// <summary>
        /// Displays the welcome message.
        /// </summary>
        /// <param name="playerName">The name of the player.</param>
        /// <returns>HTML string.</returns>
        [HttpGet]
        [AllowAnonymous]
        [Route("welcome")]
        public async Task<string> Welcome(string playerName)
        {
            var content = await this.RenderViewAsync<string>("Welcome", playerName, true);
            return content;
        }
    }
}
