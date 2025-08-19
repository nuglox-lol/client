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
            rb.interpolation = RigidbodyInterpolation.None;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rb.solverIterations = 6;
            rb.solverVelocityIterations = 1;
            rb.useGravity = true;
        }

        var col = GetComponent<Collider>();
        if (col != null)
        {
            if (PhysicsEngineMat == null)
            {
                PhysicsEngineMat = new PhysicMaterial("RBX2009");
                PhysicsEngineMat.dynamicFriction = 0.3f;
                PhysicsEngineMat.staticFriction = 0.3f;
                PhysicsEngineMat.bounciness = 0.5f;
                PhysicsEngineMat.frictionCombine = PhysicMaterialCombine.Minimum;
                PhysicsEngineMat.bounceCombine = PhysicMaterialCombine.Average;
            }
            col.material = PhysicsEngineMat;
        }

        Time.fixedDeltaTime = 1f / 60f;
        Physics.gravity = new Vector3(0, -20, 0);
    }
}