﻿// <copyright file="MobEditor.cs" company="Legendary™">
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

        // Save
        private void button1_Click(object sender, EventArgs e)
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

                if (this.propertyGrid1.SelectedObject is Mobile mobile && !string.IsNullOrWhiteSpace(mobile.Image))
                {
                    this.pictureBox1.ImageLocation = mobile.Image;
                    this.pictureBox1.Update();
                }
            }
            else
            {
                this.propertyGrid1.SelectedObject = new Mobile() { CharacterId = this.listBox1.Items.Count + 1 };
            }
        }

        private void button3_Click(object sender, EventArgs e)
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
                    WebClient client = new ();
                    Stream stream = client.OpenRead(image);

                    Image img = Image.FromStream(stream);

                    MemoryStream mstream = new ();
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

        private bool UploadMobileImage(Mobile mobile, Stream stream)
        {
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=legendaryweb;AccountKey=SZ7oVHyhh/QghSiA+XL4xqwGxszmBcHgzOYQbYbdAS/3m2SqvXhdg7Tafgew9X/DDidE93Q9TuNq+AStP6A66Q==;EndpointSuffix=core.windows.net";

            // Name of the share, directory, and file we'll create
            string shareName = "images";
            string fileName = $"{mobile.CharacterId}.png";
            ShareClient share = new (connectionString, shareName);
            ShareDirectoryClient directory = share.GetDirectoryClient("/mobiles");

            try
            {
                // Get a reference to a file and upload it
                ShareFileClient file = directory.GetFileClient(fileName);

                string imageUrl = $"https://legendaryweb.file.core.windows.net/images/mobiles/{mobile.CharacterId}.png?sv=2021-12-02&ss=bfqt&srt=sco&sp=rwdlacupiytfx&se=2025-03-02T01:01:15Z&st=2023-03-10T17:01:15Z&spr=https&sig=nNpMARshWaVt834sDpwGXLp5%2BfAQtnrMcSQmWqf8o%2Fk%3D";

                var response = file.Create(stream.Length);

                file.UploadRange(
                    new HttpRange(0, stream.Length),
                    stream);

                mobile.Image = imageUrl;

                this.pictureBox1.ImageLocation = imageUrl;
                this.pictureBox1.Update();

                this.toolStripStatusLabel1.Text = $"Uploaded OpenAI image for {mobile.FirstName}: {imageUrl}!";

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
            if (this.propertyGrid1.SelectedObject is Mobile mobile)
            {
                mobile.LongDescription = GenerateDescription(mobile);
                this.toolStripStatusLabel1.Text = $"Description created.";
            }
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

        private void button5_Click(object sender, EventArgs e)
        {
            if (this.listBox1.SelectedIndex != 0)
            {
                this.Cursor = Cursors.WaitCursor;

                if (this.propertyGrid1.SelectedObject is Mobile mobile)
                {
                    mobile.CharacterId = this.listBox1.Items.Count + 1;
                    mobile.FirstName = "CLONE of " + mobile.FirstName;
                    mobile.Image = string.Empty;
                    mobile.LongDescription = string.Empty;
                    this.mongo.Mobiles.InsertOne(mobile);
                    this.toolStripStatusLabel1.Text = "Mobile cloned.";

                    this.LoadListBox();

                    this.Cursor = Cursors.Default;
                }
            }
        }
    }
}
