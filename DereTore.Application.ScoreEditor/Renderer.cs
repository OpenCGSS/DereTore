using System;
using System.Drawing;
using DereTore.Application.ScoreEditor.Model;

namespace DereTore.Application.ScoreEditor {
    public sealed class Renderer {

        static Renderer() {
            InstanceSyncObject = new object();
        }

        public event EventHandler<NoteUpdatedEventArgs> NoteUpdated;

        public bool IsRendering {
            get {
                lock (_renderingSyncObject) {
                    return _isRendering;
                }
            }
            private set {
                lock (_renderingSyncObject) {
                    _isRendering = value;
                }
            }
        }

        public static Renderer Instance {
            get {
                lock (InstanceSyncObject) {
                    return _instance ?? (_instance = new Renderer());
                }
            }
        }

        public void RenderFrame(Graphics graphics, Size clientSize, TimeSpan timeSpan, Score score) {
            IsRendering = true;
            DrawCeilingLine(graphics, clientSize);
            DrawAvatars(graphics, clientSize);
            var now = (float)timeSpan.TotalSeconds;
            var scores = score.Items;
            int startIndex, endIndex;
            //GetVisibleNotes(now, scores, out startIndex, out endIndex);
            startIndex = 0;
            endIndex = scores.Length - 1;
            DrawNotes(graphics, clientSize, now, scores, startIndex, endIndex);
            IsRendering = false;
        }

        public void UpdateNotes(Score score, TimeSpan timeSpan) {
            var notes = score.Items;
            var now = (float)timeSpan.TotalSeconds;
            foreach (var note in notes) {
                if (IsNoteOnStage(note, now)) {
                    if (!note.Visible) {
                        NoteUpdated?.Invoke(this, new NoteUpdatedEventArgs(note));
                        note.Visible = true;
                    }
                } else {
                    if (note.Visible) {
                        NoteUpdated?.Invoke(this, new NoteUpdatedEventArgs(note));
                        note.Visible = false;
                    }
                }
            }
        }

        private Renderer() {
            _renderingSyncObject = new object();
        }

        private static void DrawAvatars(Graphics graphics, Size clientSize) {
            var centerY = clientSize.Height * BaseLineYPosition;
            foreach (var position in AvatarCenterXPositions) {
                var centerX = clientSize.Width * position;
                graphics.FillEllipse(Brushes.Firebrick, centerX - AvatarCircleRadius, centerY - AvatarCircleRadius, AvatarCircleDiameter, AvatarCircleDiameter);
            }
        }

        private static void DrawCeilingLine(Graphics graphics, Size clientSize) {
            float p1 = AvatarCenterXPositions[0], p5 = AvatarCenterXPositions[AvatarCenterXPositions.Length - 1];
            float x1 = clientSize.Width * p1, x2 = clientSize.Width * p5;
            float ceilingY = FutureNoteCeiling * clientSize.Height;
            graphics.DrawLine(Pens.Red, x1, ceilingY, x2, ceilingY);
        }

        private static void GetVisibleNotes(float now, Note[] notes, out int startIndex, out int endIndex) {
            startIndex = -1;
            endIndex = -1;
            Note lastHoldNote, lastSwipeNote;
            // Notes which have connection lines should be drawn, but only their lines. Case for holding time exceeds falling time window.
            for (var i = 0; i < notes.Length; ++i) {
                var s = notes[i];
                if (startIndex < 0 && s.Second > now - PastTimeWindow) {
                    startIndex = i;
                }
                if (s.Second > now + FutureTimeWindow) {
                    break;
                }
                endIndex = i;
            }
        }

        private static void DrawNotes(Graphics graphics, Size clientSize, float now, Note[] notes, int startIndex, int endIndex) {
            if (startIndex < 0) {
                return;
            }
            for (var i = startIndex; i <= endIndex; ++i) {
                var note = notes[i];
                switch (note.Type) {
                    case NoteType.TapOrSwipe:
                        if (note.Sync) {
                            // This will draw 2 lines, one time for each synced note.
                            DrawSyncLine(graphics, clientSize, note, notes[note.SyncPairIndex], now);
                        }
                        if (note.IsSwipe) {
                            if (note.HasNextSwipe) {
                                DrawSwipeLine(graphics, clientSize, note, notes[note.NextSwipeIndex], now);
                                //DrawSwipeLine(graphics, clientSize, notes[note.NextSwipeIndex], note, now);
                            }
                        }
                        DrawSimpleNote(graphics, clientSize, note, now);
                        break;
                    case NoteType.Hold:
                        if (note.Sync) {
                            // This will draw 2 lines, one time for each synced note.
                            DrawSyncLine(graphics, clientSize, note, notes[note.SyncPairIndex], now);
                        }
                        if (note.HasNextHolding) {
                            DrawHoldLine(graphics, clientSize, note, notes[note.NextHoldingIndex], now);
                        }
                        if (note.HasPrevHolding) {
                            if (!IsNoteOnStage(notes[note.PrevHoldingIndex], now)) {
                                DrawHoldLine(graphics, clientSize, notes[note.PrevHoldingIndex], note, now);
                            }
                        }
                        DrawSimpleNote(graphics, clientSize, note, now);
                        break;
                }
            }
        }

        private static void DrawHoldLine(Graphics graphics, Size clientSize, Note startNote, Note endNote, float now) {
            DrawSimpleLine(graphics, clientSize, startNote, endNote, now, Pens.Yellow);
        }

        private static void DrawSyncLine(Graphics graphics, Size clientSize, Note note1, Note note2, float now) {
            if (!IsNoteOnStage(note1, now) || !IsNoteOnStage(note2, now)) {
                return;
            }
            float x1 = GetNoteXPosition(note1, clientSize, now),
                y = GetNoteYPosition(note2, clientSize, now),
                x2 = GetNoteXPosition(note2, clientSize, now);
            float xLeft = Math.Min(x1, x2), xRight = Math.Max(x1, x2);
            graphics.DrawLine(Pens.DodgerBlue, xLeft + AvatarCircleRadius, y, xRight - AvatarCircleRadius, y);
        }

        private static void DrawSwipeLine(Graphics graphics, Size clientSize, Note startNote, Note endNote, float now) {
            DrawSimpleLine(graphics, clientSize, startNote, endNote, now, Pens.OliveDrab);
        }

        private static void DrawSimpleLine(Graphics graphics, Size clientSize, Note startNote, Note endNote, float now, Pen pen) {
            OnStageStatus s1 = GetNoteOnStageStatus(startNote, now), s2 = GetNoteOnStageStatus(endNote, now);
            if (s1 != OnStageStatus.OnStage && s2 != OnStageStatus.OnStage && s1 == s2) {
                return;
            }
            float x1, x2, y1, y2;
            GetNotePairPositions(startNote, endNote, clientSize, now, out x1, out x2, out y1, out y2);
            graphics.DrawLine(pen, x1, y1, x2, y2);
        }

        private static void GetNotePairPositions(Note note1, Note note2, Size clientSize, float now, out float x1, out float x2, out float y1, out float y2) {
            if (IsNotePassed(note1, now)) {
                x1 = GetAvatarXPosition(clientSize, note1.FinishPosition);
                y1 = GetAvatarYPosition(clientSize);
            } else if (IsNoteComing(note1, now)) {
                x1 = GetBirthXPosition(clientSize, note1.StartPosition);
                y1 = GetBirthYPosition(clientSize);
            } else {
                x1 = GetNoteXPosition(note1, clientSize, now);
                y1 = GetNoteYPosition(note1, clientSize, now);
            }
            if (IsNotePassed(note2, now)) {
                x2 = GetAvatarXPosition(clientSize, note2.FinishPosition);
                y2 = GetAvatarYPosition(clientSize);
            } else if (IsNoteComing(note2, now)) {
                x2 = GetBirthXPosition(clientSize, note2.StartPosition);
                y2 = GetBirthYPosition(clientSize);
            } else {
                x2 = GetNoteXPosition(note2, clientSize, now);
                y2 = GetNoteYPosition(note2, clientSize, now);
            }
        }

        private static void DrawSimpleNote(Graphics graphics, Size clientSize, Note note, float now) {
            if (!IsNoteOnStage(note, now)) {
                return;
            }
            float x = GetNoteXPosition(note, clientSize, now), y = GetNoteYPosition(note, clientSize, now);
            graphics.FillEllipse(Brushes.DarkMagenta, x - AvatarCircleRadius, y - AvatarCircleRadius, AvatarCircleDiameter, AvatarCircleDiameter);
            if (note.IsSwipe) {
                switch (note.SwipeType) {
                    case NoteStatus.SwipeLeft:
                        graphics.FillPie(Brushes.DarkOrange, x - AvatarCircleRadius, y - AvatarCircleRadius, AvatarCircleDiameter, AvatarCircleDiameter, 135, 90);
                        break;
                    case NoteStatus.SwipeRight:
                        graphics.FillPie(Brushes.DarkOrange, x - AvatarCircleRadius, y - AvatarCircleRadius, AvatarCircleDiameter, AvatarCircleDiameter, -45, 90);
                        break;
                }
            }
        }

        private static float GetNoteXPosition(Note note, Size clientSize, float now) {
            var timeRemaining = note.Second - now;
            float startPos = AvatarCenterXPositions[(int)note.StartPosition - 1] * clientSize.Width,
                endPos = AvatarCenterXPositions[(int)note.FinishPosition - 1] * clientSize.Width;
            return endPos - (endPos - startPos) * timeRemaining / FutureTimeWindow;
        }

        private static float GetNoteYPosition(Note note, Size clientSize, float now) {
            var timeRemaining = note.Second - now;
            float ceiling = FutureNoteCeiling * clientSize.Height,
                baseLine = BaseLineYPosition * clientSize.Height;
            return baseLine - (baseLine - ceiling) * timeRemaining / FutureTimeWindow;
        }

        private static float GetAvatarXPosition(Size clientSize, NotePosition position) {
            return clientSize.Width * AvatarCenterXPositions[(int)position - 1];
        }

        private static float GetAvatarYPosition(Size clientSize) {
            return clientSize.Height * BaseLineYPosition;
        }

        private static float GetBirthXPosition(Size clientSize, NotePosition position) {
            return clientSize.Width * AvatarCenterXPositions[(int)position - 1];
        }

        private static float GetBirthYPosition(Size clientSize) {
            return clientSize.Height * FutureNoteCeiling;
        }

        private static OnStageStatus GetNoteOnStageStatus(Note note, float now) {
            if (note.Second < now) {
                return OnStageStatus.Passed;
            }
            if (note.Second > now + FutureTimeWindow) {
                return OnStageStatus.Upcoming;
            }
            return OnStageStatus.OnStage;
        }

        private static bool IsNoteOnStage(Note note, float now) {
            return now <= note.Second && note.Second <= now + FutureTimeWindow;
        }

        private static bool IsNotePassed(Note note, float now) {
            return note.Second < now;
        }

        private static bool IsNoteComing(Note note, float now) {
            return note.Second > now + FutureTimeWindow;
        }

        private static float GetBottomYPosition() => BaseLineYPosition + (PastTimeWindow / FutureTimeWindow) * (BaseLineYPosition - FutureNoteCeiling);

        private static readonly float FutureTimeWindow = 1f;
        private static readonly float PastTimeWindow = 0.2f;
        private static readonly float AvatarCircleDiameter = 50;
        private static readonly float AvatarCircleRadius = AvatarCircleDiameter / 2;
        private static readonly float[] AvatarCenterXPositions = { 0.2f, 0.35f, 0.5f, 0.65f, 0.8f };
        private static readonly float BaseLineYPosition = 0.85f;
        // Then we know the bottom is <AvatarCenterY + (PastWindow / FutureWindow) * (AvatarCenterY - Ceiling))>.
        private static readonly float FutureNoteCeiling = 0.1f;

        private readonly object _renderingSyncObject;
        private static readonly object InstanceSyncObject;
        private static Renderer _instance;
        private bool _isRendering;

        private enum OnStageStatus {
            Upcoming,
            OnStage,
            Passed
        }

    }
}
