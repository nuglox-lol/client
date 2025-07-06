using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshTiling : MonoBehaviour
{
    public Mesh baseMesh;
    public bool autoFromScale = true;
    public Vector3 tileSpacing = Vector3.one;

    MeshFilter mf;
    Mesh generatedMesh;
    Vector3 lastPos;
    Quaternion lastRot;
    Vector3 lastScale;

    void Start()
    {
        mf = GetComponent<MeshFilter>();
        Generate();
    }

    void Update()
    {
        if (transform.position != lastPos || transform.rotation != lastRot || transform.localScale != lastScale)
            Generate();
    }

    void Generate()
    {
        if (!baseMesh) return;

        lastPos = transform.position;
        lastRot = transform.rotation;
        lastScale = transform.localScale;

        Vector3 fullSize = Vector3.Scale(transform.localScale, Vector3.one);
        Vector3 tileSize = tileSpacing;

        int countY = Mathf.FloorToInt(fullSize.y / tileSize.y);
        int countX = Mathf.FloorToInt(fullSize.x / tileSize.x);
        int countZ = Mathf.FloorToInt(fullSize.z / tileSize.z);

        int partialY = (fullSize.y % tileSize.y) > 0 ? 1 : 0;
        int partialX = (fullSize.x % tileSize.x) > 0 ? 1 : 0;
        int partialZ = (fullSize.z % tileSize.z) > 0 ? 1 : 0;

        int totalX = countX + partialX;
        int totalY = countY + partialY;
        int totalZ = countZ + partialZ;

        int tileCount = totalX * totalY * totalZ;

        int vCount = baseMesh.vertexCount;
        int tCount = baseMesh.triangles.Length;

        Vector3[] bVerts = baseMesh.vertices;
        Vector3[] bNormals = baseMesh.normals;
        Vector2[] bUVs = baseMesh.uv;
        int[] bTris = baseMesh.triangles;

        Vector3[] verts = new Vector3[vCount * tileCount];
        Vector3[] norms = new Vector3[vCount * tileCount];
        Vector2[] uvs = new Vector2[vCount * tileCount];
        int[] tris = new int[tCount * tileCount];

        int ti = 0;
        for (int x = 0; x < totalX; x++)
        {
            for (int y = 0; y < totalY; y++)
            {
                for (int z = 0; z < totalZ; z++)
                {
                    float sx = (x == totalX - 1 && partialX == 1) ? fullSize.x - (tileSize.x * countX) : tileSize.x;
                    float sy = (y == totalY - 1 && partialY == 1) ? fullSize.y - (tileSize.y * countY) : tileSize.y;
                    float sz = (z == totalZ - 1 && partialZ == 1) ? fullSize.z - (tileSize.z * countZ) : tileSize.z;

                    Vector3 offset = transform.right * tileSize.x * x + transform.up * tileSize.y * y + transform.forward * tileSize.z * z;

                    Matrix4x4 m = Matrix4x4.TRS(
                        transform.position + offset,
                        transform.rotation,
                        new Vector3(sx, sy, sz)
                    );

                    for (int v = 0; v < vCount; v++)
                    {
                        int vi = ti * vCount + v;
                        verts[vi] = transform.InverseTransformPoint(m.MultiplyPoint3x4(bVerts[v]));
                        norms[vi] = bNormals[v];
                        uvs[vi] = bUVs[v];
                    }

                    for (int t = 0; t < tCount; t++)
                        tris[ti * tCount + t] = bTris[t] + ti * vCount;

                    ti++;
                }
            }
        }

        if (!generatedMesh) generatedMesh = new Mesh();
        generatedMesh.Clear();
        generatedMesh.vertices = verts;
        generatedMesh.normals = norms;
        generatedMesh.uv = uvs;
        generatedMesh.triangles = tris;

        mf.mesh = generatedMesh;
    }
}