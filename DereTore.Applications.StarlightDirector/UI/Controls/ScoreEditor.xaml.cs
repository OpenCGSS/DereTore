using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DereTore.Applications.StarlightDirector.UI.Controls {
    /// <summary>
    /// ScoreEditor.xaml 的交互逻辑
    /// </summary>
    partial class ScoreEditor {

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

        private void LineLayer_OnSizeChanged(object sender, SizeChangedEventArgs e) {
            //throw new NotImplementedException();
        }

        private void AvatarLayer_OnSizeChanged(object sender, SizeChangedEventArgs e) {
            RepositionAvatars();
            RepositionAvatarLine();
        }

        private void NoteLayer_OnSizeChanged(object sender, SizeChangedEventArgs e) {
            ResizeNotes();
            RepositionNotes();
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
            AddNote(scoreBar, hitTestInfo);
            e.Handled = true;
        }

        private void ScoreBar_ScoreBarHitTest(object sender, ScoreBarHitTestEventArgs e) {
            Debug.Print($"row: {e.Info.Row}, col: {e.Info.Column}");
        }

        private void ScoreBar_MouseDown(object sender, MouseButtonEventArgs e) {
            var previousSelected = GetSelectedScoreBar();
            if (previousSelected != null) {
                previousSelected.IsSelected = false;
            }
            var current = sender as ScoreBar;
            if (current != null) {
                current.IsSelected = true;
            }
            e.Handled = true;
        }

        private void ScoreNote_MouseDown(object sender, MouseButtonEventArgs e) {
            var scoreNote = (ScoreNote)sender;
            scoreNote.IsSelected = !scoreNote.IsSelected;
            var note = scoreNote.Note;
            var barIndex = note.Bar.Index;
            var row = note.PositionInGrid;
            var column = (int)note.FinishPosition - 1;
            Debug.Print($"Note @ bar#{barIndex}, row={row}, column={column}");
            // Prevent broadcasting this event to ScoreEditor.
            e.Handled = true;
        }

        private void ScoreNote_MouseUp(object sender, MouseButtonEventArgs e) {
        }

        private void ScoreNote_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
        }

        private void ScoreEditor_OnMouseDown(object sender, MouseButtonEventArgs e) {
            UnselectAllScoreNotes();
            UnselectAllScoreBars();
        }

        private void ScoreEditor_OnPreviewMouseDown(object sender, MouseButtonEventArgs e) {
            Focus();
            if (EditMode != EditMode.Select) {
                EditMode = EditMode.Select;
            }
        }

    }
}
