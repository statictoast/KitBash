using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavafallController : TriggerableEntity
{
    public GameObject m_lavafallPrefab;
    public int m_numSteps = 4;
    public float m_durationAtFullS = 3f;
    public float m_durationAtCooldownS = 3f;
    public float m_durationToNextLavaS = 1f;
    public float m_startDelayS = 0f;

    public LAVAFALL_CONTROL_TYPE m_controlType = LAVAFALL_CONTROL_TYPE.TIMER;
    public bool m_activated = true;

    private LAVAFALL_PHASE m_phase = LAVAFALL_PHASE.COOLDOWN;
    private float m_timeToNextEventS;
    private int m_currentLavafallCount;
    private Vector3 m_startingPosition;
    private Vector3 m_cellSize;

    private List<GameObject> m_activeLavafallObjects;
    private List<GameObject> m_inactiveLavafallObjects;

    protected override void EntityAwake()
    {
        base.EntityAwake();
        if(m_lavafallPrefab == null)
        {
            Debug.LogError("[LavafallController] have no starting prefab");
            return;
        }

        m_startingPosition = m_lavafallPrefab.transform.position;
        BoxCollider2D collider = m_lavafallPrefab.GetComponent<BoxCollider2D>();
        m_cellSize = collider.bounds.size;
        m_lavafallPrefab.SetActive(false);
        m_currentLavafallCount = 0;
        if(m_controlType == LAVAFALL_CONTROL_TYPE.TIMER)
        {
            m_phase = LAVAFALL_PHASE.COOLDOWN;
            m_timeToNextEventS = m_durationAtCooldownS;
        }
        else if(m_controlType == LAVAFALL_CONTROL_TYPE.TRIGGER)
        {
            if(m_activated)
            {
                m_phase = LAVAFALL_PHASE.FALLING;
                m_timeToNextEventS = m_durationToNextLavaS;
            }
            else
            {
                m_phase = LAVAFALL_PHASE.COOLDOWN;
                m_timeToNextEventS = m_durationAtCooldownS;
            }
        }
        m_timeToNextEventS += m_startDelayS;
        m_activeLavafallObjects = new List<GameObject>();
        m_inactiveLavafallObjects = new List<GameObject>();
    }

    protected override void EntityUpdate()
    {
        base.EntityUpdate();
        m_timeToNextEventS -= Time.deltaTime;

        switch(m_phase)
        {
            case LAVAFALL_PHASE.FALLING:
            {
                if(m_timeToNextEventS <= 0f)
                {
                    GameObject lavafall = GetOrCreateLavafall();
                    lavafall.SetActive(true);
                    m_activeLavafallObjects.Add(lavafall);
                    Vector3 position = m_startingPosition;
                    position.y -= m_cellSize.y * m_currentLavafallCount;
                    lavafall.transform.position = position;

                    if(m_currentLavafallCount > 0)
                    {
                        AnimatorStateInfo stateInfo = lavafall.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);
                        foreach(GameObject lavaFallThing in m_activeLavafallObjects)
                        {
                            lavaFallThing.GetComponent<Animator>().Play(stateInfo.fullPathHash, 0, 0);
                        }
                    }

                    m_currentLavafallCount++;
                    if(m_currentLavafallCount == m_numSteps)
                    {
                        m_phase = LAVAFALL_PHASE.FULL;
                        m_timeToNextEventS = m_durationAtFullS;
                    }
                    else
                    {
                        m_timeToNextEventS = m_durationToNextLavaS;
                    }
                }
            }
                break;
            case LAVAFALL_PHASE.FULL:
            {
                if(m_controlType != LAVAFALL_CONTROL_TYPE.TRIGGER && m_timeToNextEventS <= 0f)
                {
                    m_phase = LAVAFALL_PHASE.DIMINISHING;
                    m_timeToNextEventS = m_durationToNextLavaS;
                }
            }
                break;
            case LAVAFALL_PHASE.DIMINISHING:
            {
                if(m_timeToNextEventS <= 0f)
                {
                    GameObject dimishedLavafall = m_activeLavafallObjects[0];
                    m_activeLavafallObjects.RemoveAt(0);
                    m_inactiveLavafallObjects.Add(dimishedLavafall);
                    dimishedLavafall.SetActive(false);

                    m_currentLavafallCount--;
                    if(m_currentLavafallCount == 0)
                    {
                        m_phase = LAVAFALL_PHASE.COOLDOWN;
                        m_timeToNextEventS = m_durationAtCooldownS;
                    }
                    else
                    {
                        m_timeToNextEventS = m_durationToNextLavaS;
                    }
                }
            }
                break;
            case LAVAFALL_PHASE.COOLDOWN:
            {
                if(m_activated)
                {
                    if(m_timeToNextEventS <= 0f)
                    {
                        m_phase = LAVAFALL_PHASE.FALLING;
                        m_timeToNextEventS = m_durationToNextLavaS;
                    }
                }

            }
                break;
        }
    }

    private GameObject GetOrCreateLavafall()
    {
        if(m_inactiveLavafallObjects.Count > 0)
        {
            GameObject lavaFall = m_inactiveLavafallObjects[0];
            m_inactiveLavafallObjects.RemoveAt(0);
            return lavaFall;
        }

        GameObject newLavaFall = GameObject.Instantiate(m_lavafallPrefab);

        return newLavaFall;
    }

    public override void OnTriggerHit()
    {
        if(m_controlType != LAVAFALL_CONTROL_TYPE.TRIGGER)
        {
            return;
        }

        if(m_phase == LAVAFALL_PHASE.FULL || m_phase == LAVAFALL_PHASE.COOLDOWN)
        {
            m_activated = !m_activated;
            if(m_activated)
            {
                m_phase = LAVAFALL_PHASE.FALLING;
                m_timeToNextEventS = m_durationToNextLavaS;
            }
            else
            {
                m_phase = LAVAFALL_PHASE.DIMINISHING;
                m_timeToNextEventS = m_durationToNextLavaS;
            }
        }

    }

    #region Debug Functions

    private void OnDrawGizmos()
    {
        if(m_lavafallPrefab == null)
        {
            return;
        }

        BoxCollider2D collider = m_lavafallPrefab.GetComponent<BoxCollider2D>();
        if(collider == null)
        {
            return;
        }

        Vector2 size = collider.bounds.size;
        float totalY = size.y * m_numSteps;
        Vector3 pos = m_lavafallPrefab.transform.position;
        pos.y -= totalY / 2f;
        pos.y += collider.bounds.extents.y;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(pos, new Vector3(size.x, totalY));
    }

    #endregion
}
