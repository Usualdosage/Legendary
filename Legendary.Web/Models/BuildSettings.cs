// <copyright file="ErrorViewModel.cs" company="Legendary">
//  Copyright © 2021-2022 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Web.Models
{
    using System;
    using Legendary.Web.Contracts;

    public class BuildSettings : IBuildSettings
	{
        /// <summary>
        /// Gets or sets the build version.
        /// </summary>
        public string? Version { get; set; }

        /// <summary>
        /// Gets or sets the last release date.
        /// </summary>
        public DateTime? ReleaseDate { get; set; }
    }
}

