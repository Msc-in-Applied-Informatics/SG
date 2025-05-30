using UnityEngine;

public static class UserSession
{
    public static string CurrentUsername { get; private set; }

    public static void SetUser(string username)
    {
        CurrentUsername = username;
        Debug.Log("User set: " + CurrentUsername);
    }

    public static string GetPrefixedKey(string key)
    {
        return $"{CurrentUsername}_{key}";
    }
}
