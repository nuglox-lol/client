using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class StudRenderer : MonoBehaviour
{
	public float tilePerUnit = 1f;

	private Renderer rend;

	private MeshRenderer topFaceRenderer;

	private Vector3 lastScale;

	private void Start()
	{
		SetupStudRenderer();
		Shader.Find("Unlit/Transparent");
	}

	public void SetupStudRenderer()
	{
		Transform transform = base.transform.Find("TopFace");
		if (transform != null)
		{
			topFaceRenderer = transform.GetComponent<MeshRenderer>();
			if (topFaceRenderer == null)
			{
				topFaceRenderer = transform.gameObject.AddComponent<MeshRenderer>();
			}
			rend = topFaceRenderer;
			TryInitMaterial();
		}
		else
		{
			Debug.LogError("TopFace child object not found!");
		}
	}

	private void Update()
	{
		if (topFaceRenderer == null)
		{
			Debug.LogError("TopFace renderer is missing.");
		}
		else if (base.transform.localScale != lastScale)
		{
			UpdateTiling();
			lastScale = base.transform.localScale;
		}
	}

	private void TryInitMaterial()
	{
		if (rend == null)
		{
			return;
		}
		Texture2D texture2D = Resources.Load<Texture2D>("Textures/Studs");
		if (texture2D == null)
		{
			Debug.LogError("Texture not found in Resources/Textures/Studs.png!");
			return;
		}
		Material material = rend.material;
		Shader shader = Shader.Find("Standard");
		if (shader == null)
		{
			Debug.LogError("Shader 'Standard' not found!");
			return;
		}
		material.shader = shader;
		material.mainTexture = texture2D;
		material.mainTexture.wrapMode = TextureWrapMode.Repeat;
		material.SetFloat("_Mode", 3f);
		material.SetInt("_SrcBlend", 5);
		material.SetInt("_DstBlend", 10);
		material.SetInt("_ZWrite", 0);
		material.DisableKeyword("_ALPHATEST_ON");
		material.EnableKeyword("_ALPHABLEND_ON");
		material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
		material.renderQueue = 3000;
	}

	private void UpdateTiling()
	{
		if (!(rend == null))
		{
			Vector3 localScale = base.transform.localScale;
			rend.material.mainTextureScale = new Vector2(localScale.x * tilePerUnit, localScale.z * tilePerUnit);
		}
	}
}
