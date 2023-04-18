namespace Legendary.AreaBuilder.Forms
{
    partial class PersonaEditor
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
            lstPersonas = new ListBox();
            pgPersona = new PropertyGrid();
            btnSavePersona = new Button();
            SuspendLayout();
            // 
            // lstPersonas
            // 
            lstPersonas.FormattingEnabled = true;
            lstPersonas.ItemHeight = 15;
            lstPersonas.Location = new Point(7, 11);
            lstPersonas.Name = "lstPersonas";
            lstPersonas.Size = new Size(220, 424);
            lstPersonas.TabIndex = 0;
            lstPersonas.SelectedValueChanged += LstPersonas_SelectedValueChanged;
            // 
            // pgPersona
            // 
            pgPersona.Location = new Point(233, 12);
            pgPersona.Name = "pgPersona";
            pgPersona.Size = new Size(555, 394);
            pgPersona.TabIndex = 1;
            // 
            // btnSavePersona
            // 
            btnSavePersona.Location = new Point(713, 412);
            btnSavePersona.Name = "btnSavePersona";
            btnSavePersona.Size = new Size(75, 23);
            btnSavePersona.TabIndex = 2;
            btnSavePersona.Text = "Save";
            btnSavePersona.UseVisualStyleBackColor = true;
            btnSavePersona.Click += BtnSavePersona_Click;
            // 
            // PersonaEditor
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnSavePersona);
            Controls.Add(pgPersona);
            Controls.Add(lstPersonas);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Name = "PersonaEditor";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Edit or Create Persona";
            Load += PersonaEditor_Load;
            ResumeLayout(false);
        }

        #endregion

        private ListBox lstPersonas;
        private PropertyGrid pgPersona;
        private Button btnSavePersona;
    }
}