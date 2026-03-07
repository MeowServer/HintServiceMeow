namespace HintServiceMeow.UI.Extension
{
    using System;
    using HintServiceMeow.UI.Models;

    public static class StringExtension
    {
        /// <summary>
        /// Applies one or more rich tags to the specified text, modifying the text according to each tag's priority.
        /// </summary>
        /// <remarks>If the tags array is null or contains no elements, the method returns the original
        /// text without modification. Null tags within the array are ignored and sorted to the end of the priority
        /// order.</remarks>
        /// <param name="text">The input text to which the rich tags will be applied.</param>
        /// <param name="tags">An array of rich tags to apply to the text. Tags are applied in descending order of priority, with higher
        /// priority tags applied first.</param>
        /// <returns>The modified text after applying the rich tags. Returns null if the input text is null. If no tags are
        /// provided, returns the original text unchanged.</returns>
        public static string UseTag(this string text, params RichTag[] tags)
        {
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
                text = tags[i].Apply(text);
            }

            return text;
        }

        /// <summary>
        /// Applies the specified rich tag to the given text and returns the formatted result.
        /// </summary>
        /// <remarks>Use this method to format or enhance text with a rich tag, such as for styling or
        /// markup purposes.</remarks>
        /// <param name="text">The input string to which the rich tag will be applied.</param>
        /// <param name="tag1">The rich tag to apply to the text. This parameter cannot be null.</param>
        /// <returns>A new string with the rich tag applied to the input text. If the input text is null or empty, or if the tag
        /// is null, the original text is returned.</returns>
        public static string UseTag(this string text, RichTag tag1)
        {
            return tag1.Apply(text);
        }
    }
}
