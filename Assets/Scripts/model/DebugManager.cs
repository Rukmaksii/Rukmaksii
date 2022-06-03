using System;
using System.IO;
using UnityEngine;

namespace model
{
    public static class DebugManager
    {
        public static readonly bool IsDebug;

        static DebugManager()
        {
            string root;
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                root = Environment.GetEnvironmentVariable("USERPROFILE");
            else
                root = Environment.GetEnvironmentVariable("HOME");
            IsDebug = File.Exists($"{root}/.rukmaksii-debug");
        }
    }
}