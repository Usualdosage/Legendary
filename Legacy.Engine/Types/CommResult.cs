// <copyright file="CommResult.cs" company="Legendary">
//  Copyright © 2021 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
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