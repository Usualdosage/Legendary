// <copyright file="IMIRPProcessor.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Core.Contracts
{
    /// <summary>
    /// Implementation contract for a MIRP Processor.
    /// </summary>
    public interface IMIRPProcessor
    {
        /// <summary>
        /// Creates an instance of a MIRP derived type based on the type name. Used to load program files.
        /// </summary>
        /// <param name="source">The source object invoking the program.</param>
        /// <returns>Instance of a MIRP derived type.</returns>
        object? CreateProgramInstance(object source);
    }
}
