// <copyright file="QuestType.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Core.Types
{
    /// <summary>
    /// Types of quests.
    /// </summary>
    public enum QuestType : int
    {
        /// <summary>
        /// Find.
        /// </summary>
        Find = 0,

        /// <summary>
        /// Kill.
        /// </summary>
        Kill = 1,

        /// <summary>
        /// Outlaw (kill guards).
        /// </summary>
        Outlaw = 2,
    }
}
