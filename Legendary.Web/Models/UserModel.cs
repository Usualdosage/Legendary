// <copyright file="UserModel.cs" company="Legendary">
//  Copyright © 2021-2022 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Web.Models
{
    public class UserModel
    {
        public string UserName { get; private set; }

        public string Password { get; private set; }

        public UserModel(string username, string password)
        {
            this.UserName = username;
            this.Password = password;
        }
    }
}
