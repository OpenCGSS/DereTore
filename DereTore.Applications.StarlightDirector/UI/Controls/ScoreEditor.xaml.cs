using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using DereTore.Applications.StarlightDirector.Entities;
using DereTore.Applications.StarlightDirector.Entities.Extensions;
using DereTore.Applications.StarlightDirector.Extensions;

namespace DereTore.Applications.StarlightDirector.UI.Controls {
    /// <summary>
    /// ScoreEditor.xaml 的交互逻辑
    /// </summary>
    public partial class ScoreEditor {

        private void FrameLayer_OnSizeChanged(object sender, SizeChangedEventArgs e) {
            var path = FrameLayer.Children[0] as Path;
            Debug.Assert(path != null, "path != null");
            var rectGeom = path.Data as RectangleGeometry;
            Debug.Assert(rectGeom != null, "rectGeom != null");
            var rect = new Rect(new Point(), e.NewSize);
            rectGeom.Rect = rect;
        }

        private void BarLayer_OnSizeChanged(object sender, SizeChangedEventArgs e) {
            ResizeBars();
            RepositionBars();
        }

        private void AvatarLayer_OnSizeChanged(object sender, SizeChangedEventArgs e) {
            RepositionAvatars();
            RepositionAvatarLine();
        }

        private void NoteLayer_OnSizeChanged(object sender, SizeChangedEventArgs e) {
            RepositionNotes();
            // We have to be sure the lines reposition after the notes did.
            RepositionLineLayer();
        }

        private void WorkingAreaClip_OnSizeChanged(object sender, SizeChangedEventArgs e) {
            var clip = WorkingAreaClip.Clip as RectangleGeometry;
            if (clip == null) {
                clip = new RectangleGeometry();
                WorkingAreaClip.Clip = clip;
            }
            var rect = new Rect();
            rect.Y = e.NewSize.Height * BaseLineYPosition;
            rect.Height = e.NewSize.Height - rect.Y - rect.Y;
            rect.Width = e.NewSize.Width;
            clip.Rect = rect;
            var definition = WorkingAreaUpperHalf.RowDefinitions[0];
            Debug.Assert(definition != null, "definition != null");
            definition.Height = new GridLength(rect.Y);
        }

        private void ScoreEditor_OnMouseWheel(object sender, MouseWheelEventArgs e) {
            // Zooming
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) {
                if (e.Delta > 0) {
                    ZoomIn();
                } else {
                    ZoomOut();
                }
                return;
            }

            // Normal scrolling
            var large = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            var up = e.Delta > 0;
            if (large) {
                if (up) {
                    ScrollUpLarge();
                } else {
                    ScrollDownLarge();
                }
            } else {
                if (up) {
                    ScrollUpSmall();
                } else {
                    ScrollDownSmall();
                }
            }
        }

        private void ScoreBar_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var scoreBar = (ScoreBar)sender;
            var hitTestInfo = scoreBar.HitTest(e.GetPosition(scoreBar));
            AddScoreNote(scoreBar, hitTestInfo, null);
            e.Handled = true;
        }

        private void ScoreBar_ScoreBarHitTest(object sender, ScoreBarHitTestEventArgs e) {
            Debug.Print($"row: {e.Info.Row}, col: {e.Info.Column}");
        }

        private void ScoreBar_MouseDown(object sender, MouseButtonEventArgs e) {
            SelectScoreBar(sender as ScoreBar);
            UnselectAllScoreNotes();
            e.Handled = true;
        }

        private void ScoreNote_MouseDown(object sender, MouseButtonEventArgs e) {
            var scoreNote = (ScoreNote)sender;
            if (scoreNote.IsSelected) {
                scoreNote.IsSelected = EditMode != EditMode.Select;
            } else {
                scoreNote.IsSelected = true;
            }
            if (scoreNote.IsSelected && EditMode != EditMode.Select && EditMode != EditMode.Clear) {
                switch (EditMode) {
                    case EditMode.Relations:
                        EditingLine.Stroke = LineLayer.RelationBrush;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(EditMode));
                }
                EditingLine.X1 = EditingLine.X2 = scoreNote.X;
                EditingLine.Y1 = EditingLine.Y2 = scoreNote.Y;
                _editingLineAbsoluteStartPoint = new Point(scoreNote.X, scoreNote.Y - ScrollOffset);
                EditingLine.Visibility = Visibility.Visible;
            }
            var note = scoreNote.Note;
            var barIndex = note.Bar.Index;
            var row = note.IndexInGrid;
            var column = note.IndexInTrack;
            Debug.Print($"Note @ bar#{barIndex}, row={row}, column={column}");
            DraggingStartNote = scoreNote;
            // Prevent broadcasting this event to ScoreEditor.
            e.Handled = true;
        }

        private void ScoreNote_MouseUp(object sender, MouseButtonEventArgs e) {
            DraggingEndNote = sender as ScoreNote;
            Debug.Assert(DraggingEndNote != null, "DraggingEndNote != null");
            if (DraggingStartNote != null && DraggingEndNote != null) {
                var mode = EditMode;
                if (mode == EditMode.Select) {
                    return;
                }
                var start = DraggingStartNote;
                var end = DraggingEndNote;
                var ns = start.Note;
                var ne = end.Note;
                if (mode == EditMode.Clear) {
                    ns.Reset();
                    LineLayer.NoteRelations.RemoveAll(start);
                    if (!start.Equals(end)) {
                        ne.Reset();
                        LineLayer.NoteRelations.RemoveAll(end);
                    }
                    LineLayer.InvalidateVisual();
                    Project.IsChanged = true;
                } else if (!start.Equals(end)) {
                    if (LineLayer.NoteRelations.ContainsPair(start, end)) {
                        MessageBox.Show(Application.Current.FindResource<string>(App.ResourceKeys.NoteRelationAlreadyExistsPrompt), App.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return;
                    }
                    if (mode != EditMode.Relations) {

                        throw new ArgumentOutOfRangeException(nameof(mode));
                    }
                    var first = ns < ne ? ns : ne;
                    if (ns.Bar == ne.Bar && ns.IndexInGrid == ne.IndexInGrid) {
                        // sync
                        Note.ConnectSync(ns, ne);
                        LineLayer.NoteRelations.Add(start, end, NoteRelation.Sync);
                        LineLayer.InvalidateVisual();
                    } else if (ns.FinishPosition != ne.FinishPosition && (ns.Bar != ne.Bar || ns.IndexInGrid != ne.IndexInGrid) && (!ns.IsHoldStart && !ne.IsHoldStart)) {
                        // flick
                        var second = first.Equals(ns) ? ne : ns;
                        if (first.HasNextFlick || second.HasPrevFlick) {
                            MessageBox.Show(Application.Current.FindResource<string>(App.ResourceKeys.FlickRelationIsFullPrompt), App.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            return;
                        }
                        Note.ConnectFlick(first, second);
                        LineLayer.NoteRelations.Add(start, end, NoteRelation.Flick);
                        LineLayer.InvalidateVisual();
                    } else if (ns.FinishPosition == ne.FinishPosition && !ns.IsHold && !ne.IsHold && !first.IsFlick) {
                        // hold
                        var anyObstacles = Score.Notes.AnyNoteBetween(ns, ne);
                        if (anyObstacles) {
                            MessageBox.Show(Application.Current.FindResource<string>(App.ResourceKeys.InvalidHoldCreationPrompt), App.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            return;
                        }
                        Note.ConnectHold(ns, ne);
                        LineLayer.NoteRelations.Add(start, end, NoteRelation.Hold);
                        LineLayer.InvalidateVisual();
                    } else {
                        DraggingStartNote = DraggingEndNote = null;
                        e.Handled = true;
                        return;
                    }

                    Project.IsChanged = true;
                    end.IsSelected = true;
                }
            }
            DraggingStartNote = DraggingEndNote = null;
            e.Handled = true;
        }

        private void ScoreNote_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var scoreNote = (ScoreNote)sender;
            var note = scoreNote.Note;
            if (note.IsHoldEnd) {
                if (note > note.HoldTarget) {
                    switch (note.FlickType) {
                        case NoteFlickType.Tap:
                            note.FlickType = NoteFlickType.FlickLeft;
                            Project.IsChanged = true;
                            break;
                        case NoteFlickType.FlickLeft:
                            note.FlickType = NoteFlickType.FlickRight;
                            Project.IsChanged = true;
                            break;
                        case NoteFlickType.FlickRight:
                            note.FlickType = NoteFlickType.Tap;
                            Project.IsChanged = true;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(note.FlickType));
                    }
                }
            }
            e.Handled = true;
        }

        private void ScoreEditor_OnMouseDown(object sender, MouseButtonEventArgs e) {
            UnselectAllScoreNotes();
            UnselectAllScoreBars();
            if (EditMode != EditMode.Select) {
                EditMode = EditMode.Select;
            }
        }

        private void ScoreEditor_OnMouseUp(object sender, MouseButtonEventArgs e) {
            DraggingStartNote = DraggingEndNote = null;
        }

        private void ScoreEditor_OnPreviewMouseDown(object sender, MouseButtonEventArgs e) {
            Focus();
        }

        private void ScoreEditor_OnPreviewMouseUp(object sender, MouseButtonEventArgs e) {
            EditingLine.Visibility = Visibility.Hidden;
            _editingLineAbsoluteStartPoint = null;
        }

        private void ScoreEditor_OnPreviewMouseMove(object sender, MouseEventArgs e) {
            if (EditingLine.Visibility == Visibility.Visible) {
                var position = e.GetPosition(EditingLineLayer);
                EditingLine.X2 = position.X;
                EditingLine.Y2 = position.Y;
            }
        }

    }
}
