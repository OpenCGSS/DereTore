using System.Collections.Generic;
using System.IO;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using StarlightDirector.Entities.Serialization;

namespace StarlightDirector.Entities {
    public sealed class CompiledScore {

        public CompiledScore() {
            Notes = new InternalList<CompiledNote>();
        }

        public InternalList<CompiledNote> Notes { get; }

        public string GetCsvString() {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb)) {
                var config = new CsvConfiguration();
                config.RegisterClassMap<ScoreCsvMap>();
                config.HasHeaderRecord = true;
                config.TrimFields = false;
                using (var csv = new CsvWriter(writer, config)) {
                    var newList = new List<CompiledNote>(Notes);
                    csv.WriteRecords(newList);
                }
            }

            sb.Replace("\r\n", "\n");
            return sb.ToString();
        }

    }
}
