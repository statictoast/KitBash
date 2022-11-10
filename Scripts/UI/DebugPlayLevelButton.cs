using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugPlayLevelButton : UIControl
{
    public DebugLevelDropdown m_levelDropdown;

    public void OnPlayPressed()
    {
        GameCore.Instance.LoadLevel(m_levelDropdown.GetCurrentLevelName());
    }
}
