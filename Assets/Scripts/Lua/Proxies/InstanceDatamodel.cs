using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.Networking;

[MoonSharpUserData]
public class InstanceDatamodel
{
	private GameObject go;

	private Collider selfCollider;

	private LuaEvent touchedEvent;

	private Script luaScript;

	public InstanceDatamodel(GameObject gameObject, Script lua = null)
	{
		if (gameObject == null)
		{
			throw new ArgumentNullException("gameObject");
		}
		go = gameObject;
		luaScript = lua;
		selfCollider = go.GetComponent<Collider>();
		if (selfCollider == null)
		{
			Debug.LogWarning($"Collider not found on GameObject {go.name}. GameObject type: {go.GetType()}");
		}
		if (go.GetComponent<TouchedHandler>() == null)
		{
			go.AddComponent<TouchedHandler>();
		}
	}

	public object this[string key]
	{
		get
		{
			if (go == null || go.transform == null)
				return null;

			Transform childTransform = go.transform.Find(key);
			if (childTransform == null)
				return null;

			return LuaInstance.GetCorrectInstance(childTransform.gameObject, luaScript);
		}
		set
		{
			throw new NotSupportedException("Setting children via indexer is not supported.");
		}
	}

	public float Mass
	{
		get
		{
			Rigidbody rigidbody = go?.GetComponent<Rigidbody>();
			if (rigidbody != null)
			{
				return rigidbody.mass;
			}
			return 0f;
		}
		set
		{
			Rigidbody rigidbody = go?.GetComponent<Rigidbody>();
			if (rigidbody != null)
			{
				rigidbody.mass = value;
			}
			else if (go != null)
			{
				rigidbody = go.AddComponent<Rigidbody>();
				rigidbody.mass = value;
			}
			OnPropertyChanged("Mass");
		}
	}

	public string Class 
	{
		get => go.transform.GetComponent<ObjectClass>().className;
	}

	public Vector3 Position
	{
		get => go != null ? go.transform.position : Vector3.zero;
		set
		{
			if (go != null)
			{
				go.transform.position = value;
				OnPropertyChanged("Position");
			}
		}
	}

	public Vector3 Rotation
	{
		get => go != null ? go.transform.eulerAngles : Vector3.zero;
		set
		{
			if (go != null)
			{
				go.transform.eulerAngles = value;
				OnPropertyChanged("Rotation");
			}
		}
	}

	public Vector3 LocalScale
	{
		get => go != null ? go.transform.localScale : Vector3.zero;
		set
		{
			if (go != null)
			{
				go.transform.localScale = value;
				OnPropertyChanged("LocalScale");
			}
		}
	}

	public Color Color
	{
		get
		{
			Renderer renderer = go?.GetComponent<Renderer>();
			return renderer != null ? renderer.material.color : Color.white;
		}
		set
		{
			Renderer renderer = go?.GetComponent<Renderer>();
			if (renderer != null)
			{
				renderer.material.color = value;
				OnPropertyChanged("Color");
			}
			if (go.GetComponent<ColorSync>() != null)
			{
				go.GetComponent<ColorSync>().SetColor(value);
			}
		}
	}

	public float Transparency
	{
		get
		{
			Renderer renderer = go?.GetComponent<Renderer>();
			return renderer != null ? 1f - renderer.material.color.a : 0f;
		}
		set
		{
			if (go != null)
			{
				SetTransparencyRecursively(go, value);
				OnPropertyChanged("Transparency");
			}
		}
	}

	public bool Anchored
	{
		get
		{
			Rigidbody rigidbody = go?.GetComponent<Rigidbody>();
			return rigidbody != null && rigidbody.isKinematic;
		}
		set
		{
			Rigidbody rigidbody = go?.GetComponent<Rigidbody>();
			if (rigidbody != null)
			{
				rigidbody.isKinematic = value;
			}
			else if (value)
			{
				go.AddComponent<Rigidbody>().isKinematic = true;
			}
			OnPropertyChanged("Anchored");
		}
	}

	public bool CanCollide
	{
		get => selfCollider != null && selfCollider.enabled;
		set
		{
			if (selfCollider != null)
			{
				selfCollider.enabled = value;
				OnPropertyChanged("CanCollide");
			}
		}
	}

	public bool Active
	{
		get => go != null && go.activeSelf;
		set
		{
			if (go != null)
			{
				go.SetActive(value);
				OnPropertyChanged("Active");
			}
		}
	}

	public string Name
	{
		get => go != null ? go.name ?? string.Empty : string.Empty;
		set
		{
			if (go != null)
			{
				go.name = value;
				OnPropertyChanged("Name");
			}
		}
	}

	public InstanceDatamodel Parent
	{
		get
		{
			if (go == null) return null;

			Transform parent = go.transform.parent;
			if (parent == null) return null;

			return LuaInstance.GetCorrectInstance(parent.gameObject, luaScript) as InstanceDatamodel;
		}
		set
		{
			if (go != null)
			{
				go.transform.parent = value?.Transform;
				OnPropertyChanged("Parent");
			}
		}
	}

	public Transform Transform => go != null ? go.transform : null;

	public InstanceRigidbody Rigidbody
	{
		get
		{
			if (go == null) return null;
			Rigidbody component = go.GetComponent<Rigidbody>();
			return component != null ? new InstanceRigidbody(component) : null;
		}
	}

	public int Layer
	{
		get => go != null ? go.layer : 0;
		set
		{
			if (go != null)
			{
				go.layer = value;
				OnPropertyChanged("Layer");
			}
		}
	}

	public LuaEvent Touched
	{
		get
		{
			if (touchedEvent == null)
			{
				TouchedHandler component = go.GetComponent<TouchedHandler>();
				if (component != null)
				{
					touchedEvent = component.GetTouchedEvent(go.name);
				}
			}
			return touchedEvent;
		}
	}

	private event Action<string> changedHandlers;

	public event Action<string> Changed
	{
		add => changedHandlers += value;
		remove => changedHandlers -= value;
	}

	public void Destroy()
	{
		if (go != null)
		{
			UnityEngine.Object.Destroy(go);
		}
	}

	public InstanceDatamodel Clone(Vector3 position, Quaternion rotation)
	{
		if (go == null) return null;
		GameObject gameObject = UnityEngine.Object.Instantiate(go, position, rotation);
		return gameObject != null ? LuaInstance.GetCorrectInstance(gameObject, luaScript) as InstanceDatamodel : null;
	}

	private void OnPropertyChanged(string propertyName)
	{
		changedHandlers?.Invoke(propertyName);
	}

	public InstanceDatamodel FindFirstChild(string name)
	{
		if (go == null) return null;
		Transform transform = go.transform.Find(name);
		return transform != null ? LuaInstance.GetCorrectInstance(transform.gameObject, luaScript) as InstanceDatamodel : null;
	}

	public List<InstanceDatamodel> GetChildren()
	{
		List<InstanceDatamodel> list = new List<InstanceDatamodel>();
		if (go == null) return list;
		foreach (Transform item in go.transform)
		{
			var instance = LuaInstance.GetCorrectInstance(item.gameObject, luaScript) as InstanceDatamodel;
			if (instance != null) list.Add(instance);
		}
		return list;
	}

	public List<InstanceDatamodel> GetDescendants()
	{
		List<InstanceDatamodel> list = new List<InstanceDatamodel>();
		if (go == null) return list;
		foreach (Transform item in go.transform)
		{
			var instance = LuaInstance.GetCorrectInstance(item.gameObject, luaScript) as InstanceDatamodel;
			if (instance != null)
			{
				list.Add(instance);
				list.AddRange(instance.GetDescendants());
			}
		}
		return list;
	}

	public InstanceDatamodel WaitForChild(string childName, float timeoutSeconds = 5f)
	{
		for (float num = 0f; num < timeoutSeconds; num += Time.deltaTime)
		{
			InstanceDatamodel instance = FindFirstChild(childName);
			if (instance != null)
			{
				return instance;
			}
		}
		return null;
	}

	public bool IsA(string className)
	{
		if (go == null) return false;
		ObjectClass component = go.GetComponent<ObjectClass>();
		return component != null && component.className == className;
	}

	private void SetMaterialTransparent(Material mat)
	{
		mat.SetFloat("_Mode", 3f);
		mat.SetInt("_SrcBlend", 5);
		mat.SetInt("_DstBlend", 10);
		mat.SetInt("_ZWrite", 0);
		mat.DisableKeyword("_ALPHATEST_ON");
		mat.DisableKeyword("_ALPHABLEND_ON");
		mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
		mat.renderQueue = 3000;
	}

	private void SetTransparencyRecursively(GameObject obj, float transparency)
	{
		Renderer[] componentsInChildren = obj.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Material material = componentsInChildren[i].material;
			SetMaterialTransparent(material);
			Color color = material.color;
			color.a = 1f - Mathf.Clamp01(transparency);
			material.color = color;
		}
	}
}
