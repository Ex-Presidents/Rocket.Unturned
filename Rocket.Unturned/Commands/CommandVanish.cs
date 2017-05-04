using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System.Collections.Generic;

namespace Rocket.Unturned.Commands
{
    public class CommandVanish : IRocketCommand
    {
        #region Properties

        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "vanish";

        public string Help => "Are we rushing in or are we goin' sneaky beaky like?";

        public string Syntax => "";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>() { "rocket.vanish" };

        #endregion Properties

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;
            Logger.Log(U.Translate("command_vanish_toggle", player.CharacterName));
            UnturnedChat.Say(player, U.Translate("command_vanish_toggle"));
            player.Features.VanishMode = !player.Features.VanishMode;
        }
    }
}