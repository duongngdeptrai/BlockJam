using UnityEngine;

public class PlayerProgressData
{
    public int currentLevel = 1;
    public int currentLives = 3;
}

public class SaveManager : MonoBehaviour
{
    private const string SAVE_KEY = SaveKeys.PlayerProgress;
    private static PlayerProgressData data;

    public static void SaveProgress(int level, int lives)
    {
        if (string.IsNullOrEmpty(SAVE_KEY))
        {
            Debug.LogError("SaveManager: SAVE_KEY is null or empty! Cannot save progress.");
            return;
        }

        data = new PlayerProgressData
        {
            currentLevel = level,
            currentLives = lives
        };

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    public static PlayerProgressData LoadProgress()
    {
        if (string.IsNullOrEmpty(SAVE_KEY))
        {
            Debug.LogError("SaveManager: SAVE_KEY is null or empty! Cannot load progress.");
            return new PlayerProgressData { currentLevel = 1, currentLives = 3 };
        }

        if (!PlayerPrefs.HasKey(SAVE_KEY))
            return new PlayerProgressData { currentLevel = 1, currentLives = 3 };

        string json = PlayerPrefs.GetString(SAVE_KEY);
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogWarning("SaveManager: Loaded JSON is empty or null. Returning default progress.");
            return new PlayerProgressData { currentLevel = 1, currentLives = 3 };
        }

        PlayerProgressData result = JsonUtility.FromJson<PlayerProgressData>(json);
        return result ?? new PlayerProgressData { currentLevel = 1, currentLives = 3 };
    }
}
