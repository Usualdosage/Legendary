// <copyright file="CommResult.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Types
{
    /// <summary>
    /// Represents the result of a communication attempt between two parties.
    /// </summary>
    public enum CommResult
    {
        /// <summary>
        /// Message was sent and received successfully.
        /// </summary>
        Ok = 1,

        /// <summary>
        /// Message was ignored by recipient.
        /// </summary>
        Ignored = 2,

        /// <summary>
        /// Recipient is not connected.
        /// </summary>
        NotConnected = 3,

        /// <summary>
        /// Recipient is connected but not able to receive message.
        /// </summary>
        NotAvailable = 4,
    }
}