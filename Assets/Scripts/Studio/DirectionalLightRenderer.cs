using UnityEngine;

public class DirectionalLightRuntimeGizmo : MonoBehaviour
{
    Material lineMaterial;

    void Awake()
    {
        Shader shader = Shader.Find("Hidden/Internal-Colored");
        lineMaterial = new Material(shader);
        lineMaterial.hideFlags = HideFlags.HideAndDontSave;
        lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        lineMaterial.SetInt("_ZWrite", 0);
    }

    void OnPostRender()
    {
        if (!lineMaterial) return;
        lineMaterial.SetPass(0);

        GL.PushMatrix();
        GL.LoadProjectionMatrix(Camera.current.projectionMatrix);
        GL.modelview = Camera.current.worldToCameraMatrix;

        GL.Begin(GL.LINES);
        GL.Color(Color.yellow);

        Light[] lights = FindObjectsOfType<Light>();
        foreach (var light in lights)
        {
            if (light.type == LightType.Directional)
            {
                Vector3 pos = light.transform.position;
                Vector3 dir = light.transform.forward.normalized;

                GL.Vertex(pos);
                GL.Vertex(pos + dir * 3f);

                Vector3 right = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 150, 0) * Vector3.forward;
                Vector3 left = Quaternion.LookRotation(dir) * Quaternion.Euler(0, -150, 0) * Vector3.forward;
                GL.Vertex(pos + dir * 3f);
                GL.Vertex(pos + right * 0.5f + dir * 2.5f);
                GL.Vertex(pos + dir * 3f);
                GL.Vertex(pos + left * 0.5f + dir * 2.5f);
            }
        }

        GL.End();
        GL.PopMatrix();
    }

    void OnDestroy()
    {
        if (lineMaterial)
            DestroyImmediate(lineMaterial);
    }
}
