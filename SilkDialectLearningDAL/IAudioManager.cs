using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilkDialectLearningDAL
{
    public interface IAudioManager
    {
        Task Play(Phrase phrase, int playFrom = 0);
        Task StopPlaying();
    }
}
