using System.ComponentModel;

namespace HintServiceMeow
{
    internal class PlayerUIConfig
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
    }
}
