using Exiled.API.Features;
using Exiled.CustomRoles.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HintServiceMeow.UITemplates
{
    public class CustomSCPTemplate : SCPTemplate
    {
        public override PlayerUITemplateType type { get; } = PlayerUITemplateType.CustomSCP;

        public CustomSCPTemplate(Player player) : base(player)
        {
        }

        protected override void UpdateTopBar()
        {
            string template = Config.instance.customSCPTemplate.TopBar;

            TopBar.message = UICommonTools.GetContent(template, player);
            TopBar.hide = false;
        }

        protected override void UpdateBottomBar()
        {
            string template = Config.instance.customSCPTemplate.BottomBar;

            BottomBar.message = UICommonTools.GetContent(template, player);
            BottomBar.hide = false;
        }
    }

    public class CustomHumanTemplate : GeneralHumanTemplate
    {
        public override PlayerUITemplateType type { get; } = PlayerUITemplateType.CustomHuman;

        public CustomHumanTemplate(Player player) : base(player)
        {
        }

        protected override void UpdateTopBar()
        {
            string template = Config.instance.customHumanTemplate.TopBar;

            TopBar.message = UICommonTools.GetContent(template, player);
            TopBar.hide = false;
        }

        protected override void UpdateBottomBar()
        {
            string template = Config.instance.customHumanTemplate.BottomBar;

            BottomBar.message = UICommonTools.GetContent(template, player);
            BottomBar.hide = false;
        }
    }
}
