// <copyright file="MaxCurrentEditorForm.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.AreaBuilder
{
    using Legendary.Core.Types;

    /// <summary>
    /// Editor for the MaxCurrent object.
    /// </summary>
    public partial class MaxCurrentEditorForm : Form
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaxCurrentEditorForm"/> class.
        /// </summary>
        public MaxCurrentEditorForm()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public MaxCurrent Value
        {
            get
            {
                return new MaxCurrent((int)this.numericUpDown2.Value, (int)this.numericUpDown1.Value);
            }

            set
            {
                this.numericUpDown1.Value = (int)value.Current;
                this.numericUpDown2.Value = (int)value.Max;
            }
        }
    }
}
