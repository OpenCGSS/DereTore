using System;
using System.Linq;
using DereTore.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace StarlightDirector.Entities {
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy), MemberSerialization = MemberSerialization.OptIn)]
    public sealed class Bar {

        [JsonIgnore]
        public double StartTime { get; private set; }

        [JsonIgnore]
        public double StartBpm { get; private set; }

        [JsonIgnore]
        public double TimeLength { get; private set; }

        [JsonIgnore]
        public int Signature => Params?.UserDefinedSignature ?? Score.Project.Settings.GlobalSignature;

        [JsonIgnore]
        public int GridPerSignature => Params?.UserDefinedGridPerSignature ?? Score.Project.Settings.GlobalGridPerSignature;

        [JsonIgnore]
        public int TotalGridCount => Signature * GridPerSignature;

        private double[] _timeAtGrid;

        public double TimeAtSignature(int signature) {
            if (signature < 0 || signature >= Signature)
                throw new ArgumentException("signature out of range");

            return TimeAtGrid(signature * GridPerSignature);
        }

        public double TimeAtGrid(int grid) {
            if (grid < 0 || grid >= TotalGridCount)
                throw new ArgumentException("grid out of range");

            return _timeAtGrid[grid];
        }

        public void UpdateTimings() {
            UpdateStartTime();
            UpdateStartBpm();
            UpdateTimeLength();
        }

        private void UpdateStartTime() {
            var bars = Score.Bars;
            var startTime = Score.Project.Settings.StartTimeOffset;
            var thisIndex = Index;
            if (bars.Count > 0) {
                for (var i = 0; i < thisIndex; ++i) {
                    startTime += bars[i].TimeLength;
                }
            }
            StartTime = startTime;
        }

        private void UpdateStartBpm() {
            for (int i = Index - 1; i >= 0; --i) {
                var bar = Score.Bars[i];
                if (bar.Notes.All(note => note.Type != NoteType.VariantBpm)) {
                    continue;
                }

                StartBpm = bar.Notes.Last(note => note.Type == NoteType.VariantBpm).ExtraParams.NewBpm;
                return;
            }

            StartBpm = Score.Project.Settings.GlobalBpm;
        }

        /// <summary>
        /// Fill _timeAtGrid, index in range [from, to], assuming constant BPM (bpm) starting from referenceIdx
        /// </summary>
        private void SetTimeAtGrid(int from, int to, double bpm, int referenceIdx) {
            var secondsPerSignature = DirectorHelper.BpmToSeconds(bpm);
            for (int i = from; i <= to; ++i) {
                var numGrids = i - referenceIdx;
                _timeAtGrid[i] = _timeAtGrid[referenceIdx] + numGrids * secondsPerSignature / GridPerSignature;
            }
        }

        private void UpdateTimeLength() {
            var length = 0.0;
            Note lastBpmNote = null;
            _timeAtGrid = new double[TotalGridCount];
            _timeAtGrid[0] = StartTime;

            // find all BpmNotes and compute the length between them
            foreach (var note in Notes) {
                if (note.Type != NoteType.VariantBpm)
                    continue;

                if (lastBpmNote == null) {
                    // length between start and first BpmNote
                    length += DirectorHelper.BpmToSeconds(StartBpm) * note.IndexInGrid / GridPerSignature;
                    SetTimeAtGrid(1, note.IndexInGrid, StartBpm, 0);
                } else {
                    // length between prev BpmNote and current
                    var deltaGridCount = note.IndexInGrid - lastBpmNote.IndexInGrid;
                    length += DirectorHelper.BpmToSeconds(lastBpmNote.ExtraParams.NewBpm) * deltaGridCount / GridPerSignature;
                    SetTimeAtGrid(lastBpmNote.IndexInGrid + 1, note.IndexInGrid, lastBpmNote.ExtraParams.NewBpm, lastBpmNote.IndexInGrid);
                }

                lastBpmNote = note;
            }

            // length from the last BpmNote to end
            // if it's null, there is no BpmNote in the bar
            if (lastBpmNote != null) {
                length += DirectorHelper.BpmToSeconds(lastBpmNote.ExtraParams.NewBpm) * (TotalGridCount - lastBpmNote.IndexInGrid) / GridPerSignature;
                SetTimeAtGrid(lastBpmNote.IndexInGrid + 1, TotalGridCount - 1, lastBpmNote.ExtraParams.NewBpm, lastBpmNote.IndexInGrid);
            } else {
                length = DirectorHelper.BpmToSeconds(StartBpm) * Signature;
                for (int i = 0; i < TotalGridCount; ++i) {
                    _timeAtGrid[i] = StartTime + length * i / TotalGridCount;
                }
            }

            TimeLength = length;
        }

        /// <summary>
        /// UpdateTimings() of this Bar and all Bars in the Score after this one
        /// </summary>
        internal void UpdateTimingsChain() {
            var myIdx = Score.Bars.IndexOf(this);

            if (myIdx >= 0) {
                for (int i = myIdx; i < Score.Bars.Count; ++i) {
                    Score.Bars[i].UpdateTimings();
                }
            } else {
                UpdateTimings();
            }
        }

        public Note AddNote() {
            var id = MathHelper.NextRandomPositiveInt32();
            while (Score.Project.ExistingIDs.Contains(id)) {
                id = MathHelper.NextRandomPositiveInt32();
            }
            return AddNote(id);
        }

        public bool RemoveNote(Note note) {
            if (!Notes.Contains(note)) {
                return false;
            }
            Notes.Remove(note);
            Score.Notes.Remove(note);
            Score.Project.ExistingIDs.Remove(note.ID);
            if (note.Type == NoteType.VariantBpm) {
                UpdateTimingsChain();
            }
            return true;
        }

        [JsonProperty]
        public InternalList<Note> Notes { get; }

        [JsonProperty]
        public BarParams Params { get; internal set; }

        [JsonProperty]
        public int Index { get; internal set; }

        public Score Score { get; internal set; }

        [JsonConstructor]
        internal Bar(Score score, int index) {
            Score = score;
            Notes = new InternalList<Note>();
            Index = index;
        }

        internal Note AddNote(int id) {
            if (Score.Project.ExistingIDs.Contains(id)) {
                return null;
            }
            var note = new Note(id, this);
            Notes.Add(note);
            Score.Notes.Add(note);
            Score.Project.ExistingIDs.Add(id);

            Notes.Sort(Note.TimingThenPositionComparison);
            Score.Notes.Sort(Note.TimingThenPositionComparison);
            return note;
        }

        internal Note AddNoteWithoutUpdatingGlobalNotes(int id) {
            if (Score.Project.ExistingIDs.Contains(id)) {
                return null;
            }
            var note = new Note(id, this);
            Notes.Add(note);
            Score.Project.ExistingIDs.Add(id);

            Notes.Sort(Note.TimingThenPositionComparison);
            return note;
        }

        internal void SquashParams() {
            if (Params?.CanBeSquashed ?? false) {
                Params = null;
            }
        }

        // Note object will call this when IndexInGrid is changed
        internal void SortNotes() {
            Notes.Sort(Note.TimingThenPositionComparison);
            Score.Notes.Sort(Note.TimingThenPositionComparison);
        }
    }
}
