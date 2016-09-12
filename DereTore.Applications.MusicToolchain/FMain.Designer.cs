namespace DereTore.Applications.MusicToolchain {
    partial class FMain {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent() {
            this.label1 = new System.Windows.Forms.Label();
            this.txtSourceWaveFile = new System.Windows.Forms.TextBox();
            this.btnBrowseSourceWaveFile = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txtKey1 = new System.Windows.Forms.TextBox();
            this.txtKey2 = new System.Windows.Forms.TextBox();
            this.txtSongName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.btnGo = new System.Windows.Forms.Button();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.label4 = new System.Windows.Forms.Label();
            this.txtSaveLocation = new System.Windows.Forms.TextBox();
            this.btnBrowseSaveLocation = new System.Windows.Forms.Button();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(27, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(107, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "Source Wave file:";
            // 
            // txtSourceWaveFile
            // 
            this.txtSourceWaveFile.Location = new System.Drawing.Point(146, 36);
            this.txtSourceWaveFile.Name = "txtSourceWaveFile";
            this.txtSourceWaveFile.Size = new System.Drawing.Size(272, 21);
            this.txtSourceWaveFile.TabIndex = 1;
            // 
            // btnBrowseSourceWaveFile
            // 
            this.btnBrowseSourceWaveFile.Location = new System.Drawing.Point(424, 34);
            this.btnBrowseSourceWaveFile.Name = "btnBrowseSourceWaveFile";
            this.btnBrowseSourceWaveFile.Size = new System.Drawing.Size(37, 23);
            this.btnBrowseSourceWaveFile.TabIndex = 2;
            this.btnBrowseSourceWaveFile.Text = "...";
            this.btnBrowseSourceWaveFile.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(27, 81);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(113, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "Keys (8-char hex):";
            // 
            // txtKey1
            // 
            this.txtKey1.Location = new System.Drawing.Point(146, 78);
            this.txtKey1.Name = "txtKey1";
            this.txtKey1.Size = new System.Drawing.Size(133, 21);
            this.txtKey1.TabIndex = 4;
            // 
            // txtKey2
            // 
            this.txtKey2.Location = new System.Drawing.Point(285, 78);
            this.txtKey2.Name = "txtKey2";
            this.txtKey2.Size = new System.Drawing.Size(133, 21);
            this.txtKey2.TabIndex = 5;
            // 
            // txtSongName
            // 
            this.txtSongName.Location = new System.Drawing.Point(146, 123);
            this.txtSongName.Name = "txtSongName";
            this.txtSongName.Size = new System.Drawing.Size(272, 21);
            this.txtSongName.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(27, 126);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 7;
            this.label3.Text = "Song name:";
            // 
            // txtLog
            // 
            this.txtLog.Location = new System.Drawing.Point(29, 247);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(432, 110);
            this.txtLog.TabIndex = 8;
            // 
            // btnGo
            // 
            this.btnGo.Location = new System.Drawing.Point(383, 208);
            this.btnGo.Name = "btnGo";
            this.btnGo.Size = new System.Drawing.Size(78, 33);
            this.btnGo.TabIndex = 9;
            this.btnGo.Text = "&Go";
            this.btnGo.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(27, 170);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(89, 12);
            this.label4.TabIndex = 11;
            this.label4.Text = "Save location:";
            // 
            // txtSaveLocation
            // 
            this.txtSaveLocation.Location = new System.Drawing.Point(146, 167);
            this.txtSaveLocation.Name = "txtSaveLocation";
            this.txtSaveLocation.Size = new System.Drawing.Size(272, 21);
            this.txtSaveLocation.TabIndex = 10;
            // 
            // btnBrowseSaveLocation
            // 
            this.btnBrowseSaveLocation.Location = new System.Drawing.Point(424, 165);
            this.btnBrowseSaveLocation.Name = "btnBrowseSaveLocation";
            this.btnBrowseSaveLocation.Size = new System.Drawing.Size(37, 23);
            this.btnBrowseSaveLocation.TabIndex = 12;
            this.btnBrowseSaveLocation.Text = "...";
            this.btnBrowseSaveLocation.UseVisualStyleBackColor = true;
            // 
            // FMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(498, 392);
            this.Controls.Add(this.btnBrowseSaveLocation);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtSaveLocation);
            this.Controls.Add(this.btnGo);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtSongName);
            this.Controls.Add(this.txtKey2);
            this.Controls.Add(this.txtKey1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnBrowseSourceWaveFile);
            this.Controls.Add(this.txtSourceWaveFile);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "DereTore Toolchain";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtSourceWaveFile;
        private System.Windows.Forms.Button btnBrowseSourceWaveFile;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtKey1;
        private System.Windows.Forms.TextBox txtKey2;
        private System.Windows.Forms.TextBox txtSongName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Button btnGo;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtSaveLocation;
        private System.Windows.Forms.Button btnBrowseSaveLocation;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
    }
}

