using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : GameEntity
{
    public bool m_IsOneWay = false;

    protected List<GameObject> m_TouchingObjects;
    protected Collider2D m_Collider;

    protected override void EntityAwake()
    {
        base.EntityAwake();
        Initialize();
    }

    protected override void EntityUpdate()
    {
        base.EntityUpdate();
        PerformUpdate();
    }

    protected virtual void Initialize()
    {
        m_TouchingObjects = new List<GameObject>();
        m_Collider = GetComponentInChildren<Collider2D>();

        if(!m_Collider)
        {
            Debug.LogError("Platform exists without a collider");
            return;
        }
    }

    protected virtual void PerformUpdate()
    {
        // override in derived classes
    }

    public virtual void OnGroundEntity(GameEntity aGrounder)
    {
        /*if(!m_TouchingObjects.Contains(aGrounder.gameObject))
        {
            m_TouchingObjects.Add(aGrounder.gameObject);
        }*/
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        ProcessOnCollisionEnter(collision);
    }

    protected virtual void ProcessOnCollisionEnter(Collision2D collision)
    {
        if(!m_TouchingObjects.Contains(collision.gameObject))
        {
            AddTouchingObject(collision.gameObject);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        ProcessOnCollisionStay(collision);
    }

    protected virtual void ProcessOnCollisionStay(Collision2D collision)
    {
        if(!m_TouchingObjects.Contains(collision.gameObject))
        {
            AddTouchingObject(collision.gameObject);
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        ProcessOnCollisionExit(collision);
    }

    protected virtual void ProcessOnCollisionExit(Collision2D collision)
    {
        RemoveTouchingObject(collision.gameObject);
    }

    protected virtual void RemoveTouchingObject(GameObject aGameObject)
    {
        m_TouchingObjects.Remove(aGameObject);
    }

    protected virtual void AddTouchingObject(GameObject aGameObject)
    {
        m_TouchingObjects.Add(aGameObject);
    }

    public virtual bool PlatformPositionCorrection(RaycastHit2D aRay, Vector2 aStartPos, ref Vector2 aAdjustedPos)
    {
        if(m_IsOneWay)
        {
            return false;
        }

        //aAdjustedPos = aRay.point + Vector2.Scale(aUnit.GetHalfSize(), aRay.normal);
        return true;
    }

    public bool IsPlayerTouching()
    {
        foreach(GameObject gameObject in m_TouchingObjects)
        {
            if(gameObject.tag == GameplayTags.PLAYER)
            {
                return true;
            }
        }

        return false;
    }
}
