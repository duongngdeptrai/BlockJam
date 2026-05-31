using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class LevelLoader
{
    public static LevelData LoadLevel(string jsonFileName)
    {
        string path = Path.Combine(Application.streamingAssetsPath, jsonFileName);

        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            string json = File.ReadAllText(path);
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
