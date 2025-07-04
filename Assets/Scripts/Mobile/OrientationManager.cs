using UnityEngine;

public class OrientationManager : MonoBehaviour
{
    public enum Orientation
    {
        Portrait,
        Landscape
    }

    public Orientation targetOrientation;

    void Start()
    {
        SetOrientation(targetOrientation);
    }

    void SetOrientation(Orientation orientation)
    {
		#if UNITY_IOS || UNITY_ANDROID
			Screen.autorotateToPortrait = false;
			Screen.autorotateToPortraitUpsideDown = false;
			Screen.autorotateToLandscapeLeft = false;
			Screen.autorotateToLandscapeRight = false;

			switch (orientation)
			{
				case Orientation.Portrait:
					Screen.orientation = ScreenOrientation.Portrait;
					break;
				case Orientation.Landscape:
					Screen.orientation = ScreenOrientation.LandscapeLeft;
					break;
			}
		#else
			Debug.Log("Cannot change orientation while device is not mobile");
		#endif
    }
}