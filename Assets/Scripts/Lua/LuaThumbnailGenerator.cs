using UnityEngine;

public class LuaThumbnailGenerator
{
    public static string GenerateThumbnailBase64(Vector3 position, Vector3 rotation, int width = 256, int height = 256, bool hideSky = true, bool isPlace = false)
    {
        return ThumbnailGenerator.GenerateThumbnailBase64(position, rotation, width, height, hideSky, isPlace);
    }
}
