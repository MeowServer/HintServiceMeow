using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HintServiceMeow
{
    public class HintConfig
    {
        [Description("The ID of the hint")]
        public string Id { get; set; } = null;

        [Description("The priority of the hint. Higher priority means the hint is less likely to be covered by other hint." +
                     "\nAvailable: Highest, High, Medium, Low, Lowest")]
        public HintPriority Priority { get; set; } = HintPriority.Medium;

        public HintPositionConfig Position { get; set; } = new HintPositionConfig();

        public HintContentConfig Content { get; set; } = new HintContentConfig();

        public HintConfig()
        {
        }
    }

    public class HintPositionConfig
    {
        [Description("The alignment of the hint. " +
                     "\nAvailable: Left, Right, Center ")]
        public HintAlignment Alignment { get; set; } = HintAlignment.Center;

        [Description("The initial YCoordinate of the hint, default is 500")]
        public int YCoordinate { get; set; } = 500;

        [Description("The initial size of the font, default is 20")]
        public int FontSize { get; set; } = 20;

        public static implicit operator HintConfig(HintPositionConfig v)
        {
            return new HintConfig()
            {
                Position = v,
            };
        }
    }

    public class HintContentConfig
    {
        [Description("The initial message displayed. The message migth be changed by the plugin")]
        public string Message { get; set; } = "This is a default message";

        [Description("To hide the hint or not.")]
        public bool Hide { get; set; } = true;

        public static implicit operator HintConfig(HintContentConfig v)
        {
            return new HintConfig()
            {
                Content = v,
            };
        }
    }
}
