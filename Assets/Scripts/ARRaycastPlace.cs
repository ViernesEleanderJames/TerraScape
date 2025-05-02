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
            Touch touch = Input.GetTouch(0);

            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                return;

            if (touch.phase == TouchPhase.Began)
            {
                if (TrySelectPlacedModel(touch.position)) return;
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
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("ARModel"))
            {
                ClearPreviousHighlight(); // 🔥 Clear previous model highlight
                selectedObject = hit.collider.gameObject;
                HighlightSelected(selectedObject); // 🔥 Highlight new selected model
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

    // 🟡 Highlighting Methods
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
