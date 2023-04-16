namespace Legendary.AreaBuilder
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.pgArea = new System.Windows.Forms.PropertyGrid();
            this.pgRoom = new System.Windows.Forms.PropertyGrid();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newAreaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.exportAreaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAreaToDatabaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.importAreaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadAreaFromDatabaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mobEditorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.itemEditorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.awardEditorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.roomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renumberRoomsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.validateAreaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.connectSelectedRoomsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.disconnectSelectedRoomsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.aIDescriptionGeneratorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetRoomDescriptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.generateRoomImagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.generateOpenAIRoomImagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetRoomImagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.prePopulateRoomImagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setSelectedToIndoorsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.updateExitsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setSelectedRoomsToToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mountainToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cityToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hillsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cavesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.desertToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectAllRoomsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deselectAllRoomsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.externalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.convertFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deconvertFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aITrainerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Cursor = System.Windows.Forms.Cursors.VSplit;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.statusStrip1);
            this.splitContainer1.Panel1.Controls.Add(this.tableLayoutPanel1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(949, 496);
            this.splitContainer1.SplitterDistance = 764;
            this.splitContainer1.TabIndex = 0;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 474);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(764, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(42, 17);
            this.toolStripStatusLabel1.Text = "Ready.";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 20;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 20;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(764, 496);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Cursor = System.Windows.Forms.Cursors.HSplit;
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.pgArea);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.pgRoom);
            this.splitContainer2.Size = new System.Drawing.Size(181, 496);
            this.splitContainer2.SplitterDistance = 208;
            this.splitContainer2.TabIndex = 0;
            // 
            // pgArea
            // 
            this.pgArea.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pgArea.Location = new System.Drawing.Point(0, 0);
            this.pgArea.Name = "pgArea";
            this.pgArea.Size = new System.Drawing.Size(181, 208);
            this.pgArea.TabIndex = 0;
            this.pgArea.SelectedObjectsChanged += new System.EventHandler(this.pgArea_SelectedObjectsChanged);
            // 
            // pgRoom
            // 
            this.pgRoom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pgRoom.Location = new System.Drawing.Point(0, 0);
            this.pgRoom.Name = "pgRoom";
            this.pgRoom.Size = new System.Drawing.Size(181, 284);
            this.pgRoom.TabIndex = 0;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.roomToolStripMenuItem,
            this.selectToolStripMenuItem,
            this.externalToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(949, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newAreaToolStripMenuItem,
            this.toolStripSeparator5,
            this.exportAreaToolStripMenuItem,
            this.saveAreaToDatabaseToolStripMenuItem,
            this.toolStripSeparator4,
            this.importAreaToolStripMenuItem,
            this.loadAreaFromDatabaseToolStripMenuItem,
            this.toolStripSeparator3,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // newAreaToolStripMenuItem
            // 
            this.newAreaToolStripMenuItem.Name = "newAreaToolStripMenuItem";
            this.newAreaToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newAreaToolStripMenuItem.Size = new System.Drawing.Size(227, 22);
            this.newAreaToolStripMenuItem.Text = "&New Area";
            this.newAreaToolStripMenuItem.Click += new System.EventHandler(this.newAreaToolStripMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(224, 6);
            // 
            // exportAreaToolStripMenuItem
            // 
            this.exportAreaToolStripMenuItem.Name = "exportAreaToolStripMenuItem";
            this.exportAreaToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.exportAreaToolStripMenuItem.Size = new System.Drawing.Size(227, 22);
            this.exportAreaToolStripMenuItem.Text = "&Save Area to JSON";
            this.exportAreaToolStripMenuItem.Click += new System.EventHandler(this.exportAreaToolStripMenuItem_Click);
            // 
            // saveAreaToDatabaseToolStripMenuItem
            // 
            this.saveAreaToDatabaseToolStripMenuItem.Name = "saveAreaToDatabaseToolStripMenuItem";
            this.saveAreaToDatabaseToolStripMenuItem.Size = new System.Drawing.Size(227, 22);
            this.saveAreaToDatabaseToolStripMenuItem.Text = "Save Area to Database";
            this.saveAreaToDatabaseToolStripMenuItem.Click += new System.EventHandler(this.saveAreaToDatabaseToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(224, 6);
            // 
            // importAreaToolStripMenuItem
            // 
            this.importAreaToolStripMenuItem.Name = "importAreaToolStripMenuItem";
            this.importAreaToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.L)));
            this.importAreaToolStripMenuItem.Size = new System.Drawing.Size(227, 22);
            this.importAreaToolStripMenuItem.Text = "&Load Area from JSON";
            this.importAreaToolStripMenuItem.Click += new System.EventHandler(this.importAreaToolStripMenuItem_Click);
            // 
            // loadAreaFromDatabaseToolStripMenuItem
            // 
            this.loadAreaFromDatabaseToolStripMenuItem.Name = "loadAreaFromDatabaseToolStripMenuItem";
            this.loadAreaFromDatabaseToolStripMenuItem.Size = new System.Drawing.Size(227, 22);
            this.loadAreaFromDatabaseToolStripMenuItem.Text = "Load Area from Database";
            this.loadAreaFromDatabaseToolStripMenuItem.Click += new System.EventHandler(this.loadAreaFromDatabaseToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(224, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.End)));
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(227, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mobEditorToolStripMenuItem,
            this.itemEditorToolStripMenuItem,
            this.awardEditorToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // mobEditorToolStripMenuItem
            // 
            this.mobEditorToolStripMenuItem.Name = "mobEditorToolStripMenuItem";
            this.mobEditorToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.mobEditorToolStripMenuItem.Text = "Mob Editor";
            this.mobEditorToolStripMenuItem.Click += new System.EventHandler(this.mobEditorToolStripMenuItem_Click);
            // 
            // itemEditorToolStripMenuItem
            // 
            this.itemEditorToolStripMenuItem.Name = "itemEditorToolStripMenuItem";
            this.itemEditorToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.itemEditorToolStripMenuItem.Text = "Item Editor";
            this.itemEditorToolStripMenuItem.Click += new System.EventHandler(this.ItemEditorToolStripMenuItem_Click);
            // 
            // awardEditorToolStripMenuItem
            // 
            this.awardEditorToolStripMenuItem.Name = "awardEditorToolStripMenuItem";
            this.awardEditorToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.awardEditorToolStripMenuItem.Text = "Award Editor";
            this.awardEditorToolStripMenuItem.Click += new System.EventHandler(this.awardEditorToolStripMenuItem_Click);
            // 
            // roomToolStripMenuItem
            // 
            this.roomToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.renumberRoomsToolStripMenuItem,
            this.validateAreaToolStripMenuItem,
            this.toolStripSeparator2,
            this.connectSelectedRoomsToolStripMenuItem,
            this.disconnectSelectedRoomsToolStripMenuItem,
            this.toolStripSeparator1,
            this.aIDescriptionGeneratorToolStripMenuItem,
            this.resetRoomDescriptionsToolStripMenuItem,
            this.generateRoomImagesToolStripMenuItem,
            this.generateOpenAIRoomImagesToolStripMenuItem,
            this.resetRoomImagesToolStripMenuItem,
            this.toolStripSeparator6,
            this.prePopulateRoomImagesToolStripMenuItem,
            this.setSelectedToIndoorsToolStripMenuItem,
            this.toolStripSeparator7,
            this.updateExitsToolStripMenuItem,
            this.setSelectedRoomsToToolStripMenuItem});
            this.roomToolStripMenuItem.Name = "roomToolStripMenuItem";
            this.roomToolStripMenuItem.Size = new System.Drawing.Size(46, 20);
            this.roomToolStripMenuItem.Text = "Tools";
            this.roomToolStripMenuItem.Click += new System.EventHandler(this.roomToolStripMenuItem_Click);
            // 
            // renumberRoomsToolStripMenuItem
            // 
            this.renumberRoomsToolStripMenuItem.Name = "renumberRoomsToolStripMenuItem";
            this.renumberRoomsToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.renumberRoomsToolStripMenuItem.Text = "Re-number Rooms";
            this.renumberRoomsToolStripMenuItem.Click += new System.EventHandler(this.renumberRoomsToolStripMenuItem_Click);
            // 
            // validateAreaToolStripMenuItem
            // 
            this.validateAreaToolStripMenuItem.Name = "validateAreaToolStripMenuItem";
            this.validateAreaToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.validateAreaToolStripMenuItem.Text = "Validate Area";
            this.validateAreaToolStripMenuItem.Click += new System.EventHandler(this.validateAreaToolStripMenuItem_Click_1);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(237, 6);
            // 
            // connectSelectedRoomsToolStripMenuItem
            // 
            this.connectSelectedRoomsToolStripMenuItem.Enabled = false;
            this.connectSelectedRoomsToolStripMenuItem.Name = "connectSelectedRoomsToolStripMenuItem";
            this.connectSelectedRoomsToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.connectSelectedRoomsToolStripMenuItem.Text = "Connect Selected Rooms";
            this.connectSelectedRoomsToolStripMenuItem.Click += new System.EventHandler(this.connectSelectedRoomsToolStripMenuItem_Click);
            // 
            // disconnectSelectedRoomsToolStripMenuItem
            // 
            this.disconnectSelectedRoomsToolStripMenuItem.Enabled = false;
            this.disconnectSelectedRoomsToolStripMenuItem.Name = "disconnectSelectedRoomsToolStripMenuItem";
            this.disconnectSelectedRoomsToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.disconnectSelectedRoomsToolStripMenuItem.Text = "Disconnect Selected Rooms";
            this.disconnectSelectedRoomsToolStripMenuItem.Click += new System.EventHandler(this.disconnectSelectedRoomsToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(237, 6);
            // 
            // aIDescriptionGeneratorToolStripMenuItem
            // 
            this.aIDescriptionGeneratorToolStripMenuItem.Name = "aIDescriptionGeneratorToolStripMenuItem";
            this.aIDescriptionGeneratorToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.aIDescriptionGeneratorToolStripMenuItem.Text = "Generate Room Descriptions";
            this.aIDescriptionGeneratorToolStripMenuItem.Click += new System.EventHandler(this.aIDescriptionGeneratorToolStripMenuItem_Click);
            // 
            // resetRoomDescriptionsToolStripMenuItem
            // 
            this.resetRoomDescriptionsToolStripMenuItem.Name = "resetRoomDescriptionsToolStripMenuItem";
            this.resetRoomDescriptionsToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.resetRoomDescriptionsToolStripMenuItem.Text = "Reset Room Descriptions";
            this.resetRoomDescriptionsToolStripMenuItem.Click += new System.EventHandler(this.ResetRoomDescriptionsToolStripMenuItem_Click);
            // 
            // generateRoomImagesToolStripMenuItem
            // 
            this.generateRoomImagesToolStripMenuItem.Name = "generateRoomImagesToolStripMenuItem";
            this.generateRoomImagesToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.generateRoomImagesToolStripMenuItem.Text = "Generate Room Images";
            this.generateRoomImagesToolStripMenuItem.Click += new System.EventHandler(this.generateRoomImagesToolStripMenuItem_Click);
            // 
            // generateOpenAIRoomImagesToolStripMenuItem
            // 
            this.generateOpenAIRoomImagesToolStripMenuItem.Name = "generateOpenAIRoomImagesToolStripMenuItem";
            this.generateOpenAIRoomImagesToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.generateOpenAIRoomImagesToolStripMenuItem.Text = "Generate OpenAI Room Images";
            this.generateOpenAIRoomImagesToolStripMenuItem.Click += new System.EventHandler(this.GenerateOpenAIRoomImagesToolStripMenuItem_Click);
            // 
            // resetRoomImagesToolStripMenuItem
            // 
            this.resetRoomImagesToolStripMenuItem.Name = "resetRoomImagesToolStripMenuItem";
            this.resetRoomImagesToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.resetRoomImagesToolStripMenuItem.Text = "Reset Room Images";
            this.resetRoomImagesToolStripMenuItem.Click += new System.EventHandler(this.ResetRoomImagesToolStripMenuItem_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(237, 6);
            // 
            // prePopulateRoomImagesToolStripMenuItem
            // 
            this.prePopulateRoomImagesToolStripMenuItem.Name = "prePopulateRoomImagesToolStripMenuItem";
            this.prePopulateRoomImagesToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.prePopulateRoomImagesToolStripMenuItem.Text = "Pre-Populate Room Images";
            this.prePopulateRoomImagesToolStripMenuItem.Click += new System.EventHandler(this.PrePopulateRoomImagesToolStripMenuItem_Click);
            // 
            // setSelectedToIndoorsToolStripMenuItem
            // 
            this.setSelectedToIndoorsToolStripMenuItem.Name = "setSelectedToIndoorsToolStripMenuItem";
            this.setSelectedToIndoorsToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.setSelectedToIndoorsToolStripMenuItem.Text = "Set Selected to Indoors";
            this.setSelectedToIndoorsToolStripMenuItem.Click += new System.EventHandler(this.SetSelectedToIndoorsToolStripMenuItem_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(237, 6);
            // 
            // updateExitsToolStripMenuItem
            // 
            this.updateExitsToolStripMenuItem.Name = "updateExitsToolStripMenuItem";
            this.updateExitsToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.updateExitsToolStripMenuItem.Text = "Update Exits";
            this.updateExitsToolStripMenuItem.Click += new System.EventHandler(this.UpdateExitsToolStripMenuItem_Click);
            // 
            // setSelectedRoomsToToolStripMenuItem
            // 
            this.setSelectedRoomsToToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mountainToolStripMenuItem,
            this.cityToolStripMenuItem,
            this.hillsToolStripMenuItem,
            this.cavesToolStripMenuItem,
            this.desertToolStripMenuItem});
            this.setSelectedRoomsToToolStripMenuItem.Name = "setSelectedRoomsToToolStripMenuItem";
            this.setSelectedRoomsToToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.setSelectedRoomsToToolStripMenuItem.Text = "Set Selected Rooms To";
            // 
            // mountainToolStripMenuItem
            // 
            this.mountainToolStripMenuItem.Name = "mountainToolStripMenuItem";
            this.mountainToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            this.mountainToolStripMenuItem.Text = "Mountain";
            this.mountainToolStripMenuItem.Click += new System.EventHandler(this.MountainToolStripMenuItem_Click);
            // 
            // cityToolStripMenuItem
            // 
            this.cityToolStripMenuItem.Name = "cityToolStripMenuItem";
            this.cityToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            this.cityToolStripMenuItem.Text = "City";
            // 
            // hillsToolStripMenuItem
            // 
            this.hillsToolStripMenuItem.Name = "hillsToolStripMenuItem";
            this.hillsToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            this.hillsToolStripMenuItem.Text = "Hills";
            // 
            // cavesToolStripMenuItem
            // 
            this.cavesToolStripMenuItem.Name = "cavesToolStripMenuItem";
            this.cavesToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            this.cavesToolStripMenuItem.Text = "Caves";
            this.cavesToolStripMenuItem.Click += new System.EventHandler(this.CavesToolStripMenuItem_Click);
            // 
            // desertToolStripMenuItem
            // 
            this.desertToolStripMenuItem.Name = "desertToolStripMenuItem";
            this.desertToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            this.desertToolStripMenuItem.Text = "Desert";
            this.desertToolStripMenuItem.Click += new System.EventHandler(this.DesertToolStripMenuItem_Click);
            // 
            // selectToolStripMenuItem
            // 
            this.selectToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectAllRoomsToolStripMenuItem,
            this.deselectAllRoomsToolStripMenuItem});
            this.selectToolStripMenuItem.Name = "selectToolStripMenuItem";
            this.selectToolStripMenuItem.Size = new System.Drawing.Size(50, 20);
            this.selectToolStripMenuItem.Text = "Select";
            // 
            // selectAllRoomsToolStripMenuItem
            // 
            this.selectAllRoomsToolStripMenuItem.Name = "selectAllRoomsToolStripMenuItem";
            this.selectAllRoomsToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.selectAllRoomsToolStripMenuItem.Text = "Select All Rooms";
            this.selectAllRoomsToolStripMenuItem.Click += new System.EventHandler(this.selectAllRoomsToolStripMenuItem_Click);
            // 
            // deselectAllRoomsToolStripMenuItem
            // 
            this.deselectAllRoomsToolStripMenuItem.Name = "deselectAllRoomsToolStripMenuItem";
            this.deselectAllRoomsToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.deselectAllRoomsToolStripMenuItem.Text = "Deselect All Rooms";
            this.deselectAllRoomsToolStripMenuItem.Click += new System.EventHandler(this.deselectAllRoomsToolStripMenuItem_Click);
            // 
            // externalToolStripMenuItem
            // 
            this.externalToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.convertFolderToolStripMenuItem,
            this.deconvertFilesToolStripMenuItem,
            this.aITrainerToolStripMenuItem});
            this.externalToolStripMenuItem.Name = "externalToolStripMenuItem";
            this.externalToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.externalToolStripMenuItem.Text = "External";
            // 
            // convertFolderToolStripMenuItem
            // 
            this.convertFolderToolStripMenuItem.Name = "convertFolderToolStripMenuItem";
            this.convertFolderToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.convertFolderToolStripMenuItem.Text = "Convert Files";
            this.convertFolderToolStripMenuItem.Click += new System.EventHandler(this.convertFolderToolStripMenuItem_Click);
            // 
            // deconvertFilesToolStripMenuItem
            // 
            this.deconvertFilesToolStripMenuItem.Name = "deconvertFilesToolStripMenuItem";
            this.deconvertFilesToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.deconvertFilesToolStripMenuItem.Text = "Deconvert Files";
            this.deconvertFilesToolStripMenuItem.Click += new System.EventHandler(this.deconvertFilesToolStripMenuItem_Click);
            // 
            // aITrainerToolStripMenuItem
            // 
            this.aITrainerToolStripMenuItem.Name = "aITrainerToolStripMenuItem";
            this.aITrainerToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.aITrainerToolStripMenuItem.Text = "AI Trainer";
            this.aITrainerToolStripMenuItem.Click += new System.EventHandler(this.aITrainerToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(949, 520);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Area Grid";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private SplitContainer splitContainer1;
        private TableLayoutPanel tableLayoutPanel1;
        private SplitContainer splitContainer2;
        private PropertyGrid pgRoom;
        private PropertyGrid pgArea;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem exportAreaToolStripMenuItem;
        private ToolStripMenuItem importAreaToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private ToolStripMenuItem roomToolStripMenuItem;
        private ToolStripMenuItem newAreaToolStripMenuItem;
        private ToolStripMenuItem validateAreaToolStripMenuItem;
        private ToolStripMenuItem connectSelectedRoomsToolStripMenuItem;
        private ToolStripMenuItem renumberRoomsToolStripMenuItem;
        private ToolStripMenuItem disconnectSelectedRoomsToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem aIDescriptionGeneratorToolStripMenuItem;
        private ToolStripMenuItem selectToolStripMenuItem;
        private ToolStripMenuItem selectAllRoomsToolStripMenuItem;
        private ToolStripMenuItem deselectAllRoomsToolStripMenuItem;
        private ToolStripMenuItem generateRoomImagesToolStripMenuItem;
        private ToolStripMenuItem externalToolStripMenuItem;
        private ToolStripMenuItem convertFolderToolStripMenuItem;
        private ToolStripMenuItem deconvertFilesToolStripMenuItem;
        private ToolStripMenuItem editToolStripMenuItem;
        private ToolStripMenuItem mobEditorToolStripMenuItem;
        private ToolStripMenuItem saveAreaToDatabaseToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripMenuItem loadAreaFromDatabaseToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem itemEditorToolStripMenuItem;
        private ToolStripMenuItem prePopulateRoomImagesToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator6;
        private ToolStripSeparator toolStripSeparator7;
        private ToolStripMenuItem updateExitsToolStripMenuItem;
        private ToolStripMenuItem awardEditorToolStripMenuItem;
        private ToolStripMenuItem setSelectedToIndoorsToolStripMenuItem;
        private ToolStripMenuItem aITrainerToolStripMenuItem;
        private ToolStripMenuItem setSelectedRoomsToToolStripMenuItem;
        private ToolStripMenuItem mountainToolStripMenuItem;
        private ToolStripMenuItem cityToolStripMenuItem;
        private ToolStripMenuItem hillsToolStripMenuItem;
        private ToolStripMenuItem cavesToolStripMenuItem;
        private ToolStripMenuItem resetRoomDescriptionsToolStripMenuItem;
        private ToolStripMenuItem desertToolStripMenuItem;
        private ToolStripMenuItem resetRoomImagesToolStripMenuItem;
        private ToolStripMenuItem generateOpenAIRoomImagesToolStripMenuItem;
    }
}