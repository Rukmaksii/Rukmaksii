using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace model
{
    public static class DebugManager
    {
        private const string FILE_NAME = ".rukmaksii-debug";
        public static readonly bool IsDebug;
        private static readonly Dictionary<string, bool> config = new Dictionary<string, bool>();

        public static bool ByPassCount => config.ContainsKey("bypass-count");

        static DebugManager()
        {
            string root;
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                root = Environment.GetEnvironmentVariable("USERPROFILE");
            else
                root = Environment.GetEnvironmentVariable("HOME");
            string path = $"{root}/{FILE_NAME}";
            IsDebug = File.Exists(path);
            if (!IsDebug)
                return;

            string[] lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                config[line] = true;
            }
        }
    }
}