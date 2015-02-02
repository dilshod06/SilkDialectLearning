using NAudio.Wave;
using System;
using System.IO;
using System.Threading.Tasks;
using SilkDialectLearningDAL;

namespace SilkDialectLearningAudioLayer
{
    public class AudioManager : IAudioManager
    {
        #region Fields

        private static TimeSpan SoundLength { get; set; }
        private static AudioStatus State { get; set; }
        private static WaveOut audioOutput;
        private static Mp3FileReader mp3Reader;

        #endregion

        public async Task Play(Phrase phrase, int playFrom = 0)
        {
            if (phrase == null)
            {
                throw new InvalidOperationException("Phrase.Sound is not initialized yet.");
            }
            PrepareAudio(phrase);
            //Sets for phrase's Sound Length after preparing audio
            phrase.SoundLength = SoundLength;
            await Task.Run(() =>
            {
                if (audioOutput.PlaybackState == PlaybackState.Playing)
                {
                    audioOutput.Stop();
                }
                mp3Reader.CurrentTime = TimeSpan.FromMilliseconds(playFrom);
                audioOutput.Play();
                State = AudioStatus.Playing;
            });
        }

        public async Task StopPlaying()
        {
            await Task.Run(() =>
            {
                if (audioOutput.PlaybackState != PlaybackState.Stopped)
                {
                    audioOutput.Stop();
                    State = AudioStatus.Stopped;
                }
            });
        }

        private void PrepareAudio(Phrase phrase)
        {
            try
            {
                mp3Reader = new Mp3FileReader(Helper.ByteArrayToStream(phrase.Sound));
                SoundLength = mp3Reader.TotalTime;
                var wc = new WaveChannel32(mp3Reader);
                audioOutput = new WaveOut();
                audioOutput.Init(wc);
            }
            catch (Exception ex)
            {
                throw new Exception("There was a problem with preparing the audio file. Please see the inner exception for more details.", ex);
            }
        }

        #region Helper
        public enum AudioStatus
        {
            Playing,
            Stopped,
            Paused,
        }
        private static class Helper
        {
            public static Stream ByteArrayToStream(Byte[] bytes)
            {
                Stream str = new MemoryStream(bytes);
                return str;
            }
        }
        #endregion
    }
}
