namespace DereTore.Application.ScoreEditor.Forms {
    partial class FMakeSync {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.label1 = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.txtCurrentNote = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cboNotes = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.radUseFirst = new System.Windows.Forms.RadioButton();
            this.radUseSecond = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "Current note:";
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(206, 169);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(82, 27);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(294, 169);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(82, 27);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // txtCurrentNote
            // 
            this.txtCurrentNote.Location = new System.Drawing.Point(101, 19);
            this.txtCurrentNote.Name = "txtCurrentNote";
            this.txtCurrentNote.ReadOnly = true;
            this.txtCurrentNote.Size = new System.Drawing.Size(265, 21);
            this.txtCurrentNote.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 5;
            this.label2.Text = "Sync pair:";
            // 
            // cboNotes
            // 
            this.cboNotes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboNotes.FormattingEnabled = true;
            this.cboNotes.Location = new System.Drawing.Point(101, 46);
            this.cboNotes.Name = "cboNotes";
            this.cboNotes.Size = new System.Drawing.Size(265, 20);
            this.cboNotes.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 84);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(137, 12);
            this.label3.TabIndex = 7;
            this.label3.Text = "When timings mismatch:";
            // 
            // radUseFirst
            // 
            this.radUseFirst.AutoSize = true;
            this.radUseFirst.Location = new System.Drawing.Point(115, 106);
            this.radUseFirst.Name = "radUseFirst";
            this.radUseFirst.Size = new System.Drawing.Size(173, 16);
            this.radUseFirst.TabIndex = 8;
            this.radUseFirst.TabStop = true;
            this.radUseFirst.Text = "Use current note as basis";
            this.radUseFirst.UseVisualStyleBackColor = true;
            // 
            // radUseSecond
            // 
            this.radUseSecond.AutoSize = true;
            this.radUseSecond.Location = new System.Drawing.Point(115, 128);
            this.radUseSecond.Name = "radUseSecond";
            this.radUseSecond.Size = new System.Drawing.Size(155, 16);
            this.radUseSecond.TabIndex = 9;
            this.radUseSecond.TabStop = true;
            this.radUseSecond.Text = "Use sync pair as basis";
            this.radUseSecond.UseVisualStyleBackColor = true;
            // 
            // FMakeSync
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(388, 208);
            this.Controls.Add(this.radUseSecond);
            this.Controls.Add(this.radUseFirst);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cboNotes);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtCurrentNote);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FMakeSync";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Make Sync";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TextBox txtCurrentNote;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cboNotes;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.RadioButton radUseFirst;
        private System.Windows.Forms.RadioButton radUseSecond;
    }
}