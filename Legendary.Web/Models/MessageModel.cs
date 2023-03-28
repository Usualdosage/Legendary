// <copyright file="MessageModel.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Web.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Message model for the messages API.
    /// </summary>
    public class MessageModel
    {
        /// <summary>
        /// Gets or sets the from address.
        /// </summary>
        public string? FromAddress { get; set; }

        /// <summary>
        /// Gets or sets the to addresses.
        /// </summary>
        public List<string>? ToAddresses { get; set; }

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        public string? Subject { get; set; }

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        public string? Content { get; set; }
    }
}