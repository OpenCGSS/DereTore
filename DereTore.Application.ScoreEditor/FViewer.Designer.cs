namespace DereTore.Application.ScoreEditor {
    partial class FViewer {
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
            this.btnSelectAcb = new System.Windows.Forms.Button();
            this.btnSelectScore = new System.Windows.Forms.Button();
            this.txtAcbFileName = new System.Windows.Forms.TextBox();
            this.txtScoreFileName = new System.Windows.Forms.TextBox();
            this.btnPlay = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.lblSong = new System.Windows.Forms.Label();
            this.cboDifficulty = new System.Windows.Forms.ComboBox();
            this.lblTime = new System.Windows.Forms.Label();
            this.pictureBox1 = new DereTore.Application.ScoreEditor.Controls.ScoreEditorControl();
            this.progress = new System.Windows.Forms.ProgressBar();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnSelectAcb
            // 
            this.btnSelectAcb.Location = new System.Drawing.Point(12, 12);
            this.btnSelectAcb.Name = "btnSelectAcb";
            this.btnSelectAcb.Size = new System.Drawing.Size(107, 35);
            this.btnSelectAcb.TabIndex = 1;
            this.btnSelectAcb.Text = "Select A&CB...";
            this.btnSelectAcb.UseVisualStyleBackColor = true;
            // 
            // btnSelectScore
            // 
            this.btnSelectScore.Location = new System.Drawing.Point(12, 63);
            this.btnSelectScore.Name = "btnSelectScore";
            this.btnSelectScore.Size = new System.Drawing.Size(107, 35);
            this.btnSelectScore.TabIndex = 2;
            this.btnSelectScore.Text = "Select sco&re...";
            this.btnSelectScore.UseVisualStyleBackColor = true;
            // 
            // txtAcbFileName
            // 
            this.txtAcbFileName.Location = new System.Drawing.Point(125, 20);
            this.txtAcbFileName.Name = "txtAcbFileName";
            this.txtAcbFileName.ReadOnly = true;
            this.txtAcbFileName.Size = new System.Drawing.Size(244, 21);
            this.txtAcbFileName.TabIndex = 3;
            // 
            // txtScoreFileName
            // 
            this.txtScoreFileName.Location = new System.Drawing.Point(125, 71);
            this.txtScoreFileName.Name = "txtScoreFileName";
            this.txtScoreFileName.ReadOnly = true;
            this.txtScoreFileName.Size = new System.Drawing.Size(244, 21);
            this.txtScoreFileName.TabIndex = 4;
            // 
            // btnPlay
            // 
            this.btnPlay.Location = new System.Drawing.Point(810, 33);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(59, 35);
            this.btnPlay.TabIndex = 5;
            this.btnPlay.Text = "&Play";
            this.btnPlay.UseVisualStyleBackColor = true;
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(875, 33);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(59, 35);
            this.btnStop.TabIndex = 6;
            this.btnStop.Text = "&Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            // 
            // lblSong
            // 
            this.lblSong.AutoSize = true;
            this.lblSong.Location = new System.Drawing.Point(387, 56);
            this.lblSong.Name = "lblSong";
            this.lblSong.Size = new System.Drawing.Size(35, 12);
            this.lblSong.TabIndex = 7;
            this.lblSong.Text = "Song:";
            // 
            // cboDifficulty
            // 
            this.cboDifficulty.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDifficulty.FormattingEnabled = true;
            this.cboDifficulty.Items.AddRange(new object[] {
            "Debut",
            "Regular",
            "Pro",
            "Master",
            "Master+"});
            this.cboDifficulty.Location = new System.Drawing.Point(389, 20);
            this.cboDifficulty.Name = "cboDifficulty";
            this.cboDifficulty.Size = new System.Drawing.Size(126, 20);
            this.cboDifficulty.TabIndex = 9;
            // 
            // lblTime
            // 
            this.lblTime.AutoSize = true;
            this.lblTime.Location = new System.Drawing.Point(387, 95);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(35, 12);
            this.lblTime.TabIndex = 10;
            this.lblTime.Text = "Time:";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.EnableMouse = false;
            this.pictureBox1.Location = new System.Drawing.Point(12, 118);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(922, 460);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // progress
            // 
            this.progress.Location = new System.Drawing.Point(389, 71);
            this.progress.Maximum = 65536;
            this.progress.Name = "progress";
            this.progress.Size = new System.Drawing.Size(413, 21);
            this.progress.TabIndex = 11;
            // 
            // FViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(946, 590);
            this.Controls.Add(this.progress);
            this.Controls.Add(this.lblTime);
            this.Controls.Add(this.cboDifficulty);
            this.Controls.Add(this.lblSong);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnPlay);
            this.Controls.Add(this.txtScoreFileName);
            this.Controls.Add(this.txtAcbFileName);
            this.Controls.Add(this.btnSelectScore);
            this.Controls.Add(this.btnSelectAcb);
            this.Controls.Add(this.pictureBox1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FViewer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "DereTore: Score Editor";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.ScoreEditorControl pictureBox1;
        private System.Windows.Forms.Button btnSelectAcb;
        private System.Windows.Forms.Button btnSelectScore;
        private System.Windows.Forms.TextBox txtAcbFileName;
        private System.Windows.Forms.TextBox txtScoreFileName;
        private System.Windows.Forms.Button btnPlay;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.Label lblSong;
        private System.Windows.Forms.ComboBox cboDifficulty;
        private System.Windows.Forms.Label lblTime;
        private System.Windows.Forms.ProgressBar progress;
    }
}

