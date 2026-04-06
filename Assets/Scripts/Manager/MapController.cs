using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.SceneManagement;
using UnityEngine;

public partial class MapController : MonoBehaviour
{
    public event Action LevelCompleted;

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
    private Dictionary<string, Door> doorPrefabDict = new Dictionary<string, Door>();
    private Dictionary<string, Block> blockPrefabDict = new Dictionary<string, Block>();
    private LevelData currentLevelData;
    private List<Door> listDoor = new List<Door>();
    private List<Wall> listWall = new List<Wall>();
    private List<Block> listBlock = new List<Block>();
    private int currentLevelIndex = 1;

    private void Awake()
    {
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
        
        foreach(var entry in blockPrefabs)
        {
            blockPrefabDict[entry.key] = entry.prefab;
        }
    }

    private void OnEnable()
    {
        BlockMove.BlockConsumed += OnBlockConsumed;
    }

    private void OnDisable()
    {
        BlockMove.BlockConsumed -= OnBlockConsumed;
    }

    public void InitMap(int levelIndex)
    {
        currentLevelIndex = Mathf.Max(1, levelIndex);
        levelJsonFileName = $"Levels/level{currentLevelIndex}.json";

        Clear();

        currentLevelData = LevelLoader.LoadLevel(levelJsonFileName);
        if (currentLevelData == null)
        {
            Debug.LogError($"Failed to load level data from {levelJsonFileName}");
            return;
        }

        rows = currentLevelData.rows;
        columns = currentLevelData.columns;

        GenWalls();
        GenDoors();
        GenGrid();
        GenBlocks();

        return;
    }

    public void Clear()
    {
        ClearContainer(wallContainer);
        ClearContainer(doorContainer);
        ClearContainer(gridContainer);
        ClearContainer(blockContainer);
        
        if (listDoor != null)
            listDoor.Clear();
        if (listWall != null)
            listWall.Clear();
        if (listBlock != null)
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

    public void ReloadCurrentLevel()
    {
        InitMap(currentLevelIndex);
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
            StateManager.ToWin();
            LevelCompleted?.Invoke();
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