using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class ObjectClass : NetworkBehaviour
{
    public string className;
    static PhysicMaterial PhysicsEngineMat;

    [SyncVar(hook = nameof(OnNameChanged))]
    private string syncedName;

    void Start()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        if (isServer && currentScene.name == "Player")
        {
            if (gameObject.name.EndsWith("(Clone)"))
                gameObject.name = gameObject.name.Replace("(Clone)", "").Trim();
        }

        if (!isServer && currentScene.name == "Player")
        {
            if (gameObject.name.EndsWith("(Clone)"))
                gameObject.name = gameObject.name.Replace("(Clone)", "").Trim();
        }

        if (currentScene.name == "Studio")
        {
            if (transform.parent == null || transform.parent.GetComponent<ObjectClass>()?.className != "PlayerDefaults")
                RenameIfDuplicate();
            else
                RemoveSuffix();

            syncedName = gameObject.name;
        }

        if (currentScene.name == "BCS")
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.detectCollisions = false;
            }
        }

        if (currentScene.name == "Studio")
        {
            NetworkIdentity ni = GetComponent<NetworkIdentity>();
            if (ni != null)
                Destroy(ni);
        }

        if (!DataModel.TrustCheckDataModel(gameObject))
        {
            Destroy(gameObject);
        }

        NewPhysicsEngine();
    }

    void RenameIfDuplicate()
    {
        string baseName = gameObject.name;
        int suffix = 1;

        bool Exists(string name)
        {
            var objs = GameObject.FindObjectsOfType<GameObject>();
            foreach (var obj in objs)
            {
                if (obj != gameObject && obj.name == name)
                    return true;
            }
            return false;
        }

        while (Exists(gameObject.name))
        {
            gameObject.name = baseName + "-" + suffix;
            suffix++;
        }
    }

    void RemoveSuffix()
    {
        string name = gameObject.name;
        int dashIndex = name.LastIndexOf('-');
        if (dashIndex > 0)
        {
            string possibleNumber = name.Substring(dashIndex + 1);
            if (int.TryParse(possibleNumber, out _))
            {
                gameObject.name = name.Substring(0, dashIndex);
            }
        }
    }

    void OnNameChanged(string oldName, string newName)
    {
        if (!isServer)
            gameObject.name = newName;
    }

    void NewPhysicsEngine()
    {
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.solverIterations = 12;
            rb.solverVelocityIterations = 4;
            rb.useGravity = true;
            rb.drag = 0;
            rb.angularDrag = 0.05f;
        }

        var col = GetComponent<Collider>();
        if (col != null)
        {
            if (PhysicsEngineMat == null)
            {
                PhysicsEngineMat = new PhysicMaterial("NUGLOXPhysics");
                PhysicsEngineMat.dynamicFriction = 0.1f;
                PhysicsEngineMat.staticFriction = 0.1f;
                PhysicsEngineMat.bounciness = 0.5f;
                PhysicsEngineMat.frictionCombine = PhysicMaterialCombine.Multiply;
                PhysicsEngineMat.bounceCombine = PhysicMaterialCombine.Maximum;
            }
            col.material = PhysicsEngineMat;
        }

        Time.fixedDeltaTime = 1f / 60f;
        Physics.gravity = new Vector3(0, -25, 0);
    }

    void FixedUpdate()
    {
        PreventWallStick();
    }

    void PreventWallStick()
    {
        var rb = GetComponent<Rigidbody>();
        if (rb == null) return;

        Vector3 vel = rb.velocity;
        if (vel.magnitude < 0.01f) return;

        RaycastHit hit;
        if (Physics.SphereCast(transform.position, 0.5f, vel.normalized, out hit, 0.6f))
        {
            Vector3 normal = hit.normal;

            float dot = Vector3.Dot(vel, normal);
            if (dot < 0)
                rb.velocity = vel - normal * dot;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.rigidbody != null && !collision.rigidbody.isKinematic)
        {
            if (GetComponent<Rigidbody>() != null && !GetComponent<Rigidbody>().isKinematic)
            {
                Vector3 vel = collision.rigidbody.velocity;
                if (Vector3.Dot(Vector3.up, collision.contacts[0].normal) > 0.5f)
                {
                    GetComponent<Rigidbody>().position += vel * Time.fixedDeltaTime;
                }
            }
        }
    }
}
