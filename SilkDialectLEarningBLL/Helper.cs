using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilkDialectLEarningBLL
{
    public class Helper
    {
        static Helper()
        {
            random = new Random(DateTime.Now.Millisecond);
        }

        public static Random random;

        public static List<T> MixItems<T>(List<T> items)
        {
            List<T> _items = new List<T>();
            List<int> randomNumbers = new List<int>();
            for (int i = 1; i <= items.Count; i++)
            {
                int rnd;
                while (true)
                {
                    rnd = random.Next(0, items.Count);
                    if (!randomNumbers.Contains(rnd))
                    {
                        randomNumbers.Add(rnd);
                        break;
                    }
                }
                _items.Add(items[rnd]);
            }
            return _items;
        }
    }
}
