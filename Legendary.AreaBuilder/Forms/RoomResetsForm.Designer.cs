namespace Legendary.AreaBuilder.Forms
{
    partial class RoomResetsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            groupBox1 = new GroupBox();
            lstMobiles = new ListBox();
            groupBox2 = new GroupBox();
            lstItems = new ListBox();
            btnAddMobile = new Button();
            btnAddItem = new Button();
            statusStrip1 = new StatusStrip();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(lstMobiles);
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(335, 402);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Mobiles";
            // 
            // lstMobiles
            // 
            lstMobiles.FormattingEnabled = true;
            lstMobiles.ItemHeight = 15;
            lstMobiles.Location = new Point(18, 22);
            lstMobiles.Name = "lstMobiles";
            lstMobiles.Size = new Size(300, 364);
            lstMobiles.TabIndex = 0;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(lstItems);
            groupBox2.Location = new Point(362, 12);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(335, 402);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "Items";
            // 
            // lstItems
            // 
            lstItems.FormattingEnabled = true;
            lstItems.ItemHeight = 15;
            lstItems.Location = new Point(18, 22);
            lstItems.Name = "lstItems";
            lstItems.Size = new Size(300, 364);
            lstItems.TabIndex = 1;
            // 
            // btnAddMobile
            // 
            btnAddMobile.Location = new Point(242, 420);
            btnAddMobile.Name = "btnAddMobile";
            btnAddMobile.Size = new Size(105, 23);
            btnAddMobile.TabIndex = 2;
            btnAddMobile.Text = "Add Mobile";
            btnAddMobile.UseVisualStyleBackColor = true;
            btnAddMobile.Click += BtnAddMobile_Click;
            // 
            // btnAddItem
            // 
            btnAddItem.Location = new Point(593, 420);
            btnAddItem.Name = "btnAddItem";
            btnAddItem.Size = new Size(104, 23);
            btnAddItem.TabIndex = 3;
            btnAddItem.Text = "Add Item";
            btnAddItem.UseVisualStyleBackColor = true;
            btnAddItem.Click += BtnAddItem_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1 });
            statusStrip1.Location = new Point(0, 468);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(714, 22);
            statusStrip1.TabIndex = 4;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(42, 17);
            toolStripStatusLabel1.Text = "Ready.";
            // 
            // RoomResetsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(714, 490);
            Controls.Add(statusStrip1);
            Controls.Add(btnAddItem);
            Controls.Add(btnAddMobile);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Name = "RoomResetsForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Room Resets";
            Load += RoomResetsForm_Load;
            groupBox1.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private Button btnAddMobile;
        private Button btnAddItem;
        private ListBox lstMobiles;
        private ListBox lstItems;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolStripStatusLabel1;
    }
}