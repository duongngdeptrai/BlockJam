using UnityEngine;

public class PlayerLivesManager : Singleton<PlayerLivesManager>
{
    private const int MAX_LIVES = GameConstants.MAX_LIVES;
    private int currentLives = MAX_LIVES;

    public int GetCurrentLives()
    {
        return currentLives;
    }

    public void AddCurrentLives(int amount)
    {
        currentLives += amount;
        currentLives = Mathf.Clamp(currentLives, 0, MAX_LIVES);
    }
}
