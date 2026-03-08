namespace HintServiceMeow.Core.Parameters
{
    using System;
    using global::Hints;
    using HintServiceMeow.Core.Interface;
    using Mirror;

    public class TimespanHintParameter : IHintParameter
    {
        public double SourceTime { get; set; }
        public string Format { get; set; }
        public bool Negate { get; set; }

        public TimespanHintParameter(double sourceTime, string format, bool negate)
        {
            this.SourceTime = sourceTime;
            this.Format = format;
            this.Negate = negate;
        }

        public TimespanHintParameter(DateTimeOffset sourceTime, string format, bool negate)
            : this((sourceTime - DateTimeOffset.UtcNow).TotalSeconds, format, negate)
        {
        }

        public static TimespanHintParameter FromOffset(double offset, string format, bool negate)
        {
            return new TimespanHintParameter(NetworkTime.time + offset, format, negate);
        }

        public static TimespanHintParameter FromOffset(TimeSpan offset, string format, bool negate)
        {
            return FromOffset(offset.TotalSeconds, format, negate);
        }

        public HintParameter GetScpslHintParameter()
        {
            return new global::Hints.TimespanHintParameter(this.SourceTime, this.Format, this.Negate);
        }
    }
}
