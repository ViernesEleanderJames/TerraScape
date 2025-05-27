using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;

public class ARRaycastPlace : MonoBehaviour
{
    [Header("AR Components")]
    public ARRaycastManager raycastManager;
    public Camera arCamera;

    [Header("UI Components")]
    [SerializeField] private Button deleteButton;

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private GameObject selectedModelPrefab; // Prefab selected from UI to be placed
    private GameObject selectedObject;      // Currently selected object in the scene

    // Variables for touch manipulation
    private float initialDistance;
    private float initialScale;
    private Vector2 initialTouch1Position;
    private Vector2 initialTouch2Position;

    // New flag to track if placement is primed by a UI selection
    private bool justSelectedPrefabFromUI = false;

    private void Start()
    {
        if (deleteButton != null)
        {
            deleteButton.gameObject.SetActive(false);
            deleteButton.onClick.AddListener(DeleteSelectedObject);
        }
    }

    public void SetSelectedModel(GameObject modelPrefab)
    {
        // If the same model button is clicked again, re-prime it for placement
        if (selectedModelPrefab == modelPrefab && modelPrefab != null)
        {
            justSelectedPrefabFromUI = true; // Re-prime for placement
            Debug.Log("Re-primed " + modelPrefab.name + " for placement.");
            // Ensure scene object is deselected if re-priming
            if (selectedObject != null)
            {
                ClearPreviousHighlight();
                selectedObject = null;
                deleteButton?.gameObject.SetActive(false);
            }
        }
        else // Different model selected or clearing selection
        {
            selectedModelPrefab = modelPrefab;
            if (modelPrefab != null)
            {
                justSelectedPrefabFromUI = true; // Prime for placement
                Debug.Log("Model for placement selected: " + modelPrefab.name);
                if (selectedObject != null) // If a scene object was selected, deselect it
                {
                    ClearPreviousHighlight();
                    selectedObject = null;
                    deleteButton?.gameObject.SetActive(false);
                }
            }
            else
            {
                justSelectedPrefabFromUI = false; // Clearing selection
                Debug.Log("Model for placement cleared.");
            }
        }
    }

    void Update()
    {
        if (Input.touchCount == 0) return;

        Touch touch1 = Input.GetTouch(0);
        Touch touch2 = (Input.touchCount > 1) ? Input.GetTouch(1) : default(Touch);

        if (EventSystem.current.IsPointerOverGameObject(touch1.fingerId) ||
            (Input.touchCount > 1 && EventSystem.current.IsPointerOverGameObject(touch2.fingerId)))
        {
            return;
        }

        if (Input.touchCount == 1 && touch1.phase == TouchPhase.Began)
        {
            Ray ray = arCamera.ScreenPointToRay(touch1.position);
            RaycastHit hitInfo;
            bool didHitARModel = false;
            GameObject hitObject = null;

            if (Physics.Raycast(ray, out hitInfo))
            {
                if (hitInfo.collider.CompareTag("ARModel")) //
                {
                    didHitARModel = true;
                    hitObject = hitInfo.collider.gameObject;
                }
            }

            if (didHitARModel)
            {
                // Tapped on an existing AR Model
                if (selectedObject != hitObject)
                {
                    ClearPreviousHighlight();
                    selectedObject = hitObject;
                    HighlightSelected(selectedObject);
                    deleteButton?.gameObject.SetActive(true);
                }

                // Tapping any placed model cancels "placement intent" and clears selected prefab
                if (selectedModelPrefab != null)
                {
                    selectedModelPrefab = null;
                    justSelectedPrefabFromUI = false;
                    Debug.Log("Tapped placed model, cleared UI prefab selection.");
                }
            }
            else // Tapped on empty space (or a non-ARModel object)
            {
                if (justSelectedPrefabFromUI && selectedModelPrefab != null)
                {
                    // User just selected a prefab from UI, this tap is for PLACEMENT
                    PlaceNewModelOnARPlaneOnly(touch1.position);
                    // PlaceNewModelOnARPlaneOnly will now set selectedModelPrefab = null
                    // and justSelectedPrefabFromUI = false after successful placement.
                }
                else if (selectedModelPrefab != null) // A prefab is selected, but not "just from UI" (e.g., placement failed or it's a later tap)
                {
                    // This tap is for DESELECTING the UI prefab
                    Debug.Log("Tapped empty space. Deselecting UI prefab: " + selectedModelPrefab.name);
                    selectedModelPrefab = null;
                    justSelectedPrefabFromUI = false; // Ensure this is reset
                }
                else if (selectedObject != null) // No UI prefab active, so deselect any highlighted scene object
                {
                    ClearPreviousHighlight();
                    selectedObject = null;
                    deleteButton?.gameObject.SetActive(false);
                }
            }
        }
        else if (selectedObject != null && Input.touchCount > 0) // Handle multi-touch for selected scene objects
        {
            if (Input.touchCount == 1 && touch1.phase == TouchPhase.Moved)
            {
                MoveSelectedModel(touch1.position);
            }
            else if (Input.touchCount == 2)
            {
                if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
                {
                    initialDistance = Vector2.Distance(touch1.position, touch2.position);
                    initialScale = selectedObject.transform.localScale.x;
                    initialTouch1Position = touch1.position;
                    initialTouch2Position = touch2.position;
                }
                else if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
                {
                    float currentDistance = Vector2.Distance(touch1.position, touch2.position);
                    if (initialDistance > Mathf.Epsilon)
                    {
                        float scaleFactor = currentDistance / initialDistance;
                        selectedObject.transform.localScale = Vector3.one * initialScale * scaleFactor;
                    }

                    Vector2 prevTouchVector = initialTouch2Position - initialTouch1Position;
                    Vector2 currentTouchVector = touch2.position - touch1.position;
                    float angleDelta = Vector2.SignedAngle(prevTouchVector, currentTouchVector);

                    if (Mathf.Abs(angleDelta) > 1.0f)
                    {
                        selectedObject.transform.Rotate(Vector3.up, -angleDelta, Space.World);
                    }
                    
                    initialTouch1Position = touch1.position;
                    initialTouch2Position = touch2.position;
                }
            }
        }
    }

    private void PlaceNewModelOnARPlaneOnly(Vector2 touchPosition)
    {
        if (selectedModelPrefab == null) return;

        if (raycastManager.Raycast(touchPosition, hits, TrackableType.Planes))
        {
            Pose hitPose = hits[0].pose;
            GameObject newObject = Instantiate(selectedModelPrefab, hitPose.position, hitPose.rotation);
            
            ARModelInfo modelInfo = newObject.GetComponent<ARModelInfo>();
            if (modelInfo == null) modelInfo = newObject.AddComponent<ARModelInfo>();
            modelInfo.prefabName = selectedModelPrefab.name; 
            
            if (newObject.GetComponent<Collider>() == null)
            {
                newObject.AddComponent<BoxCollider>();
            }
            newObject.tag = "ARModel"; //

            // Enforce "one placement per UI click" and clear selection to enable
            // "tap empty plane to deselect" behavior for subsequent states.
            selectedModelPrefab = null;
            justSelectedPrefabFromUI = false; // Reset placement intent flag
            Debug.Log("Placed model and cleared selectedModelPrefab.");
        }
        else
        {
            // Optional: If placement fails (no plane), reset the intent flag so the next tap deselects.
            justSelectedPrefabFromUI = false;
            Debug.Log("Placement failed (no plane hit), placement intent flag reset. Next empty plane tap will deselect UI choice if any.");
        }
    }
    
    private void MoveSelectedModel(Vector2 touchPosition)
    {
        if (selectedObject == null) return; 
        if (raycastManager.Raycast(touchPosition, hits, TrackableType.Planes))
        {
            Pose hitPose = hits[0].pose;
            selectedObject.transform.position = hitPose.position;
            selectedObject.transform.rotation = hitPose.rotation; 
        }
    }

    public void DeleteSelectedObject()
    {
        if (selectedObject != null)
        {
            Destroy(selectedObject);
            selectedObject = null;
            deleteButton?.gameObject.SetActive(false);
        }
    }

    private void HighlightSelected(GameObject obj)
    {
        if (obj == null) return; 
        var outline = obj.GetComponent<Outline>();
        if (outline == null) outline = obj.AddComponent<Outline>(); 
        outline.enabled = true;
    }

    private void ClearPreviousHighlight()
    {
        if (selectedObject != null)
        {
            var outline = selectedObject.GetComponent<Outline>();
            if (outline != null)
                outline.enabled = false;
        }
    }
}