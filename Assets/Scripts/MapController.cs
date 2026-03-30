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
    [SerializeField] private Transform gridContainer;
    private Dictionary<string, Door> doorPrefabDict;
    private Dictionary<string, Block> blockPrefabDict;
    private LevelData currentLevelData;
    private List<Door> listDoor;
    private List<Wall> listWall;
    private List<Block> listBlock;

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

        // Initialize lists
        listDoor = new List<Door>();
        listWall = new List<Wall>();
        listBlock = new List<Block>();
    }

    private void OnEnable()
    {
        BlockMove.BlockConsumed += OnBlockConsumed;
    }

    private void OnDisable()
    {
        BlockMove.BlockConsumed -= OnBlockConsumed;
    }

    public void InitMap()
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

    public void Clear()
    {
        ClearContainer(wallContainer);
        ClearContainer(doorContainer);
        ClearContainer(gridContainer);
        ClearContainer(blockContainer);
        
        // Clear lists
        listDoor.Clear();
        listWall.Clear();
        listBlock.Clear();
        
        currentLevelData = null;
    }

    private void ClearContainer(Transform container)
    {
        if (container == null)
            return;

        while (container.childCount > 0)
        {
            DestroyImmediate(container.GetChild(0).gameObject);
        }
    }

    public void Reset()
    {
        Clear();

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

    public LevelData GetCurrentLevelData()
    {
        return currentLevelData;
    }

    private void OnBlockConsumed(Block consumedBlock)
    {
        if (consumedBlock == null || listBlock == null)
        {
            return;
        }

        listBlock.Remove(consumedBlock);

        if (listBlock.Count == 0)
        {
            Debug.Log("All blocks consumed! Level complete!");
            // You can trigger level completion logic here, such as loading the next level or showing a victory screen.
        }
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