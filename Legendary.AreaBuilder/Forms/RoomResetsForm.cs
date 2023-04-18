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
    using System.Reflection;
    using Item = Types.Item;

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

                var lvi = new ListViewItem()
                {
                    Text = mobile.FirstName,
                    ImageIndex = 0,
                    Tag = mobile.CharacterId,
                };

                this.ListViewCurrent.Items.Add(lvi);

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

                var lvi = new ListViewItem()
                {
                    Text = item.Name,
                    ImageIndex = 1,
                    Tag = item.ItemId,
                };

                this.ListViewCurrent.Items.Add(lvi);

                this.toolStripStatusLabel1.Text = "Item reset added.";
            }
        }

        private void RoomResetsForm_Load(object sender, EventArgs e)
        {
            var mobiles = this.mongo.Mobiles.Find(_ => true).ToList();
            this.lstMobiles.DataSource = mobiles;
            this.lstMobiles.DisplayMember = "FirstName";

            var items = this.mongo.Items.Find(_ => true).ToList();
            this.lstItems.DataSource = items;
            this.lstItems.DisplayMember = "Name";

            this.ListViewCurrent.Items.Clear();

            foreach (var mobileReset in this.room.MobileResets)
            {
                var lvi = new ListViewItem()
                {
                    Text = mobiles.First(m => m.CharacterId == mobileReset).FirstName,
                    ImageIndex = 0,
                    Tag = mobileReset,
                };

                this.ListViewCurrent.Items.Add(lvi);
            }

            foreach (var itemReset in this.room.Items)
            {
                var lvi = new ListViewItem()
                {
                    Text = itemReset.Name,
                    ImageIndex = 1,
                    Tag = itemReset.ItemId,
                };

                this.ListViewCurrent.Items.Add(lvi);
            }

            this.Cursor = Cursors.Default;
        }

        private void ListViewCurrent_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (this.ListViewCurrent.SelectedItems[0] is ListViewItem selectedItem)
                {
                    
                    if (selectedItem.ImageIndex == 0 && selectedItem.Tag is long mobileId)
                    {
                        this.Cursor = Cursors.WaitCursor;
                        this.area.Rooms.Remove(this.room);
                        this.room.MobileResets.Remove(mobileId);
                        this.area.Rooms.Add(this.room);
                        this.mongo.Areas.ReplaceOne(a => a.AreaId == this.area.AreaId, this.area);
                        this.toolStripStatusLabel1.Text = "Mobile reset removed.";
                        this.Cursor = Cursors.Default;
                    }
                    else if (selectedItem.ImageIndex == 1 && selectedItem.Tag is long itemId)
                    {
                        this.Cursor = Cursors.WaitCursor;
                        this.area.Rooms.Remove(this.room);
                        this.room.ItemResets.Remove(itemId);
                        this.area.Rooms.Add(this.room);
                        this.mongo.Areas.ReplaceOne(a => a.AreaId == this.area.AreaId, this.area);
                        this.toolStripStatusLabel1.Text = "Item reset removed.";
                        this.Cursor = Cursors.Default;
                    }

                    this.ListViewCurrent.Items.Remove(selectedItem);
                }
            }
        }
    }
}
