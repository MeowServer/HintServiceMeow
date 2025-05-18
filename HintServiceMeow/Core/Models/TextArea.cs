namespace HintServiceMeow.Core.Models
{
    internal class TextArea
    {
        public float Top;

        public float Bottom;

        public float Left;

        public float Right;

        public bool HasIntersection(TextArea area)
        {
            return !(Left >= area.Right || area.Left >= Right || Top >= area.Bottom || area.Top >= Bottom);
        }
    }
}
