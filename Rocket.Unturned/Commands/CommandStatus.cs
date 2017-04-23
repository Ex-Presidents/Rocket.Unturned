using Rocket.API;
using Rocket.Unturned.Player;
using Rocket.Unturned.Chat;
using System.Collections.Generic;

namespace Rocket.Unturned.Commands
{
    public class CommandStatus : IRocketCommand
    {
        #region Properties

        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "status";

        public string Help => "Get the status of your vanish or god";

        public string Syntax => "";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>() { "rocket.exit" };

        #endregion Properties

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer Player = caller as UnturnedPlayer;
            UnturnedChat.Say(Player, "God: " + (Player.Features.GodMode ? "on" : "off"));
            UnturnedChat.Say(Player, "Vanish:" + (Player.Features.VanishMode ? "on" : "off"));
        }
    }
}