using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace ShortCommands
{
    [ApiVersion(1,17)]
    public class Shorten : TerrariaPlugin
    {
        public override string Name { get { return "ShortCommands"; } }
        public override string Author { get { return "Zaicon"; } }
        public override string Description { get { return "Enables live Shortcommands."; } }
        public override Version Version { get { return new Version(1, 2, 0, 0); } }

        private static Config config = new Config();
        public string configPath = Path.Combine(TShock.SavePath, "ShortCommands.json");

        public Shorten(Main game)
            : base(game)
        {
            base.Order = 1;
        }

        public override void Initialize()
        {
            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
            TShockAPI.Hooks.PlayerHooks.PlayerCommand += OnChat;
        }

        protected override void Dispose(bool Disposing)
        {
            if (Disposing)
            {
                ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
                TShockAPI.Hooks.PlayerHooks.PlayerCommand -= OnChat;
            }
            base.Dispose(Disposing);
        }

        private void OnInitialize(EventArgs args)
        {
            loadConfig();

            Commands.ChatCommands.Add(new Command("shortcmd.reload", Reload, "screload"));
        }

        private void OnChat(TShockAPI.Hooks.PlayerCommandEventArgs args)
        {
            /*
            bool exists = false;

            for (int i = 0; i < Commands.TShockCommands.Count; i++)
            {
                if (Commands.TShockCommands[i].Names.Contains(args.CommandName))
                {
                    exists = true;
                    return;
                }
            }

            for (int i = 0; i < Commands.ChatCommands.Count; i++)
            {
                if (Commands.ChatCommands[i].Names.Contains(args.CommandName))
                {
                    exists = true;
                    return;
                }
            }

            if (!exists)
            {*/
                if (config.shortcommands.ContainsKey(args.CommandName))
                {
                    KeyValuePair<string, string> shortcmd = new KeyValuePair<string,string>();
                    foreach (KeyValuePair<string, string> thecommand in config.shortcommands)
                    {
                        if (args.CommandName == thecommand.Key)
                        {
                            shortcmd = thecommand;
                        }
                    }

                    string usecmd = shortcmd.Value;
                    string usecmdname = usecmd.Split(' ')[0];

                    if (Commands.TShockCommands.Count(p => p.Name == usecmdname) == 0 && Commands.ChatCommands.Count(p => p.Name == usecmdname) == 0)
                    {
                        args.Player.SendErrorMessage("Unknown command: {0}", usecmdname);
                        args.Handled = true;
                        return;
                    }

                    if (args.CommandName == usecmdname)
                    {
                        args.Player.SendErrorMessage("An error occured. Check the logs for more details.");
                        TShock.Log.Error("\"{0}\" cannot be a shortcommand for \"{0}\"!", usecmdname);
                        args.Handled = true;
                        return;
                    }

                    List<string> param = args.Parameters;
                    int replaced = 0;

                    for (int i = 0; i < 10; i++)
                    {
                        string replacer = "{" + i.ToString() + "}";
                        if (usecmd.Contains(replacer) && param.Count > i)
                        {
                            usecmd = usecmd.Replace(replacer, param[i]);
                            replaced++;
                        }
                        else
                            break;
                    }

                    while (replaced > 0)
                    {
                        param.RemoveAt(0);
                        replaced--;
                    }

                    string replacer2 = "{+}";
                    if (usecmd.Contains(replacer2) && param.Count > 0)
                    {
                        usecmd = usecmd.Replace(replacer2, string.Join(" ", param.Select(p => p)));
                    }
                    Commands.HandleCommand(args.Player, string.Format("{0}{1}", args.CommandPrefix, usecmd));
                    args.Handled = true;
                }
            //}
        }

        private void Reload(CommandArgs args)
        {
            loadConfig();
            args.Player.SendSuccessMessage("Config reloaded!");
        }

        private void loadConfig()
        {
            (config = Config.Read(configPath)).Write(configPath);
        }
    }
}
