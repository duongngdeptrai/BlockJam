using System;
using System.Collections.Generic;
using System.IO;
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
    private Transform mapRoot;

    public Transform MapRoot => mapRoot;

    private void Awake()
    {
        Debug.Log($"MapController Awake: doorPrefabs is null={doorPrefabs == null}, count={doorPrefabs?.Count ?? -1}");
        if (doorPrefabs == null)
        {
            Debug.LogError("MapController: doorPrefabs list is NULL in Awake(). Assign prefabs in Inspector.");
        }
        else if (doorPrefabs.Count == 0)
        {
            Debug.LogError("MapController: doorPrefabs list is EMPTY in Awake(). Add entries in Inspector.");
        }
        else
        {
            foreach (var entry in doorPrefabs)
            {
                if (entry != null && entry.prefab != null && !string.IsNullOrEmpty(entry.key))
                {
                    doorPrefabDict[entry.key] = entry.prefab;
                }
                else
                {
                    Debug.LogWarning($"Skipped invalid entry: key='{entry?.key}', prefab={(entry?.prefab != null)}");
                }
            }
            Debug.Log($"MapController: doorPrefabDict loaded {doorPrefabDict.Count} entries. Keys: [{string.Join(", ", doorPrefabDict.Keys)}]");
        }

        if (blockPrefabs == null)
        {
            Debug.LogError("MapController: blockPrefabs list is NULL in Awake(). Assign prefabs in Inspector.");
        }
        else if (blockPrefabs.Count == 0)
        {
            Debug.LogError("MapController: blockPrefabs list is EMPTY in Awake(). Add entries in Inspector.");
        }
        else
        {
            foreach (var entry in blockPrefabs)
            {
                if (entry != null && entry.prefab != null && !string.IsNullOrEmpty(entry.key))
                {
                    blockPrefabDict[entry.key] = entry.prefab;
                }
            }
            Debug.Log($"MapController: blockPrefabDict loaded {blockPrefabDict.Count} entries. Keys: [{string.Join(", ", blockPrefabDict.Keys)}]");
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
        currentLevelIndex = Mathf.Max(GameConstants.MIN_LEVEL, levelIndex);
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

        EnsureMapRoot();
        GenWalls();
        GenDoors();
        GenGrid();
        GenBlocks();
        FitMapToScreen();
    }

    public void Clear()
    {
        ClearContainer(wallContainer);
        ClearContainer(doorContainer);
        ClearContainer(gridContainer);
        ClearContainer(blockContainer);

        if (listDoor != null) listDoor.Clear();
        if (listWall != null) listWall.Clear();
        if (listBlock != null) listBlock.Clear();

        if (mapRoot != null)
        {
            Destroy(mapRoot.gameObject);
            mapRoot = null;
        }

        currentLevelData = null;
    }

    private void ClearContainer(Transform container)
    {
        if (container == null) return;

        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Destroy(container.GetChild(i).gameObject);
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
            GameStateMachine.ToWin();
            LevelCompleted?.Invoke();
        }

        foreach (var block in listBlock)
        {
            if(block.GetHasMine())
            {
                block.SetMineCount(block.GetMineCount() - 1);
                if(block.GetMineCount() == 0)
                {
                    GameStateMachine.ToLose();
                }
            }
        }
    }

    public int Rows => rows;
    public int Columns => columns;
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
