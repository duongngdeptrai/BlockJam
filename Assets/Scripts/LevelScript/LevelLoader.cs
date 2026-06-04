using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public static class LevelLoader
{
    public static LevelData LoadLevel(string jsonFileName)
    {
        string path = Path.Combine(Application.streamingAssetsPath, jsonFileName);

#if UNITY_ANDROID && !UNITY_EDITOR
        string json = ReadFromStreamingAssetsAndroid(path, out bool fileFound);
        if (!fileFound || string.IsNullOrEmpty(json))
        {
            return null;
        }
#else
        if (!File.Exists(path))
        {
            return null;
        }
        string json = File.ReadAllText(path);
#endif

        try
        {
            LevelDataWrapper wrapper = JsonUtility.FromJson<LevelDataWrapper>(json);
            LevelData level = wrapper.level;
            ValidateLevelData(level);
            return level;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading level from JSON: {e.Message}");
            return null;
        }
    }

    public static bool FileExistsOnAndroid(string path)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return RequestExistsOnAndroid(path);
#else
        return File.Exists(path);
#endif
    }

    private static bool RequestExistsOnAndroid(string path)
    {
        UnityWebRequest uwr = UnityWebRequest.Head(path);
        var operation = uwr.SendWebRequest();
        while (!operation.isDone)
        {
        }

        bool exists = uwr.result == UnityWebRequest.Result.Success;
        uwr.Dispose();
        return exists;
    }

    private static string ReadFromStreamingAssetsAndroid(string path, out bool fileFound)
    {
        fileFound = false;
        UnityWebRequest uwr = UnityWebRequest.Get(path);
        var operation = uwr.SendWebRequest();
        while (!operation.isDone)
        {
        }

        if (uwr.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Failed to read StreamingAssets on Android: {uwr.error}");
            uwr.Dispose();
            return null;
        }

        fileFound = true;
        string text = uwr.downloadHandler.text;
        uwr.Dispose();
        return text;
    }

    private static void ValidateLevelData(LevelData level)
    {
        if (level == null) return;

        if (level.timeLimit <= 0)
        {
            Debug.LogWarning($"Invalid timeLimit {level.timeLimit}, using default value {GameConstants.DEFAULT_TIME_LIMIT}");
            level.timeLimit = GameConstants.DEFAULT_TIME_LIMIT;
        }

        if (level.doors != null)
        {
            List<DoorSpawnData> validDoors = new List<DoorSpawnData>();
            foreach (var door in level.doors)
            {
                if (!IsValidDoorSpawnData(door, level.rows, level.columns))
                {
                    Debug.LogWarning($"Invalid door data skipped: row={door.row}, col={door.column}, type={door.doorType}, color={door.color}");
                    continue;
                }
                validDoors.Add(door);
            }
            level.doors = validDoors;
        }

        if (level.walls != null)
        {
            List<WallSpawnData> validWalls = new List<WallSpawnData>();
            foreach (var wall in level.walls)
            {
                if (!IsValidWallSpawnData(wall, level.rows, level.columns))
                {
                    Debug.LogWarning($"Invalid wall data skipped: row={wall.row}, col={wall.column}, dir={wall.direction}");
                    continue;
                }
                validWalls.Add(wall);
            }
            level.walls = validWalls;
        }

        if (level.blocks != null)
        {
            List<BlockData> validBlocks = new List<BlockData>();
            foreach (var block in level.blocks)
            {
                if (string.IsNullOrEmpty(block.blockType))
                {
                    Debug.LogWarning($"Invalid block: blockType is empty, skipped");
                    continue;
                }
                if (string.IsNullOrEmpty(block.color))
                {
                    Debug.LogWarning($"Invalid block at row={block.row}, col={block.column}: color is empty, skipped");
                    continue;
                }
                if (!System.Enum.TryParse<ColorType>(block.color, true, out _))
                {
                    Debug.LogWarning($"Block color '{block.color}' cannot be parsed to ColorType enum, skipped");
                    continue;
                }
                if (block.hasSecondColor && !string.IsNullOrEmpty(block.secondColor))
                {
                    if (!System.Enum.TryParse<ColorType>(block.secondColor, true, out _))
                    {
                        Debug.LogWarning($"Block secondColor '{block.secondColor}' cannot be parsed to ColorType enum, clearing secondColor");
                        block.hasSecondColor = false;
                        block.secondColor = null;
                    }
                }
                validBlocks.Add(block);
            }
            level.blocks = validBlocks;
        }
    }

    private static bool IsValidDoorSpawnData(DoorSpawnData door, int rows, int columns)
    {
        if (door.row < -1 || door.row > rows) return false;
        if (door.column < -1 || door.column > columns) return false;
        if (string.IsNullOrEmpty(door.direction)) return false;
        if (!IsValidDirection(door.direction)) return false;
        if (string.IsNullOrEmpty(door.doorType)) return false;
        if (string.IsNullOrEmpty(door.color)) return false;
        if (!System.Enum.TryParse<ColorType>(door.color, out _))
        {
            Debug.LogWarning($"Door color '{door.color}' cannot be parsed to ColorType enum");
            return false;
        }
        return true;
    }

    private static bool IsValidWallSpawnData(WallSpawnData wall, int rows, int columns)
    {
        if (wall.row < 0 || wall.row >= rows) return false;
        if (wall.column < 0 || wall.column >= columns) return false;
        if (string.IsNullOrEmpty(wall.direction)) return false;
        if (!IsValidDirection(wall.direction)) return false;
        return true;
    }

    private static bool IsValidDirection(string direction)
    {
        return direction == "Up" || direction == "Down" || direction == "Left" || direction == "Right";
    }
}
