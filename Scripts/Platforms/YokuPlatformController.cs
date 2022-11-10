using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class YokuPlatformInterval
{
    public List<bool> m_ActivePlatforms;
    public float m_overrideIntervalTimeS = 0f;

    public YokuPlatformInterval(int aNumPlatforms)
    {
        m_ActivePlatforms = new List<bool>();
        for(int i = 0; i < aNumPlatforms; i++)
        {
            m_ActivePlatforms.Add(true);
        }
    }

    public void ResizeData(int aNewNumPlatforms)
    {
        if (m_ActivePlatforms.Count >= aNewNumPlatforms)
        {
            while (m_ActivePlatforms.Count > aNewNumPlatforms)
            {
                m_ActivePlatforms.RemoveAt(m_ActivePlatforms.Count - 1);
            }
        }
        else
        {
            while (m_ActivePlatforms.Count < aNewNumPlatforms)
            {
                m_ActivePlatforms.Add(true);
            }
        }
    }
}

public class YokuPlatformController : MonoBehaviour, IPausable
{
    public float m_IntervalS = 2.0f;
    [SerializeField]
    public List<YokuPlatformInterval> m_PlatformsByInterval;

    private List<YokuPlatform> m_Platforms;
    public int m_NumIntervals = 2;
    private int m_CurrntInterval;
    private float m_TimeRemainingS;
    private YokuPlatformInterval m_CurrentIntervalPlatforms;
    private bool m_paused;

    private void Awake()
    {
        m_paused = false;
        m_Platforms = new List<YokuPlatform>(GetComponentsInChildren<YokuPlatform>());
        if(m_Platforms.Count == 0)
        {
            Debug.LogError("0 Platforms in Yoku Platform Controller");
        }
        m_TimeRemainingS = m_IntervalS;
        m_CurrntInterval = 0;
        m_CurrentIntervalPlatforms = m_PlatformsByInterval[m_CurrntInterval];
        UpdateYokuPlatforms();

        if(Application.isPlaying)
        {
            GameplayManager.Instance.RegisterPausable(this);
        }
    }

    private void Update()
    {
        if(m_paused)
        {
            return;
        }

        m_TimeRemainingS -= Time.deltaTime;
        if(m_TimeRemainingS <= 0)
        {
            m_CurrntInterval = (m_CurrntInterval + 1) % m_NumIntervals;
            m_CurrentIntervalPlatforms = m_PlatformsByInterval[m_CurrntInterval];
            m_TimeRemainingS = m_CurrentIntervalPlatforms.m_overrideIntervalTimeS == 0f ? m_IntervalS : m_CurrentIntervalPlatforms.m_overrideIntervalTimeS;
            UpdateYokuPlatforms();
        }
    }

    private void UpdateYokuPlatforms()
    {
        for(int i = 0; i < m_CurrentIntervalPlatforms.m_ActivePlatforms.Count; i++)
        {
            m_Platforms[i].SetActive(m_CurrentIntervalPlatforms.m_ActivePlatforms[i]);
        }
    }

    public void SetNumIntervals(int aNumIntervals)
    {
        if(aNumIntervals > 1)
        {
            m_NumIntervals = aNumIntervals;
        }
        else
        {
            Debug.LogWarning("attempted to set Yoku Block intervals to an invalid value: " + aNumIntervals);
        }
    }

    public int GetNumIntervals()
    {
        return m_NumIntervals;
    }

    public void SetPaused(bool aPaused)
    {
        m_paused = aPaused;
    }

    public bool IsPaused()
    {
        return m_paused;
    }
}
