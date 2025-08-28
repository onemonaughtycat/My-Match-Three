using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Difficulty", order = 1)]
public class DifficultyScriptableObject : ScriptableObject
{
    public int tilePoints;
    public List<TileTypeScriptableObject> tileTypes;
}
