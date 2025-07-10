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
    private List<Button> toolButtons = new List<Button>();

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
            UpdateTools();
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void FindPlayer()
    {
        if (playerTransform != null) return;

        if (NetworkClient.connection != null && NetworkClient.connection.identity != null)
        {
            playerTransform = NetworkClient.connection.identity.transform;
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

            bool childrenDisabled = AreChildrenDisabled(tool);
            outline.enabled = childrenDisabled;

            Button button = toolUI.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                Transform capturedTool = tool;

                button.onClick.AddListener(() =>
                {
                    bool disable = !AreChildrenDisabled(capturedTool);
                    ToggleChildren(capturedTool, disable);
                    outline.enabled = disable;

                    ToolComponent toolComponent = capturedTool.GetComponent<ToolComponent>();
                    if (toolComponent != null)
                    {
                        toolComponent.IsDisabled = disable;
                    }

                    if (disable)
                        ResetAnimatorToIdle(capturedTool);
                });

                toolButtons.Add(button);
            }

            count++;
        }
    }

    private void ToggleChildren(Transform tool, bool disable)
    {
        foreach (Transform child in tool)
        {
            child.gameObject.SetActive(!disable);
        }
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