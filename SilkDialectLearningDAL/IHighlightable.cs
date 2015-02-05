
namespace SilkDialectLearning.DAL
{
    public interface IHighlightable
    {
        double XPos { get; set; }

        double YPos { get; set; }

        double Size { get; set; }

        bool IsRound { get; set; }
    }
}
