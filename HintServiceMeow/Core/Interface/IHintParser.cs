namespace HintServiceMeow.Core.Interface
{
    using HintServiceMeow.Core.Models;
    using HintServiceMeow.Core.Models.Arguments;

    /// <summary>
    /// Defines a parser that converts a <see cref="HintCollection"/> into a displayable message string.
    /// </summary>
    public interface IHintParser
    {
        /// <summary>
        /// Parses the specified hint collection into a formatted message string.
        /// </summary>
        /// <param name="arg">The hints and arguments of hint parser.</param>
        /// <returns>A formatted string representing the hints for display.</returns>
        HintParserResult ParseToMessage(HintParserArgument arg);
    }
}
