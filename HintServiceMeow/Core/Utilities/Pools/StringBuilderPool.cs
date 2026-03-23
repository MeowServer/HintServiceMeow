namespace HintServiceMeow.Core.Utilities.Pools
{
    using System.Text;

    internal class StringBuilderPool : PoolBase<StringBuilder>
    {
        public static StringBuilderPool Instance { get; } = new();

        public string ToStringReturn(StringBuilder sb)
        {
            string str = sb.ToString();
            Return(sb);
            return str;
        }

        protected override void Reset(StringBuilder sb) => sb.Clear();

        protected override StringBuilder Create() => new StringBuilder(2000);
    }
}
