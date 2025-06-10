using HintServiceMeow.Core.Interface;
using System.Collections.Concurrent;
using System.Text;

namespace HintServiceMeow.Core.Utilities.Pools
{
    internal class StringBuilderPool : IPool<StringBuilder>
    {
        public static StringBuilderPool Instance { get; } = new();

        private readonly ConcurrentBag<StringBuilder> StringBuilderQueue = new();

        public StringBuilder Rent()
        {
            if (StringBuilderQueue.TryTake(out StringBuilder sb))
            {
                return sb;
            }

            return new StringBuilder(2000);
        }

        public void Return(StringBuilder sb)
        {
            sb.Clear();

            StringBuilderQueue.Add(sb);
        }

        public string ToStringReturn(StringBuilder sb)
        {
            string str = sb.ToString();
            Return(sb);
            return str;
        }
    }
}
