using System;
using System.Collections.Concurrent;
using System.Text;

namespace HintServiceMeow.Core.Utilities.Pools
{
    internal static class StringBuilderPool
    {
        private const int DefaultSmallSize = 500;
        private const int DefaultLargeSize = 5000;

        private static readonly ConcurrentQueue<StringBuilder> SmallStringBuilderQueue = new ConcurrentQueue<StringBuilder>();
        private static readonly ConcurrentQueue<StringBuilder> LargeStringBuilderQueue = new ConcurrentQueue<StringBuilder>();

        public static StringBuilder Rent(int capacity = DefaultSmallSize)
        {
            StringBuilder sb;

            if(capacity > DefaultLargeSize)
            {
                if (LargeStringBuilderQueue.TryDequeue(out sb))
                {
                    if (sb.Capacity < capacity)
                        sb.Capacity = capacity;

                    return sb;
                }
            }
            else if (SmallStringBuilderQueue.TryDequeue(out sb))
            {
                if (sb.Capacity < capacity)
                    sb.Capacity = capacity;

                return sb;
            }

            return new StringBuilder(Math.Max(capacity, DefaultSmallSize));
        }

        public static void Return(StringBuilder sb)
        {
            sb.Clear();

            if(sb.Capacity > DefaultLargeSize)
                LargeStringBuilderQueue.Enqueue(sb);
            else
                SmallStringBuilderQueue.Enqueue(sb);
        }

        public static string ToStringReturn(StringBuilder sb)
        {
            var str = sb.ToString();
            Return(sb);
            return str;
        }
    }
}
