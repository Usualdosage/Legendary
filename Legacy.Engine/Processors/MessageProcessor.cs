// <copyright file="MessageProcessor.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Processors
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Data.Contracts;
    using Legendary.Engine.Contracts;
    using MongoDB.Driver;

    /// <summary>
    /// Concrete implementation of a message processor.
    /// </summary>
    public class MessageProcessor : IMessageProcessor
    {
        private readonly ICommunicator communicator;
        private readonly IWorld world;
        private readonly ILogger logger;
        private readonly IDataService dataService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageProcessor"/> class.
        /// </summary>
        /// <param name="communicator">The communicator.</param>
        /// <param name="world">The world.</param>
        /// <param name="dataService">The data service.</param>
        /// <param name="logger">The logger.</param>
        public MessageProcessor(ICommunicator communicator, IWorld world, IDataService dataService, ILogger logger)
        {
            this.communicator = communicator;
            this.world = world;
            this.logger = logger;
            this.dataService = dataService;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAllMessages(Character character, CancellationToken cancellationToken = default)
        {
            try
            {
                var deleteResult = await this.dataService.Messages.DeleteManyAsync(m => m.To == character.CharacterId, cancellationToken);
                return true;
            }
            catch (Exception exc)
            {
                this.logger.Error("Unable to delete all messages.", exc, this.communicator);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteMessage(Character character, int messageIndex, CancellationToken cancellationToken = default)
        {
            try
            {
                var asyncCursor = await this.dataService.Messages.FindAsync(m => m.To == character.CharacterId && (m.IsDeleted == null || m.IsDeleted == false), cancellationToken: cancellationToken);

                var messages = await asyncCursor.ToListAsync(cancellationToken: cancellationToken);

                var messageToDelete = messages[messageIndex - 1];

                messageToDelete.IsDeleted = true;

                await this.dataService.Messages.ReplaceOneAsync(m => m.MessageId == messageToDelete.MessageId, messageToDelete, cancellationToken: cancellationToken);

                return true;
            }
            catch (Exception exc)
            {
                this.logger.Error("Failed to delete message.", exc, this.communicator);

                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<Message?> GetMessage(long messageId, CancellationToken cancellationToken = default)
        {
            try
            {
                var asyncCursor = await this.dataService.Messages.FindAsync(m => m.MessageId == messageId && (m.IsDeleted == null || m.IsDeleted == false), cancellationToken: cancellationToken);

                return await asyncCursor.FirstOrDefaultAsync(cancellationToken);
            }
            catch (Exception exc)
            {
                this.logger.Error("Failed to fetch message.", exc, this.communicator);
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<Message?> GetMessage(Character character, int messageIndex, CancellationToken cancellationToken = default)
        {
            try
            {
                var asyncCursor = await this.dataService.Messages.FindAsync(m => m.To == character.CharacterId && (m.IsDeleted == null || m.IsDeleted == false), cancellationToken: cancellationToken);

                var messages = await asyncCursor.ToListAsync(cancellationToken: cancellationToken);

                var message = messages[messageIndex - 1];

                if (message != null)
                {
                    message.IsRead = true;
                    message.ReadDate ??= DateTime.UtcNow;
                    await this.dataService.Messages.ReplaceOneAsync(m => m.MessageId == message.MessageId, message, cancellationToken: cancellationToken);
                    return message;
                }
                else
                {
                    throw new Exception("Unable to load message.");
                }
            }
            catch (Exception exc)
            {
                this.logger.Error("Failed to fetch message.", exc, this.communicator);
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<Message?> GetFirstUnreadMessage(Character character, CancellationToken cancellationToken = default)
        {
            try
            {
                var asyncCursor = await this.dataService.Messages.FindAsync(m => m.To == character.CharacterId && m.ReadDate == null && (m.IsDeleted == null || m.IsDeleted == false), cancellationToken: cancellationToken);

                var message = await asyncCursor.FirstOrDefaultAsync(cancellationToken: cancellationToken);

                if (message != null)
                {
                    message.IsRead = true;
                    message.ReadDate ??= DateTime.UtcNow;
                    await this.dataService.Messages.ReplaceOneAsync(m => m.MessageId == message.MessageId, message, cancellationToken: cancellationToken);
                    return message;
                }
                else
                {
                    throw new Exception("Unable to load message.");
                }
            }
            catch (Exception exc)
            {
                this.logger.Error("Failed to fetch message.", exc, this.communicator);
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<List<Message>> GetAllMessagesForPlayer(Character character, CancellationToken cancellationToken = default)
        {
            var messages = await this.dataService.Messages.FindAsync(m => m.To == character.CharacterId && (m.IsDeleted == null || m.IsDeleted == false), cancellationToken: cancellationToken);
            return await messages.ToListAsync(cancellationToken: cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<bool> MarkAsRead(Message message, CancellationToken cancellationToken = default)
        {
            message.IsRead = true;
            message.ReadDate = DateTime.UtcNow;

            try
            {
                await this.dataService.Messages.ReplaceOneAsync(m => m.MessageId == message.MessageId, message, cancellationToken: cancellationToken);
                return true;
            }
            catch (Exception exc)
            {
                this.logger.Error("Failed to fetch message.", exc, this.communicator);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeliverMessage(Message message, CancellationToken cancellationToken = default)
        {
            if (message.To.HasValue && message.From.HasValue)
            {
                var toPlayer = this.communicator.ResolveCharacter(message.To.Value);

                if (toPlayer != null)
                {
                    StringBuilder speech = new ();

                    if (message.From == message.To)
                    {
                        speech.Append($"A messenger arrives out of thin air and hands you a sealed scroll. ");
                        string title = toPlayer.Character.Gender == Core.Types.Gender.Male ? "my Lord" : "my Lady";
                        speech.Append($"A messenger whispers to you discreetly, <span class='say'>'Message to you from, well, YOU, {title}.'.</span>");
                        await this.communicator.SendToPlayer(toPlayer.Character, speech.ToString(), cancellationToken);
                    }
                    else
                    {
                        speech.Append($"A messenger arrives out of thin air and hands you a sealed scroll. ");
                        string title = toPlayer.Character.Gender == Core.Types.Gender.Male ? "my Lord" : "my Lady";
                        speech.Append($"A messenger whispers to you discreetly, <span class='say'>'Message to you from {message.FromName}, {title}.'.</span>");
                        await this.communicator.SendToPlayer(toPlayer.Character, speech.ToString(), cancellationToken);
                    }

                    await this.communicator.SendToRoom(toPlayer.Character.Location, toPlayer.Character, null, $"A messenger arrives out of thin air and hands {toPlayer.Character.FirstName} a sealed scroll.", cancellationToken);
                }

                return true;
            }
            else
            {
                // Not sure how we got here, but oops.
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<long> GetNewMessagesForPlayer(Character character, CancellationToken cancellationToken = default)
        {
            return await this.dataService.Messages.CountDocumentsAsync(m => m.To == character.CharacterId && m.ReadDate == null && (m.IsDeleted == null || m.IsDeleted == false));
        }

        /// <inheritdoc/>
        public async Task ShowMessageToPlayer(Message message, Character character, CancellationToken cancellationToken = default)
        {
            StringBuilder messageContent = new ();

            messageContent.Append("<div class='game-message'>");
            messageContent.Append("<div class='game-message-head'></div>");
            messageContent.Append("<div class='game-message-body'></div>");
            messageContent.Append($"<h5>Message from {message.FromName}</h5>");
            messageContent.Append($"<p>\"{message.Subject}\"</p>");
            messageContent.Append(message.Content);
            messageContent.Append("</div>");
            messageContent.Append("<div class='game-message-footer'></div>");
            messageContent.Append("</div>");

            await this.communicator.SendToPlayer(character, messageContent.ToString(), cancellationToken);
        }
    }
}