using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompatibilityTest
{
    /*
    This shows using RueI to create a simple SCP list *EXILED* plugin.
    */
    using System.Text;
    using System.Drawing;

    using PlayerRoles;

    using Exiled.API.Features;
    using Exiled.API.Enums;
    using Exiled.API.Interfaces;

    using RueI.Extensions;
    using RueI.Elements;
    using RueI.Extensions.HintBuilding;
    using RueI.Displays;
    using RueI.Parsing.Enums;
    using System.Security.Policy;
    using UnityEngine;

    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;

        public bool Debug { get; set; } = false;
    }

    public class SCPList : Plugin<Config>
    {
        public override string Name => "SCPList";

        public static DynamicElement MyElement { get; } = new DynamicElement(GetContent, 900);

        public static AutoElement autoElement { get; } = AutoElement.Create(Roles.Scps, MyElement).UpdateEvery(TimeSpan.FromSeconds(0.7));

        public override void OnEnabled()
        {
            RueI.RueIMain.EnsureInit(); // make sure to always call this!
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            autoElement.Disable();
            base.OnDisabled();
        }

        private static string GetContent(DisplayCore core)
        {
            StringBuilder sb = new StringBuilder()
                .SetSize(65, MeasurementUnit.Percentage)
                .SetAlignment(HintBuilding.AlignStyle.Right);

            foreach (Player player in Player.Get(Side.Scp))
            {
                if (player.Role == RoleTypeId.Scp0492)
                {
                    continue;
                }

                string scpName = "???"; 
                switch(player.Role.Type)
                {
                    case RoleTypeId.Scp173:
                        scpName = "SCP-173";
                        break;
                    case RoleTypeId.Scp106:
                        scpName = "SCP-106";
                        break;
                    case RoleTypeId.Scp096:
                        scpName = "SCP-096";
                        break;
                    case RoleTypeId.Scp049:
                        scpName = "SCP-049";
                        break;
                    case RoleTypeId.Scp079:
                        scpName = "SCP-079";
                        break;
                    case RoleTypeId.Scp939:
                        scpName = "SCP-939";
                        break;
                    case RoleTypeId.Scp3114:
                        scpName = "SCP-3114";
                        break;
                };

                float health = player.Health;
                float max = player.MaxHealth;
                float percentage = Clamp(health / player.MaxHealth, 0, 1);

                // SCP-??? : 500/500 HP
                sb.SetBold()
                  .Append($"{scpName} : ")
                  .CloseBold()
                  .Append(health)
                  .Append('/')
                  .Append(max)
                  .Append(" HP")
                  .AddLinebreak();
                Log.Info(sb.ToString());
            }

            return sb.ToString();
        }

        private static float Clamp(float value, float min, float max) => Math.Max(Math.Min(value, min), max);
    }
}
