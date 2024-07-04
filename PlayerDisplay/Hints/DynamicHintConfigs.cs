using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HintServiceMeow
{
    [Description("Please be aware that the plugin might not be using all of the config.\n" +
                 "In other words, not all of the following configs are valid")]
    public class DynamicHintConfig
    {
        [Description("The ID of the hint")]
        public string Id { get; set; } = null;

        [Description("The priority of the hint. Higher priority means the hint is less likely to be covered by other hint.\n" +
                     "Available: Highest, High, Medium, Low, Lowest")]
        public HintPriority Priority { get; set; } = HintPriority.Medium;

        public DynamicHintPositionConfig Position { get; set; } = new DynamicHintPositionConfig();

        public DynamicHintContentConfig Content { get; set; } = new DynamicHintContentConfig();

        public DynamicHintConfig()
        {
        }
    }

    public class DynamicHintPositionConfig
    {
        [Description("The alignment of the hint.\n" +
                     "Available: Left, Right, Center ")]
        public HintAlignment Alignment { get; set; } = HintAlignment.Center;

        [Description("The size of the font, default is 20")]
        public int FontSize { get; set; } = 20;

        [Description("The Y-Coordiante of top boundary, default is 300")]
        public int TopYCoordinate { get; set; } = 300;

        [Description("The Y-Coordiante of bottom boundary, default is 700")]
        public int BottomYCoordinate { get; set; } = 700;

        public DynamicHintPositionConfig()
        {
        }

        public static implicit operator DynamicHintConfig(DynamicHintPositionConfig v)
        {
            return new DynamicHintConfig()
            {
                Position = v,
            };
        }
    }

    public class DynamicHintContentConfig
    {
        [Description("The initial message displayed. The message migth be changed by the plugin")]
        public string Message { get; set; } = "This is a default message";

        [Description("To hide the hint or not.")]
        public bool Hide { get; set; } = false;

        public DynamicHintContentConfig()
        {
        }

        public static implicit operator DynamicHintConfig(DynamicHintContentConfig v)
        {
            return new DynamicHintConfig()
            {
                Content = v,
            };
        }
    }
}
