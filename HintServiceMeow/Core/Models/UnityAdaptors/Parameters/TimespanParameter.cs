namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters
{
    using System;
    using global::Hints;
    using HintServiceMeow.Core.Interface;
    using Mirror;

    public class TimespanParameter : IParameter
    {
        public double SourceTime { get; set; }
        public string Format { get; set; }
        public bool Negate { get; set; }

        public TimespanParameter(double sourceTime, string format, bool negate)
        {
            this.SourceTime = sourceTime;
            this.Format = format;
            this.Negate = negate;
        }

        public TimespanParameter(DateTimeOffset sourceTime, string format, bool negate)
            : this((sourceTime - DateTimeOffset.UtcNow).TotalSeconds, format, negate)
        {
        }

        public static TimespanParameter FromOffset(double offset, string format, bool negate)
        {
            return new TimespanParameter(NetworkTime.time + offset, format, negate);
        }

        public static TimespanParameter FromOffset(TimeSpan offset, string format, bool negate)
        {
            return FromOffset(offset.TotalSeconds, format, negate);
        }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.TimespanHintParameter(this.SourceTime, this.Format, this.Negate);
        }
    }
}
