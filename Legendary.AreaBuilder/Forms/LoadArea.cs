// <copyright file="LoadArea.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.AreaBuilder
{
    using Legendary.AreaBuilder.Services;
    using Legendary.Core.Models;
    using MongoDB.Driver;

    /// <summary>
    /// Displays a list of areas.
    /// </summary>
    public partial class LoadArea : Form
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadArea"/> class.
        /// </summary>
        /// <param name="mongo">The mongo service.</param>
        public LoadArea(MongoService mongo)
        {
            this.InitializeComponent();

            this.Mongo = mongo;
        }

        /// <summary>
        /// Gets or sets the mongo service.
        /// </summary>
        public MongoService Mongo { get; set; }

        /// <summary>
        /// Gets or sets the selected area.
        /// </summary>
        public Area? SelectedArea { get; set; }

        private void ListBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.SelectedArea = this.listBox1.SelectedItem as Area;
            this.DialogResult = DialogResult.OK;
        }

        private void LoadArea_Load(object sender, EventArgs e)
        {
            this.listBox1.Items.Clear();

            var areas = this.Mongo.Areas.Find(_ => true).ToList();

            foreach (var area in areas)
            {
                this.listBox1.Items.Add(area);
            }

            this.listBox1.SelectedIndex = 0;

            this.Cursor = Cursors.Default;
        }
    }
}
