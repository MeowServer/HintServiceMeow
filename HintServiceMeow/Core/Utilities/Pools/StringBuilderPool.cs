namespace HintServiceMeow.Core.Utilities.Pools
{
    using System.Collections.Concurrent;
    using System.Text;
    using HintServiceMeow.Core.Interface;

    internal class StringBuilderPool : IPool<StringBuilder>
    {
        private readonly ConcurrentBag<StringBuilder> stringBuilderQueue = [];

        public static StringBuilderPool Instance { get; } = new();

        public StringBuilder Rent()
        {
            if (stringBuilderQueue.TryTake(out StringBuilder sb))
            {
                return sb;
            }

            return new StringBuilder(2000);
        }

        public void Return(StringBuilder? sb)
        {
            if (sb is null)
                return;

            sb.Clear();

            stringBuilderQueue.Add(sb);
        }

        public string ToStringReturn(StringBuilder sb)
        {
            string str = sb.ToString();
            Return(sb);
            return str;
        }
    }
}
