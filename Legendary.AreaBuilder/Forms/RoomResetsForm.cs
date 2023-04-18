// <copyright file="RoomResetsForm.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.AreaBuilder.Forms
{
    using Legendary.AreaBuilder.Services;
    using Legendary.AreaBuilder.Types;
    using Legendary.Core.Models;
    using MongoDB.Driver;

    /// <summary>
    /// Adds mobs or items to a room.
    /// </summary>
    public partial class RoomResetsForm : Form
    {
        private readonly MongoService mongo;
        private readonly Area area;
        private readonly Room room;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoomResetsForm"/> class.
        /// </summary>
        /// <param name="mongo">The mongo service.</param>
        /// <param name="area">The area.</param>
        /// <param name="room">The room.</param>
        public RoomResetsForm(MongoService mongo, Area area, Room room)
        {
            this.mongo = mongo;
            this.area = area;
            this.room = room;
            this.InitializeComponent();
        }

        private void BtnAddMobile_Click(object sender, EventArgs e)
        {
            if (this.lstMobiles.SelectedItem is Core.Models.Mobile mobile)
            {
                this.area.Rooms.Remove(this.room);
                this.room.MobileResets.Add(mobile.CharacterId);
                this.area.Rooms.Add(this.room);
                this.mongo.Areas.ReplaceOne(a => a.AreaId == this.area.AreaId, this.area);
                this.toolStripStatusLabel1.Text = "Mobile reset added.";
            }
        }

        private void BtnAddItem_Click(object sender, EventArgs e)
        {
            if (this.lstItems.SelectedItem is Core.Models.Item item)
            {
                this.area.Rooms.Remove(this.room);
                this.room.ItemResets.Add(item.ItemId);
                this.area.Rooms.Add(this.room);
                this.mongo.Areas.ReplaceOne(a => a.AreaId == this.area.AreaId, this.area);
                this.toolStripStatusLabel1.Text = "Item reset added.";
            }
        }

        private void RoomResetsForm_Load(object sender, EventArgs e)
        {
            this.lstMobiles.DataSource = this.mongo.Mobiles.Find(_ => true).ToList();
            this.lstMobiles.DisplayMember = "FirstName";

            this.lstItems.DataSource = this.mongo.Items.Find(_ => true).ToList();
            this.lstItems.DisplayMember = "Name";
        }
    }
}
