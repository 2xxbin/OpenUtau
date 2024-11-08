using System.Collections;
using Melanchall.DryWetMidi.MusicTheory;
using OpenUtau.Api;
using OpenUtau.Core;
using OpenUtau.Core.Ustx;

namespace OpenUtau.Plugin.Builtin {
  	[Phonemizer("Korean CMPX Phonemizer", "KO CMPX", "2xxbin", language:"KO")]
  	public class KoreanCMPXPhonemizer : BaseKoreanPhonemizer {
        public override void SetSinger(USinger singer) {
            if(this.singer == singer || singer == null || singer.SingerType != USingerType.Classic) { return; }
            // todo : yaml 읽는 코드 추가
            this.singer = singer;
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