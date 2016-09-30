using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using DereTore.Applications.StarlightDirector.Components;
using DereTore.Applications.StarlightDirector.Entities;
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
            RepositionLines();
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
            double change;
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) {
                change = LargeChange;
            } else {
                change = SmallChange;
            }
            var targetOffset = ScrollOffset + change * (e.Delta < 0 ? -1 : 1);
            targetOffset = -MathHelper.Clamp(-targetOffset, MinimumScrollOffset, MaximumScrollOffset);
            if (!targetOffset.Equals(ScrollOffset)) {
                ScrollOffset = targetOffset;
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
                    case EditMode.Sync:
                        EditingLine.Stroke = LineLayer.SyncRelationBrush;
                        break;
                    case EditMode.Flick:
                        EditingLine.Stroke = LineLayer.FlickRelationBrush;
                        break;
                    case EditMode.Hold:
                        EditingLine.Stroke = LineLayer.HoldRelationBrush;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(EditMode));
                }
                EditingLine.X1 = EditingLine.X2 = scoreNote.X;
                EditingLine.Y1 = EditingLine.Y2 = scoreNote.Y;
                EditingLine.Visibility = Visibility.Visible;
            }
            var note = scoreNote.Note;
            var barIndex = note.Bar.Index;
            var row = note.PositionInGrid;
            var column = (int)note.FinishPosition - 1;
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
                    if (!DraggingStartNote.Equals(DraggingEndNote)) {
                        ne.Reset();
                        LineLayer.NoteRelations.RemoveAll(end);
                    }
                    LineLayer.InvalidateVisual();
                    Project.IsChanged = true;
                } else if (!DraggingStartNote.Equals(DraggingEndNote)) {
                    if (LineLayer.NoteRelations.ContainsPair(start, end)) {
                        MessageBox.Show(Application.Current.FindResource<string>(App.ResourceKeys.NoteRelationAlreadyExists), App.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return;
                    }
                    switch (mode) {
                        case EditMode.Sync:
                            if (ns.Bar != ne.Bar || ns.PositionInGrid != ne.PositionInGrid) {
                                MessageBox.Show(Application.Current.FindResource<string>(App.ResourceKeys.InvalidSyncCreation), App.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                return;
                            }
                            ns.SyncTarget = ne;
                            ne.SyncTarget = ns;
                            LineLayer.NoteRelations.Add(start, end, NoteRelation.Sync);
                            LineLayer.InvalidateVisual();
                            break;
                        case EditMode.Flick:
                            if ((ns.Bar == ne.Bar && ns.PositionInGrid == ne.PositionInGrid) ||
                                ns.FinishPosition == ne.FinishPosition || ns.StartPosition == ne.StartPosition) {
                                MessageBox.Show(Application.Current.FindResource<string>(App.ResourceKeys.InvalidFlickCreation), App.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                return;
                            }
                            var first = ns < ne ? ns : ne;
                            var second = first.Equals(ns) ? ne : ns;
                            first.NextFlickNote = second;
                            second.PrevFlickNote = first;
                            LineLayer.NoteRelations.Add(start, end, NoteRelation.Flick);
                            LineLayer.InvalidateVisual();
                            break;
                        case EditMode.Hold:
                            if (ns.FinishPosition != ne.FinishPosition || ns.IsHoldStart || ne.IsHoldStart) {
                                MessageBox.Show(Application.Current.FindResource<string>(App.ResourceKeys.InvalidHoldCreation), App.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                return;
                            }
                            var anyObstacles = ScoreNotes.Any(scoreNote => {
                                var n = scoreNote.Note;
                                if (n.Equals(ns) || n.Equals(ne)) {
                                    return false;
                                }
                                if (n.FinishPosition == ns.FinishPosition && ns.Bar.Index <= n.Bar.Index && n.Bar.Index <= ne.Bar.Index) {
                                    if (ns.Bar.Index == ne.Bar.Index) {
                                        return ns.PositionInGrid <= n.PositionInGrid && n.PositionInGrid <= ne.PositionInGrid;
                                    } else {
                                        if (ns.Bar.Index == n.Bar.Index) {
                                            return ns.PositionInGrid <= n.PositionInGrid;
                                        } else if (ne.Bar.Index == n.Bar.Index) {
                                            return n.PositionInGrid <= ne.PositionInGrid;
                                        } else {
                                            return true;
                                        }
                                    }
                                } else {
                                    return false;
                                }
                            });
                            if (anyObstacles) {
                                MessageBox.Show(Application.Current.FindResource<string>(App.ResourceKeys.InvalidHoldCreation), App.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                return;
                            }
                            ns.HoldTarget = ne;
                            ne.HoldTarget = ns;
                            LineLayer.NoteRelations.Add(start, end, NoteRelation.Hold);
                            LineLayer.InvalidateVisual();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(mode));
                    }
                    Project.IsChanged = true;
                    DraggingEndNote.IsSelected = true;
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
