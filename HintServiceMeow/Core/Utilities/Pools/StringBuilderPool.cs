using System;
using System.Collections.Concurrent;
using System.Text;

namespace HintServiceMeow.Core.Utilities.Pools
{
    internal static class StringBuilderPool
    {
        private static readonly ConcurrentBag<StringBuilder> StringBuilderQueue = new ConcurrentBag<StringBuilder>();

        public static StringBuilder Rent(int capacity = 500)
        {
            StringBuilder sb;

            if (StringBuilderQueue.TryTake(out sb))
            {
                if (sb.Capacity < capacity)
                    sb.Capacity = capacity;

                return sb;
            }

            return new StringBuilder(Math.Max(capacity, 500));
        }

        public static void Return(StringBuilder sb)
        {
            sb.Clear();

            StringBuilderQueue.Add(sb);
        }

        public static string ToStringReturn(StringBuilder sb)
        {
            string str = sb.ToString();
            Return(sb);
            return str;
        }
    }
}
