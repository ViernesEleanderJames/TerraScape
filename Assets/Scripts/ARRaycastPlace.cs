using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARRaycastPlace : MonoBehaviour
{
    public ARRaycastManager raycastManager;
    public Camera arCamera;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private GameObject selectedModelPrefab; // Model selected from UI
    private GameObject selectedObject;      // Currently selected placed model

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                return;

            if (touch.phase == TouchPhase.Began)
            {
                Debug.Log("Touch detected at: " + touch.position);

                // 1️⃣ First, Try Selecting an Existing Model
                if (TrySelectPlacedModel(touch.position)) 
                {
                    Debug.Log("✅ Selected existing model: " + selectedObject.name);
                    return; // Exit early if selection is successful
                }

                // 2️⃣ If No Selection, Try Placing a New Model
                TryPlaceNewModel(touch.position);
            }
            else if (touch.phase == TouchPhase.Moved && selectedObject != null)
            {
                MoveSelectedModel(touch.position);
            }
        }
    }

    private bool TrySelectPlacedModel(Vector2 touchPosition)
    {
        Ray ray = arCamera.ScreenPointToRay(touchPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log("Raycast hit: " + hit.collider.gameObject.name);

            if (hit.collider.CompareTag("ARModel")) // Ensure models have this tag
            {
                selectedObject = hit.collider.gameObject;
                Debug.Log("🎯 Selected: " + selectedObject.name);
                return true;
            }
        }

        Debug.Log("❌ No model selected");
        return false;
    }

    private void TryPlaceNewModel(Vector2 touchPosition)
    {
        if (selectedModelPrefab == null)
        {
            Debug.LogError("❌ No model selected to place.");
            return;
        }

        // 1️⃣ Raycast against existing objects for SnapPoint
        Ray ray = arCamera.ScreenPointToRay(touchPosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GameObject hitObject = hit.collider.gameObject;

            if (hitObject.CompareTag("ARModel"))
            {
                Transform snapPoint = hitObject.transform.Find("SnapPoint");
                if (snapPoint != null)
                {
                    GameObject newObject = Instantiate(selectedModelPrefab, snapPoint.position, snapPoint.rotation);

                    if (newObject.GetComponent<Collider>() == null)
                        newObject.AddComponent<BoxCollider>();

                    newObject.tag = "ARModel";
                    Debug.Log("📌 Snapped model to SnapPoint on: " + hitObject.name);
                    return;
                }
            }
        }

        // 2️⃣ If no SnapPoint, place on detected AR plane
        if (raycastManager.Raycast(touchPosition, hits, TrackableType.Planes))
        {
            Pose hitPose = hits[0].pose;
            GameObject newObject = Instantiate(selectedModelPrefab, hitPose.position, hitPose.rotation);

            if (newObject.GetComponent<Collider>() == null)
                newObject.AddComponent<BoxCollider>();

            newObject.tag = "ARModel";
            Debug.Log("🚀 Placed new model on AR plane: " + newObject.name);
        }
    }

    private void MoveSelectedModel(Vector2 touchPosition)
    {
        if (raycastManager.Raycast(touchPosition, hits, TrackableType.Planes))
        {
            Pose hitPose = hits[0].pose;
            selectedObject.transform.position = hitPose.position;
            selectedObject.transform.rotation = hitPose.rotation;
            Debug.Log("🔄 Moved object: " + selectedObject.name);
        }
    }

    public void SetSelectedModel(GameObject modelPrefab)
    {
        selectedModelPrefab = modelPrefab;
        Debug.Log("📌 Model selected from UI: " + selectedModelPrefab.name);
    }
}
