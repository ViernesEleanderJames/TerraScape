using UnityEngine;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(ARPlane))]
public class ARPlaneVisualizer : MonoBehaviour
{
    [Tooltip("Fade in/out speed multiplier applied during the alpha tweening. Lower value = slower.")]
    [Range(0.1f, 1.0f)]
    public float fadeSpeed = 1f;

    private Material planeMaterial;
    private int shaderAlphaPropertyID;
    private float targetAlpha;
    private float currentAlpha;

    private ARPlane plane;
    private ARPlaneManager planeManager;

    void Awake()
    {
        shaderAlphaPropertyID = Shader.PropertyToID("_PlaneAlpha");
        planeMaterial = GetComponent<MeshRenderer>().material;
        plane = GetComponent<ARPlane>();
        planeManager = FindObjectOfType<ARPlaneManager>(); // ✅ auto-find in scene

        currentAlpha = 1f;
        targetAlpha = 1f;
    }

    void OnEnable()
    {
        // Subscribe to ARPlane boundary updates
        plane.boundaryChanged += OnPlaneBoundaryChanged;
    }

    void OnDisable()
    {
        plane.boundaryChanged -= OnPlaneBoundaryChanged;
    }

    void OnPlaneBoundaryChanged(ARPlaneBoundaryChangedEventArgs args)
    {
        if (planeManager == null) return;

        // ✅ Remove overlapping (child) planes — prevent stacking
        foreach (var otherPlane in planeManager.trackables)
        {
            if (otherPlane == plane) continue;

            // If this plane's center is inside another plane, disable it
            if (otherPlane.subsumedBy == plane)
            {
                otherPlane.gameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        // Smoothly fade toward the target alpha value
        currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.unscaledDeltaTime * fadeSpeed);
        planeMaterial.SetFloat(shaderAlphaPropertyID, currentAlpha);
    }

    /// <summary>
    /// Show or hide plane surfaces by changing alpha value.
    /// </summary>
    public void SetVisible(bool visible)
    {
        targetAlpha = visible ? 1f : 0f;
    }
}
