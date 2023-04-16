// <copyright file="ItemEditor.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.AreaBuilder
{
    using System.Net;
    using Azure;
    using Azure.Storage.Files.Shares;
    using Legendary.AreaBuilder.Services;
    using Legendary.AreaBuilder.Types;
    using Legendary.Core.Types;
    using MongoDB.Driver;

    /// <summary>
    /// Edits items.
    /// </summary>
    public partial class ItemEditor : Form
    {
        private readonly MongoService mongo;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemEditor"/> class.
        /// </summary>
        /// <param name="mongo">The mongo service.</param>
        public ItemEditor(MongoService mongo)
        {
            this.InitializeComponent();

            this.mongo = mongo;
        }

        private static string? GenerateDescription(Item item)
        {
            try
            {
                string prompt = $"Using a second person narrative voice, in a medieval fantasy setting, describe what I see when I'm looking at {item.Name}. It is of type {item.ItemType}. ";

                if (item.ItemType == ItemType.Container)
                {
                    if (item.IsClosed)
                    {
                        prompt += $"It is closed. ";
                    }

                    if (item.IsLocked)
                    {
                        prompt += $"It is locked and requires a key to open. ";
                    }
                }

                if (item.ItemType == ItemType.Weapon)
                {
                    prompt += $"It is a weapon of type {item.WeaponType}. ";
                }

                prompt += $"It can generally be described as {item.LongDescription}.";

                var chatGPT = new ChatGPTService();

                return chatGPT.Describe(prompt);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return null;
            }
        }

        // Save
        private void button1_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            if (this.propertyGrid1.SelectedObject is Item item)
            {
                if (this.listBox1.SelectedIndex > 0)
                {
                    this.mongo.Items.ReplaceOne(m => m.ItemId == item.ItemId, item);
                    this.toolStripStatusLabel1.Text = "Updated item.";
                }
                else
                {
                    this.mongo.Items.InsertOne(item);
                    this.toolStripStatusLabel1.Text = "Item created.";
                }
            }

            this.LoadListBox();

            this.Cursor = Cursors.Default;
        }

        private void ItemEditor_Load(object sender, EventArgs e)
        {
            this.LoadListBox();

            this.Cursor = Cursors.Default;
        }

        private void LoadListBox()
        {
            this.propertyGrid1.SelectedObject = new Item();

            this.listBox1.Items.Clear();
            this.listBox1.Items.Add("<New Item...>");

            var items = this.mongo.Items.Find(_ => true).ToList();

            foreach (var item in items)
            {
                this.listBox1.Items.Add(item);
            }

            this.listBox1.SelectedIndex = 0;
        }

        // Cancel
        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listBox1.SelectedIndex > 0)
            {
                this.propertyGrid1.SelectedObject = this.listBox1.SelectedItem;

                if (this.propertyGrid1.SelectedObject is Item item && !string.IsNullOrWhiteSpace(item.Image))
                {
                    this.pictureBox1.ImageLocation = item.Image;
                    this.pictureBox1.Update();
                }
            }
            else
            {
                this.propertyGrid1.SelectedObject = new Item() {  ItemId = this.listBox1.Items.Count + 1};
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (this.propertyGrid1.SelectedObject is Item item)
            {
                this.toolStripStatusLabel1.Text = $"Generating image for {item.Name} ({item.ItemId})...";

                string prompt = $"Provide a photorealistic painting of {item.Name}. It is of type {item.ItemType}. ";

                if (item.ItemType == ItemType.Container)
                {
                    if (item.IsClosed)
                    {
                        prompt += $"It is closed. ";
                    }

                    if (item.IsLocked)
                    {
                        prompt += $"It is locked and requires a key to open. ";
                    }
                }

                if (item.ItemType == ItemType.Weapon)
                {
                    prompt += $"It is a weapon of type {item.WeaponType}. ";
                }

                prompt += $"It can generally be described as {item.LongDescription}.";

                var image = new ChatGPTService().Image(prompt);

                if (!string.IsNullOrWhiteSpace(image))
                {
                    WebClient client = new ();
                    Stream stream = client.OpenRead(image);

                    Image img = Image.FromStream(stream);

                    MemoryStream mstream = new ();
                    img.Save(mstream, System.Drawing.Imaging.ImageFormat.Png);
                    mstream.Position = 0;

                    this.UploadItemImage(item, mstream);
                }
                else
                {
                    this.toolStripStatusLabel1.Text = $"Could not generate image for {item.Name}. Skipping.";
                }

                this.Cursor = Cursors.Default;

            }
        }

        private bool UploadItemImage(Item item, Stream stream)
        {
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=legendaryweb;AccountKey=SZ7oVHyhh/QghSiA+XL4xqwGxszmBcHgzOYQbYbdAS/3m2SqvXhdg7Tafgew9X/DDidE93Q9TuNq+AStP6A66Q==;EndpointSuffix=core.windows.net";

            // Name of the share, directory, and file we'll create
            string shareName = "images";
            string fileName = $"{item.ItemId}.png";
            ShareClient share = new (connectionString, shareName);
            ShareDirectoryClient directory = share.GetDirectoryClient("/items");

            try
            {
                // Get a reference to a file and upload it
                ShareFileClient file = directory.GetFileClient(fileName);

                string imageUrl = $"https://legendaryweb.file.core.windows.net/images/items/{item.ItemId}.png?sv=2021-12-02&ss=bfqt&srt=sco&sp=rwdlacupiytfx&se=2025-03-02T01:01:15Z&st=2023-03-10T17:01:15Z&spr=https&sig=nNpMARshWaVt834sDpwGXLp5%2BfAQtnrMcSQmWqf8o%2Fk%3D";

                var response = file.Create(stream.Length);

                file.UploadRange(
                    new HttpRange(0, stream.Length),
                    stream);

                item.Image = imageUrl;

                this.pictureBox1.ImageLocation = imageUrl;
                this.pictureBox1.Update();

                this.toolStripStatusLabel1.Text = $"Uploaded OpenAI image for item {item.Name}: {imageUrl}!";

                return true;
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.ToString());
            }

            return false;
        }

        /// <summary>
        /// Generate desc.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            this.toolStripStatusLabel1.Text = $"Generating description...";
            var item = this.propertyGrid1.SelectedObject as Item;
            item.LongDescription = GenerateDescription(item);
            this.toolStripStatusLabel1.Text = $"Description created.";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (this.listBox1.SelectedIndex != 0)
            {
                this.Cursor = Cursors.WaitCursor;

                var item = this.propertyGrid1.SelectedObject as Item;
                item.ItemId = this.listBox1.Items.Count + 1;
                item.Name = "CLONE of " + item.Name;
                item.Image = string.Empty;
                item.LongDescription = string.Empty;
                this.mongo.Items.InsertOne(item);
                this.toolStripStatusLabel1.Text = "Item cloned.";

                this.LoadListBox();

                this.Cursor = Cursors.Default;
            }
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
        }
    }
}
