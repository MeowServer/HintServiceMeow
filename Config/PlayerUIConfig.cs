using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HintServiceMeow.Config
{
    public class PlayerUIConfig
    {
        [Description("The default time to show for each type of common hint.\n" +
                     "Short means that the hint has on title but not decription")]
        public int ItemHintDisplayTime { get; set; } = 10;
        public int ShortItemHintDisplayTime { get; set; } = 5;

        public int MapHintDisplayTime { get; set; } = 10;
        public int ShortMapHintDisplayTime { get; set; } = 7;

        public int RoleHintDisplayTime { get; set; } = 15;
        public int ShortRoleHintDisplayTime { get; set; } = 5;

        public int OtherHintDisplayTime { get; set; } = 5;

        [Description("The default config to show for each type of common hint.\n" +
                     "Only config for alignment, font size, hint fiel, and priority are valid.")]
        public List<DynamicHintPositionConfig> ItemHints { get; set; } = new List<DynamicHintPositionConfig>()
        {
            new DynamicHintPositionConfig()
            {
                Alignment = HintAlignment.Center,
                FontSize = 25,
                TopYCoordinate = 450,
                BottomYCoordinate = 750
            },
            new DynamicHintPositionConfig()
            {
                Alignment = HintAlignment.Center,
                FontSize = 25,
                TopYCoordinate = 475,
                BottomYCoordinate = 775
            }
        };

        public List<DynamicHintPositionConfig> MapHints { get; set; } = new List<DynamicHintPositionConfig>()
        {
            new DynamicHintPositionConfig()
            {
                Alignment = HintAlignment.Right,
                FontSize = 30,
                TopYCoordinate = 0,
                BottomYCoordinate = 200
            },
            new DynamicHintPositionConfig()
            {
                Alignment = HintAlignment.Right,
                FontSize = 25,
                TopYCoordinate = 25,
                BottomYCoordinate = 225
            }
        };

        public List<DynamicHintPositionConfig> RoleHints { get; set; } = new List<DynamicHintPositionConfig>()
        {
            new DynamicHintPositionConfig()
            {
                Alignment = HintAlignment.Left,
                FontSize = 30,
                TopYCoordinate = 100,
                BottomYCoordinate = 500
            },
            new DynamicHintPositionConfig()
            {
                Alignment = HintAlignment.Left,
                FontSize = 25,
                TopYCoordinate = 130,
                BottomYCoordinate = 530
            },
            new DynamicHintPositionConfig()
            {
                Alignment = HintAlignment.Left,
                FontSize = 25,
                TopYCoordinate = 155,
                BottomYCoordinate = 555
            },
            new DynamicHintPositionConfig()
            {
                Alignment = HintAlignment.Left,
                FontSize = 25,
                TopYCoordinate = 180,
                BottomYCoordinate = 580
            }
        };

        public List<DynamicHintPositionConfig> OtherHints { get; set; } = new List<DynamicHintPositionConfig>()
        {
            new DynamicHintPositionConfig()
            {
                Alignment = HintAlignment.Center,
                FontSize = 20,
                TopYCoordinate = 520,
                BottomYCoordinate = 700
            },
            new DynamicHintPositionConfig()
            {
                Alignment = HintAlignment.Center,
                FontSize = 20,
                TopYCoordinate = 540,
                BottomYCoordinate = 700
            },
            new DynamicHintPositionConfig()
            {
                Alignment = HintAlignment.Center,
                FontSize = 20,
                TopYCoordinate = 560,
                BottomYCoordinate = 700
            },
            new DynamicHintPositionConfig()
            {
                Alignment = HintAlignment.Center,
                FontSize = 20,
                TopYCoordinate = 580,
                BottomYCoordinate = 700
            }
        };
    }
}
