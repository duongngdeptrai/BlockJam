using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class LevelLoader : MonoBehaviour
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
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading level from JSON: {e.Message}");
            return null;
        }
    }
    
    private static void ValidateLevelData(LevelData level)
    {
        if (level == null) return;
        
        // Validate timeLimit - nếu không có hoặc <= 0, dùng giá trị mặc định
        if (level.timeLimit <= 0)
        {
            Debug.LogWarning($"Invalid timeLimit {level.timeLimit}, using default value 60");
            level.timeLimit = 60f;
        }
        
        // Validate doors
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
        
        // Validate walls
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
        // Allow doors to be placed on the immediate border cells: -1 (before 0) and rows/columns (after last)
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
