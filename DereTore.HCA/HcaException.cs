using System;

namespace DereTore.HCA {
    public sealed class HcaException : Exception {

        public HcaException(string message, ActionResult actionResult)
            : base(message) {
            _actionResult = actionResult;
        }

        public ActionResult ActionResult {
            get { return _actionResult; }
        }

        private readonly ActionResult _actionResult;

    }
}
