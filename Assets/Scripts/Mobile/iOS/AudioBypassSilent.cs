using UnityEngine;
using System.Runtime.InteropServices;

public class AudioBypass : MonoBehaviour
{
	#if UNITY_IOS && !UNITY_EDITOR
		[DllImport("__Internal")]
		private static extern void SetAudioSessionPlaybackCategory();
	#endif

    void Awake()
    {
		DontDestroyOnLoad(gameObject);

		#if UNITY_IOS && !UNITY_EDITOR
			SetAudioSessionPlaybackCategory();
		#endif
    }
}
