using SilkDialectLearning.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilkDialectLearning.BLL
{
    public class PracticeResult : IDisposable
    {
        public PracticeResult(IPlayable item)
        {
            Item = item;
            Status = PracticeItemStatus.Notasked;
        }

        public IPlayable Item { get; private set; }

        public PracticeItemStatus Status { get; set; }

        public int WrongAnswersCount { get; set; }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        ~PracticeResult()
        {
            Dispose();
        }
    }

    public enum PracticeItemStatus
    {
        Notasked = 0,
        Asking = 1,
        Asked = 2
    }

    public enum PracticeItemResult
    {
        Default = 0,
        Wrong = 1,
        Right = 2,
        Fixed = 4
    }

    public delegate void HighlightItemEventHandler(object sender, HighlightItemEventArgs e);

    public class HighlightItemEventArgs : EventArgs
    {
        public HighlightItemEventArgs(IHighlightable highlightableItem, PracticeItemResult practiceItemResult = PracticeItemResult.Default)
        {
            HighlightableItem = highlightableItem;
            PracticeItemResult = practiceItemResult;
        }
        public IHighlightable HighlightableItem { get; private set; }

        public PracticeItemResult PracticeItemResult { get; private set; }
    }

    public delegate void PraceticeFinishedEventHandler(object sender, PraceticeFinishedEventArgs e);

    public class PraceticeFinishedEventArgs : EventArgs
    {
        public string Message { get; set; }

        public PraceticeFinishedEventArgs(string message)
        {
            this.Message = message;
        }
    }
}
