namespace HintServiceMeow.UI.Extension
{
    using System;
    using System.Text;
    using HintServiceMeow.UI.Models;

    public static class StringBuilderExtension
    {
        public static StringBuilder UseRichTag(this StringBuilder? sb, params RichTag[] tags)
        {
            if (sb is null)
                throw new ArgumentNullException(nameof(sb));
            if (tags is null)
                throw new ArgumentNullException(nameof(tags));

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

        public static StringBuilder AppendOpenTag(this StringBuilder sb, RichTag tag)
        {
            if (sb is null)
                throw new ArgumentNullException(nameof(sb));
            if (tag is null)
                throw new ArgumentNullException(nameof(tag));

            return sb.Append(tag.OpenTag);
        }

        public static StringBuilder AppendCloseTag(this StringBuilder sb, RichTag tag)
        {
            if (sb is null)
                throw new ArgumentNullException(nameof(sb));
            if (tag is null)
                throw new ArgumentNullException(nameof(tag));

            return sb.Append(tag.CloseTag);
        }

        public static StringBuilder AppendWithTag(this StringBuilder sb, string text, RichTag tag)
        {
            if (sb is null)
                throw new ArgumentNullException(nameof(sb));
            if (tag is null)
                throw new ArgumentNullException(nameof(tag));

            return sb.Append(tag.OpenTag).Append(text).Append(tag.CloseTag);
        }

        public static StringBuilder AppendWithTag(this StringBuilder sb, string text, params RichTag[] tags)
        {
            if (sb is null)
                throw new ArgumentNullException(nameof(sb));
            if (tags is null)
                throw new ArgumentNullException(nameof(tags));

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
                sb.Append(tags[i].OpenTag);
            }

            sb.Append(text);

            for (int i = tags.Length - 1; i >= 0; i--)
            {
                sb.Append(tags[i].CloseTag);
            }

            return sb;
        }
    }
}
