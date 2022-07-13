using UnityEngine;

[System.Serializable]
public class GameData
{
    public int currentLevelOrder;
    public Vector3 playerPosition;
    public SerializableDictionary<int, FloatingObject.ObjectData> movableObjectsData = new SerializableDictionary<int, FloatingObject.ObjectData>();

    public bool isAIDistanceSet;
    public Vector2 AIDistanceFromBound;

    public bool isGravityOn;
}
