﻿using System.Windows.Forms;

namespace DereTore.Apps.ScoreViewer.Controls {
    public class DoubleBufferedPictureBox : PictureBox {

        public DoubleBufferedPictureBox() {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.Opaque, true);
            UpdateStyles();
        }

    }
}
