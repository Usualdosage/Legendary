// <copyright file="MainForm.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.AreaBuilder
{
    using System.ComponentModel;
    using System.Net;
    using Azure;
    using Azure.Storage.Files.Shares;
    using Legendary.AreaBuilder.Forms;
    using Legendary.AreaBuilder.Services;
    using Legendary.Core.Models;
    using Legendary.Core.Types;
    using MongoDB.Driver;
    using Newtonsoft.Json;

    /// <summary>
    /// Main form for the application.
    /// </summary>
    public partial class MainForm : Form
    {
        private readonly MongoService mongo;
        private bool isNew = true;

        // private string roomImage = $"https://legendary.file.core.windows.net/images/{room.RoomId}.png?sv=2021-06-08&ss=f&srt=sco&sp=r&se=2027-07-27T07:17:16Z&st=2022-07-26T23:17:16Z&spr=https&sig=H7cbZXl63g6mhIHYC00wri8cTRiob6XrWdf1F4D0F%2Bo%3D";

        /// <summary>
        /// Initializes a new instance of the <see cref="MainForm"/> class.
        /// </summary>
        public MainForm()
        {
            this.InitializeComponent();

            this.NumberRooms();

            this.mongo = new MongoService();
        }

        /// <summary>
        /// Gets or sets the area id.
        /// </summary>
        public static long AreaId { get; set; } = 0;

        /// <summary>
        /// Gets or sets the selected containers.
        /// </summary>
        public List<RoomContainer> SelectedContainers { get; set; } = new List<RoomContainer>();

        /// <summary>
        /// Numbers all of the rooms.
        /// </summary>
        public void NumberRooms()
        {
            Random random = new();
            int areaId = random.Next(1000, 100000);

            Area area = new(areaId, null, null, null, new List<Room>());
            AreaId = areaId;

            this.pgArea.SelectedObject = area;
            this.pgRoom.SelectedObject = null;
            this.pbRoomImage.Image = null;
        }

        /// <summary>
        /// Resets the selections.
        /// </summary>
        public void ResetSelections()
        {
            foreach (var control in this.tableLayoutPanel1.Controls)
            {
                if (control is Panel pnl)
                {
                    if (pnl.Controls.Count > 0)
                    {
                        RoomContainer ctr = (RoomContainer)pnl.Controls[0];
                        ctr.Selected = false;
                    }
                }
            }
        }

        private static void AddExit(int areaId, Room room, Room? destination, Direction direction)
        {
            if (destination != null && !room.Exits.Any(d => d.Direction == direction))
            {
                room.Exits.Add(new Exit(direction, areaId, destination.RoomId));
            }
        }

        private static void RemoveExit(long areaId, Room room, Room? destination, Direction direction)
        {
            if (destination != null && room.Exits.Any(d => d.Direction == direction))
            {
                room.Exits.RemoveAll(e => e.Direction == direction && e.ToRoom == destination.RoomId && e.ToArea == areaId);
            }
        }

        private static RoomContainer CreateRoomContainer(int areaId, long roomId, Room? selectedRoom)
        {
            RoomContainer container = new()
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(0, 0, 0, 0),
                Margin = new Padding(0, 0, 0, 0),
                Cursor = Cursors.Hand,
                Selected = true,
                SelectedRoom = selectedRoom ?? new Room() { AreaId = areaId, RoomId = roomId },
                Name = $"container{roomId}",
            };
            return container;
        }

        private static string? GenerateDescription(string? areaName, string? roomTitle, string? terrain = "City")
        {
            try
            {
                var chatGPT = new ChatGPTService();

                string description = $"Describe my view of {roomTitle} in the {terrain} within the area of {areaName}.";

                var response = chatGPT.Describe(description);

                return response;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return null;
            }
        }

        private void Panel_MouseClick(object? sender, MouseEventArgs e)
        {
            if (sender != null && sender is Panel panel)
            {
                this.ResetSelections();

                var area = (Area)this.pgArea.SelectedObject;

                if (panel.Tag is int i)
                {
                    var roomId = area.AreaId + i;

                    var container = CreateRoomContainer(area.AreaId, roomId, null);

                    this.pgRoom.SelectedObject = container.SelectedRoom;

                    panel.Controls.Add(container);

                    this.SelectedContainers.Add(container);

                    container.UpdateControl();
                }
            }
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void PgArea_SelectedObjectsChanged(object sender, EventArgs e)
        {
            // Reload the grid.
            var area = (Area)this.pgArea.SelectedObject;

            MainForm.AreaId = area.AreaId;

            this.tableLayoutPanel1.Controls.Clear();

            if (area != null)
            {
                int counter = 0;

                for (var x = 0; x < 20; x++)
                {
                    for (var y = 0; y < 20; y++)
                    {
                        Panel panel = new()
                        {
                            Dock = DockStyle.Fill,
                            BorderStyle = BorderStyle.FixedSingle,
                            Padding = new Padding(0, 0, 0, 0),
                            Margin = new Padding(0, 0, 0, 0),
                            Cursor = Cursors.Hand,
                        };
                        panel.MouseClick += this.Panel_MouseClick;
                        panel.Tag = counter;
                        panel.Name = $"panel{area.AreaId + counter}";

                        if (area.Rooms.Count > 0)
                        {
                            long roomId = area.AreaId + counter;

                            var room = area.Rooms.FirstOrDefault(r => r.RoomId == roomId && r.AreaId == area.AreaId);

                            if (room != null)
                            {
                                var container = CreateRoomContainer(area.AreaId, roomId, room);
                                panel.Controls.Add(container);
                                container.UpdateControl();
                            }
                        }

                        counter++;

                        this.tableLayoutPanel1.Controls.Add(panel, y, x);
                    }
                }
            }

            this.tableLayoutPanel1.Update();
        }

        private void ConnectSelectedRoomsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.SelectedContainers.Count > 1)
            {
                foreach (var container in this.SelectedContainers)
                {
                    var room = container.SelectedRoom;

                    if (room != null)
                    {
                        // Calculate the room IDs based on a 20 x 20 grid.
                        var northLabelId = (room.RoomId - 20).ToString();
                        var southLabelId = (room.RoomId + 20).ToString();
                        var eastLabelId = (room.RoomId + 1).ToString();
                        var westLabelId = (room.RoomId - 1).ToString();

                        var northeastLabelId = (room.RoomId - 19).ToString();
                        var southeastLabelId = (room.RoomId + 21).ToString();
                        var southwestLabelId = (room.RoomId + 19).ToString();
                        var northwestLabelId = (room.RoomId - 21).ToString();

                        // Get the panels in each of the cardinal directions to see if they have rooms in them.
                        RoomContainer? north = (RoomContainer?)this.tableLayoutPanel1.Controls.Find($"container{northLabelId}", true).FirstOrDefault();
                        RoomContainer? south = (RoomContainer?)this.tableLayoutPanel1.Controls.Find($"container{southLabelId}", true).FirstOrDefault();
                        RoomContainer? east = (RoomContainer?)this.tableLayoutPanel1.Controls.Find($"container{eastLabelId}", true).FirstOrDefault();
                        RoomContainer? west = (RoomContainer?)this.tableLayoutPanel1.Controls.Find($"container{westLabelId}", true).FirstOrDefault();

                        RoomContainer? ne = (RoomContainer?)this.tableLayoutPanel1.Controls.Find($"container{northeastLabelId}", true).FirstOrDefault();
                        RoomContainer? se = (RoomContainer?)this.tableLayoutPanel1.Controls.Find($"container{southeastLabelId}", true).FirstOrDefault();
                        RoomContainer? sw = (RoomContainer?)this.tableLayoutPanel1.Controls.Find($"container{southwestLabelId}", true).FirstOrDefault();
                        RoomContainer? nw = (RoomContainer?)this.tableLayoutPanel1.Controls.Find($"container{northwestLabelId}", true).FirstOrDefault();

                        if (this.pgArea.SelectedObject is Area area)
                        {
                            AddExit(area.AreaId, room, north?.SelectedRoom, Direction.North);
                            AddExit(area.AreaId, room, south?.SelectedRoom, Direction.South);
                            AddExit(area.AreaId, room, east?.SelectedRoom, Direction.East);
                            AddExit(area.AreaId, room, west?.SelectedRoom, Direction.West);
                            AddExit(area.AreaId, room, ne?.SelectedRoom, Direction.NorthEast);
                            AddExit(area.AreaId, room, se?.SelectedRoom, Direction.SouthEast);
                            AddExit(area.AreaId, room, sw?.SelectedRoom, Direction.SouthWest);
                            AddExit(area.AreaId, room, nw?.SelectedRoom, Direction.NorthWest);
                        }
                    }

                    container.UpdateControl();
                }
            }
        }

        private void RoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.SelectedContainers.Count > 1)
            {
                this.connectSelectedRoomsToolStripMenuItem.Enabled = true;
            }
            else
            {
                this.connectSelectedRoomsToolStripMenuItem.Enabled = false;
            }
        }

        private void RenumberRoomsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Random random = new();
            int areaId = random.Next(1000, 100000);
            MainForm.AreaId = areaId;

            var area = (Area)this.pgArea.SelectedObject;
            var lastAreaId = area.AreaId;
            area.AreaId = areaId;

            area.Rooms = new List<Room>();

            long panelIndex = areaId;

            var offset = area.AreaId - lastAreaId;

            foreach (var control in this.tableLayoutPanel1.Controls)
            {
                var panel = (Panel)control;

                panel.Tag = panelIndex++;

                if (panel.Controls.Count > 0 && panel.Controls[0] is RoomContainer container && container.SelectedRoom != null)
                {
                    if (panel.Tag is int i)
                    {
                        var newRoomId = i;

                        container.SelectedRoom.AreaId = areaId;
                        container.SelectedRoom.RoomId = newRoomId;

                        area.Rooms.Add(container.SelectedRoom);
                    }
                }
            }

            // Fix all the exits
            foreach (var room in area.Rooms)
            {
                foreach (var exit in room.Exits)
                {
                    if (exit.ToArea == lastAreaId)
                    {
                        exit.ToArea = areaId;
                        exit.ToRoom += offset;
                    }
                }
            }
        }

        private void DisconnectSelectedRoomsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.SelectedContainers.Count > 1)
            {
                foreach (var container in this.SelectedContainers)
                {
                    var room = container.SelectedRoom;

                    if (room != null)
                    {
                        // Calculate the room IDs based on a 20 x 20 grid.
                        var northLabelId = (room.RoomId - 20).ToString();
                        var southLabelId = (room.RoomId + 20).ToString();
                        var eastLabelId = (room.RoomId + 1).ToString();
                        var westLabelId = (room.RoomId - 1).ToString();

                        var northeastLabelId = (room.RoomId - 19).ToString();
                        var southeastLabelId = (room.RoomId + 21).ToString();
                        var southwestLabelId = (room.RoomId + 19).ToString();
                        var northwestLabelId = (room.RoomId - 21).ToString();

                        // Get the panels in each of the cardinal directions to see if they have rooms in them.
                        RoomContainer? north = (RoomContainer?)this.tableLayoutPanel1.Controls.Find($"container{northLabelId}", true).FirstOrDefault();
                        RoomContainer? south = (RoomContainer?)this.tableLayoutPanel1.Controls.Find($"container{southLabelId}", true).FirstOrDefault();
                        RoomContainer? east = (RoomContainer?)this.tableLayoutPanel1.Controls.Find($"container{eastLabelId}", true).FirstOrDefault();
                        RoomContainer? west = (RoomContainer?)this.tableLayoutPanel1.Controls.Find($"container{westLabelId}", true).FirstOrDefault();

                        RoomContainer? ne = (RoomContainer?)this.tableLayoutPanel1.Controls.Find($"container{northeastLabelId}", true).FirstOrDefault();
                        RoomContainer? se = (RoomContainer?)this.tableLayoutPanel1.Controls.Find($"container{southeastLabelId}", true).FirstOrDefault();
                        RoomContainer? sw = (RoomContainer?)this.tableLayoutPanel1.Controls.Find($"container{southwestLabelId}", true).FirstOrDefault();
                        RoomContainer? nw = (RoomContainer?)this.tableLayoutPanel1.Controls.Find($"container{northwestLabelId}", true).FirstOrDefault();

                        if (this.pgArea.SelectedObject is Area area)
                        {
                            RemoveExit(area.AreaId, room, north?.SelectedRoom, Direction.North);
                            RemoveExit(area.AreaId, room, south?.SelectedRoom, Direction.South);
                            RemoveExit(area.AreaId, room, east?.SelectedRoom, Direction.East);
                            RemoveExit(area.AreaId, room, west?.SelectedRoom, Direction.West);
                            RemoveExit(area.AreaId, room, ne?.SelectedRoom, Direction.NorthEast);
                            RemoveExit(area.AreaId, room, se?.SelectedRoom, Direction.SouthEast);
                            RemoveExit(area.AreaId, room, sw?.SelectedRoom, Direction.SouthWest);
                            RemoveExit(area.AreaId, room, nw?.SelectedRoom, Direction.NorthWest);
                        }
                    }

                    container.UpdateControl();
                }
            }
        }

        private void AIDescriptionGeneratorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.SelectedContainers.Count > 0)
            {
                int counter = 1;

                this.Cursor = Cursors.WaitCursor;
                this.toolStripStatusLabel1.Text = $"Generating descriptions, please wait...";
                this.statusStrip1.Update();

                var area = this.pgArea.SelectedObject as Area;

                foreach (var container in this.SelectedContainers)
                {
                    var room = container.SelectedRoom;

                    if (room != null)
                    {
                        if (!string.IsNullOrWhiteSpace(room.Name))
                        {
                            if (string.IsNullOrWhiteSpace(room.Description))
                            {
                                room.Description = GenerateDescription(area?.Name, room.Name, Enum.GetName<Terrain>(room.Terrain));
                                this.toolStripStatusLabel1.Text = $"Generated description for room {room.Name} ({counter++} of {this.SelectedContainers.Count})...";
                            }
                            else
                            {
                                this.toolStripStatusLabel1.Text = $"Room {room.Name} ({counter++} of {this.SelectedContainers.Count}) had a description. Skipped.";
                            }

                            this.statusStrip1.Update();

                            if (container.Selected)
                            {
                                this.pgRoom.SelectedObject = container.SelectedRoom;
                            }
                        }
                        else
                        {
                            MessageBox.Show("Room is missing title and/or terrain. Please set these before continuing.");
                            return;
                        }
                    }
                }

                this.Cursor = Cursors.Default;
                this.toolStripStatusLabel1.Text = $"AI description generation complete!";
                this.statusStrip1.Update();
            }
            else
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show("No rooms selected!");
            }
        }

        private void DeselectAllRoomsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (var container in this.SelectedContainers)
            {
                container.Selected = false;
                container.UpdateControl();
            }

            this.SelectedContainers = new List<RoomContainer>();
        }

        private void SelectAllRoomsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.SelectedContainers = new List<RoomContainer>();

            foreach (var control in this.tableLayoutPanel1.Controls)
            {
                if (control is Panel pnl)
                {
                    if (pnl.Controls.Count > 0)
                    {
                        RoomContainer ctr = (RoomContainer)pnl.Controls[0];

                        this.SelectedContainers.Add(ctr);

                        ctr.Selected = true;
                        ctr.UpdateControl();
                    }
                }
            }

            this.toolStripStatusLabel1.Text = $"Selected {this.SelectedContainers.Count} rooms.";
        }

        private void MobEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.toolStripStatusLabel1.Text = "Retrieving mobile list...";
            this.Cursor = Cursors.WaitCursor;
            this.Update();

            var dlg = new MobEditor(this.mongo);
            dlg.ShowDialog();
        }

        private void SaveArea()
        {
            var area = (Area)this.pgArea.SelectedObject;

            area.Rooms = new List<Room>();

            foreach (var control in this.tableLayoutPanel1.Controls)
            {
                var panel = (Panel)control;

                if (panel.Controls.Count > 0 && panel.Controls[0] is RoomContainer container && container.SelectedRoom != null)
                {
                    area.Rooms.Add(container.SelectedRoom);
                }
            }

            if (this.isNew)
            {
                this.mongo.Areas.InsertOne(area);
            }
            else
            {
                this.mongo.Areas.ReplaceOne(a => a.AreaId == area.AreaId, area);
            }

            this.isNew = false;

            this.toolStripStatusLabel1.Text = "Area saved to database.";
        }

        private void SaveAreaToDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.SaveArea();
        }

        private void LoadAreaFromDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.toolStripStatusLabel1.Text = "Retrieving area list...";
            this.Cursor = Cursors.WaitCursor;
            this.Update();

            var loadArea = new LoadArea(this.mongo);

            if (loadArea.ShowDialog() == DialogResult.OK)
            {
                this.pgArea.SelectedObject = loadArea.SelectedArea;
                this.toolStripStatusLabel1.Text = "Area loaded from database.";
                this.isNew = false;
                this.Cursor = Cursors.Default;
            }
        }

        private void NewAreaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.NumberRooms();
            this.isNew = true;
        }

        private void ItemEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.toolStripStatusLabel1.Text = "Retrieving item list...";
            this.Cursor = Cursors.WaitCursor;
            this.Update();

            var dlg = new ItemEditor(this.mongo);
            dlg.ShowDialog();
        }

        private void PrePopulateRoomImagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (var container in this.SelectedContainers)
            {
                var room = container.SelectedRoom;

                if (room != null)
                {
                    string image = $"https://legendaryweb.file.core.windows.net/images/{room.RoomId}.png?sv=2021-12-02&ss=bfqt&srt=sco&sp=rwdlacupiytfx&se=2025-03-02T01:01:15Z&st=2023-03-10T17:01:15Z&spr=https&sig=nNpMARshWaVt834sDpwGXLp5%2BfAQtnrMcSQmWqf8o%2Fk%3D";
                    room.Image = image;
                }
            }
        }

        private void UpdateExitsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var area = (Area)this.pgArea.SelectedObject;

            foreach (var container in this.SelectedContainers)
            {
                var room = container.SelectedRoom;

                if (room != null)
                {
                    foreach (var exit in room.Exits)
                    {
                        // Ensure the exit is to a room in this area before we fix it.
                        var isInArea = area.Rooms.Any(r => r.RoomId == exit.ToRoom);

                        if (isInArea)
                        {
                            exit.ToArea = area.AreaId;
                        }
                    }
                }
            }

            this.toolStripStatusLabel1.Text = "Updated all exits.";
        }

        private void AwardEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void MountainToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.SelectedContainers.Count > 0)
            {
                foreach (var container in this.SelectedContainers)
                {
                    var room = container.SelectedRoom;
                    if (room != null)
                    {
                        room.Terrain = Terrain.Mountains;
                    }
                }

                this.Cursor = Cursors.Default;
                this.toolStripStatusLabel1.Text = $"Selected rooms set to mountains.";
                this.statusStrip1.Update();
            }
            else
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show("No rooms selected!");
            }
        }

        private void CavesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.SelectedContainers.Count > 0)
            {
                foreach (var container in this.SelectedContainers)
                {
                    var room = container.SelectedRoom;
                    if (room != null)
                    {
                        room.Terrain = Terrain.Caves;
                    }
                }

                this.Cursor = Cursors.Default;
                this.toolStripStatusLabel1.Text = $"Selected rooms set to caves.";
                this.statusStrip1.Update();
            }
            else
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show("No rooms selected!");
            }
        }

        private void ResetRoomDescriptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.SelectedContainers.Count > 0)
            {
                foreach (var container in this.SelectedContainers)
                {
                    var room = container.SelectedRoom;
                    if (room != null)
                    {
                        room.Description = string.Empty;
                    }
                }

                this.Cursor = Cursors.Default;
                this.toolStripStatusLabel1.Text = $"Selected room descriptions reset.";
                this.statusStrip1.Update();
            }
            else
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show("No rooms selected!");
            }
        }

        private void DesertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.SelectedContainers.Count > 0)
            {
                foreach (var container in this.SelectedContainers)
                {
                    var room = container.SelectedRoom;
                    if (room != null)
                    {
                        room.Terrain = Terrain.Desert;
                    }
                }

                this.Cursor = Cursors.Default;
                this.toolStripStatusLabel1.Text = $"Selected rooms set to desert.";
                this.statusStrip1.Update();
            }
            else
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show("No rooms selected!");
            }
        }

        private void ResetRoomImagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.SelectedContainers.Count > 0)
            {
                foreach (var container in this.SelectedContainers)
                {
                    var room = container.SelectedRoom;

                    if (room != null)
                    {
                        room.Image = string.Empty;
                    }
                }

                this.Cursor = Cursors.Default;
                this.toolStripStatusLabel1.Text = $"Selected room images reset.";
                this.statusStrip1.Update();
            }
            else
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show("No rooms selected!");
            }
        }

        private void UpdateItemImagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.toolStripStatusLabel1.Text = $"Updating...";

            var items = this.mongo.Items.Find(_ => true).ToList();

            foreach (var item in items)
            {
                var link = $"https://legendaryweb.file.core.windows.net/images/items/{item.ItemId}.png?sv=2021-12-02&ss=bfqt&srt=sco&sp=rwdlacupiytfx&se=2025-03-02T01:01:15Z&st=2023-03-10T17:01:15Z&spr=https&sig=nNpMARshWaVt834sDpwGXLp5%2BfAQtnrMcSQmWqf8o%2Fk%3D";
                item.Image = link;
                this.mongo.Items.ReplaceOne(m => m.ItemId == item.ItemId, item);
            }

            this.toolStripStatusLabel1.Text = $"Updated images.";
        }

        private bool UploadRoomImage(Room room, Stream stream)
        {
            // string storageURL = $"https://legendaryweb.file.core.windows.net/images/{room.RoomId}.png?comp=range HTTP/1.1";
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=legendaryweb;AccountKey=SZ7oVHyhh/QghSiA+XL4xqwGxszmBcHgzOYQbYbdAS/3m2SqvXhdg7Tafgew9X/DDidE93Q9TuNq+AStP6A66Q==;EndpointSuffix=core.windows.net";

            // Name of the share, directory, and file we'll create
            string shareName = "images";
            string fileName = $"{room.RoomId}.png";
            ShareClient share = new(connectionString, shareName);
            share.CreateIfNotExists();

            ShareDirectoryClient directory = share.GetDirectoryClient("/");

            try
            {
                // Get a reference to a file and upload it
                ShareFileClient file = directory.GetFileClient(fileName);

                string imageUrl = $"https://legendaryweb.file.core.windows.net/images/{room.RoomId}.png?sv=2021-12-02&ss=bfqt&srt=sco&sp=rwdlacupiytfx&se=2025-03-02T01:01:15Z&st=2023-03-10T17:01:15Z&spr=https&sig=nNpMARshWaVt834sDpwGXLp5%2BfAQtnrMcSQmWqf8o%2Fk%3D";

                var response = file.Create(stream.Length);

                file.UploadRange(
                    new HttpRange(0, stream.Length),
                    stream);

                room.Image = imageUrl;
                this.toolStripStatusLabel1.Text = $"Uploaded OpenAI image for room {room.Name}: {imageUrl}!";

                return true;
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.ToString());
            }

            return false;
        }

        private void IndoorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.SelectedContainers.Count > 0)
            {
                foreach (var container in this.SelectedContainers)
                {
                    var room = container.SelectedRoom;

                    if (room != null)
                    {
                        room.Flags ??= new List<RoomFlags>();

                        if (!room.Flags.Contains(RoomFlags.Indoors))
                        {
                            room.Flags.Add(RoomFlags.Indoors);
                        }
                    }
                }

                this.Cursor = Cursors.Default;
                this.toolStripStatusLabel1.Text = $"Selected rooms set to indoors.";
                this.statusStrip1.Update();
            }
            else
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show("No rooms selected!");
            }
        }

        private void PersonaEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Default;
            this.toolStripStatusLabel1.Text = $"Loading personas, please wait...";
            this.statusStrip1.Update();
            var frm = new PersonaEditor(this.mongo);
            frm.ShowDialog();
        }

        private void ShipToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.SelectedContainers.Count > 0)
            {
                foreach (var container in this.SelectedContainers)
                {
                    var room = container.SelectedRoom;
                    if (room != null)
                    {
                        room.Terrain = Terrain.Ship;
                    }
                }

                this.Cursor = Cursors.Default;
                this.toolStripStatusLabel1.Text = $"Selected rooms set to ship.";
                this.statusStrip1.Update();
            }
            else
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show("No rooms selected!");
            }
        }

        private void GenerateRoomImagesToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (this.SelectedContainers.Count > 0)
            {
                int counter = 1;

                this.Cursor = Cursors.WaitCursor;
                this.toolStripStatusLabel1.Text = $"Generating images, please wait...";
                this.statusStrip1.Update();

                var area = this.pgArea.SelectedObject as Area;

                foreach (var container in this.SelectedContainers)
                {
                    var room = container.SelectedRoom;

                    if (room != null)
                    {
                        if (!string.IsNullOrWhiteSpace(room.Name) && !string.IsNullOrWhiteSpace(room.Description))
                        {
                            if (string.IsNullOrWhiteSpace(room.Image))
                            {
                                string prompt = $"Create a detailed photo of a room named {room?.Name} that looks like this description: {room?.Description}.";

                                try
                                {
                                    var image = new ChatGPTService().Image(prompt);

                                    if (!string.IsNullOrWhiteSpace(image))
                                    {
                                        HttpClient client = new();
                                        Stream stream = client.GetStreamAsync(image).Result;

                                        Image img = Image.FromStream(stream);

                                        MemoryStream mstream = new();
                                        img.Save(mstream, System.Drawing.Imaging.ImageFormat.Png);
                                        mstream.Position = 0;

                                        if (room != null)
                                        {
                                            this.UploadRoomImage(room, mstream);
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("Unable to load image from DALL-E.");
                                        continue;
                                    }
                                }
                                catch (Exception exc)
                                {
                                    MessageBox.Show(exc.ToString());
                                    continue;
                                }
                            }
                            else
                            {
                                this.toolStripStatusLabel1.Text = $"Room {room.Name} ({counter++} of {this.SelectedContainers.Count}) had an image. Skipped.";
                            }

                            this.statusStrip1.Update();

                            if (container.Selected)
                            {
                                this.pgRoom.SelectedObject = container.SelectedRoom;
                            }
                        }
                        else
                        {
                            MessageBox.Show("Room is missing description or name. Please set these before continuing.");
                            return;
                        }
                    }
                }

                this.Cursor = Cursors.Default;
                this.toolStripStatusLabel1.Text = $"AI image generation complete!";
                this.statusStrip1.Update();
            }
            else
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show("No rooms selected!");
            }
        }

        private void NamesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var roomCount = 0;
            var roomsNoNames = 0;

            foreach (var control in this.tableLayoutPanel1.Controls)
            {
                if (control is Panel pnl)
                {
                    if (pnl.Controls.Count > 0)
                    {
                        RoomContainer ctr = (RoomContainer)pnl.Controls[0];

                        var room = ctr.SelectedRoom;

                        if (room != null)
                        {
                            roomCount++;

                            ctr.Highlight(false);

                            if (string.IsNullOrWhiteSpace(room.Name))
                            {
                                ctr.Highlight(true);
                                roomsNoNames++;
                            }
                        }
                    }
                }
            }

            MessageBox.Show($"There are {roomCount} total rooms.\n\rFound {roomsNoNames} with no name.");
        }

        private void DescriptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var roomCount = 0;
            var roomsNoDescs = 0;

            foreach (var control in this.tableLayoutPanel1.Controls)
            {
                if (control is Panel pnl)
                {
                    if (pnl.Controls.Count > 0)
                    {
                        RoomContainer ctr = (RoomContainer)pnl.Controls[0];

                        var room = ctr.SelectedRoom;

                        if (room != null)
                        {
                            roomCount++;

                            ctr.Highlight(false);

                            if (string.IsNullOrWhiteSpace(room.Description))
                            {
                                ctr.Highlight(true);
                                roomsNoDescs++;
                            }
                        }
                    }
                }
            }

            MessageBox.Show($"There are {roomCount} total rooms.\n\rFound {roomsNoDescs} with no description.");
        }

        private void ImagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var roomCount = 0;
            var roomsNoImage = 0;

            foreach (var control in this.tableLayoutPanel1.Controls)
            {
                if (control is Panel pnl)
                {
                    if (pnl.Controls.Count > 0)
                    {
                        RoomContainer ctr = (RoomContainer)pnl.Controls[0];

                        var room = ctr.SelectedRoom;

                        if (room != null)
                        {
                            roomCount++;

                            ctr.Highlight(false);

                            if (string.IsNullOrWhiteSpace(room.Image))
                            {
                                ctr.Highlight(true);
                                roomsNoImage++;
                            }
                        }
                    }
                }
            }

            MessageBox.Show($"There are {roomCount} total rooms.\n\rFound {roomsNoImage} with no image.");
        }

        private void ResetsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.pgArea.SelectedObject is Area area && this.pgRoom.SelectedObject is Room room)
            {
                var frm = new RoomResetsForm(this.mongo, area, room);
                frm.ShowDialog();
            }
        }
    }
}
