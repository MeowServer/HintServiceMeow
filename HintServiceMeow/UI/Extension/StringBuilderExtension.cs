namespace HintServiceMeow.UI.Extension
{
    using System;
    using System.Text;
    using HintServiceMeow.UI.Models;

    internal static class StringBuilderExtension
    {
        public static StringBuilder? UseRichTag(this StringBuilder? sb, params RichTag[] tags)
        {
            if (sb is null || tags == null || tags.Length == 0)
                return sb;

            Array.Sort(tags, (a, b) =>
            {
                if (a == null && b == null)
                    return 0;
                if (a == null)
                    return 1;
                if (b == null)
                    return -1;

                return b.Priority.CompareTo(a.Priority);
            });

            for (int i = 0; i < tags.Length; i++)
            {
                sb.Insert(0, tags[i].OpenTag);
                sb.Append(tags[i].CloseTag);
            }

            return sb;
        }

        public static StringBuilder? AppendOpenTag(this StringBuilder sb, RichTag tag)
        {
            if (sb is null || tag == null)
                return sb;

            return sb.Append(tag.OpenTag);
        }

        public static StringBuilder? AppendCloseTag(this StringBuilder sb, RichTag tag)
        {
            if (sb is null || tag == null)
                return sb;
            return sb.Append(tag.CloseTag);
        }
    }
}
