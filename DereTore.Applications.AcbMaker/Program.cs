using System;
using System.IO;
using DereTore.ACB.Serialization;
using DereTore.Applications.AcbMaker.Cgss;
using DereTore.HCA;

namespace DereTore.Applications.AcbMaker {
    internal static class Program {

        private static int Main(string[] args) {
            if (args.Length < 2) {
                Console.WriteLine(HelpMessage);
                return -1;
            }

            var inputHcaFileName = args[0];
            var outputFileName = args[1];
            var songName = DefaultSongName;

            for (var i = 2; i < args.Length; ++i) {
                var arg = args[i];
                if (arg[0] == '-' || arg[0] == '/') {
                    switch (arg.Substring(1)) {
                        case "n":
                            if (i < args.Length - 1) {
                                songName = args[++i];
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            try {
                var header = GetFullTable(inputHcaFileName, songName);
                var table = new[] { header };
                var serializer = new AcbSerializer();
                using (var fs = File.Open(outputFileName, FileMode.Create, FileAccess.Write)) {
                    serializer.Serialize(table, fs);
                }
            } catch (Exception) {
                return -2;
            }
            return 0;
        }

        private static HeaderTable GetFullTable(string hcaFileName, string songName) {
            HcaInfo info;
            uint lengthInSamples;
            float lengthInSeconds;
            using (var fileStream = File.Open(hcaFileName, FileMode.Open, FileAccess.Read)) {
                var decoder = new OneWayHcaDecoder(fileStream);
                info = decoder.HcaInfo;
                lengthInSamples = decoder.LengthInSamples;
                lengthInSeconds = decoder.LengthInSeconds;
            }
            var cue = new[] {
                new CueTable {
                    CueId = 0,
                    ReferenceType = 3,
                    ReferenceIndex = 0,
                    UserData = string.Empty,
                    WorkSize = 0,
                    AisacControlMap = null,
                    Length = (uint)(lengthInSeconds * 1000),
                    NumAisacControlMaps = 0,
                    HeaderVisibility = 1
               }
            };
            var cueName = new[] {
                new CueNameTable {
                    CueIndex = 0,
                    CueName = songName
                }
            };
            var waveform = new[] {
                new WaveformTable {
                    Id = 0,
                    EncodeType = 2, // HCA
                    Streaming = 0,
                    NumChannels = (byte)info.ChannelCount,
                    LoopFlag = 1,
                    SamplingRate = (ushort)info.SamplingRate,
                    NumSamples = lengthInSamples,
                    ExtensionData = ushort.MaxValue
               }
            };
            var synth = new[] {
                new SynthTable {
                    Type = 0,
                    VoiceLimitGroupName = string.Empty,
                    CommandIndex = ushort.MaxValue,
                    ReferenceItems = new byte[] {0x00, 0x01, 0x00, 0x00},
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
                    Command = new byte[0x0a] {0x07, 0xd0, 0x04, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00 }
                },
                new CommandTable {
                    Command = new byte[0x10] {0x00, 0x41, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x57, 0x02, 0x00, 0x32 }
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
                    TrackIndex = new byte[] {0x00, 0x00}, // {0x01, 0x00}
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
            var hcaData = File.ReadAllBytes(hcaFileName);
            var header = new HeaderTable {
                FileIdentifier = 0,
                Size = 0,
                Version = 0x01230100,
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
                AwbFile = hcaData,
                VersionString = StandardAcbVersionString,
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
                Name = songName,
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

        private static readonly string StandardAcbVersionString = "\nACB Format/PC ver.1.23.01 Build:\n";
        private static readonly string DefaultSongName = "song_1001";

        private static readonly string HelpMessage = "Usage: AcbMaker.exe <HCA live music file> <output ACB> [-n <song name>]";

    }
}
