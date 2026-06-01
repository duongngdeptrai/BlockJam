using System.IO;
using UnityEditor;
using UnityEngine;

public static class LevelEditorPreview
{
    private const string TempFileName = "_preview_temp.json";
    private static string TempFilePath => Path.Combine(Application.streamingAssetsPath, "Levels", TempFileName);
    private static bool _isPreviewActive = false;

    public static void StartPreview(MapEditorWindow window)
    {
        if (_isPreviewActive)
        {
            EditorUtility.DisplayDialog("Preview", "Preview is already running.", "OK");
            return;
        }

        // Serialize current level to temp JSON
        LevelData levelData = new LevelData
        {
            rows = window.Rows,
            columns = window.Columns,
            timeLimit = window.TimeLimit,
            blocks = new System.Collections.Generic.List<BlockData>(window.Blocks),
            doors = new System.Collections.Generic.List<DoorSpawnData>(window.Doors),
            walls = new System.Collections.Generic.List<WallSpawnData>(window.Walls)
        };

        LevelDataWrapper wrapper = new LevelDataWrapper { level = levelData };
        string json = JsonUtility.ToJson(wrapper, true);

        string levelsPath = Path.Combine(Application.streamingAssetsPath, "Levels");
        if (!Directory.Exists(levelsPath))
        {
            Directory.CreateDirectory(levelsPath);
        }

        File.WriteAllText(TempFilePath, json);
        _isPreviewActive = true;

        // Enter play mode
        EditorApplication.isPlaying = true;
    }

    public static void Cleanup()
    {
        if (File.Exists(TempFilePath))
        {
            File.Delete(TempFilePath);
        }
        _isPreviewActive = false;
    }

    public static bool IsPreviewActive => _isPreviewActive;
    public static string PreviewLevelName => "preview";
}
