// <copyright file="LoginModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
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
