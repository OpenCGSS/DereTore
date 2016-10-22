using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using DereTore.Applications.StarlightDirector.Entities;
using DereTore.Applications.StarlightDirector.Entities.Extensions;
using DereTore.Applications.StarlightDirector.Extensions;

namespace DereTore.Applications.StarlightDirector.UI.Controls {
    public partial class ScoreEditor {

        private void BarLayer_OnSizeChanged(object sender, SizeChangedEventArgs e) {
            ResizeBars();
            RepositionBars();
        }

        private void NoteLayer_OnSizeChanged(object sender, SizeChangedEventArgs e) {
            RepositionNotes();
            // We have to be sure the lines reposition after the notes did.
            RepositionLineLayer();
        }

        private void ScoreBar_MouseUp(object sender, MouseButtonEventArgs e) {
            var scoreBar = (ScoreBar)sender;
            var hitTestInfo = scoreBar.HitTest(e.GetPosition(scoreBar));
            if (e.ChangedButton == MouseButton.Left) {
                if (hitTestInfo.IsValid) {
                    AddScoreNote(scoreBar, hitTestInfo, null);
                } else {
                    UnselectAllScoreNotes();
                    SelectScoreBar(scoreBar);
                }
                e.Handled = true;
            } else {
                if (HasSelectedScoreNotes) {
                    UnselectAllScoreNotes();
                    e.Handled = true;
                }
            }
        }

        private void ScoreBar_MouseDown(object sender, MouseButtonEventArgs e) {
            e.Handled = true;
        }

        private void ScoreNote_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton != MouseButton.Left) {
                e.Handled = true;
                return;
            }
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
            if (e.ChangedButton != MouseButton.Left) {
                UnselectAllScoreNotes();
                e.Handled = true;
                return;
            }
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
                    if (ns.Bar == ne.Bar && ns.IndexInGrid == ne.IndexInGrid && !ns.IsSync && !ne.IsSync) {
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
            if (e.ChangedButton != MouseButton.Left) {
                e.Handled = true;
                return;
            }
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
            if (e.ChangedButton == MouseButton.Right) {
                var myPosition = e.GetPosition(this);
                var result = VisualTreeHelper.HitTest(this, myPosition);
                var element = result.VisualHit as FrameworkElement;
                element = element?.FindVisualParent<ScoreBar>();
                if (element != null) {
                    var hitTestInfo = ((ScoreBar)element).HitTest(e.GetPosition(element));
                    LastHitTestInfo = hitTestInfo;
                } else {
                    ScoreBar s = null;
                    foreach (var scoreBar in ScoreBars) {
                        var top = Canvas.GetTop(scoreBar);
                        var bottom = top + scoreBar.ActualHeight;
                        if (top <= myPosition.Y && myPosition.Y < bottom) {
                            s = scoreBar;
                            break;
                        }
                    }
                    if (s != null) {
                        var hitTestInfo = s.HitTest(e.GetPosition(s));
                        LastHitTestInfo = hitTestInfo;
                    }
                }
            }
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
