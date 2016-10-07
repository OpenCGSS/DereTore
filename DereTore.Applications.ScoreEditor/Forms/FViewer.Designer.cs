using DereTore.Applications.ScoreEditor.Controls;

namespace DereTore.Applications.ScoreEditor.Forms {
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
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.btnScoreLoad = new System.Windows.Forms.Button();
            this.btnScoreUnload = new System.Windows.Forms.Button();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tsbNoteCreate = new System.Windows.Forms.ToolStripButton();
            this.tsbNoteEdit = new System.Windows.Forms.ToolStripButton();
            this.tsbNoteRemove = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbMakeSync = new System.Windows.Forms.ToolStripButton();
            this.tsbMakeFlick = new System.Windows.Forms.ToolStripButton();
            this.tsbMakeHold = new System.Windows.Forms.ToolStripButton();
            this.tsbResetToTap = new System.Windows.Forms.ToolStripButton();
            this.tsbRetimingToNow = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbScoreCreate = new System.Windows.Forms.ToolStripButton();
            this.tsbScoreSave = new System.Windows.Forms.ToolStripButton();
            this.tsbScoreSaveAs = new System.Windows.Forms.ToolStripButton();
            this.cboSoundEffect = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.trkFallingSpeed = new System.Windows.Forms.TrackBar();
            this.label2 = new System.Windows.Forms.Label();
            this.editor = new DereTore.Applications.ScoreEditor.Controls.ScoreEditorControl();
            this.chkSfxOn = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.trkProgress)).BeginInit();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trkFallingSpeed)).BeginInit();
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
            // propertyGrid
            // 
            this.propertyGrid.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.propertyGrid.Location = new System.Drawing.Point(311, 154);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.PropertySort = System.Windows.Forms.PropertySort.Alphabetical;
            this.propertyGrid.Size = new System.Drawing.Size(262, 308);
            this.propertyGrid.TabIndex = 14;
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
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbNoteCreate,
            this.tsbNoteEdit,
            this.tsbNoteRemove,
            this.toolStripSeparator1,
            this.tsbMakeSync,
            this.tsbMakeFlick,
            this.tsbMakeHold,
            this.tsbResetToTap,
            this.tsbRetimingToNow,
            this.toolStripSeparator2,
            this.tsbScoreCreate,
            this.tsbScoreSave,
            this.tsbScoreSaveAs});
            this.toolStrip1.Location = new System.Drawing.Point(434, 126);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(268, 25);
            this.toolStrip1.TabIndex = 0;
            // 
            // tsbNoteCreate
            // 
            this.tsbNoteCreate.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbNoteCreate.Image = global::DereTore.Applications.ScoreEditor.Properties.Resources.NoteCreate;
            this.tsbNoteCreate.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbNoteCreate.Name = "tsbNoteCreate";
            this.tsbNoteCreate.Size = new System.Drawing.Size(23, 22);
            this.tsbNoteCreate.Text = "Create note...";
            // 
            // tsbNoteEdit
            // 
            this.tsbNoteEdit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbNoteEdit.Image = global::DereTore.Applications.ScoreEditor.Properties.Resources.NoteEdit;
            this.tsbNoteEdit.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbNoteEdit.Name = "tsbNoteEdit";
            this.tsbNoteEdit.Size = new System.Drawing.Size(23, 22);
            this.tsbNoteEdit.Text = "Edit note...";
            // 
            // tsbNoteRemove
            // 
            this.tsbNoteRemove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbNoteRemove.Image = global::DereTore.Applications.ScoreEditor.Properties.Resources.NoteRemove;
            this.tsbNoteRemove.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbNoteRemove.Name = "tsbNoteRemove";
            this.tsbNoteRemove.Size = new System.Drawing.Size(23, 22);
            this.tsbNoteRemove.Text = "Remove note";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // tsbMakeSync
            // 
            this.tsbMakeSync.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbMakeSync.Image = global::DereTore.Applications.ScoreEditor.Properties.Resources.MakeSync;
            this.tsbMakeSync.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbMakeSync.Name = "tsbMakeSync";
            this.tsbMakeSync.Size = new System.Drawing.Size(23, 22);
            this.tsbMakeSync.Text = "Make sync...";
            // 
            // tsbMakeFlick
            // 
            this.tsbMakeFlick.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbMakeFlick.Image = global::DereTore.Applications.ScoreEditor.Properties.Resources.MakeFlick;
            this.tsbMakeFlick.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbMakeFlick.Name = "tsbMakeFlick";
            this.tsbMakeFlick.Size = new System.Drawing.Size(23, 22);
            this.tsbMakeFlick.Text = "Make flick...";
            // 
            // tsbMakeHold
            // 
            this.tsbMakeHold.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbMakeHold.Image = global::DereTore.Applications.ScoreEditor.Properties.Resources.MakeHold;
            this.tsbMakeHold.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbMakeHold.Name = "tsbMakeHold";
            this.tsbMakeHold.Size = new System.Drawing.Size(23, 22);
            this.tsbMakeHold.Text = "Make hold...";
            // 
            // tsbResetToTap
            // 
            this.tsbResetToTap.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbResetToTap.Image = global::DereTore.Applications.ScoreEditor.Properties.Resources.ResetToTap;
            this.tsbResetToTap.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbResetToTap.Name = "tsbResetToTap";
            this.tsbResetToTap.Size = new System.Drawing.Size(23, 22);
            this.tsbResetToTap.Text = "Reset to tap";
            // 
            // tsbRetimingToNow
            // 
            this.tsbRetimingToNow.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbRetimingToNow.Image = global::DereTore.Applications.ScoreEditor.Properties.Resources.RetimingToNow;
            this.tsbRetimingToNow.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbRetimingToNow.Name = "tsbRetimingToNow";
            this.tsbRetimingToNow.Size = new System.Drawing.Size(23, 22);
            this.tsbRetimingToNow.Text = "Retiming to now";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // tsbScoreCreate
            // 
            this.tsbScoreCreate.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbScoreCreate.Image = global::DereTore.Applications.ScoreEditor.Properties.Resources.ScoreCreate;
            this.tsbScoreCreate.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbScoreCreate.Name = "tsbScoreCreate";
            this.tsbScoreCreate.Size = new System.Drawing.Size(23, 22);
            this.tsbScoreCreate.Text = "Create score...";
            // 
            // tsbScoreSave
            // 
            this.tsbScoreSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbScoreSave.Image = global::DereTore.Applications.ScoreEditor.Properties.Resources.ScoreSave;
            this.tsbScoreSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbScoreSave.Name = "tsbScoreSave";
            this.tsbScoreSave.Size = new System.Drawing.Size(23, 22);
            this.tsbScoreSave.Text = "Save score";
            // 
            // tsbScoreSaveAs
            // 
            this.tsbScoreSaveAs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbScoreSaveAs.Image = global::DereTore.Applications.ScoreEditor.Properties.Resources.ScoreSaveAs;
            this.tsbScoreSaveAs.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbScoreSaveAs.Name = "tsbScoreSaveAs";
            this.tsbScoreSaveAs.Size = new System.Drawing.Size(23, 22);
            this.tsbScoreSaveAs.Text = "Save score as...";
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
            this.cboSoundEffect.Location = new System.Drawing.Point(807, 113);
            this.cboSoundEffect.Name = "cboSoundEffect";
            this.cboSoundEffect.Size = new System.Drawing.Size(82, 20);
            this.cboSoundEffect.TabIndex = 26;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(772, 116);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 27;
            this.label1.Text = "SFX:";
            // 
            // trkFallingSpeed
            // 
            this.trkFallingSpeed.Location = new System.Drawing.Point(843, 154);
            this.trkFallingSpeed.Maximum = 40;
            this.trkFallingSpeed.Minimum = 2;
            this.trkFallingSpeed.Name = "trkFallingSpeed";
            this.trkFallingSpeed.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.trkFallingSpeed.Size = new System.Drawing.Size(45, 368);
            this.trkFallingSpeed.TabIndex = 28;
            this.trkFallingSpeed.Value = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(772, 322);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 24);
            this.label2.TabIndex = 29;
            this.label2.Text = "Falling\r\nspeed:";
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
            // chkSfxOn
            // 
            this.chkSfxOn.AutoSize = true;
            this.chkSfxOn.Checked = true;
            this.chkSfxOn.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSfxOn.Location = new System.Drawing.Point(807, 139);
            this.chkSfxOn.Name = "chkSfxOn";
            this.chkSfxOn.Size = new System.Drawing.Size(36, 16);
            this.chkSfxOn.TabIndex = 30;
            this.chkSfxOn.Text = "On";
            this.chkSfxOn.UseVisualStyleBackColor = true;
            // 
            // FViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(900, 534);
            this.Controls.Add(this.chkSfxOn);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.trkFallingSpeed);
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
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.propertyGrid);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FViewer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "DereTore: Score Editor";
            ((System.ComponentModel.ISupportInitialize)(this.trkProgress)).EndInit();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trkFallingSpeed)).EndInit();
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
        private System.Windows.Forms.PropertyGrid propertyGrid;
        private System.Windows.Forms.Button btnScoreLoad;
        private System.Windows.Forms.Button btnScoreUnload;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton tsbNoteCreate;
        private System.Windows.Forms.ToolStripButton tsbNoteEdit;
        private System.Windows.Forms.ToolStripButton tsbNoteRemove;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton tsbMakeSync;
        private System.Windows.Forms.ToolStripButton tsbMakeFlick;
        private System.Windows.Forms.ToolStripButton tsbMakeHold;
        private System.Windows.Forms.ToolStripButton tsbResetToTap;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton tsbScoreCreate;
        private System.Windows.Forms.ToolStripButton tsbScoreSave;
        private System.Windows.Forms.ToolStripButton tsbScoreSaveAs;
        private System.Windows.Forms.ToolStripButton tsbRetimingToNow;
        private System.Windows.Forms.ComboBox cboSoundEffect;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TrackBar trkFallingSpeed;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkSfxOn;
    }
}

