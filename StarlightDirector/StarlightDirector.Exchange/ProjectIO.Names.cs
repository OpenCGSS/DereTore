namespace StarlightDirector.Exchange {
    static partial class ProjectIO {

        private static class Names {

            public static readonly string Table_Main = "main";
            public static readonly string Table_ScoreSettings = "score_settings";
            public static readonly string Table_Metadata = "metadata";
            public static readonly string Table_NoteIDs = "note_ids";
            public static readonly string Table_Notes = "notes";
            public static readonly string Table_BarParams = "bar_params";
            public static readonly string Table_SpecialNotes = "special_notes";

            public static readonly string Field_MusicFileName = "music_file_name";
            public static readonly string Field_Version = "version";

            public static readonly string Field_GlobalBpm = "global_bpm";
            public static readonly string Field_StartTimeOffset = "start_time_offset";
            public static readonly string Field_GlobalGridPerSignature = "global_grid_per_signature";
            public static readonly string Field_GlobalSignature = "global_signature";

            public static readonly string Column_ID = "id";
            public static readonly string Column_Difficulty = "difficulty";
            public static readonly string Column_BarIndex = "bar_index";
            public static readonly string Column_IndexInGrid = "index_in_grid";
            public static readonly string Column_StartPosition = "start_position";
            public static readonly string Column_FinishPosition = "finish_position";
            public static readonly string Column_FlickType = "flick_type";
            public static readonly string Column_PrevFlickNoteID = "prev_flick_note_id";
            public static readonly string Column_NextFlickNoteID = "next_flick_note_id";
            public static readonly string Column_HoldTargetID = "hold_target_id";
            public static readonly string Column_GridPerSignature = "grid_per_signature";
            public static readonly string Column_Signature = "signature";
            public static readonly string Column_NoteType = "note_type";
            public static readonly string Column_ParamValues = "param_values";

            // Legacy. Do not use.
            public static readonly string Table_Scores = "scores";
            // Legacy. I made a spelling mistake in all v0.2 save files. :-(
            public static readonly string Field_Vesion = "vesion";
            // Lagacy. Saved for forward compatibility
            public static readonly string Column_SyncTargetID = "sync_target_id";

        }

    }
}
