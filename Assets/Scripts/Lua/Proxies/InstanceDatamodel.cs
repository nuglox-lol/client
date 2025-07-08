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
		get
		{
			if (!(go != null))
			{
				return Vector3.zero;
			}
			return go.transform.position;
		}
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
		get
		{
			if (!(go != null))
			{
				return Vector3.zero;
			}
			return go.transform.eulerAngles;
		}
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
		get
		{
			if (!(go != null))
			{
				return Vector3.zero;
			}
			return go.transform.localScale;
		}
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
			if (!(renderer != null))
			{
				return Color.white;
			}
			return renderer.material.color;
		}
		set
		{
			Renderer renderer = go?.GetComponent<Renderer>();
			if (renderer != null)
			{
				renderer.material.color = value;
				OnPropertyChanged("Color");
			}
			//go.GetComponent<ColorSync>().SetColor(value);
		}
	}

	public float Transparency
	{
		get
		{
			Renderer renderer = go?.GetComponent<Renderer>();
			if (renderer != null)
			{
				return 1f - renderer.material.color.a;
			}
			return 0f;
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
			if (rigidbody != null)
			{
				return rigidbody.isKinematic;
			}
			return false;
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
		get
		{
			if (selfCollider != null)
			{
				return selfCollider.enabled;
			}
			return false;
		}
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
		get
		{
			if (go != null)
			{
				return go.activeSelf;
			}
			return false;
		}
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
		get
		{
			if (go == null)
			{
				return string.Empty;
			}
			if (string.IsNullOrEmpty(go.name))
			{
				return string.Empty;
			}
			return go.name;
		}
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
	        if (go == null)
	            return null;

	        Transform parent = go.transform.parent;
	        if (parent == null)
	            return null;

	        GameObject parentGO = parent.gameObject;
	        ObjectClass oc = parentGO.GetComponent<ObjectClass>();

	        if (oc != null && !string.IsNullOrEmpty(oc.className))
	        {
	            switch (oc.className)
	            {
	                case "Tool":
	                    return new InstanceTool(parentGO, luaScript);
	                case "Player":
	                    return new InstancePlayer(parentGO);
	                default:
	                    return new InstanceDatamodel(parentGO, luaScript);
	            }
	        }

	        return new InstanceDatamodel(parentGO, luaScript);
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

	public Transform Transform
	{
		get
		{
			if (!(go != null))
			{
				return null;
			}
			return go.transform;
		}
	}

	public InstanceRigidbody Rigidbody
	{
		get
		{
			if (go == null)
			{
				return null;
			}
			Rigidbody component = go.GetComponent<Rigidbody>();
			if (!(component != null))
			{
				return null;
			}
			return new InstanceRigidbody(component);
		}
	}

	public int Layer
	{
		get
		{
			if (!(go != null))
			{
				return 0;
			}
			return go.layer;
		}
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
		add
		{
			changedHandlers += value;
		}
		remove
		{
			changedHandlers -= value;
		}
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
		if (go == null)
		{
			return null;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(go, position, rotation);
		if (!(gameObject != null))
		{
			return null;
		}
		return new InstanceDatamodel(gameObject, luaScript);
	}

	private void OnPropertyChanged(string propertyName)
	{
		this.changedHandlers?.Invoke(propertyName);
	}

	public InstanceDatamodel FindFirstChild(string name)
	{
		if (go == null)
		{
			return null;
		}
		Transform transform = go.transform.Find(name);
		if (!(transform != null))
		{
			return null;
		}
		return new InstanceDatamodel(transform.gameObject, luaScript);
	}

	public List<InstanceDatamodel> GetChildren()
	{
		List<InstanceDatamodel> list = new List<InstanceDatamodel>();
		if (go == null)
		{
			return list;
		}
		foreach (Transform item in go.transform)
		{
			list.Add(new InstanceDatamodel(item.gameObject, luaScript));
		}
		return list;
	}

	public List<InstanceDatamodel> GetDescendants()
	{
		List<InstanceDatamodel> list = new List<InstanceDatamodel>();
		if (go == null)
		{
			return list;
		}
		foreach (Transform item in go.transform)
		{
			InstanceDatamodel instanceDatamodel = new InstanceDatamodel(item.gameObject, luaScript);
			list.Add(instanceDatamodel);
			list.AddRange(instanceDatamodel.GetDescendants());
		}
		return list;
	}

	public InstanceDatamodel WaitForChild(string childName, float timeoutSeconds = 5f)
	{
		for (float num = 0f; num < timeoutSeconds; num += Time.deltaTime)
		{
			InstanceDatamodel instanceDatamodel = FindFirstChild(childName);
			if (instanceDatamodel != null)
			{
				return instanceDatamodel;
			}
		}
		return null;
	}

	public bool IsA(string className)
	{
		if (go == null)
		{
			return false;
		}
		ObjectClass component = go.GetComponent<ObjectClass>();
		if (component != null)
		{
			return component.className == className;
		}
		return false;
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

	public void PlayAudio(string url, float radius = 10f, bool isLoop = false)
	{
		if (go == null)
		{
			return;
		}
		AudioSource audioSource = go.GetComponent<AudioSource>();
		if (audioSource == null)
		{
			audioSource = go.AddComponent<AudioSource>();
		}
		audioSource.spatialBlend = 1f;
		audioSource.maxDistance = radius;
		audioSource.rolloffMode = AudioRolloffMode.Linear;
		audioSource.loop = isLoop;
		audioSource.playOnAwake = false;
		UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG);
		request.SendWebRequest().completed += delegate
		{
			if (request.result != UnityWebRequest.Result.Success)
			{
				Console.Report("Failed to load audio from " + url + " !");
			}
			else
			{
				AudioClip content = DownloadHandlerAudioClip.GetContent(request);
				audioSource.clip = content;
				audioSource.Play();
			}
		};
	}

	public void StopAudio()
	{
		if (!(go == null))
		{
			AudioSource component = go.GetComponent<AudioSource>();
			if (component != null)
			{
				component.Stop();
				UnityEngine.Object.Destroy(component);
			}
		}
	}
}
