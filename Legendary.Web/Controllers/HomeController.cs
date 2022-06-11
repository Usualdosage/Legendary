// <copyright file="HomeController.cs" company="Legendary">
//  Copyright © 2021 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Web.Controllers
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Legendary.Web.Models;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Handles login security, authorization, and renders the main views.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public HomeController(ILogger<HomeController> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Displays the login page when index is called.
        /// </summary>
        /// <returns>IActionResult.</returns>
        [HttpGet]
        public IActionResult Index()
        {
            return this.View("Login");
        }

        /// <summary>
        /// Displays the login page.
        /// </summary>
        /// <returns>IActionResult.</returns>
        [HttpGet]
        public IActionResult Login()
        {
            return this.View();
        }

        /// <summary>
        /// Displays the create room page.
        /// </summary>
        /// <returns>IActionResult.</returns>
        [HttpGet]
        public IActionResult CreateRoom()
        {
            return this.View();
        }

        /// <summary>
        /// Posts login information to the home page, and handles authentication.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns>IActionResult.</returns>
        [HttpPost]
        public async Task<IActionResult> Index(string username)
        {
            // TO-DO: Authenticate user via Mongo. Then, sign them in.
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, "User"),
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
            };

            await this.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            return this.View("Index", username);
        }

        /// <summary>
        /// Displays the generic error page.
        /// </summary>
        /// <returns>IActionResult.</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return this.View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
        }
    }
}
