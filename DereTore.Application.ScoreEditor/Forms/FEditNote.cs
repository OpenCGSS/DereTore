using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using DereTore.Application.ScoreEditor.Model;

namespace DereTore.Application.ScoreEditor.Forms {
    public partial class FEditNote : Form {

        ~FEditNote() {
            UnregisterEventHandlers();
        }

        public static Note ShowCreateNote(double now) {
            return ShowCreateNote(null, now);
        }

        public static Note ShowEditNote(Note template) {
            return ShowEditNote(null, template);
        }

        public static Note ShowCreateNote(IWin32Window window, double now) {
            using (var f = new FEditNote()) {
                f.CreateNodeInfo(now);
                f.ShowDialog(window);
                return f.DialogResult == DialogResult.OK ? f.GetNote() : null;
            }
        }

        public static Note ShowEditNote(IWin32Window window, Note template) {
            using (var f = new FEditNote()) {
                f.LoadNoteInfo(template);
                f.ShowDialog(window);
                return f.DialogResult == DialogResult.OK ? f.GetNote() : null;
            }
        }

        private FEditNote() {
            InitializeComponent();
            RegisterEventHandlers();
            InitializeControls();
            _editMode = EditMode.Create;
            DialogResult = DialogResult.Cancel;
        }

        private Note GetNote() {
            return _generatedNote?.Clone();
        }

        private void LoadNoteInfo(Note template) {
            _generatedNote = template.Clone();
            txtNoteTiming.Text = template.HitTiming.ToString(CultureInfo.InvariantCulture);
            cboNoteStartPos.SelectedIndex = (int)template.StartPosition - 1;
            cboNoteFinishPos.SelectedIndex = (int)template.FinishPosition - 1;
            _editMode = EditMode.Edit;
        }

        private void CreateNodeInfo(double now) {
            txtNoteTiming.Text = now.ToString(CultureInfo.InvariantCulture);
            cboNoteStartPos.SelectedIndex = cboNoteFinishPos.SelectedIndex = 0;
            _editMode = EditMode.Create;
        }

        private void UnregisterEventHandlers() {
            btnOK.Click -= BtnOK_Click;
            btnCancel.Click -= BtnCancel_Click;
            Load -= FEditNote_Load;
            btnTimingAdd.Click -= BtnTimingAdd_Click;
            btnTimingSubtract.Click -= BtnTimingSubtract_Click;
            txtNoteTiming.MouseWheel -= TxtNoteTiming_MouseWheel;
        }

        private void RegisterEventHandlers() {
            btnOK.Click += BtnOK_Click;
            btnCancel.Click += BtnCancel_Click;
            Load += FEditNote_Load;
            btnTimingAdd.Click += BtnTimingAdd_Click;
            btnTimingSubtract.Click += BtnTimingSubtract_Click;
            txtNoteTiming.MouseWheel += TxtNoteTiming_MouseWheel;
        }

        private void TxtNoteTiming_MouseWheel(object sender, MouseEventArgs e) {
            if (e.Delta > 0) {
                AddTiming();
            } else {
                SubtractTiming();
            }
        }

        private void BtnTimingSubtract_Click(object sender, EventArgs e) {
            SubtractTiming();
        }

        private void BtnTimingAdd_Click(object sender, EventArgs e) {
            AddTiming();
        }

        private void FEditNote_Load(object sender, EventArgs e) {
            switch (_editMode) {
                case EditMode.Create:
                    Text = CreateNoteTitle;
                    break;
                case EditMode.Edit:
                    Text = EditNoteTitle;
                    break;
                default:
                    break;
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void BtnOK_Click(object sender, EventArgs e) {
            string reason;
            var checkResult = CheckFields(out reason);
            if (!checkResult) {
                this.ShowMessageBox(reason, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            FillNote();
            DialogResult = DialogResult.OK;
            Close();
        }

        private bool CheckFields(out string reason) {
            double f;
            if (!double.TryParse(txtNoteTiming.Text, out f)) {
                reason = "Note time should be a real number.";
                return false;
            }
            if (f <= 0) {
                reason = "Notes should appear after the song starts.";
                return false;
            }
            reason = null;
            return true;
        }

        private void FillNote() {
            var note = _generatedNote ?? new Note();
            note.HitTiming = double.Parse(txtNoteTiming.Text);
            var sp1 = (NotePosition)(cboNoteStartPos.SelectedIndex + 1);
            var fp1 = (NotePosition)(cboNoteFinishPos.SelectedIndex + 1);
            if (!note.IsHoldRelease) {
                note.StartPosition = sp1;
                note.FinishPosition = fp1;
            } else {
                if (sp1 != note.PrevHoldNote.StartPosition || fp1 != note.PrevHoldNote.FinishPosition) {
                    this.ShowMessageBox("This note is a hold note (release), therefore modification of its start/finish position is ignored.");
                    note.StartPosition = note.PrevHoldNote.StartPosition;
                    note.FinishPosition = note.PrevHoldNote.FinishPosition;
                }
            }
            // If this is called during creation, other fields should be initialized using NoteManipulator.InitializeAsTap().
            _generatedNote = note;
        }

        private void InitializeControls() {
            foreach (var i in Enumerable.Range(0, 5)) {
                cboNoteStartPos.Items.Add(PositionDescriptions[i]);
                cboNoteFinishPos.Items.Add(PositionDescriptions[i]);
            }
        }

        private void AddTiming() {
            double d;
            double.TryParse(txtNoteTiming.Text, out d);
            d += NoteManipulator.NoteTimingStep;
            txtNoteTiming.Text = d.ToString(CultureInfo.InvariantCulture);
        }

        private void SubtractTiming() {
            double d;
            double.TryParse(txtNoteTiming.Text, out d);
            d -= NoteManipulator.NoteTimingStep;
            if (d < 0) {
                d = 0;
            }
            txtNoteTiming.Text = d.ToString(CultureInfo.InvariantCulture);
        }

        private Note _generatedNote;
        private EditMode _editMode;
        
        private static readonly string[] PositionDescriptions = {
            "Left",
            "Center Left",
            "Center",
            "Center Right",
            "Right"
        };

        private static readonly string CreateNoteTitle = "Create Note";
        private static readonly string EditNoteTitle = "Edit Note";

        private enum EditMode {
            Create,
            Edit
        }

    }
}
