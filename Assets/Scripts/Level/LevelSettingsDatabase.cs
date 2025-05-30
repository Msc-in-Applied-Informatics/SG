using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelSettingsDatabase", menuName = "Scriptable Objects/LevelSettingsDatabase")]
public class LevelSettingsDatabase : ScriptableObject
{
    public List<LevelSettings> levels;

    public LevelSettings GetSettings(int level)
    {
        return levels.Find(l => l.level == level);
    }
}
