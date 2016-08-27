using System.Drawing;

namespace DereTore.Application.ScoreEditor.Controls {
    public sealed class RenderParams {

        public RenderParams(Graphics graphics, Size clientSize, double now, bool isPreview) {
            Graphics = graphics;
            ClientSize = clientSize;
            Now = now;
            IsPreview = isPreview;
        }

        public Graphics Graphics { get; }

        public Size ClientSize { get; }

        public double Now { get; }

        public bool IsPreview { get; }

    }
}
