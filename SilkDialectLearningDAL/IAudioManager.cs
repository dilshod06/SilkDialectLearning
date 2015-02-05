using System.Threading.Tasks;

namespace SilkDialectLearning.DAL
{
    public interface IAudioManager
    {
        Task Play(Phrase phrase, int playFrom = 0);
        Task StopPlaying();
        Task PlayAsync(Phrase p);
    }
}
