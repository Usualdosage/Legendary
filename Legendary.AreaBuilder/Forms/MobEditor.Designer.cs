namespace Legendary.AreaBuilder
{
    partial class MobEditor
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
            propertyGrid1 = new PropertyGrid();
            button1 = new Button();
            button2 = new Button();
            listBox1 = new ListBox();
            statusStrip1 = new StatusStrip();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            button3 = new Button();
            button4 = new Button();
            button5 = new Button();
            pictureBox1 = new PictureBox();
            btnUploadImage = new Button();
            lstItems = new ListBox();
            btnAddReset = new Button();
            BtnWield = new Button();
            statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // propertyGrid1
            // 
            propertyGrid1.Location = new Point(282, 12);
            propertyGrid1.Name = "propertyGrid1";
            propertyGrid1.Size = new Size(598, 382);
            propertyGrid1.TabIndex = 0;
            // 
            // button1
            // 
            button1.Location = new Point(1067, 451);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 1;
            button1.Text = "Save";
            button1.UseVisualStyleBackColor = true;
            button1.Click += Button1_Click;
            // 
            // button2
            // 
            button2.Location = new Point(986, 451);
            button2.Name = "button2";
            button2.Size = new Size(75, 23);
            button2.TabIndex = 2;
            button2.Text = "Cancel";
            button2.UseVisualStyleBackColor = true;
            button2.Click += Button2_Click;
            // 
            // listBox1
            // 
            listBox1.DisplayMember = "FirstName";
            listBox1.FormattingEnabled = true;
            listBox1.ItemHeight = 15;
            listBox1.Location = new Point(12, 16);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(264, 379);
            listBox1.TabIndex = 3;
            listBox1.SelectedIndexChanged += ListBox1_SelectedIndexChanged;
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1 });
            statusStrip1.Location = new Point(0, 485);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(1154, 22);
            statusStrip1.TabIndex = 4;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(42, 17);
            toolStripStatusLabel1.Text = "Ready.";
            // 
            // button3
            // 
            button3.Location = new Point(282, 400);
            button3.Name = "button3";
            button3.Size = new Size(105, 23);
            button3.TabIndex = 5;
            button3.Text = "Find Image";
            button3.UseVisualStyleBackColor = true;
            button3.Click += Button3_Click;
            // 
            // button4
            // 
            button4.Location = new Point(504, 400);
            button4.Name = "button4";
            button4.Size = new Size(156, 23);
            button4.TabIndex = 12;
            button4.Text = "Generate Description";
            button4.UseVisualStyleBackColor = true;
            button4.Click += Button4_Click;
            // 
            // button5
            // 
            button5.Location = new Point(666, 400);
            button5.Name = "button5";
            button5.Size = new Size(105, 23);
            button5.TabIndex = 13;
            button5.Text = "Create Clone";
            button5.UseVisualStyleBackColor = true;
            button5.Click += Button5_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.BorderStyle = BorderStyle.FixedSingle;
            pictureBox1.Location = new Point(886, 38);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(256, 256);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 14;
            pictureBox1.TabStop = false;
            // 
            // btnUploadImage
            // 
            btnUploadImage.Location = new Point(393, 400);
            btnUploadImage.Name = "btnUploadImage";
            btnUploadImage.Size = new Size(105, 23);
            btnUploadImage.TabIndex = 15;
            btnUploadImage.Text = "Upload Image";
            btnUploadImage.UseVisualStyleBackColor = true;
            btnUploadImage.Click += BtnUploadImage_Click;
            // 
            // lstItems
            // 
            lstItems.FormattingEnabled = true;
            lstItems.ItemHeight = 15;
            lstItems.Location = new Point(886, 300);
            lstItems.Name = "lstItems";
            lstItems.Size = new Size(256, 94);
            lstItems.TabIndex = 16;
            // 
            // btnAddReset
            // 
            btnAddReset.Location = new Point(1057, 400);
            btnAddReset.Name = "btnAddReset";
            btnAddReset.Size = new Size(85, 23);
            btnAddReset.TabIndex = 17;
            btnAddReset.Text = "Inventory";
            btnAddReset.UseVisualStyleBackColor = true;
            btnAddReset.Click += BtnAddReset_Click;
            // 
            // BtnWield
            // 
            BtnWield.Location = new Point(974, 400);
            BtnWield.Name = "BtnWield";
            BtnWield.Size = new Size(77, 23);
            BtnWield.TabIndex = 19;
            BtnWield.Text = "Equip";
            BtnWield.UseVisualStyleBackColor = true;
            BtnWield.Click += BtnWield_Click;
            // 
            // MobEditor
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1154, 507);
            Controls.Add(BtnWield);
            Controls.Add(btnAddReset);
            Controls.Add(lstItems);
            Controls.Add(btnUploadImage);
            Controls.Add(pictureBox1);
            Controls.Add(button5);
            Controls.Add(button4);
            Controls.Add(button3);
            Controls.Add(statusStrip1);
            Controls.Add(listBox1);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(propertyGrid1);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Name = "MobEditor";
            Text = "MobEditor";
            Load += MobEditor_Load;
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PropertyGrid propertyGrid1;
        private Button button1;
        private Button button2;
        private ListBox listBox1;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private Button button3;
        private Button button4;
        private Button button5;
        private PictureBox pictureBox1;
        private Button btnUploadImage;
        private ListBox lstItems;
        private Button btnAddReset;
        private Button BtnWield;
    }
}