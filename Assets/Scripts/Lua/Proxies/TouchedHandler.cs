using System.Collections.Generic;
using System.Collections;
using MoonSharp.Interpreter;
using UnityEngine;

public class TouchedHandler : MonoBehaviour
{
	private Dictionary<string, LuaEvent> touchedEvents = new Dictionary<string, LuaEvent>();

	private Dictionary<GameObject, float> lastTouchTime = new Dictionary<GameObject, float>();

	private float touchCooldown = 0.5f;

	public LuaEvent GetTouchedEvent(string name)
	{
		if (!touchedEvents.ContainsKey(name))
		{
			ScriptService luaScriptManager = FindLuaScriptManager();
			if (luaScriptManager != null)
			{
				touchedEvents[name] = new LuaEvent(base.gameObject, luaScriptManager.script);
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

	private void TriggerTouch(Collider collider)
	{
		GameObject key = collider.gameObject;
		float time = Time.time;
		if (lastTouchTime.TryGetValue(key, out var value) && time - value < touchCooldown)
		{
			return;
		}
		lastTouchTime[key] = time;
		if (FindLuaScriptManager() == null)
		{
			return;
		}
		DynValue hitInstanceDatamodel = UserData.Create(new InstanceDatamodel(key));
		foreach (LuaEvent value2 in touchedEvents.Values)
		{
			value2.Fire(hitInstanceDatamodel);
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
