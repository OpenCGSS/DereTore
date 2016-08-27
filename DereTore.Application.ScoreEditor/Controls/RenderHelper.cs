using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DereTore.Application.ScoreEditor.Model;

namespace DereTore.Application.ScoreEditor.Controls {
    internal static class RenderHelper {

        public static void DrawAvatars(RenderParams renderParams) {
            var clientSize = renderParams.ClientSize;
            var centerY = clientSize.Height * BaseLineYPosition;
            foreach (var position in AvatarCenterXPositions) {
                var centerX = clientSize.Width * position;
                renderParams.Graphics.FillEllipse(Brushes.Firebrick, centerX - AvatarCircleRadius, centerY - AvatarCircleRadius, AvatarCircleDiameter, AvatarCircleDiameter);
            }
        }

        public static void DrawCeilingLine(RenderParams renderParams) {
            var clientSize = renderParams.ClientSize;
            float p1 = AvatarCenterXPositions[0], p5 = AvatarCenterXPositions[AvatarCenterXPositions.Length - 1];
            float x1 = clientSize.Width * p1, x2 = clientSize.Width * p5;
            var ceilingY = FutureNoteCeiling * clientSize.Height;
            renderParams.Graphics.DrawLine(Pens.Red, x1, ceilingY, x2, ceilingY);
        }

        public static void GetVisibleNotes(double now, List<Note> notes, out int startIndex, out int endIndex) {
            startIndex = -1;
            endIndex = -1;
            var i = 0;
            // Notes which have connection lines should be drawn, but only their lines. Case for holding time exceeds falling time window.
            foreach (var note in notes) {
                if (startIndex < 0 && note.HitTiming > now - PastTimeWindow) {
                    startIndex = i;
                }
                if (note.HitTiming > now + FutureTimeWindow) {
                    break;
                }
                endIndex = i;
                ++i;
            }
        }

        public static void DrawNotes(RenderParams renderParams, IList<Note> notes, int startIndex, int endIndex) {
            if (startIndex < 0) {
                return;
            }
            var selectedNotes = notes.Skip(startIndex).Take(endIndex - startIndex + 1);
            foreach (var note in selectedNotes) {
                switch (note.Type) {
                    case NoteType.TapOrFlick:
                    case NoteType.Hold:
                        if (IsNoteOnStage(note, renderParams.Now)) {
                            if (note.EditorSelected) {
                                DrawSelectedRect(renderParams, note, Pens.White);
                            } else if (note.EditorSelected2) {
                                DrawSelectedRect(renderParams, note, Pens.LightGreen);
                            }
                        }
                        if (note.IsSync) {
                            DrawSyncLine(renderParams, note, note.SyncPairNote);
                        }
                        break;
                }
                switch (note.Type) {
                    case NoteType.TapOrFlick:
                        if (note.IsFlick) {
                            if (note.HasNextFlick) {
                                DrawFlickLine(renderParams, note, note.NextFlickNote);
                            }
                        }
                        break;
                    case NoteType.Hold:
                        if (note.HasNextHold) {
                            DrawHoldLine(renderParams, note, note.NextHoldNote);
                        }
                        if (note.HasPrevHold) {
                            if (!IsNoteOnStage(note.PrevHoldNote, renderParams.Now)) {
                                DrawHoldLine(renderParams, note.PrevHoldNote, note);
                            }
                        }
                        break;
                }
                switch (note.Type) {
                    case NoteType.TapOrFlick:
                    case NoteType.Hold:
                        DrawSimpleNote(renderParams, note);
                        break;
                }
            }
        }

        public static void DrawSelectedRect(RenderParams renderParams, Note note, Pen pen) {
            float x = GetNoteXPosition(renderParams, note), y = GetNoteYPosition(renderParams, note);
            renderParams.Graphics.DrawRectangle(pen, x - AvatarCircleRadius, y - AvatarCircleRadius, AvatarCircleDiameter, AvatarCircleDiameter);
        }

        public static void DrawSyncLine(RenderParams renderParams, Note note1, Note note2) {
            var now = renderParams.Now;
            if (!IsNoteOnStage(note1, now) || !IsNoteOnStage(note2, now)) {
                return;
            }
            float x1 = GetNoteXPosition(renderParams, note1),
                y = GetNoteYPosition(renderParams, note2),
                x2 = GetNoteXPosition(renderParams, note2);
            float xLeft = Math.Min(x1, x2), xRight = Math.Max(x1, x2);
            renderParams.Graphics.DrawLine(Pens.DodgerBlue, xLeft + AvatarCircleRadius, y, xRight - AvatarCircleRadius, y);
        }

        public static void DrawHoldLine(RenderParams renderParams, Note startNote, Note endNote) {
            DrawSimpleLine(renderParams, startNote, endNote, Pens.Yellow);
        }

        public static void DrawFlickLine(RenderParams renderParams, Note startNote, Note endNote) {
            DrawSimpleLine(renderParams, startNote, endNote, Pens.OliveDrab);
        }

        public static void DrawSimpleLine(RenderParams renderParams, Note startNote, Note endNote, Pen pen) {
            var graphics = renderParams.Graphics;
            var now = renderParams.Now;
            OnStageStatus s1 = GetNoteOnStageStatus(startNote, now), s2 = GetNoteOnStageStatus(endNote, now);
            if (s1 != OnStageStatus.OnStage && s2 != OnStageStatus.OnStage && s1 == s2) {
                return;
            }
            float x1, x2, y1, y2;
            GetNotePairPositions(renderParams, startNote, endNote, out x1, out x2, out y1, out y2);
            graphics.DrawLine(pen, x1, y1, x2, y2);
        }

        public static void DrawSimpleNote(RenderParams renderParams, Note note) {
            if (!IsNoteOnStage(note, renderParams.Now)) {
                return;
            }
            var graphics = renderParams.Graphics;
            float x = GetNoteXPosition(renderParams, note), y = GetNoteYPosition(renderParams, note);
            graphics.FillEllipse(Brushes.DarkMagenta, x - AvatarCircleRadius, y - AvatarCircleRadius, AvatarCircleDiameter, AvatarCircleDiameter);
            if (note.IsFlick) {
                switch (note.FlickType) {
                    case NoteStatus.FlickLeft:
                        graphics.FillPie(Brushes.DarkOrange, x - AvatarCircleRadius, y - AvatarCircleRadius, AvatarCircleDiameter, AvatarCircleDiameter, 135, 90);
                        break;
                    case NoteStatus.FlickRight:
                        graphics.FillPie(Brushes.DarkOrange, x - AvatarCircleRadius, y - AvatarCircleRadius, AvatarCircleDiameter, AvatarCircleDiameter, -45, 90);
                        break;
                }
            }
        }

        public static void GetNotePairPositions(RenderParams renderParams, Note note1, Note note2, out float x1, out float x2, out float y1, out float y2) {
            var now = renderParams.Now;
            var clientSize = renderParams.ClientSize;
            if (IsNotePassed(note1, now)) {
                x1 = GetXByNotePosition(clientSize, note1.FinishPosition);
                y1 = GetAvatarYPosition(clientSize);
            } else if (IsNoteComing(note1, now)) {
                x1 = GetXByNotePosition(clientSize, renderParams.IsPreview ? note1.StartPosition : note1.FinishPosition);
                y1 = GetBirthYPosition(clientSize);
            } else {
                x1 = GetNoteXPosition(renderParams, note1);
                y1 = GetNoteYPosition(renderParams, note1);
            }
            if (IsNotePassed(note2, now)) {
                x2 = GetXByNotePosition(clientSize, note2.FinishPosition);
                y2 = GetAvatarYPosition(clientSize);
            } else if (IsNoteComing(note2, now)) {
                x2 = GetXByNotePosition(clientSize, renderParams.IsPreview ? note2.StartPosition : note2.FinishPosition);
                y2 = GetBirthYPosition(clientSize);
            } else {
                x2 = GetNoteXPosition(renderParams, note2);
                y2 = GetNoteYPosition(renderParams, note2);
            }
        }

        public static float GetNoteXPosition(RenderParams renderParams, Note note) {
            var timeRemaining = note.HitTiming - renderParams.Now;
            var clientSize = renderParams.ClientSize;
            var endPos = AvatarCenterXPositions[(int)note.FinishPosition - 1] * clientSize.Width;
            if (renderParams.IsPreview) {
                var startPos = AvatarCenterXPositions[(int)note.StartPosition - 1] * clientSize.Width;
                return endPos - (endPos - startPos) * (float)timeRemaining / FutureTimeWindow;
            } else {
                return endPos;
            }
        }

        public static float GetNoteYPosition(RenderParams renderParams, Note note) {
            var timeRemaining = note.HitTiming - renderParams.Now;
            var clientSize = renderParams.ClientSize;
            float ceiling = FutureNoteCeiling * clientSize.Height,
                baseLine = BaseLineYPosition * clientSize.Height;
            return baseLine - (baseLine - ceiling) * (float)timeRemaining / FutureTimeWindow;
        }

        public static float GetAvatarXPosition(Size clientSize, NotePosition position) {
            return clientSize.Width * AvatarCenterXPositions[(int)position - 1];
        }

        public static float GetAvatarYPosition(Size clientSize) {
            return clientSize.Height * BaseLineYPosition;
        }

        public static float GetXByNotePosition(Size clientSize, NotePosition position) {
            return clientSize.Width * AvatarCenterXPositions[(int)position - 1];
        }

        public static float GetBirthYPosition(Size clientSize) {
            return clientSize.Height * FutureNoteCeiling;
        }

        public static OnStageStatus GetNoteOnStageStatus(Note note, double now) {
            if (note.HitTiming < now) {
                return OnStageStatus.Passed;
            }
            if (note.HitTiming > now + FutureTimeWindow) {
                return OnStageStatus.Upcoming;
            }
            return OnStageStatus.OnStage;
        }

        public static bool IsNoteOnStage(Note note, double now) {
            return now <= note.HitTiming && note.HitTiming <= now + FutureTimeWindow;
        }

        public static bool IsNotePassed(Note note, double now) {
            return note.HitTiming < now;
        }

        public static bool IsNoteComing(Note note, double now) {
            return note.HitTiming > now + FutureTimeWindow;
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
