using UnityEngine;

[CreateAssetMenu(fileName = "NewModelData", menuName = "ScriptableObjects/ModelData")]
public class ModelDataSO : ScriptableObject
{
    public string modelName;
    public Sprite modelImage; // Ensure this field exists
    public GameObject modelPrefab;
}

