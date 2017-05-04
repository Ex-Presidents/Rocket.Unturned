using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System.Collections.Generic;

namespace Rocket.Unturned.Commands
{
    public class CommandGod : IRocketCommand
    {
        #region Properties

        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "god";

        public string Help => "Cause you ain't givin a shit";

        public string Syntax => "";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>() { "rocket.god" };

        #endregion Properties

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;
            Logger.Log(U.Translate("command_god_toggle", player.CharacterName, player.Features.GodMode ? "off" : "on"));
            UnturnedChat.Say(player, U.Translate("command_god_toggle", "You", player.Features.GodMode ? "off" : "on"));
            player.Features.GodMode = !player.Features.GodMode;
        }
    }
}