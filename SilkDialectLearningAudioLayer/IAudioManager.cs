using SilkDialectLearningDAL;
using System.Threading.Tasks;

namespace SilkDialectLearningAudioLayer
{
    public interface IAudioManager
    {
        Task Play(Phrase phrase, int playFrom = 0);
        Task StopPlaying();
    }
}
