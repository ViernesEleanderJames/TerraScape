using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class SceneSaveData
{
    public List<PlacedObjectData> placedObjectsData = new List<PlacedObjectData>();
}

public class PlacedObjectData
{
    public string prefabName; // Name of the original prefab
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
}
