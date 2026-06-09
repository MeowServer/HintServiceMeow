namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters
{
    using System;
    using global::Hints;
    using HintServiceMeow.Core.Interface;

    /// <summary>
    /// Wraps a native <see cref="HintParameter"/> so it can flow through HSM's parameter pipeline unchanged.
    /// Used by the compatibility adaptor to forward parameters carried by foreign (base game/plugin) hints.
    /// </summary>
    public class RawHintParameter : IParameter
    {
        private readonly HintParameter parameter;

        public RawHintParameter(HintParameter parameter)
        {
            this.parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
        }

        /// <inheritdoc/>
        public HintParameter GetScpslHintParameter()
        {
            return this.parameter;
        }
    }
}