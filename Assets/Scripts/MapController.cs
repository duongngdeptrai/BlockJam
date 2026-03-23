using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.SceneManagement;
using UnityEngine;

public partial class MapController : MonoBehaviour
{
    [SerializeField] private int rows = 7, columns = 14;
    [SerializeField] private List<DoorPrefabEntry> doorPrefabs;
    [SerializeField] private Wall wallPrefab;
    [SerializeField] private List<BlockPrefabEntry> blockPrefabs;
    [SerializeField] private GameObject Grid;
    [SerializeField] private string levelJsonFileName = "Levels/level1.json";
    [SerializeField] private Transform blockContainer;
    [SerializeField] private Transform doorContainer;
    [SerializeField] private Transform wallContainer;
    private Dictionary<string, Door> doorPrefabDict;
    private Dictionary<string, Block> blockPrefabDict;
    private LevelData currentLevelData;

    private void Awake()
    {
        // Initialize door prefab dictionary
        doorPrefabDict = new Dictionary<string, Door>();
        if (doorPrefabs != null)
        {
            foreach (var entry in doorPrefabs)
            {
                doorPrefabDict[entry.key] = entry.prefab;
            }
        }
        else
        {
            Debug.LogWarning("Door prefabs list is empty or not assigned!");
        }
        
        // Initialize block prefab dictionary
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

[System.Serializable]
public class DoorPrefabEntry
{
    public string key;
    public Door prefab;
}

[System.Serializable]
public class BlockPrefabEntry
{
    public string key;
    public Block prefab;
}