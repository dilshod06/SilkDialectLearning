using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilkDialectLearningAudioLayer
{
    public interface IAudioManager
    {
        Task Play(byte[] audio, int playFrom = 0);
        Task StopPlaying(byte[] audio);
    }
}
