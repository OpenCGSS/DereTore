using System.Reflection;

namespace DereTore.Applications.StarlightDirector {
    public sealed class ResourceKeys {

        public readonly string NeutralStrokeColor;
        public readonly string CuteStrokeColor;
        public readonly string CoolStrokeColor;
        public readonly string PassionStrokeColor;
        public readonly string NeutralStrokeBrush;
        public readonly string CuteStrokeBrush;
        public readonly string CoolStrokeBrush;
        public readonly string PassionStrokeBrush;
        public readonly string NeutralFillColor;
        public readonly string CuteFillColor;
        public readonly string CoolFillColor;
        public readonly string PassionFillColor;
        public readonly string NeutralFillBrush;
        public readonly string CuteFillBrush;
        public readonly string CoolFillBrush;
        public readonly string PassionFillBrush;

        public readonly string BarStrokeColor;
        public readonly string BarStrokeBrush;
        public readonly string BarStrokeStressColor;
        public readonly string BarStrokeStressBrush;

        public readonly string CardAvatar1;
        public readonly string CardAvatar2;
        public readonly string CardAvatar3;
        public readonly string CardAvatar4;
        public readonly string CardAvatar5;

        public readonly string ProjectFileFilter;
        public readonly string CsvFileFilter;
        public readonly string WaveFileFilter;
        public readonly string AcbFileFilter;
        public readonly string BdbFileFilter;
        public readonly string ProjectChangedPrompt;
        public readonly string PreviewNotImplemented;
        public readonly string ConfirmDeleteBar;
        public readonly string NoteRelationAlreadyExists;
        public readonly string InvalidSyncCreation;
        public readonly string InvalidFlickCreation;
        public readonly string InvalidHoldCreation;

        public readonly string SummaryTotalNotes;
        public readonly string SummaryTotalBars;
        public readonly string SummaryMusicFile;

        internal ResourceKeys() {
            var thisType = GetType();
            var fields = thisType.GetFields(BindingFlags.Public | BindingFlags.Instance);
            var stringType = typeof(string);
            foreach (var field in fields) {
                if (field.FieldType == stringType) {
                    field.SetValue(this, field.Name);
                }
            }
        }

    }
}
