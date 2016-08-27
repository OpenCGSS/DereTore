namespace DereTore.Application.ScoreEditor.Forms {
    partial class FEditNote {
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
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cboNoteStartPos = new System.Windows.Forms.ComboBox();
            this.cboNoteFinishPos = new System.Windows.Forms.ComboBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.txtNoteTiming = new System.Windows.Forms.TextBox();
            this.btnTimingAdd = new System.Windows.Forms.Button();
            this.btnTimingSubtract = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "Timing (s):";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 85);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(71, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "Finish Pos:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 59);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "Start Pos:";
            // 
            // cboNoteStartPos
            // 
            this.cboNoteStartPos.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboNoteStartPos.FormattingEnabled = true;
            this.cboNoteStartPos.Location = new System.Drawing.Point(89, 56);
            this.cboNoteStartPos.Name = "cboNoteStartPos";
            this.cboNoteStartPos.Size = new System.Drawing.Size(143, 20);
            this.cboNoteStartPos.TabIndex = 4;
            // 
            // cboNoteFinishPos
            // 
            this.cboNoteFinishPos.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboNoteFinishPos.FormattingEnabled = true;
            this.cboNoteFinishPos.Location = new System.Drawing.Point(89, 82);
            this.cboNoteFinishPos.Name = "cboNoteFinishPos";
            this.cboNoteFinishPos.Size = new System.Drawing.Size(143, 20);
            this.cboNoteFinishPos.TabIndex = 5;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(62, 129);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(82, 27);
            this.btnOK.TabIndex = 6;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(150, 129);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(82, 27);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // txtNoteTiming
            // 
            this.txtNoteTiming.Location = new System.Drawing.Point(89, 29);
            this.txtNoteTiming.Name = "txtNoteTiming";
            this.txtNoteTiming.Size = new System.Drawing.Size(94, 21);
            this.txtNoteTiming.TabIndex = 8;
            // 
            // btnTimingAdd
            // 
            this.btnTimingAdd.Location = new System.Drawing.Point(189, 28);
            this.btnTimingAdd.Name = "btnTimingAdd";
            this.btnTimingAdd.Size = new System.Drawing.Size(20, 20);
            this.btnTimingAdd.TabIndex = 9;
            this.btnTimingAdd.Text = "+";
            this.btnTimingAdd.UseVisualStyleBackColor = true;
            // 
            // btnTimingSubtract
            // 
            this.btnTimingSubtract.Location = new System.Drawing.Point(212, 28);
            this.btnTimingSubtract.Name = "btnTimingSubtract";
            this.btnTimingSubtract.Size = new System.Drawing.Size(20, 20);
            this.btnTimingSubtract.TabIndex = 10;
            this.btnTimingSubtract.Text = "-";
            this.btnTimingSubtract.UseVisualStyleBackColor = true;
            // 
            // FEditNote
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(250, 172);
            this.Controls.Add(this.btnTimingSubtract);
            this.Controls.Add(this.btnTimingAdd);
            this.Controls.Add(this.txtNoteTiming);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.cboNoteFinishPos);
            this.Controls.Add(this.cboNoteStartPos);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FEditNote";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Create Note";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cboNoteStartPos;
        private System.Windows.Forms.ComboBox cboNoteFinishPos;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TextBox txtNoteTiming;
        private System.Windows.Forms.Button btnTimingAdd;
        private System.Windows.Forms.Button btnTimingSubtract;
    }
}