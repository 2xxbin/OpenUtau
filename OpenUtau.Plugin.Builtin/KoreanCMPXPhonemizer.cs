using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

		public bool isUseInitalChangeCV = true;

		public bool isUseInitalCV = true;

		public bool isUseNGC = true;
		public bool isUseForeignConsonants = true;

		public Dictionary<string, object[]> initalC = new Dictionary<string, object[]>() {
			{"ㄴ", new object[]{"n", 25}},
			{"ㅁ", new object[]{"m", 25}},
			{"ㄹ", new object[]{"l", 25}},
			{"ㅅ", new object[]{"s", 100}},
			{"ㅆ", new object[]{"ss", 160}},
		};

		public Dictionary<string, string> initalChangeCV = new Dictionary<string, string>() {
			{"ㄱ", "k"},
			{"ㄷ", "t"},
			{"ㅂ", "p"},
		};

		public Dictionary<string, string> initalCV = new Dictionary<string, string>() {
			{"ㅎ", "h"},
		};

		public string[] firstForeignConsonants = {"f", "v", "z", "th", "rr", "RR"};

		public string[] endPhoneme = {"R", "H"}; 

		public Dictionary<string, int> semiVowelLength = new Dictionary<string, int>() {
			{"Y", 50},
			{"W", 50},
			{"eu", 50},
		};

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

		private Phoneme[] AddPhoneme(Phoneme[] phonemes, params Phoneme[] addPhonemes) {
			return phonemes.Concat(addPhonemes).ToArray();
		}

		private bool isNeedSemiVowel(string[] lyric) {
			if (Config.middleDiphthongVowels.ContainsKey(lyric[1])) { return true; }
			return false;
		}

		private string getVowel(string[] lyric) {
			if (isNeedSemiVowel(lyric)) {
				return Config.middleDiphthongVowels[lyric[1]][2];
			} else {
				return Config.middleShortVowels[lyric[1]];
			}
		}
		
		private string GetSingleVowel(string vowel) {
			if(Config.middleDiphthongVowels.ContainsKey(vowel)) {
				vowel = Config.middleDiphthongVowels[vowel][3];
			} else {
				vowel = Config.middleShortVowels[vowel];
			}

			return vowel;
		}

		private Result ConvertForCMPX(Note[] notes, string[] prevLyric, string[] thisLyric, string[] nextLyric, Note? nextNeighbour) {
			Note note = notes[0];
			Phoneme[] phonemes = new Phoneme[] {};
			int totalDuration = notes.Sum(n => n.duration);

			bool isNeedV = thisLyric[0] == "ㅇ" && prevLyric[2] == " ";
			bool isNeedVsV = thisLyric[2] == " " && nextLyric[0] == "ㅇ" && Config.middleDiphthongVowels.ContainsKey(nextLyric[1]);
			bool isNeedCV = !isNeedV;
			var vowel = getVowel(thisLyric);
			
			// 어두 에일리어스 구현
			if (prevLyric[0] == "null") {
				var phoneme = "";
				var position = 0;

				if (Config.isUseInitalC && Config.initalC.ContainsKey(thisLyric[0])) { // - C
					phoneme = $"- {Config.initalC[thisLyric[0]][0]}";
					position = - int.Parse((string)Config.initalC[thisLyric[0]][1]);
				} else if (thisLyric[0] == "ㅇ" && Config.middleDiphthongVowels.ContainsKey(thisLyric[1])) { // - SV
					phoneme = $"- {Config.middleDiphthongVowels[thisLyric[1]][2]}";
					position = -Config.semiVowelLength[Config.middleDiphthongVowels[thisLyric[1]][2]];
				} else if (thisLyric[0] == "ㅇ" && Config.middleShortVowels.ContainsKey(thisLyric[1])) { // - V
					phoneme = $"- {Config.middleShortVowels[thisLyric[1]]}";
					isNeedCV = false;
				} else if (Config.isUseInitalCV && Config.initalCV.ContainsKey(thisLyric[0])) { // - CV
					phoneme = $"- {Config.initalCV[thisLyric[0]]}{vowel}";
					isNeedCV = false;
				} else if (Config.isUseInitalChangeCV && Config.initalChangeCV.ContainsKey(thisLyric[0])) { // CV / 단, 어두에 올 경우 자음 변화
					phoneme = $"{Config.initalChangeCV[thisLyric[0]]}{vowel}";
					isNeedCV = false;
				}

				phonemes = AddPhoneme(phonemes, new Phoneme { phoneme = FindInOto(phoneme, note), position = position });
			}

			// CV 구현
			if (isNeedCV) {
				// CV 추가, 단 반모음이라면 반모음 행만 추가됨.
				// 가 -> ga / 갸 -> gY
				var phoneme = "";
				var position = 0;
				var consonant = Config.firstConsonants[thisLyric[0]];
				if (Config.isUseInitalC && Config.initalC.ContainsKey(thisLyric[0])) {
					consonant = (string)Config.initalC[thisLyric[0]][0];
				}
				phoneme = $"{consonant}{vowel}";
				phonemes = AddPhoneme(phonemes, new Phoneme { phoneme = FindInOto(phoneme, note), position = position });
				
				if (isNeedSemiVowel(thisLyric)) { // 만약 반모음이라면
					// 맞춰서 이중모음 추가
					// 포지션은 설정한 이중모음 길이만큼 밀림
					phoneme = $"{Config.middleDiphthongVowels[thisLyric[1]][1]}";
					position = Config.semiVowelLength[Config.middleDiphthongVowels[thisLyric[1]][2]];
					phonemes = AddPhoneme(phonemes, new Phoneme { phoneme = FindInOto(phoneme, note), position = position });
				}
			} else if (isNeedV) { // VV 구현
				var phoneme = "";

				// 만약 이중모음이라면
				if (Config.middleDiphthongVowels.ContainsKey(thisLyric[1])) {
					phoneme = Config.middleDiphthongVowels[thisLyric[1]][1];
				} else {
					phoneme = $"{GetSingleVowel(prevLyric[1])} {Config.middleShortVowels[thisLyric[1]]}";
				}
				phonemes = AddPhoneme(phonemes, new Phoneme { phoneme = FindInOto(phoneme, note) });
			}

			// 받침 구현
			if (thisLyric[2] != " ") {
				var singleVowel = GetSingleVowel(thisLyric[1]);
				var lastConsonant = Config.lastConsonants[thisLyric[2]];

				var lastConsonantPhoneme = $"_{singleVowel}{lastConsonant[0].ToUpper()}";
				var lastConsonantPosition = totalDuration - Math.Min(totalDuration / 3, 120);
				
				var CBNNVowelPhoneme = $"_{singleVowel}{lastConsonant[1]}";
				var CBNNVowelPosition = 50;

				if(isNeedSemiVowel(thisLyric)) {
					CBNNVowelPosition += Config.semiVowelLength[Config.middleDiphthongVowels[thisLyric[1]][2]];
				}

				phonemes = AddPhoneme(phonemes, new Phoneme { phoneme = FindInOto(CBNNVowelPhoneme, note), position = CBNNVowelPosition } , new Phoneme { phoneme = FindInOto(lastConsonantPhoneme, note), position = lastConsonantPosition });
			}


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

			try {
				return ConvertForCMPX(notes, prevLyric, thisLyric, nextLyric, nextNeighbour);
			} catch (Exception e) {
				Log.Error(e, $"Render Phoneme Error");
				return new Result() {
					phonemes = new Phoneme[] {
						new Phoneme { phoneme = FindInOto(note.lyric, note) }
					}
				};
			}
        }
    }
}