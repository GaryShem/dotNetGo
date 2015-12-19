using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dotNetGo
{
    static class Utils
    {
        public static void Shuffle<T>(this IList<T> list, Random r)
        {
            int n = list.Count;
            while (n > 1)
            {
                int k;
                lock(r) k = (r.Next(0, n) % n);
                n--;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
