using System;
using SilkDialectLearning.DAL;

namespace SilkDialectLearning.BLL
{
    public class PracticeResult<T> : IDisposable where T : IEntity
    {
        public PracticeResult(T item)
        {
            Item = item;
            Status = PracticeItemStatus.Notasked;
        }

        public T Item { get; private set; }

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
}