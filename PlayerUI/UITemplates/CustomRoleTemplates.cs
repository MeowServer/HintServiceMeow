using Exiled.API.Features;
using Exiled.CustomRoles.API.Features;
using HintServiceMeow.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HintServiceMeow.UITemplates
{
    internal class CustomSCPTemplate : SCPTemplate
    {
        public override PlayerUITemplateType type { get; } = PlayerUITemplateType.CustomSCP;

        public CustomSCPTemplate(Player player) : base(player)
        {
        }

        protected override void UpdateTopBar()
        {
            string template = PluginConfig.instance.CustomSCPTemplate.TopBar;

            TopBar.message = PlayerUICommonTools.GetContent(template, player);
            TopBar.hide = false;
        }

        protected override void UpdateBottomBar()
        {
            string template = PluginConfig.instance.CustomSCPTemplate.BottomBar;

            BottomBar.message = PlayerUICommonTools.GetContent(template, player);
            BottomBar.hide = false;
        }
    }

    internal class CustomHumanTemplate : GeneralHumanTemplate
    {
        public override PlayerUITemplateType type { get; } = PlayerUITemplateType.CustomHuman;

        public CustomHumanTemplate(Player player) : base(player)
        {
        }

        protected override void UpdateTopBar()
        {
            string template = PluginConfig.instance.CustomHumanTemplate.TopBar;

            TopBar.message = PlayerUICommonTools.GetContent(template, player);
            TopBar.hide = false;
        }

        protected override void UpdateBottomBar()
        {
            string template = PluginConfig.instance.CustomHumanTemplate.BottomBar;

            BottomBar.message = PlayerUICommonTools.GetContent(template, player);
            BottomBar.hide = false;
        }
    }
}
