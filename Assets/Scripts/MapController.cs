using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.SceneManagement;
using UnityEngine;

public partial class MapController : MonoBehaviour
{
    [SerializeField] private int rows = 7, columns = 14;
    [SerializeField] private Door doorPrefab;
    [SerializeField] private Wall wallPrefab;
    [SerializeField] private List<BlockPrefabEntry> blockPrefabs;
    [SerializeField] private GameObject Grid;
    [SerializeField] private string levelJsonFileName = "Levels/level1.json";
    private Dictionary<string, Block> blockPrefabDict;
    private LevelData currentLevelData;

    private void Awake()
    {
        blockPrefabDict = new Dictionary<string, Block>();
        foreach(var entry in blockPrefabs)
        {
            blockPrefabDict[entry.key] = entry.prefab;
        }
    }

    private void Start()
    {
        currentLevelData = LevelLoader.LoadLevel(levelJsonFileName);
        if (currentLevelData != null)
        {
            rows = currentLevelData.rows;
            columns = currentLevelData.columns;
        }

        GenWalls();
        GenDoors();
        GenGrid();
        GenBlocks();
    }
    
}