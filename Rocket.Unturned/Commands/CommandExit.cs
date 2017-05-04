using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections.Generic;

namespace Rocket.Unturned.Commands
{
    public class CommandExit : IRocketCommand
    {
        #region Properties

        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "exit";

        public string Help => "Exit the game without cooldown";

        public string Syntax => "";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>() { "rocket.exit" };

        #endregion Properties

        public void Execute(IRocketPlayer caller, string[] command)
        {
            Provider.kick((caller as UnturnedPlayer).CSteamID, "you exited");
        }
    }
}