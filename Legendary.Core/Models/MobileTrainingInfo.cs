// <copyright file="MobileTrainingInfo.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Core.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Tracks mobile AI training data for a character.
    /// </summary>
    public class MobileTrainingInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MobileTrainingInfo"/> class.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="trainingData">The training data for this mobile.</param>
        public MobileTrainingInfo(Character character, List<dynamic> trainingData)
        {
            this.Character = character;
            this.TrainingData = trainingData;
        }

        /// <summary>
        /// Gets the character.
        /// </summary>
        public Character Character { get; private set; }

        /// <summary>
        /// Gets the training data.
        /// </summary>
        public List<dynamic> TrainingData { get; private set; }
    }
}