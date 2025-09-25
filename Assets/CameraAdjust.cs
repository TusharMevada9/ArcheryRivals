using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraAdjust : MonoBehaviour
{
    [Header("Reference Settings")]
    public float referenceWidth = 1920f;   // Reference resolution width
    public float referenceHeight = 1080f;  // Reference resolution height
    public float referenceOrthoSize = 5f;  // Original orthographic size of camera

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        AdjustCamera();
    }

    void AdjustCamera()
    {
        float targetAspect = referenceWidth / referenceHeight;
        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        if (scaleHeight < 1.0f)
        {
            // Screen is taller than reference
            cam.orthographicSize = referenceOrthoSize / scaleHeight;
        }
        else
        {
            // Screen is wider than reference
            cam.orthographicSize = referenceOrthoSize;
        }

        Debug.Log("Adjusted Camera Size: " + cam.orthographicSize);
    }

    // Optional: update in runtime if resolution changes
    void Update()
    {
        if (Screen.width != cam.pixelWidth || Screen.height != cam.pixelHeight)
        {
            AdjustCamera();
        }
    }
}
