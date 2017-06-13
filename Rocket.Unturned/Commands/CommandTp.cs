using Rocket.API;
using Rocket.API.Extensions;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Rocket.Unturned.Commands
{
    public class CommandTp : IRocketCommand
    {
        #region Properties

        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "tp";

        public string Help => "Teleports you to another player or location";

        public string Syntax => "<player | place | x y z>";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>() { "rocket.tp", "rocket.teleport" };

        #endregion Properties

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;
            if (command.Length == 2 || command.Length > 3)
            {
                UnturnedChat.Say(player, U.Translate("command_generic_invalid_parameter"));
                throw new WrongUsageOfCommandException(caller, this);
            }

            if (player.Stance == EPlayerStance.DRIVING || player.Stance == EPlayerStance.SITTING)
            {
                UnturnedChat.Say(player, U.Translate("command_generic_teleport_while_driving_error"));
                throw new WrongUsageOfCommandException(caller, this);
            }

            if (command.Length == 0 && player.HasPermission("rocket.i.forward"))
            {
                Physics.Raycast(player.Player.look.aim.position, player.Player.look.aim.forward, out RaycastHit raycast);
                player.Teleport(new Vector3(raycast.point.x, raycast.point.y + 3f, raycast.point.z), player.Rotation);
                UnturnedChat.Say(caller, U.Translate("command_tp_teleport", "You", "forward."));
                Core.Logging.Logger.Log(U.Translate("command_tp_teleport", player.CharacterName, "forward"));
            }

            if (command.Length == 3 && float.TryParse(command[0], out float x) && float.TryParse(command[1], out float y) && float.TryParse(command[2], out float z))
            {
                player.Teleport(new Vector3(x, y, z), MeasurementTool.angleToByte(player.Rotation));
                Core.Logging.Logger.Log(U.Translate("command_tp_teleport", player.CharacterName, x + "," + y + "," + z));
                UnturnedChat.Say(player, U.Translate("command_tp_teleport", x + "," + y + "," + z));
            }
            else
            {
                if (command.Length == 0)
                    return;
                UnturnedPlayer otherplayer = UnturnedPlayer.FromName(command[0]);
                if (otherplayer != null && otherplayer != player)
                {
                    player.Teleport(otherplayer);
                    Core.Logging.Logger.Log(U.Translate("command_tp_teleport", player.CharacterName, otherplayer.CharacterName));
                    UnturnedChat.Say(player, U.Translate("command_tp_teleport", "You", otherplayer.CharacterName));
                }
                else
                {
                    Node item = LevelNodes.nodes.Where(n => n.type == ENodeType.LOCATION && ((LocationNode)n).name.ToLower().Contains(command[0].ToLower())).FirstOrDefault();
                    if (item == null)
                    {
                        UnturnedChat.Say(player, U.Translate("command_tp_failed_find_destination"));
                        return;
                    }
                    Vector3 c = item.point + new Vector3(0f, 0.5f, 0f);
                    player.Teleport(c, MeasurementTool.angleToByte(player.Rotation));
                    Core.Logging.Logger.Log(U.Translate("command_tp_teleport", player.CharacterName, ((LocationNode)item).name));
                    UnturnedChat.Say(player, U.Translate("command_tp_teleport", "You", ((LocationNode)item).name));
                }
            }
        }
    }
}