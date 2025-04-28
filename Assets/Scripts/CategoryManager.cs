using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CategoryManager : MonoBehaviour
{
    [Header("Category Buttons")]
    [SerializeField] private Button plantsButton;
    [SerializeField] private Button decorationsButton;
    [SerializeField] private Button structuresButton;
    [SerializeField] private Button pathwaysButton;
    [SerializeField] private Button furnitureButton;

    [Header("Model Scroll Manager Reference")]
    [SerializeField] private ModelScrollManager modelScrollManager;
    
    [Header("Back Button")]
    [SerializeField] private Button backButton;

    [Header("Models for Each Category")]
    [SerializeField] private List<ModelDataSO> plantModels;
    [SerializeField] private List<ModelDataSO> decorationModels;
    [SerializeField] private List<ModelDataSO> structureModels;
    [SerializeField] private List<ModelDataSO> pathwayModels;
    [SerializeField] private List<ModelDataSO> furnitureModels;

    private void Awake()
    {
        // Ensure Model Scroll Manager is hidden initially
        if (modelScrollManager != null)
        {
            modelScrollManager.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("[CategoryManager] ModelScrollManager is NOT assigned!");
        }

        // Ensure Back Button is hidden initially
        if (backButton != null)
        {
            backButton.gameObject.SetActive(false);
            backButton.onClick.AddListener(OnBackButtonClicked);
        }
        else
        {
            Debug.LogError("[CategoryManager] Back Button is NOT assigned!");
        }
    }

    private void Start()
    {
        // Assign button click events
        plantsButton?.onClick.AddListener(OnPlantsButtonClicked);
        decorationsButton?.onClick.AddListener(OnDecorationsButtonClicked);
        structuresButton?.onClick.AddListener(OnStructuresButtonClicked);
        pathwaysButton?.onClick.AddListener(OnPathwaysButtonClicked);
        furnitureButton?.onClick.AddListener(OnFurnitureButtonClicked);
    }

    // Parameterless methods that call OnCategorySelected with the correct model list
    public void OnPlantsButtonClicked()
    {
        OnCategorySelected(plantModels);
    }

    public void OnDecorationsButtonClicked()
    {
        OnCategorySelected(decorationModels);
    }

    public void OnStructuresButtonClicked()
    {
        OnCategorySelected(structureModels);
    }

    public void OnPathwaysButtonClicked()
    {
        OnCategorySelected(pathwayModels);
    }

    public void OnFurnitureButtonClicked()
    {
        OnCategorySelected(furnitureModels);
    }

    // Method to handle category selection and model scroll view display
    public void OnCategorySelected(List<ModelDataSO> models)
    {
        if (models == null || models.Count == 0)
        {
            Debug.LogWarning("[CategoryManager] No models found for this category!");
            return;
        }

        Debug.Log($"[CategoryManager] Displaying {models.Count} models.");

        // Populate model scroll view
        modelScrollManager?.PopulateModelScrollView(models);
        modelScrollManager?.gameObject.SetActive(true);
        backButton?.gameObject.SetActive(true);

        // Hide category buttons when showing models
        ToggleCategoryButtons(false);
    }

    public void OnBackButtonClicked()
    {
        // Hide model scroll view and show category buttons again
        modelScrollManager?.gameObject.SetActive(false);
        backButton?.gameObject.SetActive(false);
        ToggleCategoryButtons(true);
    }

    private void ToggleCategoryButtons(bool state)
    {
        plantsButton?.gameObject.SetActive(state);
        decorationsButton?.gameObject.SetActive(state);
        structuresButton?.gameObject.SetActive(state);
        pathwaysButton?.gameObject.SetActive(state);
        furnitureButton?.gameObject.SetActive(state);
    }
}
