using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ModelScrollManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject modelButtonPrefab; // Prefab for model buttons
    [SerializeField] private Transform contentPanel;       // Parent panel to hold buttons

    private List<GameObject> currentButtons = new List<GameObject>();

    public void PopulateModelScrollView(List<ModelDataSO> models)
    {
        Debug.Log("[ModelScrollManager] Populating model scroll view with " + models.Count + " models.");
        
        ClearModelButtons(); // Remove previous buttons

        if (models == null || models.Count == 0)
        {
            Debug.LogWarning("No models available to display in ModelScrollView.");
            return;
        }

        foreach (var model in models)
        {
            Debug.Log("Creating button for model: " + model.modelName);
            
            // Check if modelPrefab is valid
            if (model.modelPrefab == null)
            {
                Debug.LogError("[ModelScrollManager] modelPrefab is missing for " + model.modelName);
            }

            // Instantiate button for the model
            GameObject newButton = Instantiate(modelButtonPrefab, contentPanel);
            newButton.transform.localScale = Vector3.one; // Ensure correct scale
            newButton.name = model.modelName;

            // Set button text
            TMP_Text buttonText = newButton.transform.Find("ModelName")?.GetComponent<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = model.modelName;
            }
            else
            {
                Debug.LogError("TMP_Text component not found in ModelButton prefab.");
            }

            // Set button image
            Image buttonImage = newButton.transform.Find("ModelImage")?.GetComponent<Image>();
            if (buttonImage != null && model.modelImage != null)
            {
                buttonImage.sprite = model.modelImage;
            }
            else
            {
                Debug.LogError("ModelImage component or sprite not found for " + model.modelName);
            }

            // Add click event
            Button btn = newButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => PlaceModel(model.modelPrefab)); // Here, you are passing the prefab for placement.
            }
            else
            {
                Debug.LogError("Button component missing in ModelButton prefab.");
            }

            // Store button in list
            currentButtons.Add(newButton);
        }
    }

    private void ClearModelButtons()
    {
        foreach (GameObject btn in currentButtons)
        {
            Destroy(btn);
        }
        currentButtons.Clear();
    }

    public void PlaceModel(GameObject modelPrefab)
    {
        if (modelPrefab == null)
        {
            Debug.LogError("Selected model prefab is null.");
            return;
        }
        
        Debug.Log("Placing model: " + modelPrefab.name);

        ARRaycastPlace arRaycastPlace = FindObjectOfType<ARRaycastPlace>();
        if (arRaycastPlace != null)
        {
            arRaycastPlace.SetSelectedModel(modelPrefab); // Pass the model to the ARRaycastPlace to be used in placement
        }
        else
        {
            Debug.LogError("ARRaycastPlace script not found in the scene.");
        }
    }
}
