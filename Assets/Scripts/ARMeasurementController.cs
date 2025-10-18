using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AreaMeasurementManager : MonoBehaviour
{
    [SerializeField] private GameObject pointPrefab; // prefab for measurement points
    [SerializeField] private LineRenderer lineRenderer; // draws lines between points
    [SerializeField] private TextMeshProUGUI areaText; // shows area or distance
    [SerializeField] private Button resetButton; // resets all points
    [SerializeField] private Button backButton; // goes back to home
    [SerializeField] private Button deleteButton; // deletes selected point
    [SerializeField] private GameObject distanceLabelPrefab; // shows distance text

    private ARRaycastManager raycastManager;
    private List<Vector3> points = new List<Vector3>();
    private List<GameObject> pointObjects = new List<GameObject>();
    private List<GameObject> distanceLabels = new List<GameObject>();

    private GameObject selectedPoint;
    private int selectedIndex = -1;
    private Vector3 originalScale;

    void Start()
    {
        raycastManager = FindObjectOfType<ARRaycastManager>();
        lineRenderer.positionCount = 0;
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
        areaText.text = "Tap to place points";

        resetButton.onClick.AddListener(ResetMeasurement);
        backButton.onClick.AddListener(GoBackToMainScene);
        deleteButton.onClick.AddListener(DeleteSelectedPoint);
        deleteButton.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // ignore touches on UI
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                return;

            Vector2 touchPos = touch.position;

            if (touch.phase == TouchPhase.Began)
            {
                // check if user tapped on existing point
                Ray ray = Camera.main.ScreenPointToRay(touchPos);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (pointObjects.Contains(hit.collider.gameObject))
                    {
                        SelectPoint(hit.collider.gameObject);
                        return; // don’t place a new one
                    }
                }

                // place new point
                List<ARRaycastHit> hits = new List<ARRaycastHit>();
                if (raycastManager.Raycast(touchPos, hits, TrackableType.Planes))
                {
                    Pose hitPose = hits[0].pose;
                    Vector3 newPoint = hitPose.position;

                    GameObject marker = Instantiate(pointPrefab, newPoint, Quaternion.identity);
                    marker.tag = "Point";
                    pointObjects.Add(marker);
                    points.Add(newPoint);

                    UpdateLine();
                    UpdateMeasurements();
                }
            }
            else if (touch.phase == TouchPhase.Moved && selectedPoint != null)
            {
                // move selected point
                List<ARRaycastHit> hits = new List<ARRaycastHit>();
                if (raycastManager.Raycast(touchPos, hits, TrackableType.Planes))
                {
                    Pose hitPose = hits[0].pose;
                    selectedPoint.transform.position = hitPose.position;
                    points[selectedIndex] = hitPose.position;

                    UpdateLine();
                    UpdateMeasurements();
                }
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                // stop dragging but keep point selected
            }
        }

        // make distance labels always face the camera
        foreach (var label in distanceLabels)
        {
            if (label != null && Camera.main != null)
            {
                label.transform.LookAt(Camera.main.transform);
                label.transform.Rotate(0, 180, 0);
            }
        }
    }

    void SelectPoint(GameObject point)
    {
        // reset previous highlight
        if (selectedPoint != null)
        {
            selectedPoint.transform.localScale = originalScale;
        }

        selectedPoint = point;
        selectedIndex = pointObjects.IndexOf(selectedPoint);

        // highlight selected point
        originalScale = selectedPoint.transform.localScale;
        selectedPoint.transform.localScale = originalScale * 1.3f;

        deleteButton.gameObject.SetActive(true);
    }

    void DeselectPoint()
    {
        if (selectedPoint != null)
        {
            selectedPoint.transform.localScale = originalScale;
        }
        selectedPoint = null;
        selectedIndex = -1;
        deleteButton.gameObject.SetActive(false);
    }

    void UpdateLine()
    {
        if (points.Count > 2)
        {
            lineRenderer.positionCount = points.Count + 1;
            lineRenderer.SetPositions(points.ToArray());
            lineRenderer.SetPosition(points.Count, points[0]); // closes the shape
        }
        else
        {
            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPositions(points.ToArray());
        }
    }

    void UpdateMeasurements()
    {
        // clear old distance labels
        foreach (var label in distanceLabels)
        {
            Destroy(label);
        }
        distanceLabels.Clear();

        if (points.Count >= 2)
        {
            for (int i = 0; i < points.Count; i++)
            {
                Vector3 p1 = points[i];
                Vector3 p2 = (i == points.Count - 1) ? points[0] : points[i + 1];

                float distance = Vector3.Distance(p1, p2);

                Vector3 midPoint = (p1 + p2) / 2;
                GameObject labelObj = Instantiate(distanceLabelPrefab, midPoint, Quaternion.identity);
                TextMeshPro textMesh = labelObj.GetComponentInChildren<TextMeshPro>();
                if (textMesh != null)
                {
                    textMesh.text = $"{distance:F2} m";
                }
                distanceLabels.Add(labelObj);

                if (points.Count == 2 && i == 1) break;
            }

            if (points.Count > 2)
            {
                float area = CalculatePolygonArea(points);
                areaText.text = $"Area: {area:F2} m²";
            }
            else
            {
                areaText.text = $"Distance: {Vector3.Distance(points[0], points[1]):F2} m";
            }
        }
    }

    float CalculatePolygonArea(List<Vector3> verts)
    {
        float area = 0;
        for (int i = 0; i < verts.Count; i++)
        {
            Vector3 p1 = verts[i];
            Vector3 p2 = verts[(i + 1) % verts.Count];
            area += (p1.x * p2.z) - (p2.x * p1.z);
        }
        return Mathf.Abs(area / 2.0f);
    }

    void ResetMeasurement()
    {
        points.Clear();
        lineRenderer.positionCount = 0;
        areaText.text = "Tap to place points";

        // remove all points
        foreach (var marker in pointObjects)
        {
            Destroy(marker);
        }
        pointObjects.Clear();

        // remove all labels
        foreach (var label in distanceLabels)
        {
            Destroy(label);
        }
        distanceLabels.Clear();

        DeselectPoint();
    }

    void DeleteSelectedPoint()
    {
        if (selectedPoint != null && selectedIndex >= 0)
        {
            Destroy(selectedPoint);
            pointObjects.RemoveAt(selectedIndex);
            points.RemoveAt(selectedIndex);

            DeselectPoint();

            UpdateLine();
            UpdateMeasurements();
        }
    }

    void GoBackToMainScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("HomeScreen"); // go back to main menu
    }
}
