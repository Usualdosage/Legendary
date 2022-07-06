// <copyright file="Emotion.cs" company="Legendary™">
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
    /// Emotions for mobiles that dictate their conversation.
    /// </summary>
    public enum Emotion
    {
        /// <summary>
        /// Neutral.
        /// </summary>
        Neutral = 0,

        /// <summary>
        /// Happy.
        /// </summary>
        Happy = 1,

        /// <summary>
        /// Sad.
        /// </summary>
        Sad = 2,

        /// <summary>
        /// Angry.
        /// </summary>
        Angry = 3,
    }
}