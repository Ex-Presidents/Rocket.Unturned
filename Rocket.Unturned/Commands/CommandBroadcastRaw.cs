using Rocket.API;
using Rocket.Unturned.Chat;
using SDG.Unturned;
using Steamworks;
using System.Collections.Generic;

namespace Rocket.Unturned.Commands
{
    public class CommandBroadcastRaw : IRocketCommand
    {
        #region Properties

        public AllowedCaller AllowedCaller => AllowedCaller.Both;
        public string Name => "broadcastraw";
        public string Help => "Broadcasts a message without 'Server: ' prefix.";
        public string Syntax => "/broadcastraw <message>";
        public List<string> Aliases => new List<string>();
        public List<string> Permissions => new List<string>() { "rocket.broadcastraw" };

        #endregion Properties

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length != 1)
            {
                UnturnedChat.Say(caller, U.Translate("command_generic_invalid_syntax", Syntax));
                return;
            }
            List<string> raw = UnturnedChat.wrapMessage(command[0]);
            for (int i = 0; i < raw.Count; i++)
                ChatManager.instance.channel.send("tellChat", ESteamCall.OTHERS, ESteamPacket.UPDATE_UNRELIABLE_BUFFER, new object[] { CSteamID.Nil, (byte)EChatMode.GLOBAL, Palette.SERVER, raw[i] });
        }
    }
}
