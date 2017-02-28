using DereTore.Applications.ScoreViewer.Controls;

namespace DereTore.Applications.ScoreViewer.Forms {
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
            this.components = new System.ComponentModel.Container();
            this.btnSelectAudio = new System.Windows.Forms.Button();
            this.btnSelectScore = new System.Windows.Forms.Button();
            this.txtAudioFileName = new System.Windows.Forms.TextBox();
            this.txtScoreFileName = new System.Windows.Forms.TextBox();
            this.btnPlay = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.lblSong = new System.Windows.Forms.Label();
            this.cboDifficulty = new System.Windows.Forms.ComboBox();
            this.lblTime = new System.Windows.Forms.Label();
            this.trkProgress = new System.Windows.Forms.TrackBar();
            this.btnPause = new System.Windows.Forms.Button();
            this.btnScoreLoad = new System.Windows.Forms.Button();
            this.btnScoreUnload = new System.Windows.Forms.Button();
            this.cboSoundEffect = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.trkFallingSpeed = new System.Windows.Forms.TrackBar();
            this.label2 = new System.Windows.Forms.Label();
            this.chkSfxOn = new System.Windows.Forms.CheckBox();
            this.trkMusicVolume = new System.Windows.Forms.TrackBar();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.trkSfxVolume = new System.Windows.Forms.TrackBar();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.editor = new DereTore.Applications.ScoreViewer.Controls.ScoreEditorControl();
            this.chkLimitFrameRate = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.trkProgress)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkFallingSpeed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkMusicVolume)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkSfxVolume)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.editor)).BeginInit();
            this.SuspendLayout();
            // 
            // btnSelectAudio
            // 
            this.btnSelectAudio.Location = new System.Drawing.Point(12, 12);
            this.btnSelectAudio.Name = "btnSelectAudio";
            this.btnSelectAudio.Size = new System.Drawing.Size(107, 35);
            this.btnSelectAudio.TabIndex = 1;
            this.btnSelectAudio.Text = "Select &Audio...";
            this.btnSelectAudio.UseVisualStyleBackColor = true;
            // 
            // btnSelectScore
            // 
            this.btnSelectScore.Location = new System.Drawing.Point(12, 63);
            this.btnSelectScore.Name = "btnSelectScore";
            this.btnSelectScore.Size = new System.Drawing.Size(107, 35);
            this.btnSelectScore.TabIndex = 2;
            this.btnSelectScore.Text = "Select Sco&re...";
            this.btnSelectScore.UseVisualStyleBackColor = true;
            // 
            // txtAudioFileName
            // 
            this.txtAudioFileName.Location = new System.Drawing.Point(125, 20);
            this.txtAudioFileName.Name = "txtAudioFileName";
            this.txtAudioFileName.ReadOnly = true;
            this.txtAudioFileName.Size = new System.Drawing.Size(149, 21);
            this.txtAudioFileName.TabIndex = 3;
            // 
            // txtScoreFileName
            // 
            this.txtScoreFileName.Location = new System.Drawing.Point(125, 71);
            this.txtScoreFileName.Name = "txtScoreFileName";
            this.txtScoreFileName.ReadOnly = true;
            this.txtScoreFileName.Size = new System.Drawing.Size(149, 21);
            this.txtScoreFileName.TabIndex = 4;
            // 
            // btnPlay
            // 
            this.btnPlay.Location = new System.Drawing.Point(700, 63);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(59, 35);
            this.btnPlay.TabIndex = 5;
            this.btnPlay.Text = "&Play";
            this.btnPlay.UseVisualStyleBackColor = true;
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(830, 63);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(59, 35);
            this.btnStop.TabIndex = 6;
            this.btnStop.Text = "&Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            // 
            // lblSong
            // 
            this.lblSong.AutoSize = true;
            this.lblSong.Location = new System.Drawing.Point(382, 23);
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
            this.cboDifficulty.Location = new System.Drawing.Point(280, 20);
            this.cboDifficulty.Name = "cboDifficulty";
            this.cboDifficulty.Size = new System.Drawing.Size(96, 20);
            this.cboDifficulty.TabIndex = 9;
            // 
            // lblTime
            // 
            this.lblTime.AutoSize = true;
            this.lblTime.Location = new System.Drawing.Point(278, 86);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(35, 12);
            this.lblTime.TabIndex = 10;
            this.lblTime.Text = "Time:";
            // 
            // trkProgress
            // 
            this.trkProgress.LargeChange = 30;
            this.trkProgress.Location = new System.Drawing.Point(280, 47);
            this.trkProgress.Maximum = 65536;
            this.trkProgress.Name = "trkProgress";
            this.trkProgress.Size = new System.Drawing.Size(415, 45);
            this.trkProgress.TabIndex = 12;
            this.trkProgress.TickStyle = System.Windows.Forms.TickStyle.None;
            // 
            // btnPause
            // 
            this.btnPause.Location = new System.Drawing.Point(765, 63);
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(59, 35);
            this.btnPause.TabIndex = 13;
            this.btnPause.Text = "Pa&use";
            this.btnPause.UseVisualStyleBackColor = true;
            // 
            // btnScoreLoad
            // 
            this.btnScoreLoad.Location = new System.Drawing.Point(700, 12);
            this.btnScoreLoad.Name = "btnScoreLoad";
            this.btnScoreLoad.Size = new System.Drawing.Size(59, 35);
            this.btnScoreLoad.TabIndex = 24;
            this.btnScoreLoad.Text = "&Load";
            this.btnScoreLoad.UseVisualStyleBackColor = true;
            // 
            // btnScoreUnload
            // 
            this.btnScoreUnload.Location = new System.Drawing.Point(765, 12);
            this.btnScoreUnload.Name = "btnScoreUnload";
            this.btnScoreUnload.Size = new System.Drawing.Size(59, 35);
            this.btnScoreUnload.TabIndex = 25;
            this.btnScoreUnload.Text = "U&nload";
            this.btnScoreUnload.UseVisualStyleBackColor = true;
            // 
            // cboSoundEffect
            // 
            this.cboSoundEffect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSoundEffect.FormattingEnabled = true;
            this.cboSoundEffect.Items.AddRange(new object[] {
            "SE 1",
            "SE 2",
            "SE 3",
            "SE 4"});
            this.cboSoundEffect.Location = new System.Drawing.Point(807, 135);
            this.cboSoundEffect.Name = "cboSoundEffect";
            this.cboSoundEffect.Size = new System.Drawing.Size(82, 20);
            this.cboSoundEffect.TabIndex = 26;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(772, 138);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 27;
            this.label1.Text = "SFX:";
            // 
            // trkFallingSpeed
            // 
            this.trkFallingSpeed.Location = new System.Drawing.Point(843, 234);
            this.trkFallingSpeed.Maximum = 40;
            this.trkFallingSpeed.Minimum = 2;
            this.trkFallingSpeed.Name = "trkFallingSpeed";
            this.trkFallingSpeed.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.trkFallingSpeed.Size = new System.Drawing.Size(45, 288);
            this.trkFallingSpeed.TabIndex = 28;
            this.trkFallingSpeed.Value = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(772, 363);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 24);
            this.label2.TabIndex = 29;
            this.label2.Text = "Falling\r\nspeed:";
            // 
            // chkSfxOn
            // 
            this.chkSfxOn.AutoSize = true;
            this.chkSfxOn.Checked = true;
            this.chkSfxOn.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSfxOn.Location = new System.Drawing.Point(807, 161);
            this.chkSfxOn.Name = "chkSfxOn";
            this.chkSfxOn.Size = new System.Drawing.Size(60, 16);
            this.chkSfxOn.TabIndex = 30;
            this.chkSfxOn.Text = "SFX On";
            this.chkSfxOn.UseVisualStyleBackColor = true;
            // 
            // trkMusicVolume
            // 
            this.trkMusicVolume.LargeChange = 50;
            this.trkMusicVolume.Location = new System.Drawing.Point(807, 183);
            this.trkMusicVolume.Maximum = 1000;
            this.trkMusicVolume.Name = "trkMusicVolume";
            this.trkMusicVolume.Size = new System.Drawing.Size(82, 45);
            this.trkMusicVolume.TabIndex = 31;
            this.trkMusicVolume.TickStyle = System.Windows.Forms.TickStyle.None;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(772, 183);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 24);
            this.label3.TabIndex = 32;
            this.label3.Text = "Music\r\nvol.";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(772, 210);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 24);
            this.label4.TabIndex = 34;
            this.label4.Text = "SFX\r\nvol.";
            // 
            // trkSfxVolume
            // 
            this.trkSfxVolume.LargeChange = 50;
            this.trkSfxVolume.Location = new System.Drawing.Point(807, 210);
            this.trkSfxVolume.Maximum = 1000;
            this.trkSfxVolume.Name = "trkSfxVolume";
            this.trkSfxVolume.Size = new System.Drawing.Size(82, 45);
            this.trkSfxVolume.TabIndex = 33;
            this.trkSfxVolume.TickStyle = System.Windows.Forms.TickStyle.None;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1000;
            // 
            // editor
            // 
            this.editor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.editor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.editor.IsPreview = false;
            this.editor.Location = new System.Drawing.Point(12, 113);
            this.editor.MouseEventsEnabled = true;
            this.editor.Name = "editor";
            this.editor.Score = null;
            this.editor.Size = new System.Drawing.Size(747, 409);
            this.editor.TabIndex = 0;
            this.editor.TabStop = false;
            // 
            // chkLimitFrameRate
            // 
            this.chkLimitFrameRate.AutoSize = true;
            this.chkLimitFrameRate.Checked = true;
            this.chkLimitFrameRate.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkLimitFrameRate.Location = new System.Drawing.Point(774, 113);
            this.chkLimitFrameRate.Name = "chkLimitFrameRate";
            this.chkLimitFrameRate.Size = new System.Drawing.Size(120, 16);
            this.chkLimitFrameRate.TabIndex = 35;
            this.chkLimitFrameRate.Text = "Limit frame rate";
            this.chkLimitFrameRate.UseVisualStyleBackColor = true;
            // 
            // FViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(900, 534);
            this.Controls.Add(this.chkLimitFrameRate);
            this.Controls.Add(this.trkFallingSpeed);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.trkSfxVolume);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.trkMusicVolume);
            this.Controls.Add(this.chkSfxOn);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cboSoundEffect);
            this.Controls.Add(this.btnScoreUnload);
            this.Controls.Add(this.btnScoreLoad);
            this.Controls.Add(this.btnPause);
            this.Controls.Add(this.lblTime);
            this.Controls.Add(this.trkProgress);
            this.Controls.Add(this.cboDifficulty);
            this.Controls.Add(this.lblSong);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnPlay);
            this.Controls.Add(this.txtScoreFileName);
            this.Controls.Add(this.txtAudioFileName);
            this.Controls.Add(this.btnSelectScore);
            this.Controls.Add(this.btnSelectAudio);
            this.Controls.Add(this.editor);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FViewer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "DereTore: Score Viewer";
            ((System.ComponentModel.ISupportInitialize)(this.trkProgress)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkFallingSpeed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkMusicVolume)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkSfxVolume)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.editor)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.ScoreEditorControl editor;
        private System.Windows.Forms.Button btnSelectAudio;
        private System.Windows.Forms.Button btnSelectScore;
        private System.Windows.Forms.TextBox txtAudioFileName;
        private System.Windows.Forms.TextBox txtScoreFileName;
        private System.Windows.Forms.Button btnPlay;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.Label lblSong;
        private System.Windows.Forms.ComboBox cboDifficulty;
        private System.Windows.Forms.Label lblTime;
        private System.Windows.Forms.TrackBar trkProgress;
        private System.Windows.Forms.Button btnPause;
        private System.Windows.Forms.Button btnScoreLoad;
        private System.Windows.Forms.Button btnScoreUnload;
        private System.Windows.Forms.ComboBox cboSoundEffect;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TrackBar trkFallingSpeed;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkSfxOn;
        private System.Windows.Forms.TrackBar trkMusicVolume;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TrackBar trkSfxVolume;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.CheckBox chkLimitFrameRate;
    }
}

