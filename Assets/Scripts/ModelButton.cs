using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ModelButton : MonoBehaviour
{
    public Image modelImage;  // Reference to the Image component
    public TMP_Text modelNameText;  // Reference to the Text component

    // Method to set model data dynamically from ModelData
    public void SetModelData(Sprite image, string name)
    {
        modelImage.sprite = image;  // Set the image
        modelNameText.text = name;  // Set the name
    }
}
