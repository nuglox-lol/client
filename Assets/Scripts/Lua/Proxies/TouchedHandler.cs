using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;
using Mirror;

public class TouchedHandler : MonoBehaviour
{
    private Dictionary<string, LuaEvent> touchedEvents = new Dictionary<string, LuaEvent>();
    private Dictionary<GameObject, float> lastTouchTime = new Dictionary<GameObject, float>();
    private float touchCooldown = 0.5f;
    private float checkInterval = 0.2f;
    private float lastCheckTime = 0f;
    public float detectionRadius = 1f;

    public LuaEvent GetTouchedEvent(string name)
    {
        if (!touchedEvents.ContainsKey(name))
        {
            ScriptService luaScriptManager = FindLuaScriptManager();
            if (luaScriptManager != null)
            {
                touchedEvents[name] = new LuaEvent(gameObject, luaScriptManager.script);
            }
            else
            {
                Debug.LogError("LuaScriptManager not found or missing 'script' property.");
            }
        }
        return touchedEvents[name];
    }

    private void OnCollisionEnter(Collision collision)
    {
        TriggerTouch(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        TriggerTouch(other);
    }

    private void OnCollisionStay(Collision collision)
    {
        TriggerTouch(collision.collider);
    }

    private void OnTriggerStay(Collider other)
    {
        TriggerTouch(other);
    }

    private void Update()
    {
        if (!IsServer()) return;

        if (Time.time - lastCheckTime > checkInterval)
        {
            lastCheckTime = Time.time;
            CheckForTouches();
        }
    }

    private void CheckForTouches()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (var col in hits)
        {
            if (col.gameObject != gameObject)
            {
                TriggerTouch(col);
            }
        }
    }

    private void TriggerTouch(Collider collider)
    {
        GameObject key = collider.gameObject;
        float time = Time.time;
        if (lastTouchTime.TryGetValue(key, out var value) && time - value < touchCooldown)
        {
            return;
        }
        lastTouchTime[key] = time;
        ScriptService luaScriptManager = FindLuaScriptManager();
        if (luaScriptManager == null)
        {
            return;
        }

        InstanceDatamodel instance = LuaInstance.GetCorrectInstance(key, luaScriptManager.script);
        DynValue hitInstanceDatamodel = instance != null ? UserData.Create(instance) : DynValue.Nil;

        foreach (LuaEvent luaEvent in touchedEvents.Values)
        {
            luaEvent.Fire(hitInstanceDatamodel);
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

    private bool IsServer()
    {
        return NetworkServer.active;
    }
}