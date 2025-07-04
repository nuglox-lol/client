using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainSceneLoader : MonoBehaviour
{
	public GameObject unityEditorOnlyCanvas;
	public GameObject LoadingCircle;
	
    void Start()
    {
		#if UNITY_SERVER
			Debug.Log("----------------------------------------------------------------------");
			Debug.Log("Welcome to the NUGLOX Dedicated Server program!");
			Debug.Log("Loading server scene...");
			Debug.Log("----------------------------------------------------------------------");
			GameObject.Find("MainMenuAudioSource").GetComponent<AudioSource>().Stop();
			SceneManager.LoadScene("Player");
		#elif UNITY_EDITOR
			unityEditorOnlyCanvas.SetActive(true);
			LoadingCircle.SetActive(false);
	
			unityEditorOnlyCanvas.transform.Find("Player")?.GetComponent<Button>().onClick.AddListener(() =>
			{
				unityEditorOnlyCanvas.SetActive(false);
				LoadingCircle.SetActive(true);
                SceneManager.LoadScene("Player");
			});

			unityEditorOnlyCanvas.transform.Find("MobileApp")?.GetComponent<Button>().onClick.AddListener(() =>
			{
				unityEditorOnlyCanvas.SetActive(false);
				LoadingCircle.SetActive(true);
                SceneManager.LoadScene("MobileApp");
			});

			unityEditorOnlyCanvas.transform.Find("Editor")?.GetComponent<Button>().onClick.AddListener(() =>
			{
				unityEditorOnlyCanvas.SetActive(false);
				LoadingCircle.SetActive(true);
				GameObject.Find("MainMenuAudioSource").GetComponent<AudioSource>().Stop();
                SceneManager.LoadScene("Editor");
			});
		#elif UNITY_IOS || UNITY_ANDROID
			unityEditorOnlyCanvas.SetActive(false);
			LoadingCircle.SetActive(true);

			SceneManager.LoadScene("MobileApp");
		#else
			unityEditorOnlyCanvas.SetActive(false);
			LoadingCircle.SetActive(true);

			string[] args = System.Environment.GetCommandLineArgs();

			string sceneToLoad = "Editor";

			foreach (string arg in args)
			{
				if (arg == "--player")
				{
					sceneToLoad = "Player";
					break;
				}
				
				if (arg == "--server")
				{
					sceneToLoad = "Player";
					break;
				}

				if (arg == "--mobileui")
				{
					sceneToLoad = "MobileApp";
					break;
				}
			}

			if(sceneToLoad == "Editor")
			{
				GameObject.Find("MainMenuAudioSource").GetComponent<AudioSource>().Stop();
			}

			SceneManager.LoadScene(sceneToLoad);
		#endif
    }
}