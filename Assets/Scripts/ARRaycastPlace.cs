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

    // Variables for touch manipulation (from your original script)
    private float initialDistance;
    private float initialScale;
    private Vector2 initialTouch1Position;
    private Vector2 initialTouch2Position;

    private void Start()
    {
        if (deleteButton != null)
        {
            deleteButton.gameObject.SetActive(false);
            deleteButton.onClick.AddListener(DeleteSelectedObject);
        }
    }

    void Update()
    {
        if (Input.touchCount == 0) return;

        Touch touch1 = Input.GetTouch(0);
        Touch touch2 = (Input.touchCount > 1) ? Input.GetTouch(1) : default(Touch); // Ensure touch2 is properly initialized

        // Ignore touch input if it's on UI
        // Check fingerId for touch2 only if Input.touchCount > 1 to avoid using default(Touch).fingerId
        if (EventSystem.current.IsPointerOverGameObject(touch1.fingerId) ||
            (Input.touchCount > 1 && EventSystem.current.IsPointerOverGameObject(touch2.fingerId)))
        {
            return;
        }

        // Logic for TouchPhase.Began (Selection, Deselection, or Initiating Placement)
        if (Input.touchCount == 1 && touch1.phase == TouchPhase.Began)
        {
            Ray ray = arCamera.ScreenPointToRay(touch1.position);
            RaycastHit hitInfo;
            bool didHitARModel = false;
            GameObject hitObject = null;

            if (Physics.Raycast(ray, out hitInfo)) // Ensure models have colliders
            {
                if (hitInfo.collider.CompareTag("ARModel")) // Ensure models are tagged "ARModel"
                {
                    didHitARModel = true;
                    hitObject = hitInfo.collider.gameObject;
                }
            }

            if (didHitARModel)
            {
                // Tapped on an existing AR Model
                if (selectedObject != hitObject) // If it's a different model
                {
                    ClearPreviousHighlight();
                    selectedObject = hitObject;
                    HighlightSelected(selectedObject);
                    deleteButton?.gameObject.SetActive(true);
                }
                // If tapped on the already selected model, do nothing here.
            }
            else
            {
                // Tapped on empty space (or a non-ARModel object)
                if (selectedModelPrefab != null)
                {
                    // Attempt to place the new model
                    PlaceNewModelOnARPlaneOnly(touch1.position);
                }
                else if (selectedObject != null)
                {
                    // No prefab to place, and tapped empty space, so deselect.
                    ClearPreviousHighlight();
                    selectedObject = null;
                    deleteButton?.gameObject.SetActive(false);
                }
            }
        }
        // Handling touch interactions (Move, Scale, Rotate) if a model is already selected
        else if (selectedObject != null && Input.touchCount > 0) // Covers Moved, Stationary, Ended phases for 1 or 2 touches
        {
            // Move model with a single touch (original logic)
            if (Input.touchCount == 1 && touch1.phase == TouchPhase.Moved)
            {
                MoveSelectedModel(touch1.position);
            }
            // Resize or rotate the model with two touches
            else if (Input.touchCount == 2)
            {
                // touch2 would have been fetched at the start of Update if Input.touchCount > 1
                if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
                {
                    initialDistance = Vector2.Distance(touch1.position, touch2.position);
                    initialScale = selectedObject.transform.localScale.x; // Assuming uniform scale
                    initialTouch1Position = touch1.position; // Capture initial positions for rotation logic
                    initialTouch2Position = touch2.position;
                }
                else if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
                {
                    // --- Scaling Logic (uses initialDistance and initialScale from Began phase) ---
                    float currentDistance = Vector2.Distance(touch1.position, touch2.position);
                    if (initialDistance > Mathf.Epsilon) // Avoid division by zero
                    {
                        float scaleFactor = currentDistance / initialDistance;
                        selectedObject.transform.localScale = Vector3.one * initialScale * scaleFactor;
                    }

                    // --- Revised Rotation Logic ---
                    // Vector connecting the two touches in the PREVIOUS frame (or from Began phase)
                    Vector2 prevTouchVector = initialTouch2Position - initialTouch1Position; // These are from the last frame or Began
                    // Vector connecting the two touches in the CURRENT frame
                    Vector2 currentTouchVector = touch2.position - touch1.position;

                    // Calculate the change in angle of the vector connecting the touches
                    float angleDelta = Vector2.SignedAngle(prevTouchVector, currentTouchVector);

                    // Apply rotation if the change is significant enough
                    // You might need to adjust the threshold (e.g., 0.5f or 1.0f)
                    if (Mathf.Abs(angleDelta) > 1.0f) 
                    {
                        // The sign (-angleDelta or angleDelta) might need to be flipped depending on desired rotation intuitiveness
                        selectedObject.transform.Rotate(Vector3.up, -angleDelta, Space.World); 
                    }
                    
                    // Update touch positions to be used as "previous" positions in the next Moved frame
                    initialTouch1Position = touch1.position;
                    initialTouch2Position = touch2.position;
                }
            }
        }
    }

    // This method replaces the part of your old TryPlaceNewModel that dealt with ARPlanes.
    private void PlaceNewModelOnARPlaneOnly(Vector2 touchPosition)
    {
        if (selectedModelPrefab == null)
            return;

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
            newObject.tag = "ARModel";

            // Optional: Auto-select the new model. You can uncomment if desired.
            // ClearPreviousHighlight();
            // selectedObject = newObject;
            // HighlightSelected(selectedObject);
            // deleteButton?.gameObject.SetActive(true);
            // selectedModelPrefab = null; 
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

    public void SetSelectedModel(GameObject modelPrefab)
    {
        selectedModelPrefab = modelPrefab;
        // if (selectedObject != null)
        // {
        //     ClearPreviousHighlight();
        //     selectedObject = null;
        //     deleteButton?.gameObject.SetActive(false);
        // }
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