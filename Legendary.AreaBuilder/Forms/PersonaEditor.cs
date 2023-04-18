// <copyright file="PersonaEditor.cs" company="Legendary™">
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
    using Legendary.Core.Models;
    using MongoDB.Driver;

    /// <summary>
    /// Creates or edits a persona.
    /// </summary>
    public partial class PersonaEditor : Form
    {
        private readonly MongoService mongo;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonaEditor"/> class.
        /// </summary>
        /// <param name="mongo">The mongo service.</param>
        public PersonaEditor(MongoService mongo)
        {
            this.mongo = mongo;
            this.InitializeComponent();
        }

        private void PersonaEditor_Load(object sender, EventArgs e)
        {
            var personas = this.mongo.Personas.Find(_ => true).ToList();

            this.lstPersonas.Items.Clear();

            this.lstPersonas.DisplayMember = "Name";

            foreach (var persona in personas)
            {
                this.lstPersonas.Items.Add(persona);
            }

            this.lstPersonas.Items.Insert(0, new Persona() { Name = "<New Persona>" });

            this.lstPersonas.SelectedIndex = 0;

            this.pgPersona.SelectedObject = this.lstPersonas.SelectedItem;

            this.Cursor = Cursors.Default;
        }

        private void BtnSavePersona_Click(object sender, EventArgs e)
        {
            var persona = this.pgPersona.SelectedObject as Persona;

            if (persona != null)
            {
                if (this.lstPersonas.SelectedIndex == 0)
                {
                    // New persona
                    persona.Id = this.lstPersonas.Items.Count + 10;
                    this.mongo.Personas.InsertOne(persona);
                }
                else
                {
                    // Update persona
                    this.mongo.Personas.ReplaceOne(u => u.Id == persona.Id, persona);
                }

                this.DialogResult = DialogResult.OK;
            }
        }

        private void LstPersonas_SelectedValueChanged(object sender, EventArgs e)
        {
            this.pgPersona.SelectedObject = this.lstPersonas.SelectedItem;
        }
    }
}
