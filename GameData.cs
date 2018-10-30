using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CellulAR;

[CreateAssetMenu(fileName = "GameData", menuName = "Cellular/Game data", order = 1)]
public class GameData : ScriptableObject
{
    [Header("Colors")]
    public Color noneColor;
    public Color player1Color;
    public Color player2Color;
    public Gradient player1Points;
    public Gradient player2Points;
    [Space(20)]

    public float secondPerPoint;    
    public float spawnRadius;
    public int startingPoints;
    [Header("Prefabs")]
    public GameObject particlePrefab;
}
