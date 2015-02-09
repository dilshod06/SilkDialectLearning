using System;
using System.Collections.Generic;

namespace SilkDialectLearning.BLL
{
    public static class Helper
    {
        static Helper()
        {
            Random = new Random(DateTime.Now.Millisecond);
        }

        private static readonly Random Random;

        public static List<T> MixItems<T>(List<T> items)
        {
            List<T> listItems = new List<T>();
            List<int> randomNumbers = new List<int>();
            for (int i = 1; i <= items.Count; i++)
            {
                int rnd;
                while (true)
                {
                    rnd = Random.Next(0, items.Count);
                    if (!randomNumbers.Contains(rnd))
                    {
                        randomNumbers.Add(rnd);
                        break;
                    }
                }
                listItems.Add(items[rnd]);
            }
            return listItems;
        }
    }
}
