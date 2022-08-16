// <copyright file="ProgramProcessor.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Processors
{
    using Legendary.Core.Contracts;

    /// <summary>
    /// Process mob, object, and room program code.
    /// </summary>
    public class ProgramProcessor
    {
        private readonly ICommunicator communicator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgramProcessor"/> class.
        /// </summary>
        /// <param name="communicator">The communicator.</param>
        public ProgramProcessor(ICommunicator communicator)
        {
            this.communicator = communicator;
        }
    }
}