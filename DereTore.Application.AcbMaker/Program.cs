using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using DereTore.ACB.Serialization;
using DereTore.Application.AcbMaker.Cgss;

namespace DereTore.Application.AcbMaker {
    internal static class Program {

        private static void Main(string[] args) {
            var header = GetFullTable();
            var table = new[] { header };
            var serializer = new AcbSerializer();
            using (var fs = File.Open("sample.acb", FileMode.Create, FileAccess.Write)) {
                serializer.Serialize(table, fs);
            }
        }

        private static HeaderTable GetFullTable() {
            var cue = new[] {
                new CueTable {
                    CueId = 0,
                    ReferenceType = 3,
                    ReferenceIndex = 0,
                    UserData = string.Empty,
                    WorkSize = 0,
                    AisacControlMap = null,
                    Length = 126171,
                    NumAisacControlMaps = 0,
                    HeaderVisibility = 1
               }
            };
            var cueName = new[] {
                new CueNameTable {
                    CueIndex = 0,
                    CueName = "song_1001"
                }
            };
            var waveform = new[] {
                new WaveformTable {
                    Id = 0,
                    EncodeType = 2,
                    Streaming = 0,
                    NumChannels = 2,
                    LoopFlag = 1,
                    SamplingRate = 22050,
                    NumSamples = 2782079,
                    ExtensionData = ushort.MaxValue
               }
            };
            var synth = new[] {
                new SynthTable {
                    Type = 0,
                    VoiceLimitGroupName = string.Empty,
                    CommandIndex = ushort.MaxValue,
                    ReferenceItems = new byte[4],
                    LocalAisacs = null,
                    GlobalAisacStartIndex = ushort.MaxValue,
                    GlobalAisacNumRefs = 0,
                    ControlWorkArea1 = 0,
                    ControlWorkArea2 = 0,
                    TrackValues = null,
                    ParameterPallet = ushort.MaxValue,
                    ActionTrackStartIndex = ushort.MaxValue,
                    NumActionTracks = 0
                }
            };
            var command = new[] {
                new CommandTable {
                    Command = new byte[0x0a]
                },
                new CommandTable {
                    Command = new byte[0x10]
                }
            };
            var track = new[] {
                new TrackTable {
                    EventIndex = 0,
                    CommandIndex = ushort.MaxValue,
                    LocalAisacs = null,
                    GlobalAisacStartIndex = ushort.MaxValue,
                    GlobalAisacNumRefs = 0,
                    ParameterPallet = ushort.MaxValue,
                    TargetType = 0,
                    TargetName = string.Empty,
                    TargetId = uint.MaxValue,
                    TargetAcbName = string.Empty,
                    Scope = 0,
                    TargetTrackNo = ushort.MaxValue
                }
            };
            var sequence = new[] {
                new SequenceTable {
                    PlaybackRatio = 100,
                    NumTracks = 1,
                    TrackIndex = new byte[] {0x01, 0x00},
                    CommandIndex = 1,
                    LocalAisacs = null,
                    GlobalAisacStartIndex = ushort.MaxValue,
                    GlobalAisacNumRefs = 0,
                    ParameterPallet = ushort.MaxValue,
                    ActionTrackStartIndex = ushort.MaxValue,
                    NumActionTracks = 0,
                    TrackValues = null,
                    Type = 0,
                    ControlWorkArea1 = 0,
                    ControlWorkArea2 = 0
                }
            };
            var acfReference = new[] {
                new AcfReferenceTable {
                    Type = 3,
                    Name = "system",
                    Name2 = null,
                    Id = 0
                },
                new AcfReferenceTable {
                    Name = "bgm",
                    Id = 3
                }
            };
            var acbGuid = Guid.NewGuid();
            var header = new HeaderTable {
                FileIdentifier = 0,
                Size = 0,
                Version = 19071232,
                Type = 0,
                Target = 0,
                AcfMd5Hash = new byte[0x10] { 0x0e, 0xf7, 0x50, 0x41, 0x55, 0x0d, 0xda, 0xda, 0x89, 0xd0, 0x4e, 0x74, 0xbc, 0x91, 0x32, 0x2c },
                CategoryExtension = 0,
                CueTable = cue,
                CueNameTable = cueName,
                WaveformTable = waveform,
                AisacTable = null,
                GraphTable = null,
                AisacNameTable = null,
                GlobalAisacReferenceTable = null,
                SynthTable = synth,
                CommandTable = command,
                TrackTable = track,
                SequenceTable = sequence,
                AisacControlNameTable = null,
                AutoModulationTable = null,
                StreamAwbTocWorkOld = null,
                AwbFile = null,
                VersionString = "\nACB Format/PC ver.1.23.01 Build:\n",
                CueLimitWorkTable = null,
                NumCueLimitListWorks = 0,
                NumCueLimitNodeWorks = 0,
                AcbGuid = acbGuid.ToByteArray(),
                StreamAwbHash = new byte[0x10],
                StreamAwbTocWork_Old = null,
                AcbVolume = 1f,
                StringValueTable = null,
                OutsideLinkTable = null,
                BlockSequenceTable = null,
                BlockTable = null,
                Name = "song_1001",
                CharacterEncodingType = 0,
                EventTable = null,
                ActionTrackTable = null,
                AcfReferenceTable = acfReference,
                WaveformExtensionDataTable = null,
                BeatSyncInfoTable = null,
                CuePriorityType = byte.MaxValue,
                NumCueLimit = 0,
                PaddingArea = null,
                StreamAwbTocWork = null,
                StreamAwbAfs2Header = null
            };
            return header;
        }

    }
}
