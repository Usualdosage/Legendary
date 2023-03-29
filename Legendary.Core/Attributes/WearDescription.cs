// <copyright file="WearDescription.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Core.Attributes
{
    using System;

    /// <summary>
    /// Describes the wear location of an item.
    /// </summary>
    public class WearDescription : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WearDescription"/> class.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <param name="wearAction">The action.</param>
        /// <param name="removeAction">The remove action.</param>
        public WearDescription(string description, string wearAction, string removeAction)
        {
            this.Description = description;
            this.WearAction = wearAction;
            this.RemoveAction = removeAction;
        }

        /// <summary>
        /// Gets or sets the wear description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the wear action.
        /// </summary>
        public string WearAction { get; set; }

        /// <summary>
        /// Gets or sets the remove action.
        /// </summary>
        public string RemoveAction { get; set; }
    }
}