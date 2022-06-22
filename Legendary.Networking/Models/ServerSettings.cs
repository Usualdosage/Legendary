// <copyright file="ServerSettings.cs" company="Legendary">
//  Copyright © 2021-2022 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Networking.Models
{
    using Legendary.Networking.Contracts;

    /// <summary>
    /// Used to house server settings loaded from a config file.
    /// </summary>
    public class ServerSettings : IServerSettings
    {
        /// <inheritdoc/>
        public string? ApiUrl { get; set; }

        /// <inheritdoc/>
        public int? ApiPort { get; set; }
    }
}