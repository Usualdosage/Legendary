// <copyright file="LoginModel.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Web.Models
{
    using Legendary.Web.Contracts;

    /// <summary>
    /// The login model.
    /// </summary>
    public class LoginModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoginModel"/> class.
        /// </summary>
        public LoginModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginModel"/> class.
        /// </summary>
        /// <param name="message">The login message.</param>
        /// <param name="buildSettings">The build settings.</param>
        public LoginModel(string? message, IBuildSettings? buildSettings)
        {
            this.Message = message;
            this.BuildSettings = buildSettings;
        }

        /// <summary>
        /// Gets the displayed message.
        /// </summary>
        public string? Message { get; private set; }

        /// <summary>
        /// Gets or sets the build settings.
        /// </summary>
        public IBuildSettings? BuildSettings { get; set; }
    }
}
