using Rocket.Core.Utils;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using UnityEngine;

namespace Rocket.Unturned
{
    public class UnturnedPlayerMovement : UnturnedPlayerComponent
    {
        public bool VanishMode = false;
        private DateTime lastUpdate = DateTime.Now;
        private Vector3 lastVector = new Vector3(0, -1, 0);

        private DateTime? requested = null;
        private string webClientResult = null;

        private void OnEnable()
        {
            if (U.Settings.Instance.RocketModObservatory.CommunityBans)
            {
                using (RocketWebClient webClient = new RocketWebClient())
                {
                    try
                    {
                        webClient.DownloadStringCompleted += (object sender, System.Net.DownloadStringCompletedEventArgs e) =>
                        {
                            if (e.Error == null)
                            {
                                if (e.Result.Contains(","))
                                {
                                    string[] result = e.Result.Split(',');
                                    if (result[0] == "true")
                                    {
                                        Core.Logging.Logger.Log("[RocketMod Observatory] Kicking Player " + Player.CharacterName + "because he is banned:" + result[1]);
                                        webClientResult = result[1];
                                        requested = DateTime.Now;
                                        Player.Kick("you are banned from observatory: " + result[1]);
                                    }
                                    else if (U.Settings.Instance.RocketModObservatory.KickLimitedAccounts && result.Length >= 2 && result[1] == "true")
                                    {
                                        Core.Logging.Logger.Log("[RocketMod Observatory] Kicking Player " + Player.CharacterName + " because his account is limited");
                                        Player.Kick("your Steam account is limited");
                                    }
                                    else if (U.Settings.Instance.RocketModObservatory.KickTooYoungAccounts && result.Length == 3 && long.TryParse(result[2].ToString(), out long age))
                                    {
                                        long epochTicks = new DateTime(1970, 1, 1).Ticks;
                                        long unixTime = ((DateTime.UtcNow.Ticks - epochTicks) / TimeSpan.TicksPerSecond);
                                        long d = (unixTime - age);
                                        if (d < U.Settings.Instance.RocketModObservatory.MinimumAge)
                                        {
                                            Core.Logging.Logger.Log("[RocketMod Observatory] Kicking Player " + Player.CharacterName + " because his account is younger then " + U.Settings.Instance.RocketModObservatory.MinimumAge + " seconds (" + d + " seconds)");
                                            Player.Kick("your Steam account is not old enough");
                                        }
                                    }
                                }
                            }
                        };
                        webClient.DownloadStringAsync(new Uri(string.Format("http://banlist.observatory.rocketmod.net/?steamid={0}", Player.CSteamID)));
                    }
                    catch (Exception)
                    {
                        //
                    }
                }
            }
        }

        private void FixedUpdate()
        {
            if (U.Settings.Instance.EnableItemBlacklist && lastUpdate.AddSeconds(1) < DateTime.Now)
            {
                ushort[] Items = U.Settings.Instance.Items.ToArray();
                for (int i = 0; i < Items.Length; i++)
                {
                    InventorySearch[] Search = Player.Inventory.search(Items[i], true, true).ToArray();
                    if (Search.Length == 0)
                        continue;
                    for (int e = 0; e < Search.Length; e++)
                    {
                        InventorySearch ISearch = Search[e];
                        Player.Inventory.removeItem(ISearch.page, ISearch.jar.y);
                        Rocket.Core.Logging.Logger.Log(Player.CharacterName + " had a restricted item (" + Items[i] + "), and it was removed passively.");
                    }
                }
            }
            if (requested.HasValue && (DateTime.Now - requested.Value).TotalSeconds >= 2)
            {
                Provider.kick(Player.CSteamID, webClientResult);
                requested = null;
            }

            PlayerMovement movement = (PlayerMovement)Player.GetComponent<PlayerMovement>();

            if (!VanishMode)
            {
                if (U.Settings.Instance.LogSuspiciousPlayerMovement && lastUpdate.AddSeconds(1) < DateTime.Now)
                {
                    lastUpdate = DateTime.Now;

                    Vector3 positon = movement.real;

                    if (lastVector.y != -1)
                    {
                        float x = Math.Abs(lastVector.x - positon.x);
                        float y = positon.y - lastVector.y;
                        float z = Math.Abs(lastVector.z - positon.z);
                        if (y > 15)
                        {
                            RaycastHit raycastHit = new RaycastHit();
                            Physics.Raycast(positon, Vector3.down, out raycastHit);
                            Vector3 floor = raycastHit.point;
                            float distance = Math.Abs(floor.y - positon.y);
                            Core.Logging.Logger.Log(Player.DisplayName + " moved x:" + positon.x + " y:" + positon.y + "(+" + y + ") z:" + positon.z + " in the last second (" + distance + ")");
                        }
                    }
                    lastVector = movement.real;
                }
            }
        }
    }
}