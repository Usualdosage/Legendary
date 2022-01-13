// <copyright file="Logger.cs" company="Legendary">
//  Copyright © 2021 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Engine
{
    using System;
    using System.Threading.Tasks;
    using Legendary.Engine.Contracts;

    /// <summary>
    /// Creates a new instance of a logger.
    /// </summary>
    public class Logger : ILogger
    {
        /// <inheritdoc/>
        public async Task Debug(string message)
        {
            await Console.Out.WriteAsync(DateTime.UtcNow.ToString());
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            await Console.Out.WriteAsync(" Debug: ");
            Console.ForegroundColor = ConsoleColor.White;
            await Console.Out.WriteLineAsync(message);
        }

        /// <inheritdoc/>
        public async Task Info(string message)
        {
            await Console.Out.WriteAsync(DateTime.UtcNow.ToString());
            Console.ForegroundColor = ConsoleColor.Green;
            await Console.Out.WriteAsync($" Info: ");
            Console.ForegroundColor = ConsoleColor.White;
            await Console.Out.WriteLineAsync(message);
        }

        /// <inheritdoc/>
        public async Task Warn(string message, Exception exception)
        {
            await Console.Out.WriteAsync(DateTime.UtcNow.ToString());
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            await Console.Out.WriteAsync($" Warning: {message} - ");
            Console.ForegroundColor = ConsoleColor.White;
            await Console.Out.WriteLineAsync(message);
        }

        /// <inheritdoc/>
        public async Task Warn(string message)
        {
            await Console.Out.WriteAsync(DateTime.UtcNow.ToString());
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            await Console.Out.WriteAsync(" Warning: ");
            Console.ForegroundColor = ConsoleColor.White;
            await Console.Out.WriteLineAsync(message);
        }

        /// <inheritdoc/>
        public async Task Error(string message)
        {
            await Console.Out.WriteAsync(DateTime.UtcNow.ToString());
            Console.ForegroundColor = ConsoleColor.Red;
            await Console.Out.WriteAsync(" Error: ");
            Console.ForegroundColor = ConsoleColor.White;
            await Console.Out.WriteLineAsync(message);
        }

        /// <inheritdoc/>
        public async Task Error(string message, Exception exception)
        {
            await Console.Out.WriteAsync(DateTime.UtcNow.ToString());
            Console.ForegroundColor = ConsoleColor.Red;
            await Console.Out.WriteAsync($" Error: {message} - ");
            Console.ForegroundColor = ConsoleColor.White;
            await Console.Out.WriteLineAsync(message);
        }

        /// <inheritdoc/>
        public async Task Error(Exception exception)
        {
            await Console.Out.WriteAsync(DateTime.UtcNow.ToString());
            Console.ForegroundColor = ConsoleColor.Red;
            await Console.Out.WriteAsync(" Error: ");
            Console.ForegroundColor = ConsoleColor.White;
            await Console.Out.WriteLineAsync(exception.ToString());
        }
    }
}




