using System.Collections;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugLevelDropdown : UIControl
{
    private Dropdown m_allLevelsDropdown;

    // Use this for initialization
    void Start ()
    {
        m_allLevelsDropdown = GetComponent<Dropdown>();

        m_allLevelsDropdown.ClearOptions();

        LevelDataRepository m_levels = (LevelDataRepository)Resources.Load("LevelDataRepository");

        List<string> allSceneNames = new List<string>();
        for (int i = 0; i < m_levels.m_playableLevelPaths.Count; i++)
        {
            string path = m_levels.m_playableLevelPaths[i];
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
            allSceneNames.Add(sceneName);
        }

        m_allLevelsDropdown.AddOptions(allSceneNames);
    }
    
    public string GetCurrentLevelName()
    {
        return m_allLevelsDropdown.options[m_allLevelsDropdown.value].text;
    }
}
