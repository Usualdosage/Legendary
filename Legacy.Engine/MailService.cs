// <copyright file="MailService.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine
{
    using System;
    using System.Net;
    using System.Net.Mail;
    using System.Threading.Tasks;
    using Legendary.Core.Contracts;
    using Legendary.Data.Contracts;
    using Legendary.Engine.Contracts;

    /// <summary>
    /// Implementation of an IMailService for sending emails.
    /// </summary>
    public class MailService : IMailService
    {
        private readonly IServerSettings serverSettings;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MailService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="serverSettings">The server settings.</param>
        public MailService(ILogger logger, IServerSettings serverSettings)
        {
            this.logger = logger;
            this.serverSettings = serverSettings;
        }

        /// <inheritdoc/>
        public async Task SendEmailMessage(string address, string subject, string body)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(this.serverSettings.FromEmailAddress) && !string.IsNullOrWhiteSpace(this.serverSettings.FromEmailName))
                {
                    var fromAddress = new MailAddress(this.serverSettings.FromEmailAddress, this.serverSettings.FromEmailName);
                    var toAddress = new MailAddress(address, address);
                    var fromPassword = this.serverSettings.FromEmailPassword;

                    var smtp = new SmtpClient
                    {
                        Host = "smtp.gmail.com",
                        Port = 587,
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                    };

                    using (var message = new MailMessage(fromAddress, toAddress)
                    {
                        Subject = subject,
                        Body = body,
                    })
                    {
                        await smtp.SendMailAsync(message);
                    }
                }
                else
                {
                    throw new Exception("Unable to send email due to missing values in ServerSettings.");
                }
            }
            catch (Exception exc)
            {
                this.logger.Error(exc, null);
                throw;
            }
        }
    }
}