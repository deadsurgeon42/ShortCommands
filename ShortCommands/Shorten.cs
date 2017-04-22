using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace ShortCommands
{
    [ApiVersion(2,1)]
    public class Shorten : TerrariaPlugin
    {
        public override string Name { get { return "ShortCommands"; } }
        public override string Author { get { return "Zaicon"; } }
        public override string Description { get { return "Enables live Shortcommands."; } }
        public override Version Version { get { return Assembly.GetExecutingAssembly().GetName().Version; } }

        public static Config config = new Config();
        public string configPath = Path.Combine(TShock.SavePath, "ShortCommands.json");

        public Shorten(Main game)
            : base(game)
        {
            base.Order = 5;
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
            LoadConfig();

            Commands.ChatCommands.Add(new Command("shortcmd.reload", Reload, "screload"));
        }

        private void OnChat(TShockAPI.Hooks.PlayerCommandEventArgs args)
        {
            //If the used command is an existing command, ignore.
            if (Commands.TShockCommands.Count(p => p.Name == args.CommandName) > 0 || Commands.ChatCommands.Count(p => p.Name == args.CommandName) > 0)
            {
                return;
            }

            //Check if our config has the typed command
            if (config.shortcommands.ContainsKey(args.CommandName))
            {
                KeyValuePair<string, string> shortcmd = new KeyValuePair<string, string>();

                //Retrieve the long commands by finding the short command.
                foreach (KeyValuePair<string, string> thecommand in config.shortcommands)
                {
                    if (args.CommandName == thecommand.Key)
                    {
                        shortcmd = thecommand;
                    }
                }

                //Split the long commands into the different commands
                List<string> usecmds = shortcmd.Value.Split(';').ToList();
                List<string> usecmdnames = new List<string>();

                //Make a separate list of the actual command names
                foreach (string cmd in usecmds)
                {
                    usecmdnames.Add(cmd.Split(' ')[0]);
                }

                //Loop through each bound command
                for (int j = 0; j < usecmds.Count; j++)
                {
                    //Temporarily store the individual command & name
                    string usecmd = usecmds[j];
                    string usecmdname = usecmdnames[j];

                    //Check if the command is real or not (no shortcommands!)
                    if (Commands.TShockCommands.Count(p => p.Name == usecmdname) == 0 && Commands.ChatCommands.Count(p => p.Name == usecmdname) == 0)
                    {
                        args.Player.SendErrorMessage("Unknown command: {0}", usecmdname);
                        TShock.Log.Warn("Unknown ShortCommand entry: {0}", usecmdname);
                        args.Handled = true;
                        return;
                    }
                    
                    //Handle replacing params with {0} and {+}
                    List<string> param = new List<string>();

                    for (int i = 0; i < args.Parameters.Count; i++)
                    {
                        param.Add(args.Parameters[i]);
                    }

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

                    string replacer3 = "{player}";
                    if (usecmd.Contains(replacer3))
                    {
                        usecmd = usecmd.Replace(replacer3, args.Player.User.Name);
                    }

                    string replacer4 = "{website}";
                    if (usecmd.Contains(replacer4))
                    {
                        usecmd = usecmd.Replace(replacer4, config.website);
                    }

                    //Handle the Command.
                    Commands.HandleCommand(args.Player, string.Format("{0}{1}", args.CommandPrefix, usecmd));
                } //Loop back through the rest of the bound commands

                //Don't let TShock handle it (it would give "Invalid Command" after already running the commands.
                args.Handled = true;
            }
        }
        
        private void Reload(CommandArgs args)
        {
            LoadConfig();
            args.Player.SendSuccessMessage("Shortcommand config reloaded!");
        }

        private void LoadConfig()
        {
            (config = Config.Read(configPath)).Write(configPath);
        }
    }
}
