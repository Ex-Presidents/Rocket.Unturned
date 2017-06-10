using System;
using System.Linq;
using System.Collections.Generic;
using Steamworks;

namespace Rocket.Unturned.Player
{
    public class PlayerSpawnInfo
    {
        public CSteamID Player;
        public int LastSpawnAmount;
        public int RecentSpawnAmount;
        public DateTime LastSpawnTime;

        public PlayerSpawnInfo(CSteamID player, int lastspawnamount, int recentspawnamount, DateTime lastspawntime)
        {
            Player = player;
            LastSpawnAmount = lastspawnamount;
            RecentSpawnAmount = recentspawnamount;
            LastSpawnTime = lastspawntime;
        }
    }
}