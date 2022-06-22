// <copyright file="IServerSettings.cs" company="Legendary">
//  Copyright © 2021-2022 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Networking.Contracts
{
    /// <summary>
    /// Implementation contract for server settings.
    /// </summary>
    public interface IServerSettings
    {
        /// <summary>
        /// Gets or sets the URL of the content API.
        /// </summary>
        string? ApiUrl { get; set; }

        /// <summary>
        /// Gets or sets the port of the content API.
        /// </summary>
        int? ApiPort { get; set; }
    }
}