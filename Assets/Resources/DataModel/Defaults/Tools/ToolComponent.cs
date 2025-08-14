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
    private ParentSync parentSync;

    public bool IsDisabled { get; set; } = false;

    public event Action<GameObject> OnEquipped;
    public event Action<GameObject> OnActivated;
    public event Action<GameObject> OnUnequipped;

    private float pickupCooldown = 0f;

    void Start()
    {
        parentSync = GetComponent<ParentSync>();
        if (isServer) pickupCooldown = 1f;
        isEquipped = false;
    }

    void Update()
    {
        if (isServer)
        {
            pickupCooldown -= Time.deltaTime;
            if (!isEquipped) CheckForPickupOverlapServer();
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
        if (pickupCooldown > 0f) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRange, ~0, QueryTriggerInteraction.Collide);
        foreach (var hit in hits)
        {
            var root = hit.transform.root;
            var player = root.GetComponent<Player>();
            if (player != null)
            {
                AttachToPlayer(player);
                pickupCooldown = 1f;
                break;
            }
        }
    }

    [Server]
    void AttachToPlayer(Player player)
    {
        Transform attachmentPoint = player.transform.Find("LeftArm/ToolAttachmentPoint");
        if (attachmentPoint == null) return;

        transform.SetParent(attachmentPoint, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        if (parentSync != null)
            parentSync.ForceUpdate();

        leftArm = player.transform.Find("LeftArm");
        animator = leftArm?.parent?.GetComponent<Animator>();

        isEquipped = true;

        RpcOnEquipped(player.netIdentity);
        OnEquipped?.Invoke(player.gameObject);
    }

    void PlaySwingAnimation()
    {
        if (animator == null && leftArm != null) animator = leftArm.parent?.GetComponent<Animator>();
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

    [Server]
    void DoActivate()
    {
        OnActivated?.Invoke(gameObject);
        RpcActivate();
    }

    [ClientRpc]
    void RpcActivate() { }

    [Server]
    public void Unequip()
    {
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

        if (parentSync != null)
            parentSync.ForceUpdate();

        RpcOnUnequipped();
        OnUnequipped?.Invoke(gameObject);
    }

    [ClientRpc]
    void RpcOnUnequipped() { }

    [ClientRpc]
    void RpcOnEquipped(NetworkIdentity playerNetId)
    {
        GameObject playerObj = playerNetId != null ? playerNetId.gameObject : null;
        if (playerObj == null) return;
        Transform attachmentPoint = playerObj.transform.Find("LeftArm/ToolAttachmentPoint");
        if (attachmentPoint == null) return;
        transform.SetParent(attachmentPoint, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        if (parentSync != null)
            parentSync.ForceUpdate();

        leftArm = playerObj.transform.Find("LeftArm");
        animator = leftArm?.parent?.GetComponent<Animator>();
        isEquipped = true;
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
