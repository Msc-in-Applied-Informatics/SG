using UnityEngine;
using System;

public static class SaveSystem
{
    private static string CurrentUser => PlayerPrefs.GetString("CurrentUser", "Default");

    private static string UserKey(string key) => $"{CurrentUser}_{key}";
    private static string UserLevelKey(string key, int level) => $"{CurrentUser}_{key}_Level_{level}";

    // ========== LEVEL PROGRESS ==========

    public static bool IsLevelUnlocked(int level)
    {
        return PlayerPrefs.GetInt(UserLevelKey("Unlocked", level), level == 1 ? 1 : 0) == 1;
    }

    public static void UnlockLevel(int level)
    {
        PlayerPrefs.SetInt(UserLevelKey("Unlocked", level), 1);
    }

    public static int GetStars(int level)
    {
        return PlayerPrefs.GetInt(UserLevelKey("Stars", level), 0);
    }

    public static void SaveStars(int level, int stars)
    {
        int current = GetStars(level);
        if (stars > current)
        {
            PlayerPrefs.SetInt(UserLevelKey("Stars", level), stars);
        }
        UnlockLevel(level + 1);
    }

    public static int GetScore(int level)
    {
        return PlayerPrefs.GetInt(UserLevelKey("Score", level), 0);
    }

    public static void SaveScore(int level, int score)
    {
        int current = GetScore(level);
        if (score > current)
        {
            PlayerPrefs.SetInt(UserLevelKey("Score", level), score);
        }
    }

    // ========== GLOBAL CURRENT LEVEL ==========

    public static int CurrentLevel
    {
        get => PlayerPrefs.GetInt(UserKey("CurrentLevel"), 1);
        set => PlayerPrefs.SetInt(UserKey("CurrentLevel"), value);
    }

    // ========== GLOBAL LIVES ==========

    public static void SaveGlobalLives(int lives)
    {
        PlayerPrefs.SetInt(UserKey("GlobalLives"), lives);
    }

    public static int GetGlobalLives()
    {
        return PlayerPrefs.GetInt(UserKey("GlobalLives"), 3);
    }

    // ========== GLOBAL ARMOR ==========

    public static void SaveGlobalArmor(int armor)
    {
        PlayerPrefs.SetInt(UserKey("GlobalArmor"), armor);
    }

    public static int GetGlobalArmor()
    {
        return PlayerPrefs.GetInt(UserKey("GlobalArmor"), 0);
    }

    // ========== NEXT LIFE TIMER ==========

    public static void SaveNextLifeRegenTime(DateTime time)
    {
        PlayerPrefs.SetString(UserKey("NextLifeTime"), time.ToBinary().ToString());
    }

    public static DateTime GetNextLifeRegenTime()
    {
        if (PlayerPrefs.HasKey(UserKey("NextLifeTime")))
        {
            long binary = long.Parse(PlayerPrefs.GetString(UserKey("NextLifeTime")));
            return DateTime.FromBinary(binary);
        }
        return DateTime.Now;
    }

    // ========== LIVES & ARMOR PER LEVEL (OPTIONAL) ==========

    public static int GetLives(int level)
    {
        return PlayerPrefs.GetInt(UserLevelKey("Lives", level), 3);
    }

    public static void SaveLives(int level, int lives)
    {
        PlayerPrefs.SetInt(UserLevelKey("Lives", level), lives);
    }

    public static int GetArmor(int level)
    {
        return PlayerPrefs.GetInt(UserLevelKey("Armor", level), 0);
    }

    public static void SaveArmor(int level, int armor)
    {
        PlayerPrefs.SetInt(UserLevelKey("Armor", level), armor);
    }

    // ========== RESET ==========

    public static void ClearAllData()
    {
        PlayerPrefs.DeleteAll();
    }
}
