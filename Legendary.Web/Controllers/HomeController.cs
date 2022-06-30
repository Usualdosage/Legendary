// <copyright file="HomeController.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Web.Controllers
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Legendary.Data.Contracts;
    using Legendary.Engine.Models.Skills;
    using Legendary.Web.Contracts;
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
        private readonly IBuildSettings buildSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="dataService">The data service.</param>
        /// <param name="buildSettings">The build settings.</param>
        public HomeController(ILogger<HomeController> logger, IDataService dataService, IBuildSettings buildSettings)
        {
            this.logger = logger;
            this.dataService = dataService;
            this.buildSettings = buildSettings;
        }

        /// <summary>
        /// Displays the login page when index is called.
        /// </summary>
        /// <returns>IActionResult.</returns>
        [HttpGet]
        public IActionResult Index()
        {
            return this.View("Login", new LoginModel(string.Empty, this.buildSettings));
        }

        /// <summary>
        /// Displays the login page.
        /// </summary>
        /// <returns>IActionResult.</returns>
        /// <param name="model">The login model.</param>
        [HttpGet]
        public IActionResult Login(LoginModel model)
        {
            if (model.BuildSettings == null)
            {
                model.BuildSettings = this.buildSettings;
            }

            return this.View(model);
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
        /// <param name="message">The create message.</param>
        /// <returns>IActionResult.</returns>
        [HttpGet]
        public IActionResult CreateUser(string message)
        {
            return this.View(message);
        }

        /// <summary>
        /// Creates a character.
        /// </summary>
        /// <param name="firstName">The first name.</param>
        /// <param name="lastName">The last name.</param>
        /// <param name="password">The password.</param>
        /// <returns>IActionResult.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateCharacter(string firstName, string lastName, string password)
        {
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(password))
            {
                return this.View("CreateUser", "You need to provide a first name and a password.");
            }

            // Make sure the character doesn't exist yet.
            var character = await this.dataService.FindCharacter(c => c.FirstName == firstName);

            if (character == null)
            {
                var pwHash = Engine.Crypt.ComputeSha256Hash(password);
                await this.dataService.CreateCharacter(firstName, lastName, pwHash);
                return this.View("Login", new LoginModel("Character created. Please login.", this.buildSettings));
            }
            else
            {
                return this.View("Login", new LoginModel("That character already exists. Please login.", this.buildSettings));
            }
        }

        /// <summary>
        /// Posts login information to the home page, and handles authentication.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>IActionResult.</returns>
        [HttpPost]
        public async Task<IActionResult> Index(string username, string password)
        {
            var ipAddress = this.Request.Host.ToString();

            this.logger.LogInformation("{username} is attempting to login from {ipAddress}...", username, ipAddress);

            var userModel = new UserModel(username, password);

            var dbUser = await this.dataService.FindCharacter(c => c.FirstName == username);

            if (dbUser == null)
            {
                this.logger.LogWarning("{username} is not an existing character. Redirecting to login.", username);
                return this.View("Login", new LoginModel("That character does not exist. You should create one!", this.buildSettings));
            }

            var pwHash = Engine.Crypt.ComputeSha256Hash(password);

            if (pwHash != dbUser.Password)
            {
                this.logger.LogWarning("{username} provided an invalid password. Redirecting to login.", username);
                return this.View("Login", new LoginModel("Invalid password. Try again.", this.buildSettings));
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

                this.logger.LogInformation("{username} is logging in from {ipAddress}...", username, ipAddress);

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
