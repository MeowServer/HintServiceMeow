namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters
{
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    /// <summary>
    /// Represents a parameter that holds a long value.
    /// </summary>
    public class LongValueParameter : IParameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LongValueParameter"/> class.
        /// </summary>
        /// <param name="value">The long value.</param>
        public LongValueParameter(long value)
        {
            this.Value = value;
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public long Value { get; set; }

        /// <inheritdoc/>
        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.LongHintParameter(this.Value);
        }
    }
}
