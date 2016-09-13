using DereTore.Applications.StarlightDirector.Components;

namespace DereTore.Applications.StarlightDirector.Entities {
    public sealed class CompiledScore {

        public CompiledScore(Score original) {
            Notes = new InternalList<CompiledNote>();
            Original = original;
        }

        public InternalList<CompiledNote> Notes { get; }

        public Score Original { get; }

    }
}
