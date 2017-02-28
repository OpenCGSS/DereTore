using System.Windows;
using StarlightDirector.Entities;
using StarlightDirector.UI.Controls.Primitives;

namespace StarlightDirector.UI {
    public sealed class ScoreBarHitTestInfo {

        public ScoreBarHitTestInfo(ScoreBar scoreBar, Bar bar, Point hitPoint, int column, int row, bool isInNextBar, bool isValid) {
            ScoreBar = scoreBar;
            Bar = bar;
            HitPoint = hitPoint;
            Column = column;
            Row = row;
            IsInNextBar = isInNextBar;
            IsValid = isValid;
        }

        public ScoreBar ScoreBar { get; }
        public Bar Bar { get; }
        public int Column { get; }
        public int Row { get; }
        public Point HitPoint { get; }
        public bool IsInNextBar { get; }
        public bool IsValid { get; }

    }
}
