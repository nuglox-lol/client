using System;
using System.Collections.Generic;

public static class GetArgs
{
    private static readonly Dictionary<string, string> args = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, string> deepLinkArgs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

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

    public static void SetDeepLinkArgs(string url)
    {
        deepLinkArgs.Clear();
        if (string.IsNullOrEmpty(url)) return;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return;

        string query = uri.Query;
        if (string.IsNullOrEmpty(query)) return;

        var queryParams = query.TrimStart('?').Split('&');
        foreach (var param in queryParams)
        {
            if (string.IsNullOrWhiteSpace(param)) continue;
            var kvp = param.Split(new[] { '=' }, 2);
            string key = Uri.UnescapeDataString(kvp[0]).ToLower();
            string value = kvp.Length > 1 ? Uri.UnescapeDataString(kvp[1]) : "true";
            deepLinkArgs[key] = value;
        }
    }

    public static string Get(string key)
    {
        key = key.ToLower();
        if (deepLinkArgs.TryGetValue(key, out var val))
            return val;

        if (args.TryGetValue(key, out val))
            return val;

        /*
        if (key == "baseurl")
            return "https://nuglox.com/";
        */

        return null;
    }

    public static bool Exists(string key)
    {
        key = key.ToLower();
        if (deepLinkArgs.ContainsKey(key))
            return true;

        return args.ContainsKey(key);
    }
}