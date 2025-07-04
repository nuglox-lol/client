using UnityEngine;
using TMPro;
using System.Collections;
using Mirror;

public class ShowTools : MonoBehaviour
{
    private int maxTools = 7;
    private Transform layoutGroup;
    private GameObject toolUIPrefab;
    private Transform playerTransform;

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

    private void UpdateTools()
    {
        if (playerTransform == null) return;

        Transform attachmentPoint = playerTransform.Find("LeftArm/ToolAttachmentPoint");
        if (attachmentPoint == null) return;

        foreach (Transform child in layoutGroup)
        {
            Destroy(child.gameObject);
        }

        int count = 0;
        foreach (Transform tool in attachmentPoint)
        {
            if (count >= maxTools) break;

            GameObject toolUI = Instantiate(toolUIPrefab, layoutGroup);
            toolUI.name = tool.name;

            TMP_Text nameText = toolUI.GetComponentInChildren<TMP_Text>();
            if (nameText != null)
                nameText.text = tool.name;

            count++;
        }
    }
}