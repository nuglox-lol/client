using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;
using Mirror;

public class ClickDetector : NetworkBehaviour
{
    private Dictionary<string, LuaEvent> clickEvents = new Dictionary<string, LuaEvent>();
    private Dictionary<NetworkIdentity, float> lastClickTime = new Dictionary<NetworkIdentity, float>();
    private float clickCooldown = 0.5f;

    public LuaEvent GetClickEvent(string name)
    {
        if (!clickEvents.ContainsKey(name))
        {
            ScriptService luaScriptManager = FindLuaScriptManager();
            if (luaScriptManager != null)
            {
                clickEvents[name] = new LuaEvent(gameObject, luaScriptManager.script);
            }
            else
            {
                Debug.LogError("LuaScriptManager not found or missing 'script' property.");
            }
        }
        return clickEvents[name];
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.gameObject == gameObject || hit.collider.gameObject == transform.parent.gameObject)
                {
                    CmdHandleClick();
                }
            }
        }
    }

    [Command(requiresAuthority = false)]
    void CmdHandleClick(NetworkConnectionToClient sender = null)
    {
        NetworkIdentity playerIdentity = sender.identity;
        float time = Time.time;

        if (lastClickTime.TryGetValue(playerIdentity, out float lastTime) && time - lastTime < clickCooldown)
            return;

        lastClickTime[playerIdentity] = time;

        FireClickEvent(playerIdentity.gameObject);
    }

    private void FireClickEvent(GameObject player)
    {
        ScriptService luaScriptManager = FindLuaScriptManager();
        if (luaScriptManager == null) return;

        InstanceDatamodel instance = LuaInstance.GetCorrectInstance(player, luaScriptManager.script);
        DynValue clickInstance = instance != null ? UserData.Create(instance) : DynValue.Nil;

        foreach (LuaEvent luaEvent in clickEvents.Values)
        {
            luaEvent.Fire(clickInstance);
        }
    }

    private ScriptService FindLuaScriptManager()
    {
        ScriptService[] array = Object.FindObjectsOfType<ScriptService>();
        foreach (ScriptService luaScriptManager in array)
        {
            if (luaScriptManager != null && luaScriptManager.script != null)
            {
                return luaScriptManager;
            }
        }
        return null;
    }
}
