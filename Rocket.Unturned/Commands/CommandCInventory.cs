using Rocket.API;
using Rocket.Core;
using Rocket.Core.Commands;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System.Collections.Generic;
using SDG.Unturned;

namespace Rocket.Unturned.Commands
{
    public class CommandCInventory : IRocketCommand
    {
        #region Properties

        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "cinventory";

        public string Help => "Check a player's inventory for an item";

        public string Syntax => "/cinventory <player> [item]";

        public List<string> Permissions => new List<string>() { "rocket.cinventory" };

        public List<string> Aliases => new List<string>();

        #endregion Properties

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length != 2)
                UnturnedChat.Say(caller, "Correct syntax is: " + Syntax);
            if (UnturnedPlayer.FromName(command[0]) == null)
                UnturnedChat.Say(command[0] + " does not exist.");
            UnturnedPlayer Target = UnturnedPlayer.FromName(command[0]);
            if (ushort.TryParse(command[1], out ushort id))
            {
                if (Target.Player.inventory.search(id, true, true).Count > 0)
                    UnturnedChat.Say(caller, "True");
                else
                    UnturnedChat.Say(caller, "False");
            }
            if (Assets.find(EAssetType.ITEM, command[1]) == null)
            {
                UnturnedChat.Say(caller, "Nonexistent item: " + command[1]);
                return;
            }
            if (Target.Player.inventory.search((Assets.find(EAssetType.ITEM, command[1]).id), true, true).Count > 0)
                UnturnedChat.Say(caller, "True");
            else
                UnturnedChat.Say(caller, "False");
        }
    }
}