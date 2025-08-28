using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/TileType", order = 1)]
public class TileTypeScriptableObject : ScriptableObject
{
    public int id;
    public Sprite sprite;
}
