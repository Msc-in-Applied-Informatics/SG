using UnityEngine;

public static class SaveSystem
{
    private static string CurrentUser => PlayerPrefs.GetString("CurrentUser", "Default");

    private static string UserKey(string prefix, int level) =>
        $"{prefix}_{CurrentUser}_Level_{level}";

    public static bool IsLevelUnlocked(int level)
    {
        return PlayerPrefs.GetInt(UserKey("Unlocked", level), level == 1 ? 1 : 0) == 1;
    }

    public static void UnlockLevel(int level)
    {
        PlayerPrefs.SetInt(UserKey("Unlocked", level), 1);
    }

    public static int GetStars(int level)
    {
        return PlayerPrefs.GetInt(UserKey("Stars", level), 0);
    }

    public static void SaveStars(int level, int stars)
    {
        int current = GetStars(level);
        if (stars > current)
        {
            PlayerPrefs.SetInt(UserKey("Stars", level), stars);
        }
        UnlockLevel(level + 1);
    }

    public static int GetScore(int level)
    {
        return PlayerPrefs.GetInt(UserKey("Score", level), 0);
    }

    public static void SaveScore(int level, int score)
    {
        int current = GetScore(level);
        if (score > current)
        {
            PlayerPrefs.SetInt(UserKey("Score", level), score);
        }
    }

    public static int GetLives(int level)
    {
        return PlayerPrefs.GetInt(UserKey("Lives", 0), 3); // Default lives = 3
    }

    public static void SaveLives(int level, int lives)
    {
        PlayerPrefs.SetInt(UserKey("Lives", 0), lives);
    }

    public static int CurrentLevel
    {
        get => PlayerPrefs.GetInt(UserKey("CurrentLevel",0), 1);
        set => PlayerPrefs.SetInt(UserKey("CurrentLevel",0), value);
    }

    public static void ClearAllData()
    {
        PlayerPrefs.DeleteAll();
    }

 
}
