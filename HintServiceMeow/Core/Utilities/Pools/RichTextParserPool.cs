namespace HintServiceMeow.Core.Utilities.Pools
{
    using HintServiceMeow.Core.Utilities.Parser;

    internal class RichTextParserPool : PoolBase<RichTextParser>
    {
        public static RichTextParserPool Instance { get; } = new();

        protected override void Reset(RichTextParser parser)
        {
        }

        protected override RichTextParser Create() => new();
    }
}
