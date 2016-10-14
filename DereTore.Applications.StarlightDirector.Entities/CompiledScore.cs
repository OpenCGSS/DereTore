using System.Collections.Generic;
using System.IO;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using DereTore.Applications.StarlightDirector.Entities.Serialization;

namespace DereTore.Applications.StarlightDirector.Entities {
    public sealed class CompiledScore {

        public CompiledScore() {
            Notes = new InternalList<CompiledNote>();
        }

        public InternalList<CompiledNote> Notes { get; }

        public string GetCsvString() {
            var tempFileName = Path.GetTempFileName();
            using (var stream = File.Open(tempFileName, FileMode.Create, FileAccess.Write)) {
                using (var writer = new StreamWriter(stream, Encoding.UTF8)) {
                    var config = new CsvConfiguration();
                    config.RegisterClassMap<ScoreCsvMap>();
                    config.HasHeaderRecord = true;
                    config.TrimFields = false;
                    using (var csv = new CsvWriter(writer, config)) {
                        var newList = new List<CompiledNote>(Notes);
                        newList.Sort(CompiledNote.IDComparison);
                        csv.WriteRecords(newList);
                    }
                }
            }
            // BUG: WTF? Why can't all the data be written into a MemoryStream right after the csv.WriteRecords() call, but a FileStream?
            var text = File.ReadAllText(tempFileName, Encoding.UTF8);
            text = text.Replace("\r\n", "\n");
            File.Delete(tempFileName);
            return text;
        }

    }
}
