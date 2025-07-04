using UnityEngine;
using System.Collections.Generic;

public class ResolutionLimiter : MonoBehaviour
{
    public int minWidth = 1280;
    public int minHeight = 720;

    void Start()
    {
		DontDestroyOnLoad(gameObject);

        EnforceMinResolution();
    }

    void Update()
    {
        EnforceMinResolution();
    }

    void EnforceMinResolution()
    {
        int width = Screen.width;
        int height = Screen.height;

        if (width < minWidth || height < minHeight)
        {
            int newWidth = Mathf.Max(width, minWidth);
            int newHeight = Mathf.Max(height, minHeight);
            Screen.SetResolution(newWidth, newHeight, false);
        }
    }
}
