using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CategoryManager : MonoBehaviour
{
    [SerializeField] private Button plantsButton;
    [SerializeField] private Button decorationsButton;
    [SerializeField] private Button structuresButton;
    [SerializeField] private Button pathwaysButton;
    [SerializeField] private Button furnitureButton;
    [SerializeField] private Button groundCoverButton;
    [SerializeField] private ModelScrollManager modelScrollManager;
    [SerializeField] private Button backButton;

    [SerializeField] private List<ModelDataSO> plantModels;
    [SerializeField] private List<ModelDataSO> decorationModels;
    [SerializeField] private List<ModelDataSO> structureModels;
    [SerializeField] private List<ModelDataSO> pathwayModels;
    [SerializeField] private List<ModelDataSO> furnitureModels;
    [SerializeField] private List<ModelDataSO> groundCoverModels;

    public List<ModelDataSO> GetAllModels()
    {
        List<ModelDataSO> allModels = new List<ModelDataSO>();
        if (plantModels != null) allModels.AddRange(plantModels); //
        if (decorationModels != null) allModels.AddRange(decorationModels); //
        if (structureModels != null) allModels.AddRange(structureModels); //
        if (pathwayModels != null) allModels.AddRange(pathwayModels); //
        if (furnitureModels != null) allModels.AddRange(furnitureModels); //
        if (groundCoverModels != null) allModels.AddRange(groundCoverModels); //
        return allModels;
    }

    private void Awake()
    {
        if (modelScrollManager != null)
            modelScrollManager.gameObject.SetActive(false);

        if (backButton != null)
        {
            backButton.gameObject.SetActive(false);
            backButton.onClick.AddListener(OnBackButtonClicked);
        }
    }

    private void Start()
    {
        plantsButton?.onClick.AddListener(() => OnCategorySelected(plantModels));
        decorationsButton?.onClick.AddListener(() => OnCategorySelected(decorationModels));
        structuresButton?.onClick.AddListener(() => OnCategorySelected(structureModels));
        pathwaysButton?.onClick.AddListener(() => OnCategorySelected(pathwayModels));
        furnitureButton?.onClick.AddListener(() => OnCategorySelected(furnitureModels));
        groundCoverButton?.onClick.AddListener(() => OnCategorySelected(groundCoverModels));
    }

    public void OnCategorySelected(List<ModelDataSO> models)
    {
        if (models == null || models.Count == 0)
            return;

        modelScrollManager?.PopulateModelScrollView(models);
        modelScrollManager?.gameObject.SetActive(true);
        backButton?.gameObject.SetActive(true);
        ToggleCategoryButtons(false);
    }

    public void OnBackButtonClicked()
    {
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
        groundCoverButton?.gameObject.SetActive(state);
    }
}
