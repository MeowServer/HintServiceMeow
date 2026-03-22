namespace HintServiceMeow.Core.Utilities.Pools
{
    using HintServiceMeow.Core.Models.Hints;

    internal class HintPool : PoolBase<Hint>
    {
        public static HintPool Instance { get; } = new();

        protected override void Reset(Hint hint) => hint.ResetFields();

        protected override Hint Create() => new Hint();
    }
}
