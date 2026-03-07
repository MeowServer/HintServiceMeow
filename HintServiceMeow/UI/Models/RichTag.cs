namespace HintServiceMeow.UI.Models
{
    using HintServiceMeow.UI.Extension;

    public abstract class RichTag
    {
        /// <summary>
        /// Gets the open tag of this tag.
        /// For example, if this tag is a color tag with color red, this property should return <c>&lt;color=red&gt;</c>.
        /// </summary>
        public abstract string OpenTag { get; }

        /// <summary>
        /// Gets the close tag of this tag. If this tag does not have a close tag, this property should return an empty string.
        /// </summary>
        public abstract string CloseTag { get; }

        /// <summary>
        /// Gets priority of this tag. Higher priority tags will be applied before lower priority tags.
        /// </summary>
        internal abstract int Priority { get; }

        public static string operator /(string text, RichTag tag)
        {
            return text.UseRichTag(tag);
        }

        /// <summary>
        /// Apply this tag to the given text.
        /// </summary>
        /// <param name="str">The string the tag will be applied to.</param>
        /// <returns>A string with tag applied.</returns>
        public virtual string Apply(string str)
        {
            return OpenTag + str + CloseTag;
        }
    }
}
