using UnityEngine;
public class PlayerLivesManager : MonoBehaviour
{
    public static PlayerLivesManager Instance { get; private set; }
    
    private int currentLives = 5;
      
    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        Instance = this;
    }
    
    public int GetCurrentLives()
    {
        return currentLives;
    }

    public void AddCurrentLives(int amount)
    {
        currentLives += amount;
        currentLives = Mathf.Max(currentLives, 0);
        currentLives = Mathf.Min(currentLives, 5);
    }

}