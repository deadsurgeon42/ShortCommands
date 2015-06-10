﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShortCommands
{
    public static class Default
    {
        public static Dictionary<string, string> sc_default = new Dictionary<string, string>() { { "h", "history" }, { "rb", "rollback" }, { "rd", "region define" }, { "r1", "region set 1" }, { "r2", "region set 2" }, { "rn", "region name" }, { "ci", "clear item 30000" }, { "cp", "clear projectile 30000" }, { "p1", "/point1" }, { "p2", "/point2" } };

        public static string default_website = @"https://tshock.co/";
    }

    public class Config
    {
        public Dictionary<string, string> shortcommands = Default.sc_default;

        public string website = Default.default_website;

        public void Write(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public static Config Read(string path)
        {
            return !File.Exists(path)
                ? new Config()
                : JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));
        }
    }
}