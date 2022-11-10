using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class ParallaxBackgroundManager : MonoBehaviour
{
    protected static ParallaxBackgroundManager m_instance;

    public enum EditMode
    {
        EDIT,
        PLAY
    }

    public EditMode m_currentMode;

    List<ParallaxObject> m_backgrounds;

    Vector3 m_previousPosition;
    [SerializeField]
    GameObject m_relativeTarget;

    private ParallaxBackgroundManager()
    {
        m_backgrounds = new List<ParallaxObject>();
        m_currentMode = EditMode.PLAY;
        m_instance = this;
    }

    private void Awake()
    {
        if (m_relativeTarget)
        {
            m_previousPosition = m_relativeTarget.transform.position;
        }
    }

    private void Update()
    {
        if(Application.isPlaying && m_currentMode != EditMode.PLAY)
        {
            m_currentMode = EditMode.PLAY;
        }

        if (m_relativeTarget)
        {
            Vector3 newPos = m_relativeTarget.transform.position;
            Vector3 movementDelta = newPos - m_previousPosition;
            switch (m_currentMode)
            {
                case EditMode.EDIT:
                    {
                        
                    }
                    break;
                case EditMode.PLAY:
                    {
                        for(int i = 0; i < m_backgrounds.Count; i++)
                        {
                            m_backgrounds[i].UpdateParallax(movementDelta);
                        }
                    }
                    break;
            }
            m_previousPosition = newPos;
        }
    }

    public static void RegisterBackground(ParallaxObject aObj)
    {
        if(!m_instance)
        {
            return;
        }

        m_instance.m_backgrounds.Add(aObj);
    }

    public static void UnregisterBackground(ParallaxObject aObj)
    {
        if(!m_instance)
        {
            return;
        }

        m_instance.m_backgrounds.Remove(aObj);
    }

    public void SetRelativeTarget(GameObject aObj)
    {
        m_relativeTarget = aObj;
        m_previousPosition = m_relativeTarget.transform.position;
    }

    public GameObject GetRelativeTarget()
    {
        return m_relativeTarget;
    }
}
