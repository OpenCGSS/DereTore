using DereTore.Applications.StarlightDirector.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DereTore.Applications.StarlightDirector.Entities {
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public sealed class IDGenerators {

        public IDGenerators() {
            NoteIDGenerator = new IntegerIDGenerator(1);
            FlickGroupIDGenerator = new IntegerIDGenerator(1);
            CompiledNoteIDGenerator = new IntegerIDGenerator(1);
            CompiledFlickGroupIDGenerator = new IntegerIDGenerator(1);
        }

        public IntegerIDGenerator NoteIDGenerator { get; private set; }
        public IntegerIDGenerator CompiledNoteIDGenerator { get; private set; }
        public IntegerIDGenerator FlickGroupIDGenerator { get; private set; }
        public IntegerIDGenerator CompiledFlickGroupIDGenerator { get; private set; }

        public void ResetOriginal() {
            NoteIDGenerator.Reset();
            FlickGroupIDGenerator.Reset();
        }

        public void ResetCompiled() {
            CompiledNoteIDGenerator.Reset();
            CompiledFlickGroupIDGenerator.Reset();
        }

    }
}
