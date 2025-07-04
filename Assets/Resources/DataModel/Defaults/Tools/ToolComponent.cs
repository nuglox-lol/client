using Mirror;
using UnityEngine;
using System;
using System.Collections;

public class ToolComponent : NetworkBehaviour
{
    public float swingDuration = 0.3f;
    private bool isSwinging = false;
    private Transform leftArm;
    private bool isEquipped = false;
    private Animator animator;

    public event Action<GameObject> OnEquipped;
    public event Action<GameObject> OnActivated;
    public event Action<GameObject> OnUnequipped;

    void Start()
    {
        if (!IsOwner()) return;

        if (transform.parent != null && transform.parent.parent != null)
        {
            leftArm = transform.parent.parent;
            if (leftArm != null)
            {
                animator = leftArm.parent?.GetComponent<Animator>();
                isEquipped = true;
                if (animator != null)
                    PlayAnimationNetworked("Tool");
                OnEquipped?.Invoke(leftArm.root.gameObject);
            }
        }
    }

    void Update()
    {
        if (!IsOwner()) return;

        if (Input.GetMouseButtonDown(0) && !isSwinging && isEquipped && leftArm != null)
        {
            isSwinging = true;
            PlaySwingAnimationNetworked();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServerOrHost()) return;

        var root = other.transform.root;

        if (!isEquipped)
        {
            var attachmentPoint = root.Find("LeftArm/ToolAttachmentPoint");
            if (attachmentPoint != null)
            {
                transform.SetParent(attachmentPoint, false);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                transform.localScale = Vector3.one;

                leftArm = root.Find("LeftArm");
                if (leftArm != null)
                {
                    isEquipped = true;
                    animator = leftArm.parent?.GetComponent<Animator>();
                    PlayAnimationNetworked("Tool");
                    OnEquipped?.Invoke(root.gameObject);
                }
            }
        }
    }

    void PlaySwingAnimationNetworked()
    {
        if (NetworkClient.active)
        {
            CmdPlaySwingAnimation();
        }
        else
        {
            PlaySwingAnimation();
        }
    }

    [Command]
    void CmdPlaySwingAnimation()
    {
        RpcPlaySwingAnimation();
    }

    [ClientRpc]
    void RpcPlaySwingAnimation()
    {
        PlaySwingAnimation();
    }

    void PlaySwingAnimation()
    {
        if (animator == null && leftArm != null)
            animator = leftArm.parent?.GetComponent<Animator>();

        if (animator != null)
        {
            animator.SetTrigger("Swing");
            StartCoroutine(ResetSwingFlagAfterAnimation());
        }
    }

    IEnumerator ResetSwingFlagAfterAnimation()
    {
        yield return new WaitForSeconds(swingDuration);
        isSwinging = false;
    }

    void PlayAnimationNetworked(string anim)
    {
        if (NetworkClient.active)
            CmdPlayAnimation(anim);
        else
            PlayAnimation(anim);
    }

    void PlayAnimation(string animName)
    {
        if (animator == null && leftArm != null)
            animator = leftArm.parent?.GetComponent<Animator>();
        if (animator != null)
        {
            animator.Play(animName);
        }
    }

    bool IsOwner()
    {
        return !NetworkClient.active || isLocalPlayer;
    }

    bool IsServerOrHost()
    {
        return !NetworkClient.active || isServer;
    }

    [Command]
    void CmdPlayAnimation(string anim)
    {
        RpcPlayAnimation(anim);
    }

    [ClientRpc]
    void RpcPlayAnimation(string animName)
    {
        PlayAnimation(animName);
    }
}