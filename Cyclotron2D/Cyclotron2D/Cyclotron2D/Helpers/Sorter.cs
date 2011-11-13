using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cyclotron2D.Helpers
{
    public static class Sorter
    {

        public static void Sort<T>(List<T> list, IComparer<T> comparer)
        {
            for (int i = 1; i < list.Count; i++)
            {
                var tmp = list[i];
                int j = i - 1;
                while (j >=0 && comparer.Compare(list[j], tmp) > 0)
                {
                    list[j + 1] = list[j--];
                }
                list[j + 1] = tmp;
            }
        }
    }
}
