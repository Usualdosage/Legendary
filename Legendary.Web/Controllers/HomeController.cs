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
    using Legendary.Data.Contracts;
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
        private readonly IDataService dataService;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="dataService">The data service.</param>
        public HomeController(ILogger<HomeController> logger, IDataService dataService)
        {
            this.logger = logger;
            this.dataService = dataService;
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
        public IActionResult Login(string? message)
        {
            return this.View(message);
        }

        [HttpGet]
        public IActionResult Health()
        {
            List<string> messages = new();

            // See if we can connect to Mongo
            messages.Add($"Can connect to database: {this.dataService.TestConnection()}");

            // See if we can hit our own API

            var healthModel = new HealthModel(messages);

            return this.View("Health", healthModel);
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
        /// Displays the create user page.
        /// </summary>
        /// <returns>IActionResult.</returns>
        [HttpGet]
        public IActionResult CreateUser()
        {
            return this.View();
        }

        /// <summary>
        /// Creates a character.
        /// </summary>
        /// <returns>IActionResult.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateCharacter(string firstName, string lastName, string password)
        {
            var pwHash = Engine.Crypt.ComputeSha256Hash(password);
            await this.dataService.CreateCharacter(firstName, lastName, pwHash);
            return this.View("Login", "Character created. Please login.");
        }

        /// <summary>
        /// Posts login information to the home page, and handles authentication.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns>IActionResult.</returns>
        [HttpPost]
        public async Task<IActionResult> Index(string username, string password)
        {
            var ipAddress = Request.Host.ToString();

            logger.LogInformation($"{username} is attempting to login from {ipAddress}...");

            var userModel = new UserModel(username, password);

            var dbUser = await this.dataService.FindCharacter(c => c.FirstName == username);

            if (dbUser == null)
            {
                logger.LogWarning($"{username} is not an existing character. Redirecting to login.");
                return this.View("Login", "That character does not exist. You should create one!");
            }

            var pwHash = Engine.Crypt.ComputeSha256Hash(password);

            if (pwHash != dbUser.Password)
            {
                logger.LogWarning($"{username} provided an invalid password. Redirecting to login.");
                return this.View("Login", "Invalid password. Try again.");
            }
            else
            {
                // User has authenticated, so move along and log them in.
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, userModel.UserName),
                    new Claim(ClaimTypes.Role, "User"),
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    AllowRefresh = true,
                };

                await this.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                logger.LogInformation($"{username} is logging in from {ipAddress}...");

                return this.View("Index", userModel);
            }
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
