using System;
using System.Collections.Generic;

public static class GetArgs
{
    private static readonly Dictionary<string, string> args = new Dictionary<string, string>();

    static GetArgs()
    {
        string[] rawArgs = Environment.GetCommandLineArgs();
        for (int i = 0; i < rawArgs.Length; i++)
        {
            if (rawArgs[i].StartsWith("--"))
            {
                string key = rawArgs[i].Substring(2).ToLower();
                string value = (i + 1 < rawArgs.Length && !rawArgs[i + 1].StartsWith("--")) ? rawArgs[i + 1] : "true";
                args[key] = value;
            }
        }
    }

    public static string Get(string key)
    {
        args.TryGetValue(key.ToLower(), out var value);
        return value;
    }

    public static bool Exists(string key)
    {
        return args.ContainsKey(key.ToLower());
    }
}