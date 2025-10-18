using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;

public class PlaneCleanupManager : MonoBehaviour
{
    public ARPlaneManager planeManager;

    void OnEnable()
    {
        planeManager.planesChanged += OnPlanesChanged;
    }

    void OnDisable()
    {
        planeManager.planesChanged -= OnPlanesChanged;
    }

    private void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        // Remove duplicate or overlapping planes
        foreach (var newPlane in args.added)
        {
            foreach (var existingPlane in planeManager.trackables)
            {
                if (existingPlane == newPlane) continue;

                float distance = Vector3.Distance(newPlane.center, existingPlane.center);
                if (distance < 0.05f) // same area, likely overlap
                {
                    newPlane.gameObject.SetActive(false);
                    Debug.Log("🧹 Disabled overlapping plane: " + newPlane.trackableId);
                }
            }
        }

        // Hide subsumed (merged) planes automatically
        foreach (var updatedPlane in args.updated)
        {
            if (updatedPlane.subsumedBy != null)
            {
                updatedPlane.gameObject.SetActive(false);
                Debug.Log("🧩 Hiding subsumed plane: " + updatedPlane.trackableId);
            }
        }
    }
}
