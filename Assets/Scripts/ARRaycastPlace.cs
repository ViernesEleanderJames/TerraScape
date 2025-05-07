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
    private GameObject selectedModelPrefab;
    private GameObject selectedObject;

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
        if (Input.touchCount > 0)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = (Input.touchCount > 1) ? Input.GetTouch(1) : default;

            // Ignore touch input if it's on UI
            if (EventSystem.current.IsPointerOverGameObject(touch1.fingerId) || (Input.touchCount > 1 && EventSystem.current.IsPointerOverGameObject(touch2.fingerId)))
                return;

            // Handling touch interactions after selecting a model
            if (selectedObject != null)
            {
                // Move model with a single touch
                if (Input.touchCount == 1 && touch1.phase == TouchPhase.Moved)
                {
                    MoveSelectedModel(touch1.position);
                }

                // Resize or rotate the model with two touches
                if (Input.touchCount == 2)
                {
                    if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
                    {
                        // Store initial distance for resizing
                        initialDistance = Vector2.Distance(touch1.position, touch2.position);
                        initialScale = selectedObject.transform.localScale.x; // Assuming uniform scale
                        initialTouch1Position = touch1.position;
                        initialTouch2Position = touch2.position;
                    }
                    else if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
                    {
                        // Resize the model: Pinch (distance between two touches)
                        float currentDistance = Vector2.Distance(touch1.position, touch2.position);
                        float scaleFactor = currentDistance / initialDistance;
                        selectedObject.transform.localScale = Vector3.one * initialScale * scaleFactor;

                        // Rotate the model: Change in angle between two touches
                        Vector2 deltaTouch1 = touch1.position - initialTouch1Position;
                        Vector2 deltaTouch2 = touch2.position - initialTouch2Position;

                        float angle = Vector2.SignedAngle(deltaTouch1, deltaTouch2);
                        selectedObject.transform.Rotate(Vector3.up, angle, Space.World);

                        // Update initial touch positions for next frame
                        initialTouch1Position = touch1.position;
                        initialTouch2Position = touch2.position;
                    }
                }
            }
            else // If no model is selected, try placing a new model
            {
                // Handle model placement
                if (Input.touchCount == 1 && touch1.phase == TouchPhase.Began)
                {
                    TryPlaceNewModel(touch1.position);
                }
            }

            // Handling model selection and deselection
            if (Input.touchCount == 1 && touch1.phase == TouchPhase.Began)
            {
                // Try to deselect if tapped on empty space or another model
                if (TryDeselectModel(touch1.position)) return;

                // Try to select a model if tapped on one
                if (TrySelectPlacedModel(touch1.position)) return;
            }
        }
    }

    private bool TryDeselectModel(Vector2 touchPosition)
    {
        // Raycast to check if the touch is on an empty area (no model hit)
        Ray ray = arCamera.ScreenPointToRay(touchPosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // If a model is hit, don't deselect
            if (hit.collider.CompareTag("ARModel"))
                return false;
        }

        // Deselect the current model (if any)
        ClearPreviousHighlight();
        selectedObject = null;
        deleteButton?.gameObject.SetActive(false);
        return true;
    }

    private bool TrySelectPlacedModel(Vector2 touchPosition)
    {
        Ray ray = arCamera.ScreenPointToRay(touchPosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("ARModel"))
            {
                ClearPreviousHighlight(); // Clear previous model highlight
                selectedObject = hit.collider.gameObject;
                HighlightSelected(selectedObject); // Highlight new selected model
                deleteButton?.gameObject.SetActive(true);
                return true;
            }
        }
        return false;
    }

    private void TryPlaceNewModel(Vector2 touchPosition)
    {
        if (selectedModelPrefab == null)
            return;

        Ray ray = arCamera.ScreenPointToRay(touchPosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("ARModel"))
            {
                Transform snapPoint = hit.collider.transform.Find("SnapPoint");
                if (snapPoint != null)
                {
                    GameObject newObject = Instantiate(selectedModelPrefab, snapPoint.position, snapPoint.rotation);
                    if (newObject.GetComponent<Collider>() == null)
                        newObject.AddComponent<BoxCollider>();
                    newObject.tag = "ARModel";
                }
                else
                {
                    GameObject newObject = Instantiate(selectedModelPrefab, hit.point, Quaternion.identity);
                    if (newObject.GetComponent<Collider>() == null)
                        newObject.AddComponent<BoxCollider>();
                    newObject.tag = "ARModel";
                }
                return;
            }
        }

        if (raycastManager.Raycast(touchPosition, hits, TrackableType.Planes))
        {
            Pose hitPose = hits[0].pose;
            GameObject newObject = Instantiate(selectedModelPrefab, hitPose.position, hitPose.rotation);
            if (newObject.GetComponent<Collider>() == null)
                newObject.AddComponent<BoxCollider>();
            newObject.tag = "ARModel";
        }
    }

    private void MoveSelectedModel(Vector2 touchPosition)
    {
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

    // Highlighting Methods
    private void HighlightSelected(GameObject obj)
    {
        var outline = obj.GetComponent<Outline>();
        if (outline != null)
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
