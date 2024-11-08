using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using OpenUtau.Api;
using OpenUtau.Core;
using OpenUtau.Core.Ustx;
using Serilog;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.EventEmitters;

namespace OpenUtau.Plugin.Builtin {
	[Serializable]
	public class KoreanCMPXConfigYAML {
		public bool isUseInitalC = true;

		public bool isUseInitalChangeC = true;

		public bool isUseInitalCV = true;

		public bool isUseNGC = true;
		public bool isUseForeignConsonants = true;

		public Dictionary<string, string> initalC = new Dictionary<string, string>() {
			{"ㄴ", "n"},
			{"ㅁ", "m"},
			{"ㄹ", "l"},
			{"ㅅ", "s"},
			{"ㅆ", "ss"},
		};

		public Dictionary<string, string> initalChangeC = new Dictionary<string, string>() {
			{"ㄱ", "k"},
			{"ㄷ", "t"},
			{"ㅂ", "p"},
		};

		public Dictionary<string, string> initalCV = new Dictionary<string, string>() {
			{"ㅎ", "h"},
		};

		public string[] firstForeignConsonants = {"f", "v", "z", "th", "rr", "RR"};

		public string[] endPhoneme = {"R", "H"}; 

		public Dictionary<string, string> firstConsonants = new Dictionary<string, string>() {
			{"ㄱ", "g"},
			{"ㄴ", "n"},
			{"ㄷ", "d"},
			{"ㄹ", "r"},
			{"ㅁ", "m"},
			{"ㅂ", "b"},
			{"ㅅ", "s"},
			{"ㅇ", ""},
			{"ㅈ", "j"},
			{"ㅊ", "ch"},
			{"ㅋ", "k"},
			{"ㅌ", "t"},
			{"ㅍ", "p"},
			{"ㅎ", "h"},
			{"ㄲ", "kk"},
			{"ㄸ", "tt"},
			{"ㅃ", "pp"},
			{"ㅆ", "ss"},
			{"ㅉ", "jj"},
			{"null", ""},
		};

		public Dictionary<string, string> middleShortVowels = new Dictionary<string, string>() {
			{"ㅏ", "a"},
			{"ㅣ", "i"},
			{"ㅜ", "u"},
			{"ㅐ", "e"},
			{"ㅔ", "e"},
			{"ㅗ", "o"},
			{"ㅡ", "eu"},
			{"ㅓ", "eo"},
		};

		public Dictionary<string, string[]> middleDiphthongVowels = new Dictionary<string, string[]>() {
			{"ㅑ", new string[4]{"ya", "_Ya", "Y", "a"}},
			{"ㅕ", new string[4]{"yeo", "_Yeo", "Y", "eo"}},
			{"ㅛ", new string[4]{"yo", "_Yo", "Y", "o"}},
			{"ㅠ", new string[4]{"yu", "_Yu", "Y", "u"}},
			{"ㅖ", new string[4]{"ye", "_Ye", "Y", "e"}},
			{"ㅒ", new string[4]{"ye", "_Ye", "Y", "e"}},
			{"ㅘ", new string[4]{"wa", "_Wa", "W", "wa"}},
			{"ㅟ", new string[4]{"wi", "_Wi", "W", "i"}},
			{"ㅝ", new string[4]{"wo", "_Wo", "W", "eo"}},
			{"ㅙ", new string[4]{"we", "_We", "W", "e"}},
			{"ㅚ", new string[4]{"we", "_We", "W", "e"}},
			{"ㅞ", new string[4]{"we", "_We", "W", "e"}},
			{"ㅢ", new string[4]{"ui", "_ui", "eu", "i"}},
		};

		public Dictionary<string, string[]> lastConsonants = new Dictionary<string, string[]>() {
			{"ㄱ", new string[]{"k", ""}},
            {"ㄲ", new string[]{"k", ""}},
            {"ㄳ", new string[]{"k", ""}},
            {"ㄴ", new string[]{"n", "2"}},
            {"ㄵ", new string[]{"n", "2"}},
            {"ㄶ", new string[]{"n", "2"}},
            {"ㄷ", new string[]{"t", "1"}},
            {"ㄹ", new string[]{"l", "4"}},
            {"ㄺ", new string[]{"k", ""}},
            {"ㄻ", new string[]{"m", "1"}},
            {"ㄼ", new string[]{"l", "4"}},
            {"ㄽ", new string[]{"l", "4"}},
            {"ㄾ", new string[]{"l", "4"}},
            {"ㄿ", new string[]{"p", "1"}},
            {"ㅀ", new string[]{"l", "4"}},
            {"ㅁ", new string[]{"m", "1"}},
            {"ㅂ", new string[]{"p", "1"}},
            {"ㅄ", new string[]{"p", "1"}},
            {"ㅅ", new string[]{"t", "1"}},
            {"ㅆ", new string[]{"t", "1"}},
            {"ㅇ", new string[]{"ng", "3"}},
            {"ㅈ", new string[]{"t", "1"}},
            {"ㅊ", new string[]{"t", "1"}},
            {"ㅋ", new string[]{"k", ""}},
            {"ㅌ", new string[]{"t", "1"}},
            {"ㅍ", new string[]{"p", "1"}},
            {"ㅎ", new string[]{"t", "1"}},
            {" ", new string[]{"", ""}},
            {"null", new string[]{"", ""}},
		};
	}

  	[Phonemizer("Korean CMPX Phonemizer", "KO CMPX", "2xxbin", language:"KO")]
  	public class KoreanCMPXPhonemizer : BaseKoreanPhonemizer {
		private KoreanCMPXConfigYAML Config;

		private void CreateConfigFile(string path) {
			Log.Information("Cannot Find 'kocmpx.yaml', creating new one...");
			var serializer = new SerializerBuilder().WithEventEmitter(next => new FlowStyleIntegerSequences(next)).Build();
			File.WriteAllText(path, serializer.Serialize(new KoreanCMPXConfigYAML{}));
			Log.Information("New 'kocmpx.yaml' created with default values.");
		}

		private void LoadConfigYaml(string path) {
			try {
				var deserializer = new DeserializerBuilder().Build();
				this.Config = deserializer.Deserialize<KoreanCMPXConfigYAML>(File.ReadAllText(path));
			} catch (Exception e) {
				Log.Error(e, $"Fail to local 'kocmpx.yaml' (path: '{path}')");
				try {
					CreateConfigFile(path);
				} catch(Exception e2) {
					Log.Error(e2, "Fail to create 'kocmpx.yaml'");
				}
			}
		}

        public override void SetSinger(USinger singer) {
            if(this.singer == singer || singer == null || singer.SingerType != USingerType.Classic) { return; }
            
			LoadConfigYaml(Path.Join(singer.Location, "kocmpx.yaml"));
			if(this.Config == null) {
				Log.Error("Failed to load 'kocmpx.yaml', using default settings.");
				this.Config = new KoreanCMPXConfigYAML();
			}

            this.singer = singer;
        }

		private class FlowStyleIntegerSequences : ChainedEventEmitter {
			public FlowStyleIntegerSequences(IEventEmitter nextEmitter)
				: base(nextEmitter) { }

			public override void Emit(SequenceStartEventInfo eventInfo, IEmitter emitter) {
				eventInfo = new SequenceStartEventInfo(eventInfo.Source) {
					Style = YamlDotNet.Core.Events.SequenceStyle.Flow
				};

				nextEmitter.Emit(eventInfo, emitter);
			}
		}

		private string FindInOto(string phoneme, Note note) {
			return BaseKoreanPhonemizer.FindInOto(this.singer, phoneme, note, false);
		}

		private Result ConvertForCMPX(Note[] notes, string[] prevLyric, string[] thisLyric, string[] nextLyric, Note? nextNeighbour) {
			Phoneme[] phonemes = new Phoneme[] {};
			
			return new Result() {
				phonemes = phonemes
			};
		}

        public override Result ConvertPhonemes(Note[] notes, Note? prev, Note? next, Note? prevNeighbour, Note? nextNeighbour, Note[] prevNeighbours) {
            var note = notes[0];

			var lyrics = KoreanPhonemizerUtil.Variate(prevNeighbour, note, nextNeighbour);
			string[] prevLyric = new string[] {
				(string) lyrics[0],
				(string) lyrics[1],
				(string) lyrics[2],
			};
			string[] thisLyric = new string[] {
				(string) lyrics[3],
				(string) lyrics[4],
				(string) lyrics[5],
			};
			string[] nextLyric = new string[] {
				(string) lyrics[6],
				(string) lyrics[7],
				(string) lyrics[8],
			};

			if(thisLyric[0] == "null") {
				return new Result() {
					phonemes = new Phoneme[] {
						new Phoneme { phoneme = FindInOto(note.lyric, note) }
					}
				};
			}

			return ConvertForCMPX(notes, prevLyric, thisLyric, nextLyric, nextNeighbour);
        }
    }
}