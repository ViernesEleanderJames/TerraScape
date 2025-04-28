using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;

public class ARPlaneToggle : MonoBehaviour
{
    public ARPlaneManager planeManager;
    public Toggle toggle;

    void Start()
    {
        toggle.onValueChanged.AddListener(TogglePlaneVisibility);
        TogglePlaneVisibility(toggle.isOn);
    }

    public void TogglePlaneVisibility(bool isOn)
    {
        // Enable/disable plane detection
        planeManager.enabled = isOn;

        // Get all existing AR planes and toggle their visibility
        foreach (var plane in planeManager.trackables)
        {
            var visualizer = plane.GetComponent<ARPlaneMeshVisualizer>();
            if (visualizer != null)
            {
                visualizer.enabled = isOn;
                plane.gameObject.SetActive(isOn); // Optional if you want to completely hide the plane
            }
        }
    }
}
