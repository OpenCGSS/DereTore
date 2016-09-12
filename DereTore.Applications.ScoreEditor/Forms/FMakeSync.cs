using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using DereTore.Applications.ScoreEditor.Model;

namespace DereTore.Applications.ScoreEditor.Forms {
    public partial class FMakeSync : Form {

        ~FMakeSync() {
            UnregisterEventHandlers();
        }

        public static MakeSyncBasis SelectNotes(Score score, Note note, out Note anotherNote) {
            return SelectNotes(null, score, note, out anotherNote);
        }

        public static MakeSyncBasis SelectNotes(IWin32Window window, Score score, Note note, out Note anotherNote) {
            using (var f = new FMakeSync()) {
                f.Init(score, note);
                var result = f.ShowDialog(window);
                anotherNote = result == DialogResult.OK ? f.GetTheOtherNote() : null;
                return f._basis;
            }
        }

        private FMakeSync() {
            InitializeComponent();
            RegisterEventHandlers();
            DialogResult = DialogResult.Cancel;
            _basis = MakeSyncBasis.None;
        }

        private void Init(Score score, Note note) {
            txtCurrentNote.Text = $"Note ID #{note.Id} ({note.HitTiming})";
            _initNote = note;
            var notes = score.Notes.Where(n => n.IsGamingNote && n != note).ToList();
            notes.Sort((n1, n2) => n1.HitTiming.CompareTo(n2.HitTiming));
            var dataSource = (from n in notes
                              let diff = n.HitTiming - note.HitTiming
                              let diffDesc = diff > 0 ? "+" + diff.ToString(CultureInfo.InvariantCulture) : diff.ToString(CultureInfo.InvariantCulture)
                              let value = new NoteAndDesc(n, $"Note ID #{n.Id} ({n.HitTiming}, {diffDesc})")
                              select value).ToArray();
            cboNotes.DataSource = dataSource;
            cboNotes.DisplayMember = "Description";
            cboNotes.ValueMember = "Note";
            if (cboNotes.Items.Count > 0) {
                var index = -1;
                double? lastAbs = null;
                foreach (var n in notes) {
                    ++index;
                    var abs = Math.Abs(n.HitTiming - note.HitTiming);
                    if (lastAbs == null) {
                        lastAbs = abs;
                        continue;
                    }
                    if (lastAbs.Value >= abs) {
                        lastAbs = abs;
                    } else {
                        --index;
                        break;
                    }
                }
                if (index >= 0) {
                    cboNotes.SelectedIndex = index;
                }
            }
        }

        private Note GetTheOtherNote() {
            var value = cboNotes.SelectedItem as NoteAndDesc;
            return value?.Note;
        }

        private void UnregisterEventHandlers() {
            btnOK.Click -= BtnOK_Click;
            btnCancel.Click -= BtnCancel_Click;
            Load -= FMakeSync_Load;
            FormClosing -= FMakeSync_FormClosing;
            cboNotes.SelectedIndexChanged -= CboNotes_SelectedIndexChanged;
        }

        private void RegisterEventHandlers() {
            btnOK.Click += BtnOK_Click;
            btnCancel.Click += BtnCancel_Click;
            Load += FMakeSync_Load;
            FormClosing += FMakeSync_FormClosing;
            cboNotes.SelectedIndexChanged += CboNotes_SelectedIndexChanged;
        }

        private void CboNotes_SelectedIndexChanged(object sender, EventArgs e) {
            var value = cboNotes.SelectedItem as NoteAndDesc;
            if (value != null) {
                var equals = value.Note.HitTiming.Equals(_initNote.HitTiming);
                label3.Enabled = radUseFirst.Enabled = radUseSecond.Enabled = !equals;
                if (_selectedNote != null) {
                    _selectedNote.EditorSelected2 = false;
                }
                value.Note.EditorSelected2 = true;
                _selectedNote = value.Note;
            }
        }

        private void FMakeSync_FormClosing(object sender, FormClosingEventArgs e) {
            if (_selectedNote != null) {
                _selectedNote.EditorSelected2 = false;
            }
        }

        private void FMakeSync_Load(object sender, EventArgs e) {
            radUseFirst.Checked = true;
        }

        private void BtnCancel_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void BtnOK_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.OK;
            if (radUseFirst.Enabled || radUseSecond.Enabled) {
                if (radUseFirst.Checked) {
                    _basis = MakeSyncBasis.SelectedNote;
                } else if (radUseSecond.Checked) {
                    _basis = MakeSyncBasis.SyncPair;
                }
            }
            Close();
        }

        private MakeSyncBasis _basis;
        private Note _initNote;
        private Note _selectedNote;

        private sealed class NoteAndDesc {

            public NoteAndDesc(Note note, string description) {
                Note = note;
                Description = description;
            }

            public Note Note { get; }

            public string Description { get; }
        }

    }

}
