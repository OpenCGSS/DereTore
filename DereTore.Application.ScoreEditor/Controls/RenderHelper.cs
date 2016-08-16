using System;
using System.Collections.Generic;
using System.Drawing;
using DereTore.Application.ScoreEditor.Model;

namespace DereTore.Application.ScoreEditor.Controls {
    internal static class RenderHelper {

        public static void DrawAvatars(Graphics graphics, Size clientSize) {
            var centerY = clientSize.Height * BaseLineYPosition;
            foreach (var position in AvatarCenterXPositions) {
                var centerX = clientSize.Width * position;
                graphics.FillEllipse(Brushes.Firebrick, centerX - AvatarCircleRadius, centerY - AvatarCircleRadius, AvatarCircleDiameter, AvatarCircleDiameter);
            }
        }

        public static void DrawCeilingLine(Graphics graphics, Size clientSize) {
            float p1 = AvatarCenterXPositions[0], p5 = AvatarCenterXPositions[AvatarCenterXPositions.Length - 1];
            float x1 = clientSize.Width * p1, x2 = clientSize.Width * p5;
            float ceilingY = FutureNoteCeiling * clientSize.Height;
            graphics.DrawLine(Pens.Red, x1, ceilingY, x2, ceilingY);
        }

        public static void GetVisibleNotes(float now, Note[] notes, out int startIndex, out int endIndex) {
            startIndex = -1;
            endIndex = -1;
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

        public static void DrawNotes(Graphics graphics, Size clientSize, float now, List<Note> notes, int startIndex, int endIndex) {
            if (startIndex < 0) {
                return;
            }
            for (var i = startIndex; i <= endIndex; ++i) {
                var note = notes[i];
                switch (note.Type) {
                    case NoteType.TapOrSwipe:
                    case NoteType.Hold:
                        if (note.Selected && IsNoteOnStage(note, now)) {
                            float x = GetNoteXPosition(note, clientSize, now), y = GetNoteYPosition(note, clientSize, now);
                            graphics.DrawRectangle(Pens.White, x - AvatarCircleRadius, y - AvatarCircleRadius, AvatarCircleDiameter, AvatarCircleDiameter);
                        }
                        if (note.Sync) {
                            DrawSyncLine(graphics, clientSize, note, notes[note.SyncPairIndex], now);
                        }
                        break;
                }
                switch (note.Type) {
                    case NoteType.TapOrSwipe:
                        if (note.IsSwipe) {
                            if (note.HasNextSwipe) {
                                DrawSwipeLine(graphics, clientSize, note, notes[note.NextSwipeIndex], now);
                                //DrawSwipeLine(graphics, clientSize, notes[note.NextSwipeIndex], note, now);
                            }
                        }
                        break;
                    case NoteType.Hold:
                        if (note.HasNextHolding) {
                            DrawHoldLine(graphics, clientSize, note, notes[note.NextHoldingIndex], now);
                        }
                        if (note.HasPrevHolding) {
                            if (!IsNoteOnStage(notes[note.PrevHoldingIndex], now)) {
                                DrawHoldLine(graphics, clientSize, notes[note.PrevHoldingIndex], note, now);
                            }
                        }
                        break;
                }
                switch (note.Type) {
                    case NoteType.TapOrSwipe:
                    case NoteType.Hold:
                        DrawSimpleNote(graphics, clientSize, note, now);
                        break;
                }
            }
        }

        public static void DrawHoldLine(Graphics graphics, Size clientSize, Note startNote, Note endNote, float now) {
            DrawSimpleLine(graphics, clientSize, startNote, endNote, now, Pens.Yellow);
        }

        public static void DrawSyncLine(Graphics graphics, Size clientSize, Note note1, Note note2, float now) {
            if (!IsNoteOnStage(note1, now) || !IsNoteOnStage(note2, now)) {
                return;
            }
            float x1 = GetNoteXPosition(note1, clientSize, now),
                y = GetNoteYPosition(note2, clientSize, now),
                x2 = GetNoteXPosition(note2, clientSize, now);
            float xLeft = Math.Min(x1, x2), xRight = Math.Max(x1, x2);
            graphics.DrawLine(Pens.DodgerBlue, xLeft + AvatarCircleRadius, y, xRight - AvatarCircleRadius, y);
        }

        public static void DrawSwipeLine(Graphics graphics, Size clientSize, Note startNote, Note endNote, float now) {
            DrawSimpleLine(graphics, clientSize, startNote, endNote, now, Pens.OliveDrab);
        }

        public static void DrawSimpleLine(Graphics graphics, Size clientSize, Note startNote, Note endNote, float now, Pen pen) {
            OnStageStatus s1 = GetNoteOnStageStatus(startNote, now), s2 = GetNoteOnStageStatus(endNote, now);
            if (s1 != OnStageStatus.OnStage && s2 != OnStageStatus.OnStage && s1 == s2) {
                return;
            }
            float x1, x2, y1, y2;
            GetNotePairPositions(startNote, endNote, clientSize, now, out x1, out x2, out y1, out y2);
            graphics.DrawLine(pen, x1, y1, x2, y2);
        }

        public static void GetNotePairPositions(Note note1, Note note2, Size clientSize, float now, out float x1, out float x2, out float y1, out float y2) {
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

        public static void DrawSimpleNote(Graphics graphics, Size clientSize, Note note, float now) {
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

        public static float GetNoteXPosition(Note note, Size clientSize, float now) {
            var timeRemaining = note.Second - now;
            float startPos = AvatarCenterXPositions[(int)note.StartPosition - 1] * clientSize.Width,
                endPos = AvatarCenterXPositions[(int)note.FinishPosition - 1] * clientSize.Width;
            return endPos - (endPos - startPos) * timeRemaining / FutureTimeWindow;
        }

        public static float GetNoteYPosition(Note note, Size clientSize, float now) {
            var timeRemaining = note.Second - now;
            float ceiling = FutureNoteCeiling * clientSize.Height,
                baseLine = BaseLineYPosition * clientSize.Height;
            return baseLine - (baseLine - ceiling) * timeRemaining / FutureTimeWindow;
        }

        public static float GetAvatarXPosition(Size clientSize, NotePosition position) {
            return clientSize.Width * AvatarCenterXPositions[(int)position - 1];
        }

        public static float GetAvatarYPosition(Size clientSize) {
            return clientSize.Height * BaseLineYPosition;
        }

        public static float GetBirthXPosition(Size clientSize, NotePosition position) {
            return clientSize.Width * AvatarCenterXPositions[(int)position - 1];
        }

        public static float GetBirthYPosition(Size clientSize) {
            return clientSize.Height * FutureNoteCeiling;
        }

        public static OnStageStatus GetNoteOnStageStatus(Note note, float now) {
            if (note.Second < now) {
                return OnStageStatus.Passed;
            }
            if (note.Second > now + FutureTimeWindow) {
                return OnStageStatus.Upcoming;
            }
            return OnStageStatus.OnStage;
        }

        public static bool IsNoteOnStage(Note note, float now) {
            return now <= note.Second && note.Second <= now + FutureTimeWindow;
        }

        public static bool IsNotePassed(Note note, float now) {
            return note.Second < now;
        }

        public static bool IsNoteComing(Note note, float now) {
            return note.Second > now + FutureTimeWindow;
        }

        public enum OnStageStatus {
            Upcoming,
            OnStage,
            Passed
        }

        public static readonly float FutureTimeWindow = 1f;
        public static readonly float PastTimeWindow = 0.2f;
        public static readonly float AvatarCircleDiameter = 50;
        public static readonly float AvatarCircleRadius = AvatarCircleDiameter / 2;
        public static readonly float[] AvatarCenterXPositions = { 0.2f, 0.35f, 0.5f, 0.65f, 0.8f };
        public static readonly float BaseLineYPosition = 5f / 6;
        // Then we know the bottom is <AvatarCenterY + (PastWindow / FutureWindow) * (AvatarCenterY - Ceiling))>.
        public static readonly float FutureNoteCeiling = 1f / 6;

        public static float GetBottomYPosition() => BaseLineYPosition + (PastTimeWindow / FutureTimeWindow) * (BaseLineYPosition - FutureNoteCeiling);

    }
}
