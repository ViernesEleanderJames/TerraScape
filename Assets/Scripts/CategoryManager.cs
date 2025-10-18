using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq; // <-- Needed for Where/ToList

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

    [Header("Plant Filters")]
    [SerializeField] private Button withPotButton;    // assign in Inspector
    [SerializeField] private Button withoutPotButton; // assign in Inspector

    [SerializeField] private List<ModelDataSO> plantModels;
    [SerializeField] private List<ModelDataSO> decorationModels;
    [SerializeField] private List<ModelDataSO> structureModels;
    [SerializeField] private List<ModelDataSO> pathwayModels;
    [SerializeField] private List<ModelDataSO> furnitureModels;
    [SerializeField] private List<ModelDataSO> groundCoverModels;

    // Keeps a reference to plants for filtering and prevents clicking filters too early
    private List<ModelDataSO> currentPlantModels;
    private bool plantsViewActive = false;

    public List<ModelDataSO> GetAllModels()
    {
        List<ModelDataSO> allModels = new List<ModelDataSO>();
        if (plantModels != null) allModels.AddRange(plantModels);
        if (decorationModels != null) allModels.AddRange(decorationModels);
        if (structureModels != null) allModels.AddRange(structureModels);
        if (pathwayModels != null) allModels.AddRange(pathwayModels);
        if (furnitureModels != null) allModels.AddRange(furnitureModels);
        if (groundCoverModels != null) allModels.AddRange(groundCoverModels);
        return allModels;
    }

    private void Awake()
    {
        if (modelScrollManager != null)
            modelScrollManager.gameObject.SetActive(false);

        if (backButton != null)
        {
            backButton.gameObject.SetActive(false);
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(OnBackButtonClicked);
        }

        if (withPotButton != null) withPotButton.gameObject.SetActive(false);
        if (withoutPotButton != null) withoutPotButton.gameObject.SetActive(false);
    }

    private void Start()
    {
        // Category listeners
        plantsButton?.onClick.AddListener(OnPlantsCategorySelected);
        decorationsButton?.onClick.AddListener(() => OnNonPlantsCategorySelected(decorationModels));
        structuresButton?.onClick.AddListener(() => OnNonPlantsCategorySelected(structureModels));
        pathwaysButton?.onClick.AddListener(() => OnNonPlantsCategorySelected(pathwayModels));
        furnitureButton?.onClick.AddListener(() => OnNonPlantsCategorySelected(furnitureModels));
        groundCoverButton?.onClick.AddListener(() => OnNonPlantsCategorySelected(groundCoverModels));

        // Filter listeners (safe even if clicked early)
        if (withPotButton != null)
        {
            withPotButton.onClick.RemoveAllListeners();
            withPotButton.onClick.AddListener(() => ShowPlantsFiltered(true));
        }

        if (withoutPotButton != null)
        {
            withoutPotButton.onClick.RemoveAllListeners();
            withoutPotButton.onClick.AddListener(() => ShowPlantsFiltered(false));
        }
    }

    private void OnPlantsCategorySelected()
    {
        if (plantModels == null || plantModels.Count == 0)
            return;

        plantsViewActive = true;
        currentPlantModels = plantModels;

        // Show all plants by default
        modelScrollManager?.PopulateModelScrollView(currentPlantModels);
        modelScrollManager?.gameObject.SetActive(true);

        // Show filter buttons
        if (withPotButton != null) withPotButton.gameObject.SetActive(true);
        if (withoutPotButton != null) withoutPotButton.gameObject.SetActive(true);

        backButton?.gameObject.SetActive(true);
        ToggleCategoryButtons(false);
    }

    private void OnNonPlantsCategorySelected(List<ModelDataSO> models)
    {
        plantsViewActive = false; // leaving plants view
        HidePlantFilters();

        OnCategorySelected(models);
    }

    public void OnCategorySelected(List<ModelDataSO> models)
    {
        if (models == null || models.Count == 0)
        {
            // Still show the panel (optional) but empty list is allowed
            modelScrollManager?.PopulateModelScrollView(new List<ModelDataSO>());
            modelScrollManager?.gameObject.SetActive(true);
            backButton?.gameObject.SetActive(true);
            ToggleCategoryButtons(false);
            return;
        }

        modelScrollManager?.PopulateModelScrollView(models);
        modelScrollManager?.gameObject.SetActive(true);
        backButton?.gameObject.SetActive(true);
        ToggleCategoryButtons(false);
    }

    private void ShowPlantsFiltered(bool wantPot)
    {
        // If filters are clicked before Plants is opened, fall back safely
        var source = plantsViewActive ? (currentPlantModels ?? plantModels) : plantModels;
        if (source == null) return;

        var filtered = source.Where(p => p != null && p.hasPot == wantPot).ToList();
        modelScrollManager?.PopulateModelScrollView(filtered);
        modelScrollManager?.gameObject.SetActive(true);

        // keep filters visible in Plants view so user can switch anytime
    }

    public void OnBackButtonClicked()
    {
        plantsViewActive = false;

        modelScrollManager?.gameObject.SetActive(false);
        backButton?.gameObject.SetActive(false);
        HidePlantFilters();

        ToggleCategoryButtons(true);
    }

    private void HidePlantFilters()
    {
        if (withPotButton != null) withPotButton.gameObject.SetActive(false);
        if (withoutPotButton != null) withoutPotButton.gameObject.SetActive(false);
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
