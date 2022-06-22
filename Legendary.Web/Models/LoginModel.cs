// <copyright file="LoginModel.cs" company="Legendary">
//  Copyright © 2021-2022 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Web.Models
{
    using Legendary.Web.Contracts;

    public class LoginModel
	{
		public LoginModel()
        {

        }

		public LoginModel(string? message, IBuildSettings? buildSettings)
		{
			this.Message = message;
			this.BuildSettings = buildSettings;
		}

        public string? Message { get; set; }

        public IBuildSettings? BuildSettings { get; set; }
    }
}

