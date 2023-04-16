// <copyright file="MaxCurrentEditor.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.AreaBuilder.Types
{
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Windows.Forms.Design;
    using Legendary.Core.Types;

    /// <summary>
    /// Edits the max current type.
    /// </summary>
    public class MaxCurrentEditor : UITypeEditor
    {
        /// <inheritdoc/>
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext? context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        /// <inheritdoc/>
        public override object? EditValue(ITypeDescriptorContext? context, IServiceProvider provider, object? value)
        {
            MaxCurrent? maxCurrent = value as MaxCurrent;

            if (provider.GetService(typeof(IWindowsFormsEditorService)) is IWindowsFormsEditorService svc && maxCurrent != null)
            {
                using (MaxCurrentEditorForm form = new ())
                {
                    form.Value = maxCurrent;

                    if (svc.ShowDialog(form) == DialogResult.OK)
                    {
                        maxCurrent = form.Value; // update object
                    }
                }
            }

            return maxCurrent;
        }
    }
}
