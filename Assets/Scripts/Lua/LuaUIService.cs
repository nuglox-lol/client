using Mirror;
using UnityEngine;
using MoonSharp.Interpreter;
using ImGuiNET;
using ImGuiNET.Unity;

public class LuaUIService
{
    public static bool IsLocal = false;
    public static string ConnId = "";
    private static DynValue luaDrawFn;

    private static DearImGui _dearImGui;

    public static void Init(string connId, bool isLocal = false)
    {
        IsLocal = isLocal;
        ConnId = isLocal ? "" : connId;

        if (_dearImGui == null)
        {
            _dearImGui = Object.FindObjectOfType<DearImGui>();
            if (_dearImGui == null)
            {
                Debug.LogError("DearImGui component not found in scene! Please add it to a GameObject with a Camera.");
                return;
            }

            _dearImGui.Layout += DoLayout;
        }
    }

    public static void RegisterDraw(DynValue drawFn)
    {
        luaDrawFn = drawFn;
    }

    public static void DoLayout()
    {
        if (IsLocal)
        {
            luaDrawFn?.Function.Call();
            return;
        }

        if (NetworkServer.active)
        {
            if (IsServerHost())
            {
                luaDrawFn?.Function.Call();
                return;
            }
        }
        
        if (!string.IsNullOrEmpty(ConnId) && IsLocalPlayer(ConnId))
        {
            luaDrawFn?.Function.Call();
        }
    }

    static bool IsLocalPlayer(string connId)
    {
        if (NetworkClient.connection == null) return false;

        var localPlayer = NetworkClient.connection.identity;
        if (localPlayer == null) return false;

        return localPlayer.netId.ToString() == connId;
    }

    static bool IsServerHost()
    {
        // Running as host means server and client in one process
        return NetworkServer.active && NetworkClient.isConnected && NetworkClient.connection != null;
    }

    public static string GetLocalNetID()
    {
        if (NetworkClient.connection == null) return "";
        var identity = NetworkClient.connection.identity;
        if (identity == null) return "";
        return identity.netId.ToString();
    }

    public static void Shutdown()
    {
        if (_dearImGui != null)
        {
            _dearImGui.Layout -= DoLayout;
            _dearImGui = null;
        }
    }
}
