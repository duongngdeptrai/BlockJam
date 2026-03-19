using UnityEngine;
using System.IO;

public class LevelLoader : MonoBehaviour
{
    public static LevelData LoadLevel(string jsonFileName)
    {
        string path = Path.Combine(Application.streamingAssetsPath, jsonFileName);
        
        if (!File.Exists(path))
        {
            Debug.LogError($"Level file not found at: {path}");
            return null;
        }

        try
        {
            string json = File.ReadAllText(path);
            LevelDataWrapper wrapper = JsonUtility.FromJson<LevelDataWrapper>(json);
            return wrapper.level;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading level from JSON: {e.Message}");
            return null;
        }
    }
}
