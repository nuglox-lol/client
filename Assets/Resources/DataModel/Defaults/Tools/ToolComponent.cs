using Mirror;
using UnityEngine;
using System;
using System.Collections;

public class ToolComponent : NetworkBehaviour
{
    public float swingDuration = 0.3f;
    public float pickupRange = 1.5f;

    private bool isSwinging = false;
    private Transform leftArm;
    private bool isEquipped = false;
    private Animator animator;

    public bool IsDisabled { get; set; } = false;

    public event Action<GameObject> OnEquipped;
    public event Action<GameObject> OnActivated;
    public event Action<GameObject> OnUnequipped;

    private float pickupCooldown = 0f;

    void Start()
    {
        if (isServer)
            pickupCooldown = 1f;
            
        isEquipped = false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }

    void Update()
    {
        if (isServer)
        {
            pickupCooldown -= Time.deltaTime;
            if (!isEquipped)
                CheckForPickupOverlapServer();
        }

        if (isClient && isEquipped && !isSwinging && Input.GetMouseButtonDown(0))
        {
            if (IsDisabled) return;
            isSwinging = true;
            PlaySwingAnimation();
            CmdActivate();
        }
    }

    void CheckForPickupOverlapServer()
    {
        if (pickupCooldown > 0f)
            return;

        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRange, ~0, QueryTriggerInteraction.Collide);
        foreach (var hit in hits)
        {
            var root = hit.transform.root;
            var objectClass = root.GetComponent<ObjectClass>();
            if (objectClass != null && objectClass.className == "Player")
            {
                var connection = root.GetComponent<NetworkIdentity>()?.connectionToClient;
                if (connection != null)
                {
                    if (!netIdentity.isOwned)
                        netIdentity.AssignClientAuthority(connection);

                    TargetAttachTool(connection);
                    pickupCooldown = 1f;
                    break;
                }
            }
        }
    }

    [TargetRpc]
    void TargetAttachTool(NetworkConnection target)
    {
        GameObject playerRoot = NetworkClient.localPlayer.gameObject;
        var attachmentPoint = playerRoot.transform.Find("LeftArm/ToolAttachmentPoint");
        if (attachmentPoint == null) return;

        transform.SetParent(attachmentPoint, false);

        transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        float zPos = transform.localScale.y * 1.42857f;
        transform.localPosition = new Vector3(0, 0, zPos);

        leftArm = playerRoot.transform.Find("LeftArm");
        animator = leftArm?.parent?.GetComponent<Animator>();

        if (animator != null)
        {
            animator.ResetTrigger("Idle");
            animator.SetTrigger("Tool");
        }

        isEquipped = true;

        OnEquipped?.Invoke(playerRoot);
    }

    void PlaySwingAnimation()
    {
        if (animator == null && leftArm != null)
            animator = leftArm.parent?.GetComponent<Animator>();

        if (animator != null)
        {
            animator.ResetTrigger("Idle");
            animator.SetTrigger("Swing");
            StartCoroutine(ResetSwingFlagAfterAnimation());
        }
    }

    IEnumerator ResetSwingFlagAfterAnimation()
    {
        yield return new WaitForSeconds(swingDuration);
        isSwinging = false;
    }

    [Command(requiresAuthority = false)]
    void CmdActivate()
    {
        DoActivate();
    }

    void DoActivate()
    {
        if (!isServer) return;

        OnActivated?.Invoke(gameObject);
        RpcActivate();
    }

    [ClientRpc]
    void RpcActivate()
    {
    }

    [Server]
    public void Unequip()
    {
        if (netIdentity.connectionToClient != null)
            netIdentity.RemoveClientAuthority();

        transform.SetParent(null, true);

        isEquipped = false;

        if (animator != null)
        {
            animator.ResetTrigger("Tool");
            animator.ResetTrigger("Swing");
            animator.SetTrigger("Idle");
        }

        leftArm = null;
        animator = null;

        RpcOnUnequipped();
    }

    [ClientRpc]
    void RpcOnUnequipped()
    {
        OnUnequipped?.Invoke(gameObject);
    }

    public void SetDisabled(bool disabled)
    {
        IsDisabled = disabled;
        if (disabled && animator != null)
        {
            animator.ResetTrigger("Tool");
            animator.ResetTrigger("Swing");
            animator.SetTrigger("Idle");
        }
    }
}