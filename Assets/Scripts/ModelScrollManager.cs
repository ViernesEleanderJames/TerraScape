using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ModelScrollManager : MonoBehaviour
{
    [SerializeField] private GameObject modelButtonPrefab;
    [SerializeField] private Transform contentPanel;

    private List<GameObject> currentButtons = new List<GameObject>();

    public void PopulateModelScrollView(List<ModelDataSO> models)
    {
        ClearModelButtons();

        if (models == null || models.Count == 0)
            return;

        foreach (var model in models)
        {
            if (model.modelPrefab == null)
                continue;

            GameObject newButton = Instantiate(modelButtonPrefab, contentPanel);
            newButton.transform.localScale = Vector3.one;
            newButton.name = model.modelName;

            TMP_Text buttonText = newButton.transform.Find("ModelName")?.GetComponent<TMP_Text>();
            if (buttonText != null)
                buttonText.text = model.modelName;

            Image buttonImage = newButton.transform.Find("ModelImage")?.GetComponent<Image>();
            if (buttonImage != null && model.modelImage != null)
                buttonImage.sprite = model.modelImage;

            Button btn = newButton.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() => PlaceModel(model.modelPrefab));

            currentButtons.Add(newButton);
        }
    }

    private void ClearModelButtons()
    {
        foreach (GameObject btn in currentButtons)
            Destroy(btn);

        currentButtons.Clear();
    }

    public void PlaceModel(GameObject modelPrefab)
    {
        if (modelPrefab == null)
            return;

        ARRaycastPlace arRaycastPlace = FindObjectOfType<ARRaycastPlace>();
        if (arRaycastPlace != null)
            arRaycastPlace.SetSelectedModel(modelPrefab);
    }
}
