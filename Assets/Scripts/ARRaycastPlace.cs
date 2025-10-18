using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI; 

public class ARRaycastPlace : MonoBehaviour
{
    [Header("AR Components")]
    public ARRaycastManager raycastManager; // handles ar raycasts to detect planes
    public Camera arCamera; // main ar camera

    [Header("UI Components")]
    [SerializeField] private Button deleteButton; // delete button for removing placed models

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private GameObject selectedModelPrefab; // model picked from ui to place
    private GameObject selectedObject;      // model currently selected in scene

    // touch controls for scaling and rotating
    private float initialDistance;
    private float initialScale;
    private Vector2 initialTouch1Position;
    private Vector2 initialTouch2Position;

    // true if a model was just selected from ui for placement
    private bool justSelectedPrefabFromUI = false;

    private void Start()
    {
        if (deleteButton != null)
        {
            deleteButton.gameObject.SetActive(false); // hide delete button on start
            deleteButton.onClick.AddListener(DeleteSelectedObject); // connect delete button event
        }
    }

    public void SetSelectedModel(GameObject modelPrefab)
    {
        // same model clicked again = re-prime for placement
        if (selectedModelPrefab == modelPrefab && modelPrefab != null)
        {
            justSelectedPrefabFromUI = true;
            Debug.Log("Re-primed " + modelPrefab.name + " for placement.");

            // remove highlight if an object was selected
            if (selectedObject != null)
            {
                ClearPreviousHighlight();
                selectedObject = null;
                deleteButton?.gameObject.SetActive(false);
            }
        }
        else // new model selected or cleared
        {
            selectedModelPrefab = modelPrefab;
            if (modelPrefab != null)
            {
                justSelectedPrefabFromUI = true;
                Debug.Log("Model for placement selected: " + modelPrefab.name);

                // deselect any highlighted object
                if (selectedObject != null)
                {
                    ClearPreviousHighlight();
                    selectedObject = null;
                    deleteButton?.gameObject.SetActive(false);
                }
            }
            else
            {
                justSelectedPrefabFromUI = false;
                Debug.Log("Model for placement cleared.");
            }
        }
    }

    void Update()
    {
        if (Input.touchCount == 0) return;

        Touch touch1 = Input.GetTouch(0);
        Touch touch2 = (Input.touchCount > 1) ? Input.GetTouch(1) : default(Touch);

        // ignore touches on ui
        if (EventSystem.current.IsPointerOverGameObject(touch1.fingerId) ||
            (Input.touchCount > 1 && EventSystem.current.IsPointerOverGameObject(touch2.fingerId)))
        {
            return;
        }

        // handle tap
        if (Input.touchCount == 1 && touch1.phase == TouchPhase.Began)
        {
            Ray ray = arCamera.ScreenPointToRay(touch1.position);
            RaycastHit hitInfo;
            bool didHitARModel = false;
            GameObject hitObject = null;

            if (Physics.Raycast(ray, out hitInfo))
            {
                if (hitInfo.collider.CompareTag("ARModel")) // check if we tapped an ar model
                {
                    didHitARModel = true;
                    hitObject = hitInfo.collider.gameObject;
                }
            }

            if (didHitARModel)
            {
                // tap selects an existing model
                if (selectedObject != hitObject)
                {
                    ClearPreviousHighlight();
                    selectedObject = hitObject;
                    HighlightSelected(selectedObject);
                    deleteButton?.gameObject.SetActive(true);
                }

                // clear prefab selection if we tapped a model
                if (selectedModelPrefab != null)
                {
                    selectedModelPrefab = null;
                    justSelectedPrefabFromUI = false;
                    Debug.Log("Tapped placed model, cleared UI prefab selection.");
                }
            }
            else // tapped empty plane
            {
                if (justSelectedPrefabFromUI && selectedModelPrefab != null)
                {
                    // just selected from ui → place model
                    PlaceNewModelOnARPlaneOnly(touch1.position);
                }
                else if (selectedModelPrefab != null)
                {
                    // deselect prefab
                    Debug.Log("Tapped empty space. Deselecting UI prefab: " + selectedModelPrefab.name);
                    selectedModelPrefab = null;
                    justSelectedPrefabFromUI = false;
                }
                else if (selectedObject != null)
                {
                    // clear selected object
                    ClearPreviousHighlight();
                    selectedObject = null;
                    deleteButton?.gameObject.SetActive(false);
                }
            }
        }
        else if (selectedObject != null && Input.touchCount > 0) // move, scale, rotate
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

                    // rotation gesture
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
            
            // attach model info for saving/loading
            ARModelInfo modelInfo = newObject.GetComponent<ARModelInfo>();
            if (modelInfo == null) modelInfo = newObject.AddComponent<ARModelInfo>();
            modelInfo.prefabName = selectedModelPrefab.name; 
            
            // add collider if missing
            if (newObject.GetComponent<Collider>() == null)
            {
                newObject.AddComponent<BoxCollider>();
            }
            newObject.tag = "ARModel"; // tag it as ar model

            // reset selection after placement
            selectedModelPrefab = null;
            justSelectedPrefabFromUI = false;
            Debug.Log("Placed model and cleared selectedModelPrefab.");
        }
        else
        {
            // no plane found
            justSelectedPrefabFromUI = false;
            Debug.Log("Placement failed (no plane hit), placement intent flag reset.");
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
