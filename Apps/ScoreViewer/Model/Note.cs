using System;
using System.ComponentModel;

namespace DereTore.Apps.ScoreViewer.Model {
    public sealed class Note : ICloneable {

        [Category("Note"), Description("该音符的ID。")]
        [ReadOnly(true)]
        public int ID { get; set; }
        [Category("Note"), Description("该音符的标准触击时间（秒）。")]
        [ReadOnly(true)]
        public double HitTiming { get; set; }
        [Category("Note"), Description("该音符的类别。")]
        [ReadOnly(true)]
        [TypeConverter(typeof(DescribedEnumConverter))]
        public NoteType Type { get; set; }
        [Category("Note"), Description("该音符的起始位置。")]
        [ReadOnly(true)]
        [TypeConverter(typeof(DescribedEnumConverter))]
        public NotePosition StartPosition { get; set; }
        [Category("Note"), Description("该音符的结束位置。")]
        [ReadOnly(true)]
        [TypeConverter(typeof(DescribedEnumConverter))]
        public NotePosition FinishPosition { get; set; }
        [Category("Note"), Description("该音符的按/滑类型。")]
        [ReadOnly(true)]
        [TypeConverter(typeof(DescribedEnumConverter))]
        public NoteStatus FlickType { get; set; }
        [Category("Note"), Description("该音符是否应与另一个音符一起同时按下。")]
        [ReadOnly(true)]
        public bool IsSync { get; set; }
        [Category("Note"), Description("该音符的组ID。仅对滑条/不可断滑条音符有效。")]
        [ReadOnly(true)]
        public int GroupID { get; set; }

        [Browsable(false)]
        public Note NextHoldNote { get; set; }
        [Browsable(false)]
        public Note NextFlickNote { get; set; }
        [Browsable(false)]
        public Note PrevHoldNote { get; set; }
        [Browsable(false)]
        public Note PrevFlickNote { get; set; }
        [Browsable(false)]
        public Note SyncPairNote { get; set; }
        [Browsable(false)]
        public Note NextSlideNote { get; set; }
        [Browsable(false)]
        public Note PrevSlideNote { get; set; }

        [Browsable(false)]
        public bool HasNextHold => NextHoldNote != null;
        [Browsable(false)]
        public bool HasNextFlick => NextFlickNote != null;
        [Browsable(false)]
        public bool HasNextSlide => NextSlideNote != null;
        [Browsable(false)]
        public bool HasPrevHold => PrevHoldNote != null;
        [Browsable(false)]
        public bool HasPrevFlick => PrevFlickNote != null;
        [Browsable(false)]
        public bool HasPrevSlide => PrevSlideNote != null;
        [Browsable(false)]
        public bool IsFlick => Type == NoteType.TapOrFlick && (FlickType == NoteStatus.FlickLeft || FlickType == NoteStatus.FlickRight);
        [Browsable(false)]
        public bool IsTap => Type == NoteType.TapOrFlick && FlickType == NoteStatus.Tap;
        [Browsable(false)]
        public bool IsHold => Type == NoteType.Hold;
        [Browsable(false)]
        public bool IsHoldPress => Type == NoteType.Hold && HasNextHold;
        [Browsable(false)]
        public bool IsHoldRelease => (Type == NoteType.TapOrFlick || Type == NoteType.Slide) && HasPrevHold;
        [Browsable(false)]
        public bool IsSlide => Type == NoteType.Slide;
        [Browsable(false)]
        public bool IsSlideBegin => Type == NoteType.Slide && HasNextSlide;
        [Browsable(false)]
        public bool IsSlideMiddle => Type == NoteType.Slide && HasNextSlide && HasPrevSlide;
        [Browsable(false)]
        public bool IsSlideEnd => Type == NoteType.Slide && HasPrevSlide;
        [Browsable(false)]
        public bool IsGamingNote => Type == NoteType.TapOrFlick || Type == NoteType.Hold || Type == NoteType.Slide;

        // Properties for editor control.
        [Browsable(false)]
        public bool EditorVisible { get; set; }
        [Browsable(false)]
        public bool EditorSelected { get; set; }
        [Browsable(false)]
        public bool EditorSelected2 { get; set; }

        public Note Clone() {
            var note = new Note {
                ID = ID,
                HitTiming = HitTiming,
                Type = Type,
                StartPosition = StartPosition,
                FinishPosition = FinishPosition,
                FlickType = FlickType,
                IsSync = IsSync,
                GroupID = GroupID,
                EditorSelected = EditorSelected,
                EditorVisible = EditorVisible,
                NextHoldNote = NextHoldNote,
                PrevHoldNote = PrevHoldNote,
                NextFlickNote = NextFlickNote,
                PrevFlickNote = PrevFlickNote,
                SyncPairNote = SyncPairNote,
                NextSlideNote = NextSlideNote,
                PrevSlideNote = PrevSlideNote
            };
            return note;
        }

        public void CopyFrom(Note template) {
            ID = template.ID;
            HitTiming = template.HitTiming;
            Type = template.Type;
            StartPosition = template.StartPosition;
            FinishPosition = template.FinishPosition;
            FlickType = template.FlickType;
            IsSync = template.IsSync;
            GroupID = template.GroupID;
            EditorSelected = template.EditorSelected;
            EditorVisible = template.EditorVisible;
            NextHoldNote = template.NextHoldNote;
            PrevHoldNote = template.PrevHoldNote;
            NextFlickNote = template.NextFlickNote;
            PrevFlickNote = template.PrevFlickNote;
            SyncPairNote = template.SyncPairNote;
            PrevSlideNote = template.PrevSlideNote;
            NextSlideNote = template.NextSlideNote;
        }

        object ICloneable.Clone() {
            return Clone();
        }

        public override string ToString() {
            return $"ID: {ID}, Timing: {HitTiming}, Type: {Type}";
        }
    }
}
