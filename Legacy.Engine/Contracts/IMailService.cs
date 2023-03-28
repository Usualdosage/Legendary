// <copyright file="IMailService.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Contracts
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Contract to send mail.
    /// </summary>
    public interface IMailService
    {
        /// <summary>
        /// Sends an email message to the specified address with the subject and body.
        /// </summary>
        /// <param name="address">The email address to send to.</param>
        /// <param name="subject">The email subject.</param>
        /// <param name="body">The email body.</param>
        /// <returns>Task.</returns>
        Task SendEmailMessage(string address, string subject, string body);
    }
}