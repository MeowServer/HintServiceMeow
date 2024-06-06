using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Roles;
using Exiled.CustomRoles.API.Features;
using HintServiceMeow.Config;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HintServiceMeow
{
    public static class PlayerUICommonTools
    {
        /// <summary>
        /// This code return a value that indicate whether a player is a scp, this include custom roles that include "SCP" in their name
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool IsSCP(Player player)
        {
            foreach (CustomRole customRole in CustomRole.Registered)
            {
                if (customRole.Check(player) && customRole.Name.ToLower().Contains("scp"))//wtf am I doing....
                {
                    return true;
                }
            }

            return player.Role.Team == Team.SCPs;
        }

        public static bool IsCustomRole(Player player)
        {
            if (CustomRole.Registered.Any(x => x.Check(player)))
            {
                return true;
            }

            return false;
        }

        public static CustomRole GetCustomRole(Player player)
        {
            foreach (CustomRole customRole in CustomRole.Registered)
            {
                if (customRole.Check(player))
                {
                    return customRole;
                }
            }

            return null;
        }

        public static string GetArmorInfo(Player player)
        {
            if (player.CurrentArmor != null)
            {
                return PluginConfig.instance.GeneralConfig.ItemName[player.CurrentArmor.Type];
            }

            return PluginConfig.instance.PlayerUIToolsConfig.NoArmor;
        }

        public static string GetAmmoInfo(Player player)
        {
            Dictionary<ItemType, ushort> ammos =
                player.Ammo
                .Where(x => x.Key != ItemType.None && x.Value > 0)
                .ToDictionary(x => x.Key, x => x.Value);

            string ammoStatus;

            if (player.CurrentItem is Firearm firearm)
            {
                AmmoType ammoType = firearm.AmmoType;
                ItemType itemType = ammoType.GetItemType();

                if (!ammos.ContainsKey(itemType) || ammos[itemType] == 0)
                {
                    ammoStatus = PluginConfig.instance.PlayerUIToolsConfig.NoAmmo;
                }
                else
                {
                    ammoStatus = PluginConfig.instance.PlayerUIToolsConfig.AmmoHint1
                    .Replace("{Ammo}", PluginConfig.instance.GeneralConfig.AmmoName[ammoType])
                    .Replace("{NumOfAmmo}", ammos[itemType].ToString());
                }
            }
            else
            {
                var numOfAmmoTypes = ammos.Keys.Count;

                if (numOfAmmoTypes <= 0)
                {
                    ammoStatus = PluginConfig.instance.PlayerUIToolsConfig.NoAmmo;
                }
                else if (numOfAmmoTypes <= 1)
                {
                    ammoStatus = PluginConfig.instance.PlayerUIToolsConfig.AmmoHint1
                        .Replace("{Ammo}", PluginConfig.instance.GeneralConfig.ItemName[ammos.First().Key])
                        .Replace("{NumOfAmmo}", ammos.First().Value.ToString());
                }
                else
                {
                    var totalAmmos = 0;

                    foreach (var numOfAmmo in ammos.Values)
                    {
                        totalAmmos += numOfAmmo;
                    }

                    ammoStatus = PluginConfig.instance.PlayerUIToolsConfig.AmmoHint2
                        .Replace("{NumOfAmmo}", totalAmmos.ToString());
                }
            }

            return ammoStatus;
        }

        public static string GetContent(String template, Player player) //replace the template with the actual content
        {
            //player nickname, actual name, role, rolecolor, ammo, armor, team,
            //TPS, 
            PluginConfig config = PluginConfig.instance;

            template = template
                //Player Name
                .Replace("{PlayerNickname}", player.CustomName)
                .Replace("{PlayerName}", player.Nickname)
                //Roles
                .Replace("{Role}", GetCustomRole(player)?.Name ?? config.GeneralConfig.RoleName[player.Role.Type])
                .Replace("{RoleColor}", player.Role.Color.ToHex())
                .Replace("{Team}", config.GeneralConfig.TeamName[player.Role.Team])
                .Replace("{TeammateCount}", (Player.List.Count(x => player.LeadingTeam == x.LeadingTeam) - 1).ToString())
                //Ammo/Armor info
                .Replace("{AmmoInfo}", GetAmmoInfo(player))
                .Replace("{ArmorInfo}", GetArmorInfo(player))
                //Server info
                .Replace("{TPS}", Server.Tps.ToString());

            return template;
        }

        public static List<Player> GetSpectatorInfo(Player player)//get a list of all the spectators watching this player
        {
            List<Player> spectatingPlayers = new List<Player>();

            if (player == null)
                return spectatingPlayers;

            foreach (Player item in Player.List)
            {
                if(item == null || item.Role == null)
                    continue;

                if (item.Role.Type != RoleTypeId.Spectator)
                    continue;

                try
                {
                    SpectatorRole spectator = player.Role.As<SpectatorRole>();

                    if (spectator != null && spectator.SpectatedPlayer == player)
                    {
                        spectatingPlayers.Add(player);
                    }
                }
                catch (Exception)
                {
                    continue;
                }
                
            }

            return spectatingPlayers;
        }
    }
}
