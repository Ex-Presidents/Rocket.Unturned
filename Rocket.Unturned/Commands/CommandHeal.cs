using System;
using Rocket.API;
using Rocket.Core.Commands;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System.Collections.Generic;

namespace Rocket.Unturned.Commands
{
    public class CommandHeal : IRocketCommand
    {
        #region Properties

        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "heal";

        public string Help => "Heals yourself or somebody else";

        public string Syntax => "[player]";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>() { "rocket.heal" };

        #endregion Properties

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (caller is UnturnedPlayer && command.Length != 1)
            {
                UnturnedPlayer player = (UnturnedPlayer)caller;
                player.Heal(100, true, true);
                player.Player.life.askDisinfect(100);
                player.Player.life.askDrink(100);
                player.Player.life.askEat(100);
                UnturnedChat.Say(player, U.Translate("command_heal_success"));
            }
            else
            {
                UnturnedPlayer otherPlayer = UnturnedPlayer.FromName(command[0]);
                if (otherPlayer != null)
                {
                    otherPlayer.Heal(100, true, true);
                    otherPlayer.Player.life.askDisinfect(100);
                    otherPlayer.Player.life.askDrink(100);
                    otherPlayer.Player.life.askEat(100);
                    UnturnedChat.Say(caller, U.Translate("command_heal_success_me", otherPlayer.CharacterName));

                    if (caller != null)
                        UnturnedChat.Say(otherPlayer, U.Translate("command_heal_success_other", caller.DisplayName));
                }
                else
                {
                    UnturnedChat.Say(caller, U.Translate("command_generic_target_player_not_found"));
                    throw new WrongUsageOfCommandException(caller, this);
                }
            }
        }
    }
}