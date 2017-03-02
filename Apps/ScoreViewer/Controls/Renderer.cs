using System.Collections.Generic;
using DereTore.Apps.ScoreViewer.Model;
using DereTore.Common;

namespace DereTore.Apps.ScoreViewer.Controls {
    public sealed class Renderer {

        public bool IsRendering {
            get {
                lock (_renderingSyncObject) {
                    return _isRendering;
                }
            }
            private set {
                lock (_renderingSyncObject) {
                    _isRendering = value;
                }
            }
        }

        public static Renderer Instance => SingletonManager<Renderer>.Instance;

        public void RenderFrame(RenderParams renderParams, IList<Note> notes) {
            IsRendering = true;
            RenderHelper.DrawCeilingLine(renderParams);
            RenderHelper.DrawAvatars(renderParams);
            if (notes == null) {
                IsRendering = false;
                return;
            }
            int startIndex, endIndex;
            //GetVisibleNotes(now, scores, out startIndex, out endIndex);
            startIndex = 0;
            endIndex = notes.Count - 1;
            RenderHelper.DrawNotes(renderParams, notes, startIndex, endIndex);
            IsRendering = false;
        }

        private Renderer() {
            _renderingSyncObject = new object();
        }

        private readonly object _renderingSyncObject;
        private bool _isRendering;

    }
}
