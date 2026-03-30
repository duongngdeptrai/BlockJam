using UnityEngine;

public class PlayerProgressData
{
    public int currentLevel = 1;      // Level hiện tại
    public int currentLives = 3;      // Số mạng
    public long lastPlayedTime;       // Thời gian chơi gần nhất
}

public class SaveManager : MonoBehaviour
{
    private const string SAVE_KEY = "PlayerProgress";
    private static PlayerProgressData data;

    public static void SaveProgress(int level, int lives)
    {
        data = new PlayerProgressData
        {
            currentLevel = level,
            currentLives = lives,
            lastPlayedTime = System.DateTime.Now.Ticks
        };
        
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    public static PlayerProgressData LoadProgress()
    {
        if (!PlayerPrefs.HasKey(SAVE_KEY))
            return new PlayerProgressData { currentLevel = 1, currentLives = 3 };
        
        string json = PlayerPrefs.GetString(SAVE_KEY);
        return JsonUtility.FromJson<PlayerProgressData>(json);
    }
}
