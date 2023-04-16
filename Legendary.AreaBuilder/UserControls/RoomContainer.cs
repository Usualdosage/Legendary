// <copyright file="RoomContainer.cs" company="Legendary™">
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
    using Legendary.Core.Models;

    /// <summary>
    /// UI representation of a single room.
    /// </summary>
    public partial class RoomContainer : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RoomContainer"/> class.
        /// </summary>
        public RoomContainer()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the room is selected.
        /// </summary>
        public bool Selected { get; set; } = false;

        /// <summary>
        /// Gets or sets the selected room.
        /// </summary>
        public Room? SelectedRoom { get; set; }

        /// <summary>
        /// Updates the control with the selected values.
        /// </summary>
        public void UpdateControl()
        {
            this.lblRoomId.Text = this.SelectedRoom?.RoomId.ToString();

            var upExit = this.SelectedRoom?.Exits.FirstOrDefault(e => e.Direction == Direction.Up);
            var downExit = this.SelectedRoom?.Exits.FirstOrDefault(e => e.Direction == Direction.Down);
            var eastExit = this.SelectedRoom?.Exits.FirstOrDefault(e => e.Direction == Direction.East);
            var westExit = this.SelectedRoom?.Exits.FirstOrDefault(e => e.Direction == Direction.West);
            var northExit = this.SelectedRoom?.Exits.FirstOrDefault(e => e.Direction == Direction.North);
            var southExit = this.SelectedRoom?.Exits.FirstOrDefault(e => e.Direction == Direction.South);
            var northwestExit = this.SelectedRoom?.Exits.FirstOrDefault(e => e.Direction == Direction.NorthWest);
            var northeastExit = this.SelectedRoom?.Exits.FirstOrDefault(e => e.Direction == Direction.NorthEast);
            var southwestExit = this.SelectedRoom?.Exits.FirstOrDefault(e => e.Direction == Direction.SouthWest);
            var southeastExit = this.SelectedRoom?.Exits.FirstOrDefault(e => e.Direction == Direction.SouthEast);

            this.upPanel.BackColor = upExit != null ? (upExit.IsDoor ? Color.Red : Color.Green) : Color.White;
            this.downPanel.BackColor = downExit != null ? (downExit.IsDoor ? Color.Red : Color.Green) : Color.White;
            this.eastDoor.BackColor = eastExit != null ? (eastExit.IsDoor ? Color.Red : Color.Blue) : Color.White;
            this.westDoor.BackColor = westExit != null ? (westExit.IsDoor ? Color.Red : Color.Blue) : Color.White;
            this.northDoor.BackColor = northExit != null ? (northExit.IsDoor ? Color.Red : Color.Blue) : Color.White;
            this.southDoor.BackColor = southExit != null ? (southExit.IsDoor ? Color.Red : Color.Blue) : Color.White;
            this.sePanel.BackColor = southeastExit != null ? (southeastExit.IsDoor ? Color.Red : Color.Blue) : Color.White;
            this.swPanel.BackColor = southwestExit != null ? (southwestExit.IsDoor ? Color.Red : Color.Blue) : Color.White;
            this.nwPanel.BackColor = northwestExit != null ? (northwestExit.IsDoor ? Color.Red : Color.Blue) : Color.White;
            this.nePanel.BackColor = northeastExit != null ? (northeastExit.IsDoor ? Color.Red : Color.Blue) : Color.White;

            this.BackColor = Color.White;
            this.lblRoomId.BackColor = Color.White;
            this.lblRoomId.Show();

            if (this.Selected)
            {
                this.BorderStyle = BorderStyle.Fixed3D;
            }
            else
            {
                this.BorderStyle = BorderStyle.FixedSingle;
            }

            this.Update();
        }

        private void LblRoomId_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (this.FindForm() is MainForm form)
                {
                    if (this.Selected)
                    {
                        this.Selected = false;
                        form.SelectedContainers.RemoveAll(c => c.Name == this.Name);
                    }
                    else
                    {
                        form.ResetSelections();

                        this.Selected = true;

                        if (!form.SelectedContainers.Contains(this))
                        {
                            form.SelectedContainers.Add(this);
                        }

                        var grid = form.Controls.Find("pgRoom", true).FirstOrDefault();

                        if (grid != null && grid is PropertyGrid pgRoom)
                        {
                            pgRoom.SelectedObject = this.SelectedRoom;
                        }
                    }

                    this.UpdateControl();
                }
            }
            else
            {
                if (this.Parent is Panel panel)
                {
                    panel.Controls.Clear();
                }
            }
        }

        private void LblRoomId_Leave(object sender, EventArgs e)
        {
            this.UpdateControl();
        }
    }
}
