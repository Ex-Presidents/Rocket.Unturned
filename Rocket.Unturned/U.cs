﻿using Rocket.API;
using Rocket.API.Collections;
using Rocket.API.Extensions;
using Rocket.Core;
using Rocket.Core.Assets;
using Rocket.Core.Extensions;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Commands;
using Rocket.Unturned.Effects;
using Rocket.Unturned.Events;
using Rocket.Unturned.Permissions;
using Rocket.Unturned.Player;
using Rocket.Unturned.Plugins;
using Rocket.Unturned.Serialisation;
using Rocket.Unturned.Utils;
using SDG.Framework.Modules;
using SDG.Unturned;
using Steamworks;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Rocket.Unturned
{
    public class U : MonoBehaviour, IRocketImplementation, IModuleNexus
    {
        private static GameObject rocketGameObject; 
        public static U Instance;

        private static readonly TranslationList defaultTranslations = new TranslationList(){
            { "command_generic_failed_find_player","Failed to find player"},
                { "command_generic_invalid_parameter","Invalid parameter"},
                { "command_generic_invalid_syntax", "The correct syntax is: {0}" },
                { "command_generic_target_player_not_found","Target player not found"},
                { "command_generic_teleport_while_driving_error","You cannot teleport while driving or riding in a vehicle."},
                { "command_god_toggle_console","{0} toggled Godmode"},
                { "command_god_toggle_private","You toggled godmode"},
                { "command_vanish_toggle_console","{0} toggled Vanishmode"},
                { "command_vanish_toggle_private","You toggled vanish"},
                { "command_duty_enable_console","{0} is in duty"},
                { "command_duty_enable_private","You are in duty now..."},
                { "command_duty_disable_console","{0} is no longer in duty"},
                { "command_duty_disable_private","You are no longer in duty..."},
                { "command_bed_no_bed_found_private","You do not have a bed to teleport to."},
                { "command_i_too_much","You have tried to spawn too many items! The limit is {0}." },
                { "command_i_blacklisted","This item is restricted!" },
                { "command_i_giving_console","Giving {0} item {1}:{2}"},
                { "command_i_giving_private","Giving you item {0}x {1} ({2})"},
                { "command_z_giving_console","Spawning {1} zombies near {0}"},
                { "command_z_giving_private","Spawning {0} zombies nearby"},
                { "command_i_giving_failed_private","Failed giving you item {0}x {1} ({2})"},
                { "command_v_giving_console","Giving {0} vehicle {1}"},
                { "command_v_giving_private","Giving you a {0} ({1})"},
                { "command_v_giving_failed_private","Failed giving you a {0} ({1})"},
                { "command_tps_tps","TPS: {0}"},
                { "command_tps_running_since","Running since: {0} UTC"},
                { "command_p_reload_private","Reloaded permissions"},
                { "command_p_groups_private","{0} groups are: {1}"},
                { "command_p_permissions_private","{0} permissions are: {1}"},
                { "command_tp_teleport_console","{0} teleported to {1}"},
                { "command_tp_teleport_private","Teleported to {0}"},
                { "command_tp_failed_find_destination","Failed to find destination"},
                { "command_tphere_vehicle", "The player you are trying to teleport is in a vehicle!"},
                { "command_tphere_teleport_console","{0} was teleported to {1}"},
                { "command_tphere_teleport_from_private","Teleported {0} to you"},
                { "command_tphere_teleport_to_private","You were teleported to {0}"},
                { "command_clear_error","There was an error clearing {0} inventory."},
                { "command_clear_private","Your inventory was cleared!"},
                { "command_clear_other","Your inventory was cleared by {0}!"},
                { "command_clear_other_success","You successfully cleared {0} inventory."},
                { "command_investigate_private","{0} SteamID64 is {1}"},
                { "command_heal_success_me","{0} was successfully healed"},
                { "command_heal_success_other","You were healed by {0}"},
                { "command_heal_success","You were healed"},
                { "command_compass_facing_private","You are facing {0}"},
                { "command_compass_north","N"},
                { "command_compass_east","E"},
                { "command_compass_south","S"},
                { "command_compass_west","W"},
                { "command_compass_northwest","NW"},
                { "command_compass_northeast","NE"},
                { "command_compass_southwest","SW"},
                { "command_compass_southeast","SE"},
                { "command_rocket_plugins_loaded","Loaded: {0}"},
                { "command_rocket_plugins_unloaded","Unloaded: {0}"},
                { "command_rocket_plugins_failure","Failure: {0}"},
                { "command_rocket_plugins_cancelled","Cancelled: {0}"},
                { "command_rocket_reload_plugin","Reloading {0}"},
                { "command_rocket_not_loaded","The plugin {0} is not loaded"},
                { "command_rocket_unload_plugin","Unloading {0}"},
                { "command_rocket_load_plugin","Loading {0}"},
                { "command_rocket_already_loaded","The plugin {0} is already loaded"},
                { "command_rocket_reload","Reloading Rocket"},
                { "command_p_group_not_found","Group not found"},
                { "command_p_group_player_added","{0} was added to the group {1}"},
                { "command_p_group_player_removed","{0} was removed from from the group {1}"},
                { "command_p_unknown_error","Unknown error"},
                { "command_p_player_not_found","{0} was not found"},
                { "command_p_group_not_found","{1} was not found"},
                { "command_p_duplicate_entry","{0} is already in the group {1}"},
                { "command_p_permissions_reload","Permissions reloaded"},
                { "command_rocket_plugin_not_found","Plugin {0} not found"},
                { "command_clear_success","You successfully cleared {0} items"},
                { "invalid_character_name","invalid character name"},
                { "command_not_found","Command not found."}
        };
         

        public static XMLFileAsset<UnturnedSettings> Settings;
        public static XMLFileAsset<TranslationList> Translation;

        public IRocketImplementationEvents ImplementationEvents { get { return Events; } }
        public static UnturnedEvents Events;

        public event RocketImplementationInitialized OnRocketImplementationInitialized;

        public static string Translate(string translationKey, params object[] placeholder)
        {
            return Translation.Instance.Translate(translationKey, placeholder);
        }

#if LINUX
        public static UnturnedConsole Console;
#endif

        public void initialize()
        {
            SDG.Unturned.Player.onPlayerCreated += (SDG.Unturned.Player player) =>
            {
                player.transform.gameObject.TryAddComponent<HUD>();
            };

            if (Dedicator.isDedicated)
            {
                rocketGameObject = new GameObject("Rocket");
                DontDestroyOnLoad(rocketGameObject);
#if LINUX
    Console = rocketGameObject.AddComponent<UnturnedConsole>();
#endif
                System.Console.Clear();
                System.Console.ForegroundColor = ConsoleColor.Cyan;
                System.Console.WriteLine("Rocket Unturned v" + Assembly.GetExecutingAssembly().GetName().Version.ToString() + " for Unturned v" + Provider.APP_VERSION + "\n");

                R.OnRockedInitialized += () =>
                {
                    Instance.Initialize();
                };

                Provider.onServerHosted += () =>
                {
                    rocketGameObject.TryAddComponent<U>();
                    rocketGameObject.TryAddComponent<R>();
                };
            }
        }
        
        private void Awake()
        {
            Instance = this;
            Environment.Initialize();
        }

        internal void Initialize()
        {
            try
            {
                Settings = new XMLFileAsset<UnturnedSettings>(Environment.SettingsFile);
                Translation = new XMLFileAsset<TranslationList>(String.Format(Environment.TranslationFile, Core.R.Settings.Instance.LanguageCode), new Type[] { typeof(TranslationList), typeof(TranslationListEntry) }, defaultTranslations);
                defaultTranslations.AddUnknownEntries(Translation);
                Events = gameObject.TryAddComponent<UnturnedEvents>();

                gameObject.TryAddComponent<UnturnedPermissions>();
                gameObject.TryAddComponent<UnturnedChat>();
                gameObject.TryAddComponent<UnturnedCommands>();

                gameObject.TryAddComponent<AutomaticSaveWatchdog>();

                bindDelegates();

                RocketPlugin.OnPluginLoading += (IRocketPlugin plugin, ref bool cancelLoading) =>
                {
                    try
                    {
                        plugin.TryAddComponent<PluginUnturnedPlayerComponentManager>();
                    }
                    catch (Exception ex)
                    {
                        Core.Logging.Logger.LogException(ex, "Failed to load plugin " + plugin.Name + ".");
                        cancelLoading = true;
                    }
                };

                RocketPlugin.OnPluginUnloading += (IRocketPlugin plugin) =>
                {
                    plugin.TryRemoveComponent<PluginUnturnedPlayerComponentManager>();
                };

                R.Commands.RegisterFromAssembly(Assembly.GetExecutingAssembly());

                try
                {
                    R.Plugins.OnPluginsLoaded += () =>
                    {
                        SteamGameServer.SetKeyValue("rocketplugins", String.Join(",", R.Plugins.GetPlugins().Select(p => p.Name).ToArray()));
                    };


                    SteamGameServer.SetKeyValue("unturned", Provider.APP_VERSION);
                    SteamGameServer.SetKeyValue("rocket", Assembly.GetExecutingAssembly().GetName().Version.ToString());
                    SteamGameServer.SetBotPlayerCount(1);

                }
                catch (Exception ex)
                {
                    Core.Logging.Logger.LogError("Steam can not be initialized: " + ex.Message);
                }

                OnRocketImplementationInitialized.TryInvoke();

            }
            catch (Exception ex)
            {
                Core.Logging.Logger.LogException(ex);
            }
        }
        
        private void bindDelegates()
        {
            CommandWindow.onCommandWindowInputted += (string text, ref bool shouldExecuteCommand) =>
            {
                if (text.StartsWith("/")) text.Substring(1);
                if (R.Commands != null) R.Commands.Execute(new ConsolePlayer(), text);
                shouldExecuteCommand = false;
            };

            CommandWindow.onCommandWindowOutputted += (object text, ConsoleColor color) =>
            {
                Core.Logging.Logger.ExternalLog(text, color);
            };

            /*
            SteamChannel.onTriggerReceive += (SteamChannel channel, CSteamID steamID, byte[] packet, int offset, int size) =>
             {
                 UnturnedPlayerEvents.TriggerReceive(channel, steamID, packet, offset, size);
             };
             */

            SteamChannel.onTriggerSend += (SteamPlayer player, string name, ESteamCall mode, ESteamPacket type, object[] arguments) =>
            {
                UnturnedPlayerEvents.TriggerSend(player, name, mode, type, arguments);
            };

            ChatManager.onCheckPermissions += (SteamPlayer player, string text, ref bool shouldExecuteCommand, ref bool shouldList) =>
            {
                if (text.StartsWith("/"))
                {
                    text.Substring(1);
                    if (R.Commands != null && UnturnedPermissions.CheckPermissions(player, text))
                    {
                        R.Commands.Execute(UnturnedPlayer.FromSteamPlayer(player), text);
                    }
                    shouldList = false;
                }
                shouldExecuteCommand = false;
            };

            Provider.onCheckValid += (ValidateAuthTicketResponse_t callback, ref bool isValid) =>
            {
                isValid = UnturnedPermissions.CheckValid(callback);
            };
    }

        public void Reload()
        {
            Translation.Load();
            Settings.Load();
        }

        public void shutdown()
        {
            Shutdown();
        }

        public void Shutdown()
        {

        }

        public string InstanceId
        {
            get
            {
                return Dedicator.serverID;
            } 
        }
    }
               
}
