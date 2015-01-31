using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SilkDialectLearningAudioLayer
{
    public  class AudioManager : IAudioManager
    {
        #region Properties

        public TimeSpan SoundLength { get; set; }
        public AudioStatus State { get; protected set; }
        private WaveOut audioOutput;
        private Mp3FileReader mp3Reader;

        #endregion

        public async Task Play(byte[] audio, int playFrom = 0)
        {
            if (audio == null)
            {
                throw new InvalidOperationException("Phrase.Sound is not initialized yet.");
            }
            PrepareAudio(audio);
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

        public async Task StopPlaying(byte[] audio)
        {
            if (audio == null)
            {
                throw new InvalidOperationException("Phrase.Sound is not initialized yet.");
            }
            PrepareAudio(audio);
            await Task.Run(() =>
            {
                if (audioOutput.PlaybackState != PlaybackState.Stopped)
                {
                    audioOutput.Stop();
                    State = AudioStatus.Stopped;
                }
            });
        }

        private void PrepareAudio(byte[] audio)
        {
            try
            {
                mp3Reader = new Mp3FileReader(Helper.ByteArrayToStream(audio));
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
        private class Helper
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
