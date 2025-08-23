using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine.UI;

public class ShowTools : MonoBehaviour
{
    [SerializeField] private int maxTools = 7;

    private Transform layoutGroup;
    private GameObject toolUIPrefab;
    private Transform playerTransform;
    private Player playerScript;
    private List<Button> toolButtons = new List<Button>();
    private List<Outline> toolOutlines = new List<Outline>();

    private Color enabledOutlineColor = Color.blue;
    private Color disabledOutlineColor = Color.black;

    private int previousToolCount = -1;

    private void Awake()
    {
        layoutGroup = transform;
        toolUIPrefab = Resources.Load<GameObject>("Tool");
    }

    private void Start()
    {
        StartCoroutine(UpdateToolsRoutine());
    }

    private IEnumerator UpdateToolsRoutine()
    {
        while (true)
        {
            FindPlayer();

            if (playerTransform != null)
            {
                Transform attachmentPoint = playerTransform.Find("LeftArm/ToolAttachmentPoint");
                if (attachmentPoint != null)
                {
                    int currentCount = attachmentPoint.childCount;
                    if (currentCount != previousToolCount)
                    {
                        previousToolCount = currentCount;
                        UpdateTools();
                    }
                }
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    private void FindPlayer()
    {
        if (playerTransform != null) return;

        if (NetworkClient.connection != null && NetworkClient.connection.identity != null)
        {
            playerTransform = NetworkClient.connection.identity.transform;
            playerScript = playerTransform.GetComponent<Player>();
        }
    }

    private void Update()
    {
        for (int i = 1; i <= toolButtons.Count; i++)
        {
            if (Input.GetKeyDown(i.ToString()))
            {
                toolButtons[i - 1]?.onClick?.Invoke();
            }
        }
    }

    private void UpdateTools()
    {
        if (playerTransform == null) return;

        Transform attachmentPoint = playerTransform.Find("LeftArm/ToolAttachmentPoint");
        if (attachmentPoint == null) return;

        foreach (Transform child in layoutGroup)
        {
            Destroy(child.gameObject);
        }

        toolButtons.Clear();
        toolOutlines.Clear();

        int count = 0;
        foreach (Transform tool in attachmentPoint)
        {
            if (count >= maxTools) break;

            GameObject toolUI = Instantiate(toolUIPrefab, layoutGroup);
            toolUI.name = tool.name;

            TMP_Text nameText = toolUI.GetComponentInChildren<TMP_Text>();
            if (nameText != null)
                nameText.text = $"{count + 1}. {tool.name}";

            Outline outline = toolUI.GetComponent<Outline>();
            if (outline == null)
                outline = toolUI.AddComponent<Outline>();

            toolOutlines.Add(outline);

            Button button = toolUI.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                Transform capturedTool = tool;

                SetToolState(capturedTool, false, outline);

                button.onClick.AddListener(() =>
                {
                    if (playerScript == null) return;

                    bool currentlyDisabled = AreChildrenDisabled(capturedTool);

                    if (currentlyDisabled)
                        playerScript.CmdEnableTool(capturedTool.gameObject);
                    else
                        playerScript.CmdDisableTool(capturedTool.gameObject);
                });

                toolButtons.Add(button);
            }

            count++;
        }
    }

    public void RpcSetToolState(GameObject toolObj, bool enabled)
    {
        if (toolObj == null) return;

        Transform attachmentPoint = toolObj.transform.parent;
        if (attachmentPoint == null) return;

        int index = 0;
        foreach (Transform tool in attachmentPoint)
        {
            Outline outline = index < toolOutlines.Count ? toolOutlines[index] : null;
            if (tool == toolObj.transform)
            {
                SetToolState(tool, enabled, outline);
            }
            else if (enabled)
            {
                SetToolState(tool, false, outline);
            }
            index++;
        }
    }

    private void SetToolState(Transform tool, bool enabled, Outline outline)
    {
        foreach (Transform child in tool)
        {
            child.gameObject.SetActive(enabled);
        }

        if (outline != null)
        {
            outline.enabled = enabled;
            outline.effectColor = enabled ? enabledOutlineColor : disabledOutlineColor;
        }

        ToolComponent toolComponent = tool.GetComponent<ToolComponent>();
        if (toolComponent != null)
        {
            toolComponent.IsDisabled = !enabled;
        }

        if (!enabled)
            ResetAnimatorToIdle(tool);
    }

    private bool AreChildrenDisabled(Transform tool)
    {
        foreach (Transform child in tool)
        {
            if (child.gameObject.activeSelf)
                return false;
        }
        return true;
    }

    private void ResetAnimatorToIdle(Transform tool)
    {
        Transform parent = tool.parent?.parent;
        if (parent == null) return;

        Animator animator = parent.GetComponent<Animator>();
        if (animator != null)
        {
            animator.ResetTrigger("Tool");
            animator.ResetTrigger("Swing");
            animator.SetTrigger("Idle");
        }
    }
}