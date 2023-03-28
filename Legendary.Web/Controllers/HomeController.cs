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
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Legendary.Core;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Core.Types;
    using Legendary.Data.Contracts;
    using Legendary.Engine;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Extensions;
    using Legendary.Engine.Helpers;
    using Legendary.Web.Contracts;
    using Legendary.Web.Models;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using MongoDB.Driver;
    using Newtonsoft.Json;

    /// <summary>
    /// Handles login security, authorization, and renders the main views.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly IDataService dataService;
        private readonly IBuildSettings buildSettings;
        private readonly IServerSettings serverSettings;
        private readonly IMailService mailService;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="dataService">The data service.</param>
        /// <param name="buildSettings">The build settings.</param>
        /// <param name="serverSettings">The server settings.</param>
        /// <param name="mailService">The mail service.</param>
        public HomeController(ILogger<HomeController> logger, IDataService dataService, IBuildSettings buildSettings, IServerSettings serverSettings, IMailService mailService)
        {
            this.logger = logger;
            this.dataService = dataService;
            this.buildSettings = buildSettings;
            this.serverSettings = serverSettings;
            this.mailService = mailService;
        }

        /// <summary>
        /// Health check endpoint for Azure.
        /// </summary>
        /// <returns>JsonResult.</returns>
        [Route("/Health")]
        public JsonResult Health()
        {
            return this.Json("200: Ok.");
        }

        /// <summary>
        /// Displays the login page when index is called.
        /// </summary>
        /// <returns>IActionResult.</returns>
        [HttpGet]
        [Route("/Index")]
        [Route("")]
        public async Task<IActionResult> Index()
        {
            var gameMetrics = await this.dataService.GetGameMetrics();
            return this.View("Index", new IndexModel() { GameMetrics = gameMetrics, BuildSettings = this.buildSettings });
        }

        /// <summary>
        /// Displays the login page.
        /// </summary>
        /// <returns>IActionResult.</returns>
        /// <param name="model">The login model.</param>
        [HttpGet]
        [Route("/Login")]
        public IActionResult Login(LoginModel? model)
        {
            return this.View("Login", new LoginModel(model?.Message ?? string.Empty, this.buildSettings));
        }

        /// <summary>
        /// Displays the create user page.
        /// </summary>
        /// <param name="race">The the selected race.</param>
        /// <param name="infoMessage">The information message if applicable.</param>
        /// <returns>IActionResult.</returns>
        [HttpGet]
        [Route("/CreateUser")]
        public IActionResult CreateUser(string? race, string? infoMessage)
        {
            var statModel = this.GetStatModel(race);

            if (this.TempData.ContainsKey("StatModel"))
            {
                this.TempData["StatModel"] = JsonConvert.SerializeObject(statModel);
            }
            else
            {
                this.TempData.Add("StatModel", JsonConvert.SerializeObject(statModel));
            }

            if (!string.IsNullOrWhiteSpace(infoMessage))
            {
                this.ViewData.Add("message", infoMessage);
            }

            return this.View(statModel);
        }

        /// <summary>
        /// Updates the stat model with the user's selection.
        /// </summary>
        /// <param name="race">The selected race.</param>
        /// <returns>Json stat model.</returns>
        [HttpGet]
        public JsonResult UpdateUser(string? race)
        {
            var statModel = this.GetStatModel(race);

            if (this.TempData.ContainsKey("StatModel"))
            {
                this.TempData["StatModel"] = JsonConvert.SerializeObject(statModel);
            }
            else
            {
                this.TempData.Add("StatModel", JsonConvert.SerializeObject(statModel));
            }

            return this.Json(statModel);
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
                        return this.CreateUser(null, "Nice try, bot.");
                    }

                    // Make sure the character doesn't exist yet.
                    var existingCharacter = await this.dataService.FindCharacter(c => c.FirstName.ToLower() == form["FirstName"].ToString().ToLower());

                    // Make sure we don't have a mob with this name.
                    var existingMobile = await this.dataService.FindMobile(m => m.FirstName.ToLower() == form["FirstName"].ToString().ToLower());

                    if (existingCharacter == null && existingMobile == null)
                    {
                        var character = new Character();

                        character.FirstName = form["FirstName"].ToString().FirstCharToUpper();
                        character.LastName = form["LastName"].ToString().FirstCharToUpper();
                        character.ShortDescription = form["FirstName"].ToString().FirstCharToUpper();
                        character.LongDescription = form["LongDescription"].ToString();
                        character.Health = new MaxCurrent(30, 30);
                        character.Movement = new MaxCurrent(30, 30);
                        character.Mana = new MaxCurrent(30, 30);
                        character.Str = new MaxCurrent(stats.Str, stats.Str);
                        character.Int = new MaxCurrent(stats.Int, stats.Int);
                        character.Wis = new MaxCurrent(stats.Wis, stats.Wis);
                        character.Dex = new MaxCurrent(stats.Dex, stats.Dex);
                        character.Con = new MaxCurrent(stats.Con, stats.Con);
                        character.Gender = Enum.Parse<Gender>(form["Gender"].ToString());
                        character.Race = Enum.Parse<Race>(form["SelectedRace"].ToString());
                        character.Ethos = Enum.Parse<Ethos>(form["Ethos"].ToString());
                        character.Alignment = Enum.Parse<Alignment>(form["SelectedAlignment"].ToString());
                        character.Title = "the Adventurer";

                        // Make sure the alignment is correct
                        var raceData = Races.RaceData.First(r => r.Key == character.Race);

                        if (raceData.Value.Alignments != null && !raceData.Value.Alignments.Contains(character.Alignment))
                        {
                            return this.View("CreateUser", new StatModel() { Message = "That's not a valid alignment selection for this race. Please try again." });
                        }

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

                        var rawData = form["Password"].ToString();

                        if (!string.IsNullOrWhiteSpace(rawData))
                        {
                            var pwHash = Crypt.ComputeSha256Hash(rawData);

                            character.Password = pwHash;

                            character.CharacterId = Math.Abs(character.GetHashCode());

                            await this.dataService.CreateCharacter(character);

                            try
                            {
                                await this.mailService.SendEmailMessage("m9049934009@gmail.com", "New Player", $"{character.FirstName} {character.LastName} has joined the realms as of {DateTime.Now}!");
                            }
                            catch
                            {
                                // Do nothing.
                            }

                            return this.View("Login", new LoginModel("Character created. Please login.", this.buildSettings));
                        }
                        else
                        {
                            return this.View("Login", new LoginModel("Password not provided. Please try again..", this.buildSettings));
                        }
                    }
                    else
                    {
                        return this.View("Login", new LoginModel("That character already exists. Please login.", this.buildSettings));
                    }
                }
                else
                {
                    return this.View("CreateUser", new StatModel() { Message = "Stat model has been modified. Try again." });
                }
            }
            else
            {
                return this.View("CreateUser", new StatModel() { Message = "Stat model has been modified. Try again." });
            }
        }

        /// <summary>
        /// Posts login information to the home page, and handles authentication.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>IActionResult.</returns>
        [HttpPost]
        [Route("/Game")]
        public async Task<IActionResult> Game(string username, string password)
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

                // Grab the list of users for the message center.
                var characters = await this.dataService.Characters.Find(_ => true).ToListAsync();
                userModel.Usernames = characters.Select(u => u.FirstName).ToList();

                return this.View("Game", userModel);
            }
        }

        /// <summary>
        /// Submits a message to the database.
        /// </summary>
        /// <param name="data">The message model.</param>
        /// <returns>JsonResult.</returns>
        [HttpPost]
        [Route("/Message")]
        public async Task<JsonResult> Message([FromBody]MessageModel data)
        {
            if (string.IsNullOrEmpty(data.FromAddress))
            {
                return this.Json("Player was null.");
            }

            if (data.ToAddresses == null || data.ToAddresses.Count == 0)
            {
                return this.Json("No one to send to.");
            }

            if (string.IsNullOrEmpty(data.Subject))
            {
                return this.Json("No subject provided.");
            }

            if (string.IsNullOrWhiteSpace(data.Content))
            {
                return this.Json("No content provided.");
            }

            var from = await this.dataService.FindCharacter(c => c.FirstName.ToLower() == data.FromAddress.ToLower());

            if (from == null)
            {
                return this.Json("Coold not find player.");
            }

            try
            {
                var maxMessages = await this.dataService.Messages.CountDocumentsAsync(_ => true) + 2;

                List<Command> jsonCommands = new List<Command>();

                foreach (var playerName in data.ToAddresses)
                {
                    var player = await this.dataService.FindCharacter(c => c.FirstName.ToLower() == playerName.ToLower());

                    if (player != null)
                    {
                        var message = new Message()
                        {
                            From = from.CharacterId,
                            To = player.CharacterId,
                            FromName = from.FirstName.FirstCharToUpper(),
                            ToName = player.FirstName.FirstCharToUpper(),
                            Subject = data.Subject,
                            Content = data.Content,
                            SentDate = DateTime.UtcNow,
                            MessageId = maxMessages++,
                        };

                        await this.dataService.Messages.InsertOneAsync(message);

                        var response = new Command()
                        {
                            Action = "Message",
                            Context = message.MessageId.ToString(),
                        };

                        jsonCommands.Add(response);
                    }
                }

                return this.Json(jsonCommands);
            }
            catch (Exception exc)
            {
                return this.Json(exc);
            }
        }

        /// <summary>
        /// Displays the generic error page.
        /// </summary>
        /// <returns>IActionResult.</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("/Error")]
        public IActionResult Error()
        {
            return this.View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// Gets the stat model for creating a character.
        /// </summary>
        /// <param name="race">The race of the character.</param>
        /// <returns>StatModel.</returns>
        private StatModel GetStatModel(string? race)
        {
            var random = new Legendary.Engine.Random();

            StatModel statModel = new StatModel();

            if (!string.IsNullOrWhiteSpace(race))
            {
                Race selectedRace = Enum.Parse<Race>(race);
                statModel = new StatModel() { SelectedRace = selectedRace, SelectedAlignment = Alignment.Neutral };
            }
            else
            {
                statModel = new StatModel() { SelectedRace = Race.Human, SelectedAlignment = Alignment.Neutral };
            }

            var raceStats = Races.RaceData.First(r => r.Key == statModel.SelectedRace);

            statModel.Str = random.Next(12, raceStats.Value.StrMax + 1);
            statModel.Int = random.Next(12, raceStats.Value.IntMax + 1);
            statModel.Dex = random.Next(12, raceStats.Value.DexMax + 1);
            statModel.Wis = random.Next(12, raceStats.Value.WisMax + 1);
            statModel.Con = random.Next(12, raceStats.Value.ConMax + 1);

            if (raceStats.Value.Alignments != null)
            {
                statModel.Alignments = new List<Alignment>();
                foreach (var align in raceStats.Value.Alignments)
                {
                    statModel.Alignments?.Add(align);
                }
            }

            return statModel;
        }
    }
}
