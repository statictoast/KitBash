using System.Collections.Generic;
using UnityEngine;
using System;


public class GlobalGameDataRepository : ScriptableObject
{
    public GameObject m_enemyDeathFX;
    public GameObject m_healthPickupPrefab;
    public float m_levelStartReadyFadeOutTime = 0.2f;

    public GlobalGameDataRepository()
    {
        
    }
}