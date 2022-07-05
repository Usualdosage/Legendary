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
    using Legendary.Core.Models;
    using Legendary.Data.Contracts;
    using Legendary.Engine.Helpers;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Accesses creation tools.
    /// </summary>
    public class CreatorController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly IDataService dataService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreatorController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="dataService">The data service.</param>
        public CreatorController(ILogger<HomeController> logger, IDataService dataService)
        {
            this.logger = logger;
            this.dataService = dataService;
        }

        /// <summary>
        /// Accesses the creator page.
        /// </summary>
        /// <returns>IActionResult.</returns>
        [HttpGet]
        public IActionResult Creator()
        {
            return this.View();
        }

        /// <summary>
        /// Accesses the CreateItem page.
        /// </summary>
        /// <returns>IActionResult.</returns>
        [HttpGet]
        public IActionResult CreateItem()
        {
            return this.View();
        }

        /// <summary>
        /// Creates an item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>IActionResult.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateItem(Item? item)
        {
            if (item != null)
            {
                item.ItemId = Math.Abs(item.GetHashCode());
                await this.dataService.CreateItem(item);
                return this.View($"Item '{item.Name}' successfully created.");
            }
            else
            {
                return this.View("Failed to create item.");
            }
        }

        /// <summary>
        /// Accesses the CreateMob page.
        /// </summary>
        /// <returns>IActionResult.</returns>
        [HttpGet]
        public IActionResult CreateMob()
        {
            return this.View();
        }

        /// <summary>
        /// Creates a mob.
        /// </summary>
        /// <param name="mob">The mob.</param>
        /// <returns>IActionResult.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateMob(Character mob)
        {
            if (mob != null)
            {
                mob.CharacterId = Math.Abs(mob.GetHashCode());
                mob.IsNPC = true;
                await this.dataService.CreateMobile(mob);
                return this.View($"Mobile '{mob.FirstName}' successfully created.");
            }
            else
            {
                return this.View("Failed to create mobile.");
            }
        }
    }
}
