using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraScaler : MonoBehaviour
{
    public int targetHeight = 1080;     // reference height
    public float pixelsPerUnit = 100f;  // same as your sprite settings

    void Start()
    {
        Camera cam = GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = (float)targetHeight / (2f * pixelsPerUnit);
    }
}
