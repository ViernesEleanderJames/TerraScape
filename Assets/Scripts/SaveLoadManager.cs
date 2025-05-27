using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq; // Required for LINQ if used, though direct iteration is fine

public class SaveLoadManager : MonoBehaviour
{
    public CategoryManager categoryManager; // Assign in Inspector
    public ARRaycastPlace arRaycastPlace;   // Assign in Inspector

    private string saveFileName = "terraScapeSaveData.json";
    private string savePath;
    private Dictionary<string, GameObject> modelPrefabMap = new Dictionary<string, GameObject>();

    void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, saveFileName); //
        BuildPrefabMap();
    }

    // Helper method to get all ModelDataSO from CategoryManager
    // Add this method to your CategoryManager.cs script
    /*
    In CategoryManager.cs:
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
    */

    void BuildPrefabMap()
    {
        modelPrefabMap.Clear();
        if (categoryManager == null)
        {
            Debug.LogError("CategoryManager not assigned to SaveLoadManager.");
            return;
        }

        // Assuming CategoryManager has a public method GetAllModels() as described above
        List<ModelDataSO> allModelDataSOs = categoryManager.GetAllModels(); 

        foreach (ModelDataSO modelData in allModelDataSOs)
        {
            if (modelData != null && modelData.modelPrefab != null) //
            {
                if (!modelPrefabMap.ContainsKey(modelData.modelPrefab.name))
                {
                    modelPrefabMap.Add(modelData.modelPrefab.name, modelData.modelPrefab);
                }
                else
                {
                    Debug.LogWarning($"Duplicate prefab name '{modelData.modelPrefab.name}' found. The first one will be used. Consider unique prefab names.");
                }
            }
        }
        Debug.Log($"Prefab map built with {modelPrefabMap.Count} entries.");
    }

    public void SaveScene()
    {
        SceneSaveData saveData = new SceneSaveData();
        GameObject[] placedObjects = GameObject.FindGameObjectsWithTag("ARModel"); //

        foreach (GameObject obj in placedObjects)
        {
            ARModelInfo modelInfo = obj.GetComponent<ARModelInfo>();
            if (modelInfo == null || string.IsNullOrEmpty(modelInfo.prefabName))
            {
                Debug.LogWarning($"Object {obj.name} is tagged 'ARModel' but missing ARModelInfo or prefabName. Skipping save.");
                continue;
            }

            PlacedObjectData objectData = new PlacedObjectData
            {
                prefabName = modelInfo.prefabName,
                position = obj.transform.position,
                rotation = obj.transform.rotation,
                scale = obj.transform.localScale
            };
            saveData.placedObjectsData.Add(objectData);
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(savePath, json);
        Debug.Log($"Scene saved to {savePath} with {saveData.placedObjectsData.Count} objects.");
    }

    public void LoadScene()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("Save file not found at: " + savePath);
            return;
        }

        ClearExistingARModels();
        if (arRaycastPlace != null)
        {
            arRaycastPlace.DeleteSelectedObject(); // Clear current selection visuals
        }


        string json = File.ReadAllText(savePath);
        SceneSaveData saveData = JsonUtility.FromJson<SceneSaveData>(json);

        if (modelPrefabMap.Count == 0) {
            Debug.LogWarning("Prefab map is empty. Attempting to rebuild.");
            BuildPrefabMap();
             if (modelPrefabMap.Count == 0) {
                Debug.LogError("Failed to build prefab map. Cannot load scene models. Ensure CategoryManager has ModelDataSOs with valid prefabs.");
                return;
            }
        }

        foreach (PlacedObjectData objectData in saveData.placedObjectsData)
        {
            if (modelPrefabMap.TryGetValue(objectData.prefabName, out GameObject prefabToLoad))
            {
                GameObject loadedObject = Instantiate(prefabToLoad, objectData.position, objectData.rotation);
                loadedObject.transform.localScale = objectData.scale;
                loadedObject.tag = "ARModel"; //

                ARModelInfo newModelInfo = loadedObject.AddComponent<ARModelInfo>();
                newModelInfo.prefabName = objectData.prefabName;

                if (loadedObject.GetComponent<Collider>() == null) //
                {
                    loadedObject.AddComponent<BoxCollider>(); //
                }

                // If using the Outline script for highlighting
                Outline outline = loadedObject.GetComponent<Outline>();
                if (outline == null && prefabToLoad.GetComponent<Outline>() != null) // If original prefab had an outline
                {
                    outline = loadedObject.AddComponent<Outline>();
                    // Optionally copy properties from prefab's outline
                    Outline prefabOutline = prefabToLoad.GetComponent<Outline>();
                    outline.OutlineMode = prefabOutline.OutlineMode;
                    outline.OutlineColor = prefabOutline.OutlineColor;
                    outline.OutlineWidth = prefabOutline.OutlineWidth;
                }
                if (outline != null) outline.enabled = false; // Disable highlight on load
            }
            else
            {
                Debug.LogWarning($"Prefab with name '{objectData.prefabName}' not found in map. Cannot load this object.");
            }
        }
        Debug.Log($"Scene loaded from {savePath}. Attempted to load {saveData.placedObjectsData.Count} objects.");
    }

    private void ClearExistingARModels()
    {
        GameObject[] placedObjects = GameObject.FindGameObjectsWithTag("ARModel"); //
        foreach (GameObject obj in placedObjects)
        {
            Destroy(obj);
        }
        Debug.Log($"Cleared {placedObjects.Length} existing ARModels.");
    }
}