// <copyright file="UserModel.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Web.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a user forthe purpose of login.
    /// </summary>
    public class UserModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserModel"/> class.
        /// </summary>
        public UserModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserModel"/> class.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        public UserModel(string username, string password)
        {
            this.UserName = username;
            this.Password = password;
        }

        /// <summary>
        /// Gets the username.
        /// </summary>
        public string UserName { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the password.
        /// </summary>
        public string Password { get; private set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of usernames for the message center.
        /// </summary>
        public List<string>? Usernames { get; set; }
    }
}
