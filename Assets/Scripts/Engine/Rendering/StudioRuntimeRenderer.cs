using UnityEngine;
using System.Collections.Generic;

public class StudioRuntimeRenderer : MonoBehaviour
{
    private Material lineMaterial;
    private List<Vector3> textPositions = new List<Vector3>();
    private List<Vector3> toolTextPositions = new List<Vector3>();

    private void Awake()
    {
        Shader shader = Shader.Find("Hidden/Internal-Colored");
        lineMaterial = new Material(shader);
        lineMaterial.hideFlags = HideFlags.HideAndDontSave;
        lineMaterial.SetInt("_SrcBlend", 5);
        lineMaterial.SetInt("_DstBlend", 10);
        lineMaterial.SetInt("_Cull", 0);
        lineMaterial.SetInt("_ZWrite", 0);
    }

    private void OnPostRender()
    {
        if (!lineMaterial) return;

        textPositions.Clear();
        toolTextPositions.Clear();

        lineMaterial.SetPass(0);
        GL.PushMatrix();
        GL.LoadProjectionMatrix(Camera.current.projectionMatrix);
        GL.modelview = Camera.current.worldToCameraMatrix;

        GL.Begin(GL.LINES);

        GL.Color(Color.yellow);
        Light[] lights = Object.FindObjectsOfType<Light>();
        foreach (Light light in lights)
        {
            if (light.type == LightType.Directional)
            {
                Vector3 pos = light.transform.position;
                Vector3 dir = light.transform.forward.normalized;

                GL.Vertex(pos);
                GL.Vertex(pos + dir * 3f);

                Vector3 leftWing = Quaternion.LookRotation(dir) * Quaternion.Euler(0f, 150f, 0f) * Vector3.forward;
                Vector3 rightWing = Quaternion.LookRotation(dir) * Quaternion.Euler(0f, -150f, 0f) * Vector3.forward;

                GL.Vertex(pos + dir * 3f);
                GL.Vertex(pos + leftWing * 0.5f + dir * 2.5f);

                GL.Vertex(pos + dir * 3f);
                GL.Vertex(pos + rightWing * 0.5f + dir * 2.5f);

                textPositions.Add(pos + dir * -0.5f);
            }
        }

        GL.Color(Color.red);
        var toolObjects = GameObject.FindGameObjectsWithTag("Object");
        foreach (var obj in toolObjects)
        {
            var oc = obj.GetComponent<ObjectClass>();
            if (oc != null && oc.className == "Tool")
            {
                Vector3 pos = obj.transform.position;
                Vector3 dir = obj.transform.forward.normalized;

                GL.Vertex(pos);
                GL.Vertex(pos + dir * 2f);

                Vector3 leftWing = Quaternion.LookRotation(dir) * Quaternion.Euler(0f, 150f, 0f) * Vector3.forward;
                Vector3 rightWing = Quaternion.LookRotation(dir) * Quaternion.Euler(0f, -150f, 0f) * Vector3.forward;

                GL.Vertex(pos + dir * 2f);
                GL.Vertex(pos + leftWing * 0.3f + dir * 1.7f);

                GL.Vertex(pos + dir * 2f);
                GL.Vertex(pos + rightWing * 0.3f + dir * 1.7f);

                toolTextPositions.Add(pos + Vector3.up * 1f);
            }
        }

        GL.End();
        GL.PopMatrix();
    }

    private void OnGUI()
    {
        if (Camera.current == null) return;

        foreach (var worldPos in textPositions)
        {
            Vector3 screenPos = Camera.current.WorldToScreenPoint(worldPos);
            if (screenPos.z > 0)
            {
                screenPos.y = Screen.height - screenPos.y;
                Vector2 size = GUI.skin.label.CalcSize(new GUIContent("Directional Light"));
                Rect rect = new Rect(screenPos.x - size.x / 2, screenPos.y - size.y, size.x, size.y);
                GUI.color = Color.yellow;
                GUI.Label(rect, "Directional Light");
            }
        }

        foreach (var worldPos in toolTextPositions)
        {
            Vector3 screenPos = Camera.current.WorldToScreenPoint(worldPos);
            if (screenPos.z > 0)
            {
                screenPos.y = Screen.height - screenPos.y;
                Vector2 size = GUI.skin.label.CalcSize(new GUIContent("Tool"));
                Rect rect = new Rect(screenPos.x - size.x / 2, screenPos.y - size.y, size.x, size.y);
                GUI.color = Color.red;
                GUI.Label(rect, "Tool");
            }
        }
    }

    private void OnDestroy()
    {
        if (lineMaterial)
        {
            DestroyImmediate(lineMaterial);
        }
    }
}
