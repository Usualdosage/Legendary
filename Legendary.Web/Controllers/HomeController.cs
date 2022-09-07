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
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Legendary.Core;
    using Legendary.Core.Models;
    using Legendary.Core.Types;
    using Legendary.Data.Contracts;
    using Legendary.Engine;
    using Legendary.Web.Contracts;
    using Legendary.Web.Models;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

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
        /// Health check endpoint for Azure.
        /// </summary>
        /// <returns>JsonResult.</returns>
        public JsonResult Health()
        {
            return this.Json("200: Ok.");
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

            return this.View("Login", model);
        }

        /// <summary>
        /// Displays the create user page.
        /// </summary>
        /// <returns>IActionResult.</returns>
        [HttpGet]
        public IActionResult CreateUser()
        {
            var random = new Legendary.Engine.Random();

            var statModel = new StatModel();

            statModel.Str = random.Next(12, 19);
            statModel.Int = random.Next(12, 19);
            statModel.Dex = random.Next(12, 19);
            statModel.Wis = random.Next(12, 19);
            statModel.Con = random.Next(12, 19);

            if (this.TempData.ContainsKey("StatModel"))
            {
                this.TempData["StatModel"] = JsonConvert.SerializeObject(statModel);
            }
            else
            {
                this.TempData.Add("StatModel", JsonConvert.SerializeObject(statModel));
            }

            return this.View(statModel);
        }

        /// <summary>
        /// Creates a character.
        /// </summary>
        /// <returns>IActionResult.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateCharacter()
        {
            var form = this.Request.Form;

            var tempDataStatModel = this.TempData["StatModel"]?.ToString();

            if (tempDataStatModel != null)
            {
                var stats = JsonConvert.DeserializeObject<StatModel>(tempDataStatModel);

                if (stats != null)
                {
                    // This field would only be submitted by a bot.
                    if (form["submittalfield"] != string.Empty)
                    {
                        return this.CreateUser();
                    }

                    // Make sure the character doesn't exist yet.
                    var existingCharacter = await this.dataService.FindCharacter(c => c.FirstName.ToLower() == form["FirstName"].ToString().ToLower());

                    // Make sure we don't have a mob with this name.
                    var existingMobile = await this.dataService.FindMobile(m => m.FirstName.ToLower() == form["FirstName"].ToString().ToLower());

                    if (existingCharacter == null && existingMobile == null)
                    {
                        var character = new Character()
                        {
                            FirstName = form["FirstName"],
                            LastName = form["LastName"],
                            ShortDescription = form["FirstName"],
                            LongDescription = form["LongDescription"],
                            Health = new MaxCurrent(30, 30),
                            Movement = new MaxCurrent(30, 30),
                            Mana = new MaxCurrent(30, 30),
                            Str = new MaxCurrent(stats.Str, stats.Str),
                            Int = new MaxCurrent(stats.Int, stats.Int),
                            Wis = new MaxCurrent(stats.Wis, stats.Wis),
                            Dex = new MaxCurrent(stats.Dex, stats.Dex),
                            Con = new MaxCurrent(stats.Con, stats.Con),
                            Gender = Enum.Parse<Gender>(form["Gender"]),
                            Race = Enum.Parse<Race>(form["Race"]),
                            Ethos = Enum.Parse<Ethos>(form["Ethos"]),
                            Alignment = Enum.Parse<Alignment>(form["Alignment"]),
                            Title = "the Adventurer",
                        };

                        switch (character.Alignment)
                        {
                            case Alignment.Good:
                                {
                                    character.Home = new KeyValuePair<long, long>(Constants.GRIFFONSHIRE_AREA, Constants.GRIFFONSHIRE_LIGHT_TEMPLE);
                                    character.Location = character.Home;
                                    break;
                                }

                            case Alignment.Evil:
                                {
                                    character.Home = new KeyValuePair<long, long>(Constants.GRIFFONSHIRE_AREA, Constants.GRIFFONSHIRE_DARK_TEMPLE);
                                    character.Location = character.Home;
                                    break;
                                }

                            case Alignment.Neutral:
                                {
                                    character.Home = new KeyValuePair<long, long>(Constants.GRIFFONSHIRE_AREA, Constants.GRIFFONSHIRE_NEUTRAL_TEMPLE);
                                    character.Location = character.Home;
                                    break;
                                }
                        }

                        var pwHash = Crypt.ComputeSha256Hash(form["Password"]);
                        character.Password = pwHash;

                        character.CharacterId = Math.Abs(character.GetHashCode());

                        var avatarUrl = form["AvatarUrl"];

                        if (!string.IsNullOrWhiteSpace(avatarUrl))
                        {
                            // Save the avatar to the player. TODO: Upload this file to our server instead.
                            character.Image = avatarUrl;
                        }

                        await this.dataService.CreateCharacter(character);

                        return this.View("Login", new LoginModel("Character created. Please login.", this.buildSettings));
                    }
                    else
                    {
                        return this.View("Login", new LoginModel("That character already exists. Please login.", this.buildSettings));
                    }
                }
                else
                {
                    return this.View("Login", new LoginModel("Stat model has been modified. Try again.", this.buildSettings));
                }
            }
            else
            {
                return this.View("Login", new LoginModel("Stat model has been modified. Try again.", this.buildSettings));
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

            var pwHash = Crypt.ComputeSha256Hash(password);

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
