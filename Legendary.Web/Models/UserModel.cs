// <copyright file="UserModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Legendary.Web.Models
{
    /// <summary>
    /// Represents a user forthe purpose of login.
    /// </summary>
    public class UserModel
    {
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
        public string UserName { get; private set; }

        /// <summary>
        /// Gets the password.
        /// </summary>
        public string Password { get; private set; }
    }
}
