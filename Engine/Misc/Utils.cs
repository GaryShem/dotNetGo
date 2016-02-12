using System.Collections.Generic;

namespace Engine.Misc
{
    static class Utils
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                var k = RandomGen.Next(0, n) % n;
                n--;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static void Shuffle<T>(this IList<T> list, int elementCount)
        {
            int n = elementCount;
            while (n > 1)
            {
                var k = RandomGen.Next(0, n) % n;
                n--;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
