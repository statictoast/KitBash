using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelDataRepository : ScriptableObject
{
    public List<string> m_playableLevelPaths;
    public string m_mainMenuPath;

    public LevelDataRepository()
    {
        m_playableLevelPaths = new List<string>();
    }

    static public string GetLevelNameWithoutPath(string levelName)
    {
        return System.IO.Path.GetFileNameWithoutExtension(levelName);
    }
}
