using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using SilkDialectLearning.DAL;

namespace SilkDialectLearning.AudioLayer
{
    public class AudioManager : IAudioManager
    {
        #region Fields

        private TimeSpan soundLength; 
        private AudioStatus State { get; set; }
        private WaveOut audioOutput;
        private Mp3FileReader mp3Reader;

        private IWavePlayer player;
        private IWaveProvider provider;


        #endregion

        public async Task Play(Phrase phrase, int playFrom = 0)
        {
            if (phrase == null)
            {
                throw new InvalidOperationException("Phrase.Sound is not initialized yet.");
            }
            PrepareAudio(phrase);
            phrase.SoundLength = soundLength;
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

        public async Task PlayAsync(Phrase phrase)
        {
            player = new WasapiOut(AudioClientShareMode.Shared, 200);
            using (Stream stream = Helper.ByteArrayToStream(phrase.Sound))
            {
                await Task.Run(() =>
                {
                    provider = new Mp3FileReader(stream);
                });
                player.Init(provider);
                player.Play();
            }
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
                soundLength = mp3Reader.TotalTime; //Sets for phrase's Sound Length after preparing audio
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
