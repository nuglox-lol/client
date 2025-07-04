using UnityEngine;
using Mirror;

public class PlayerInteractWithPhysics : NetworkBehaviour
{
    private float pushForce = 75f;

    void OnCollisionStay(Collision collision)
    {
        if (!isLocalPlayer || isServer) return;

        Rigidbody rb = collision.rigidbody;
        if (rb == null) return;

        NetworkIdentity netIdentity = rb.GetComponent<NetworkIdentity>();

        if (netIdentity == null) return;

        Vector3 pushDir = collision.contacts[0].point - transform.position;
        pushDir.y = 0;
        pushDir.Normalize();

        CmdPushObject(netIdentity.gameObject, pushDir * pushForce);
    }

    [Command]
    void CmdPushObject(GameObject obj, Vector3 force)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb == null) return;

        rb.AddForce(force, ForceMode.Impulse);
    }
}
