// <copyright file="MobEditor.cs" company="Legendary™">
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
    using Legendary.AreaBuilder.Extensions;
    using Legendary.AreaBuilder.Services;
    using Legendary.AreaBuilder.Types;
    using MongoDB.Driver;

    /// <summary>
    /// Edits a mobile.
    /// </summary>
    public partial class MobEditor : Form
    {
        private readonly MongoService mongo;

        /// <summary>
        /// Initializes a new instance of the <see cref="MobEditor"/> class.
        /// </summary>
        /// <param name="mongo">The mongo service.</param>
        public MobEditor(MongoService mongo)
        {
            this.InitializeComponent();

            this.mongo = mongo;
        }

        private static string? GenerateDescription(Mobile mobile)
        {
            try
            {
                string prompt = $"Using a second person narrative voice, in a medieval fantasy setting, describe what I see when I'm looking at {mobile.FirstName}. ";

                prompt += $"{mobile.FirstName} is a {mobile.Race} {mobile.Gender} who is around {mobile.Age} years old. ";

                prompt += $"{mobile.FirstName} can generally be described as {mobile.LongDescription}. ";

                if (!string.IsNullOrWhiteSpace(mobile.LastName))
                {
                    prompt += $"{mobile.FirstName}'s last name is {mobile.LastName}. ";
                }

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
        private void Button1_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            if (this.propertyGrid1.SelectedObject is Mobile mobile)
            {
                if (this.listBox1.SelectedIndex > 0)
                {
                    this.mongo.Mobiles.ReplaceOne(m => m.CharacterId == mobile.CharacterId, mobile);
                    this.toolStripStatusLabel1.Text = "Updated mobile.";
                }
                else
                {
                    this.mongo.Mobiles.InsertOne(mobile);
                    this.toolStripStatusLabel1.Text = "Mobile created.";
                }
            }

            this.LoadListBox();

            this.Cursor = Cursors.Default;
        }

        private void MobEditor_Load(object sender, EventArgs e)
        {
            this.LoadListBox();

            this.Cursor = Cursors.Default;
        }

        private void LoadListBox()
        {
            this.propertyGrid1.SelectedObject = new Mobile();

            this.listBox1.Items.Clear();
            this.listBox1.Items.Add("<New Mobile...>");

            var mobs = this.mongo.Mobiles.Find(_ => true).ToList();

            foreach (var mob in mobs)
            {
                this.listBox1.Items.Add(mob);
            }

            this.listBox1.SelectedIndex = 0;

            var items = this.mongo.Items.Find(_ => true).ToList();

            this.lstItems.DataSource = items;
            this.lstItems.DisplayMember = "Name";
        }

        // Cancel
        private void Button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listBox1.SelectedIndex > 0)
            {
                this.propertyGrid1.SelectedObject = this.listBox1.SelectedItem;

                if (this.propertyGrid1.SelectedObject is Mobile mobile && mobile.Images != null && mobile.Images.Count > 0)
                {
                    this.pictureBox1.ImageLocation = mobile.Images[0];
                    this.pictureBox1.Update();
                }
            }
            else
            {
                this.propertyGrid1.SelectedObject = new Mobile() { CharacterId = this.listBox1.Items.Count + 1 };
            }
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            if (this.propertyGrid1.SelectedObject is Mobile mobile)
            {
                this.toolStripStatusLabel1.Text = $"Generating image for {mobile.FirstName} ({mobile.CharacterId})...";

                string prompt = $"Provide a photorealistic painting in a medieval fantasy setting of what I see when I'm looking at {mobile.FirstName}, ";

                prompt += $"a {mobile.Race} {mobile.Gender} who is around {mobile.Age} years old, ";

                prompt += $"who can generally be described as: {mobile.LongDescription}. ";

                var image = new ChatGPTService().Image(prompt);

                if (!string.IsNullOrWhiteSpace(image))
                {
                    HttpClient client = new();
                    Stream stream = client.GetStreamAsync(image).Result;

                    Image img = Image.FromStream(stream);

                    MemoryStream mstream = new();
                    img.Save(mstream, System.Drawing.Imaging.ImageFormat.Png);
                    mstream.Position = 0;

                    this.UploadMobileImage(mobile, mstream);
                }
                else
                {
                    this.toolStripStatusLabel1.Text = $"Could not generate image for {mobile.FirstName}. Skipping.";
                }

                this.Cursor = Cursors.Default;
            }
        }

        private bool UploadMobileImage(Mobile mobile, Stream stream, string extension = ".png", bool isXImage = false)
        {
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=legendaryweb;AccountKey=SZ7oVHyhh/QghSiA+XL4xqwGxszmBcHgzOYQbYbdAS/3m2SqvXhdg7Tafgew9X/DDidE93Q9TuNq+AStP6A66Q==;EndpointSuffix=core.windows.net";

            // Name of the share, directory, and file we'll create
            string shareName = "images";
            string fileName = $"{mobile.CharacterId}_{Guid.NewGuid()}{extension}";
            ShareClient share = new(connectionString, shareName);
            ShareDirectoryClient directory = share.GetDirectoryClient("/mobiles");

            try
            {
                string imageUrl = $"https://legendaryweb.file.core.windows.net/images/mobiles/{fileName}?sv=2021-12-02&ss=bfqt&srt=sco&sp=rwdlacupiytfx&se=2025-03-02T01:01:15Z&st=2023-03-10T17:01:15Z&spr=https&sig=nNpMARshWaVt834sDpwGXLp5%2BfAQtnrMcSQmWqf8o%2Fk%3D";

                if (isXImage)
                {
                    shareName = "mob-x";
                    share = new(connectionString, shareName);
                    directory = share.GetDirectoryClient("/");
                    imageUrl = $"https://legendaryweb.file.core.windows.net/mob-x/{fileName}?sv=2021-12-02&ss=bfqt&srt=sco&sp=rwdlacupiytfx&se=2025-03-02T01:01:15Z&st=2023-03-10T17:01:15Z&spr=https&sig=nNpMARshWaVt834sDpwGXLp5%2BfAQtnrMcSQmWqf8o%2Fk%3D";
                }

                // Get a reference to a file and upload it
                ShareFileClient file = directory.GetFileClient(fileName);

                var response = file.Create(stream.Length);

                file.UploadRange(
                    new HttpRange(0, stream.Length),
                    stream);

                if (isXImage)
                {
                    mobile.XImages ??= new List<string>();

                    mobile.XImages.Add(imageUrl);
                }
                else
                {
                    mobile.Images ??= new List<string>();

                    mobile.Images.Add(imageUrl);
                }

                this.pictureBox1.ImageLocation = imageUrl;
                this.pictureBox1.Update();

                this.toolStripStatusLabel1.Text = $"Uploaded image for {mobile.FirstName}: {imageUrl}!";

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
        private void Button4_Click(object sender, EventArgs e)
        {
            this.toolStripStatusLabel1.Text = $"Generating description...";
            if (this.propertyGrid1.SelectedObject is Mobile mobile)
            {
                mobile.LongDescription = GenerateDescription(mobile);
                this.toolStripStatusLabel1.Text = $"Description created.";
            }
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            if (this.listBox1.SelectedIndex != 0)
            {
                this.Cursor = Cursors.WaitCursor;

                if (this.propertyGrid1.SelectedObject is Mobile mobile)
                {
                    mobile.CharacterId = this.listBox1.Items.Count + 1;
                    mobile.FirstName = "CLONE of " + mobile.FirstName;
                    mobile.Images = new List<string>();
                    mobile.LongDescription = string.Empty;
                    this.mongo.Mobiles.InsertOne(mobile);
                    this.toolStripStatusLabel1.Text = "Mobile cloned.";

                    this.LoadListBox();

                    this.Cursor = Cursors.Default;
                }
            }
        }

        private void BtnUploadImage_Click(object sender, EventArgs e)
        {
            var openDlg = new OpenFileDialog()
            {
                Title = "Add Image File",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;",
            };

            if (openDlg.ShowDialog() == DialogResult.OK)
            {
                if (this.propertyGrid1.SelectedObject is Mobile mobile)
                {
                    var stream = File.OpenRead(openDlg.FileName);
                    this.UploadMobileImage(mobile, stream, Path.GetExtension(openDlg.FileName));
                }
            }
        }

        private void BtnAddReset_Click(object sender, EventArgs e)
        {
            if (this.lstItems.SelectedItem != null && this.lstItems.SelectedItem is Item item && this.propertyGrid1.SelectedObject is Mobile mobile)
            {
                mobile.Inventory.Add(item.ToDomainModel());
                this.toolStripStatusLabel1.Text = "Item added to inventory.";
            }
        }

        private void BtnWield_Click(object sender, EventArgs e)
        {
            if (this.lstItems.SelectedItem != null && this.lstItems.SelectedItem is Item item && this.propertyGrid1.SelectedObject is Mobile mobile)
            {
                if (!item.WearLocation.Contains(Core.Types.WearLocation.InventoryOnly) && !item.WearLocation.Contains(Core.Types.WearLocation.None))
                {
                    var wearLoc = item.WearLocation.FirstOrDefault();
                    mobile.EquipmentResets.Add(new Core.Types.EquipmentReset() { ItemId = item.ItemId, WearLocation = wearLoc });
                    this.toolStripStatusLabel1.Text = "Item equipped.";
                }
                else
                {
                    this.toolStripStatusLabel1.Text = "Item could not be equipped. No valid wear location.";
                }
            }
        }

        private void UploadXImage_Click(object sender, EventArgs e)
        {
            var openDlg = new OpenFileDialog()
            {
                Title = "Add Image File",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;",
            };

            if (openDlg.ShowDialog() == DialogResult.OK)
            {
                if (this.propertyGrid1.SelectedObject is Mobile mobile)
                {
                    var stream = File.OpenRead(openDlg.FileName);
                    this.UploadMobileImage(mobile, stream, Path.GetExtension(openDlg.FileName), true);
                }
            }
        }
    }
}
