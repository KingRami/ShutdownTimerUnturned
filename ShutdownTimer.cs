// Decompiled with JetBrains decompiler
// Type: ShutdownTimer.ShutdownTimer
// Assembly: ShutdownTimer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 777D4D7A-8747-4E92-A090-092FC80FAED8
// Assembly location: E:\PycharmProjects\Bot-Projekte\unturned\ShutdownTimer.dll

using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace ShutdownTimer
{
    public class ShutdownTimer : RocketPlugin<PluginConfig>
    {
        public static ShutdownTimer Instance;
        public static PluginConfig Config;

        protected override void Load()
        {
            Logger.Log(((RocketPlugin)this).Name + " 1.0.0 has been loaded!", ConsoleColor.Cyan);
            Logger.Log("Thanks for using my plugin! Contact me on Discord for support: 'KingRami'!", ConsoleColor.Blue);
            ShutdownTimer.Instance = this;
            ShutdownTimer.Config = this.Configuration.Instance;
        }

        protected override void Unload()
        {
            Logger.Log(((RocketPlugin)this).Name + " has been unloaded!", ConsoleColor.Cyan);
        }

        public override TranslationList DefaultTranslations
        {
            get
            {
                TranslationList defaultTranslations = new TranslationList();
                defaultTranslations.Add("sdtimer_command_help", "Initiate a shutdown with a countdown, to alert your players. Command Usage: '/sdtimer [minutes]'. Use '/sdcancel' to stop shutdown.");
                defaultTranslations.Add("sdtimer_command_usage", "Command Usage: '/sdtimer [minutes]'. Use '/sdcancel' to stop shutdown.");
                defaultTranslations.Add("sdtimer_command_usage_invalid", "Invalid arguments. Command Usage: '/sdtimer [minutes]'. Use '/sdcancel' to stop shutdown.");
                defaultTranslations.Add("sdtimer_countdown_in_progress", "There is already a countdown in progress!");
                defaultTranslations.Add("sdcancel_command_help", "Cancels a shutdown countdown.");
                defaultTranslations.Add("sdcancel_cancelled", "Shutdown has been cancelled!");
                defaultTranslations.Add("sdcancel_no_active_shutdown", "There is no active countdown to cancel!");
                defaultTranslations.Add("announce_minutes", "Server will shutdown in {0} minutes!");
                defaultTranslations.Add("announce_seconds", "Server will shutdown in {0} seconds!");
                defaultTranslations.Add("announce_shutdown", "Server is saving and shutting down...");
                return defaultTranslations;
            }
        }

        public static void SendMessage(IRocketPlayer caller, string message)
        {
            if (caller is ConsolePlayer)
            {
                Logger.Log(message, ConsoleColor.Cyan);
            }
            else
            {
                UnturnedPlayer unturnedPlayer = (UnturnedPlayer)caller;
                ChatManager.serverSendMessage(message, Color.white, (SteamPlayer)null, unturnedPlayer.SteamPlayer(), (EChatMode)4, ShutdownTimer.Config.MessageIcon, ShutdownTimer.Config.UseRichText);
            }
        }

        public static class SharedMemory
        {
        }

        public class CommandTimerStart : IRocketCommand
        {
            public AllowedCaller AllowedCaller => (AllowedCaller)2;

            public string Name => "sdtimer";

            public string Help
            {
                get
                {
                    return ((RocketPlugin)ShutdownTimer.Instance).Translate("sdtimer_command_help", Array.Empty<object>());
                }
            }

            public string Syntax => string.Empty;

            public List<string> Aliases => new List<string>();

            public List<string> Permissions
            {
                get => new List<string>() { "shutdowntimer" };
            }

            public void Execute(IRocketPlayer caller, string[] command)
            {
                if (command.Length < 1)
                    ShutdownTimer.SendMessage(caller, ((RocketPlugin)ShutdownTimer.Instance).Translate("sdtimer_command_usage", Array.Empty<object>()));
                else if (command.Length > 1)
                {
                    ShutdownTimer.SendMessage(caller, ((RocketPlugin)ShutdownTimer.Instance).Translate("sdtimer_command_usage_invalid", Array.Empty<object>()));
                }
                else
                {
                    int result;
                    if (int.TryParse(command[0], out result))
                    {
                        if (!ShutdownTimer.TimerManagement.isCounting)
                        {
                            ShutdownTimer.TimerManagement.isCounting = true;
                            ShutdownTimer.TimerManagement.coroutine = ShutdownTimer.TimerManagement.MyTimer(result);
                            ((MonoBehaviour)ShutdownTimer.Instance).StartCoroutine(ShutdownTimer.TimerManagement.coroutine);
                            ShutdownTimer.SendMessage(caller, "Server restarting in " + result + " min");

                        }
                        else
                            ShutdownTimer.SendMessage(caller, ((RocketPlugin)ShutdownTimer.Instance).Translate("sdtimer_countdown_in_progress", Array.Empty<object>()));
                    }
                    else
                        ShutdownTimer.SendMessage(caller, ((RocketPlugin)ShutdownTimer.Instance).Translate("sdtimer_command_usage_invalid", Array.Empty<object>()));
                }
            }
        }

        public class CommandTimerCancel : IRocketCommand
        {
            public AllowedCaller AllowedCaller => (AllowedCaller)2;

            public string Name => "sdcancel";

            public string Help
            {
                get
                {
                    return ((RocketPlugin)ShutdownTimer.Instance).Translate("sdcancel_command_help", Array.Empty<object>());
                }
            }

            public string Syntax => string.Empty;

            public List<string> Aliases => new List<string>();

            public List<string> Permissions
            {
                get => new List<string>() { "shutdowntimer" };
            }

            public void Execute(IRocketPlayer caller, string[] command)
            {
                if (ShutdownTimer.TimerManagement.isCounting)
                {
                    ((MonoBehaviour)ShutdownTimer.Instance).StopCoroutine(ShutdownTimer.TimerManagement.coroutine);
                    ShutdownTimer.TimerManagement.isCounting = false;
                    ShutdownTimer.SendMessage(caller, ((RocketPlugin)ShutdownTimer.Instance).Translate("sdcancel_cancelled", Array.Empty<object>()));
                }
                else
                    ShutdownTimer.SendMessage(caller, ((RocketPlugin)ShutdownTimer.Instance).Translate("sdcancel_no_active_shutdown", Array.Empty<object>()));
            }
        }

        public class TimerManagement
        {
            public static bool isCounting;
            public static IEnumerator coroutine;

            public static IEnumerator MyTimer(int minutes)
            {
                int counter = minutes * 60;
                var config = ShutdownTimer.Instance.Configuration.Instance;


                List<int> announceTimes = config.AnnounceTimes
                    .Split(',')
                    .Select(int.Parse)
                    .Select(t => t * 60)
                    .ToList();

                while (counter > 0)
                {
                    if (counter > 60)
                    {
                        if (announceTimes.Contains(counter))
                        {
                            int minute = counter / 60;
                            string message = ((RocketPlugin)ShutdownTimer.Instance).Translate("announce_minutes", new object[] { (object)(minute) });

                            ChatManager.serverSendMessage(message, Color.cyan, null, null, (EChatMode)0, config.MessageIcon, config.UseRichText);
                            Logger.Log(message, ConsoleColor.White);
                        }

                        yield return new WaitForSeconds(60f);
                        counter -= 60;
                    }


                    else if (counter == 60)
                    {
                        ChatManager.serverSendMessage(((RocketPlugin)ShutdownTimer.Instance).Translate("announce_minutes", new object[1]
                        {
              (object) 1
                        }), Color.cyan, (SteamPlayer)null, (SteamPlayer)null, (EChatMode)0, ShutdownTimer.Config.MessageIcon, ShutdownTimer.Config.UseRichText);
                        Logger.Log(((RocketPlugin)ShutdownTimer.Instance).Translate("announce_minutes", new object[1]
                        {
              (object) 1
                        }), ConsoleColor.White);
                        yield return (object)new WaitForSeconds(30f);
                        ChatManager.serverSendMessage(((RocketPlugin)ShutdownTimer.Instance).Translate("announce_seconds", new object[1]
                        {
              (object) 30
                        }), Color.cyan, (SteamPlayer)null, (SteamPlayer)null, (EChatMode)0, ShutdownTimer.Config.MessageIcon, ShutdownTimer.Config.UseRichText);
                        Logger.Log(((RocketPlugin)ShutdownTimer.Instance).Translate("announce_seconds", new object[1]
                        {
              (object) 30
                        }), ConsoleColor.White);
                        yield return (object)new WaitForSeconds(20f);
                        for (int i = 10; i > 0; --i)
                        {
                            ChatManager.serverSendMessage(((RocketPlugin)ShutdownTimer.Instance).Translate("announce_seconds", new object[1]
                            {
                (object) i
                            }), Color.cyan, (SteamPlayer)null, (SteamPlayer)null, (EChatMode)0, ShutdownTimer.Config.MessageIcon, ShutdownTimer.Config.UseRichText);
                            Logger.Log(((RocketPlugin)ShutdownTimer.Instance).Translate("announce_seconds", new object[1]
                            {
                (object) i
                            }), ConsoleColor.White);
                            yield return (object)new WaitForSeconds(1f);
                        }
                        ChatManager.serverSendMessage(((RocketPlugin)ShutdownTimer.Instance).Translate("announce_shutdown", Array.Empty<object>()), Color.cyan, (SteamPlayer)null, (SteamPlayer)null, (EChatMode)0, ShutdownTimer.Config.MessageIcon, ShutdownTimer.Config.UseRichText);
                        Logger.Log(((RocketPlugin)ShutdownTimer.Instance).Translate("announce_shutdown", Array.Empty<object>()), ConsoleColor.White);
                        ShutdownTimer.TimerManagement.isCounting = false;
                        counter = 0;
                        SaveManager.save();
                        Provider.shutdown();
                    }
                }
            }
        }
    }
}
