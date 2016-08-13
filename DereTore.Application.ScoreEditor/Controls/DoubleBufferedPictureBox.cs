using System.Windows.Forms;

namespace DereTore.Application.ScoreEditor.Controls {
    public sealed class DoubleBufferedPictureBox : PictureBox {
        
        public DoubleBufferedPictureBox() {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.Opaque, false);
            UpdateStyles();
        }

    }
}
