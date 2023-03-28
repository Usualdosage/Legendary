// <copyright file="IMessageProcessor.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Core.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core.Models;

    /// <summary>
    /// Handles processing of messages between players.
    /// </summary>
    public interface IMessageProcessor
    {
        /// <summary>
        /// Gets a list of messages for a player.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>List of all messages.</returns>
        Task<List<Message>> GetAllMessagesForPlayer(Character character, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a single message.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="messageIndex">The message index.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if deleted.</returns>
        Task<bool> DeleteMessage(Character character, int messageIndex, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes all of a player's messages.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if successful.</returns>
        Task<bool> DeleteAllMessages(Character character, CancellationToken cancellationToken = default);

        /// <summary>
        /// Marks a message as read.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if successful.</returns>
        Task<bool> MarkAsRead(Message message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the message by Id.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="messageIndex">The message Id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Message.</returns>
        Task<Message?> GetMessage(Character character, int messageIndex, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delivers a message to a character, sending an in-game message if possible.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        Task<bool> DeliverMessage(Message message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the first unread message.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Message.</returns>
        Task<Message?> GetFirstUnreadMessage(Character character, CancellationToken cancellationToken = default);

        /// <summary>
        /// Displays a message to a player.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="character">The player.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        Task ShowMessageToPlayer(Message message, Character character, CancellationToken cancellationToken);

        /// <summary>
        /// Gets a count of new messages for a character.
        /// </summary>
        /// <param name="character">The player.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Long.</returns>
        Task<long> GetNewMessagesForPlayer(Character character, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a message by message Id.
        /// </summary>
        /// <param name="messageId">The message Id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Message.</returns>
        Task<Message?> GetMessage(long messageId, CancellationToken cancellationToken = default);
    }
}