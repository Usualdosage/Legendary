// <copyright file="ContentController.cs" company="Legendary">
//  Copyright © 2021 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
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

        /// <summary>
        /// Displays the welcome message.
        /// </summary>
        /// <param name="roomId">The room number.</param>
        /// <returns>HTML string.</returns>
        [HttpGet]
        [Route("room")]
        public async Task<string> Room(string roomId)
        {
            string imagePath = $"{this.webHostEnvironment.WebRootPath}/img/rooms/{roomId}.jpg";

            if (System.IO.File.Exists(imagePath))
            {
                byte[] imageArray = await System.IO.File.ReadAllBytesAsync(imagePath);
                return Convert.ToBase64String(imageArray);
            }
            else
            {
                return string.Empty;
            }
        }
    }
}



