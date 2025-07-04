using UnityEngine.SceneManagement;

public static class SceneHelper
{
    public static string GetCurrentSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }
}
