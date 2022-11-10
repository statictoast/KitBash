using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteAlways]
public class MovingPlatform : Platform
{
    public GameObject m_PointsContainer;
    public int m_startingPointIndex = 0;
    public float m_Velocity = 1.0f;
    public bool m_AlwaysShowGizmos = false;

    private int m_TargetPoint;
    private List<Vector3> m_AllTargetPoints = new List<Vector3>();

    protected override void Initialize()
    {
        base.Initialize();

        if (m_PointsContainer != null)
        {
            int allPoints = m_PointsContainer.transform.childCount;
            for (int i = 0; i < allPoints; i++)
            {
                m_AllTargetPoints.Add(m_PointsContainer.transform.GetChild(i).position);
            }
        }

        if(m_startingPointIndex < m_AllTargetPoints.Count)
        {
            transform.position = m_AllTargetPoints[m_startingPointIndex];
            m_TargetPoint = m_startingPointIndex;
        }
        else
        {
            m_TargetPoint = 0;
        }

    }

    protected override void EntityFixedUpdate()
    {
        if(!Application.IsPlaying(gameObject))
        {
            m_AllTargetPoints.Clear();
            Initialize();
            if(m_AllTargetPoints.Count > 0)
            {
                transform.position = m_AllTargetPoints[m_startingPointIndex];
            }
            return;
        }

        if (m_Velocity == 0)
        {
            return;
        }

        Vector3 currentPoint = m_AllTargetPoints[m_TargetPoint];
        Vector3 targetPoint = m_AllTargetPoints[(m_TargetPoint + 1) % m_AllTargetPoints.Count];

        float correctdVelocity = m_Velocity * Time.deltaTime;
        Vector3 delta = Vector3.zero;
        if (Vector3.Distance(transform.position, targetPoint) <= correctdVelocity)
        {
            delta = targetPoint - transform.position;
            transform.position = targetPoint;
            m_TargetPoint = (m_TargetPoint + 1) % m_AllTargetPoints.Count;
        }
        else
        {
            Vector3 normal = (targetPoint - currentPoint).normalized;
            Vector3 nextPos = normal * correctdVelocity + transform.position;
            delta = nextPos - transform.position;
            transform.position = nextPos;
        }

        foreach(GameObject gameObject in m_TouchingObjects)
        {
            /*GameEntity entity = gameObject.GetComponent<GameEntity>();
            if(entity)
            {
                Vector2 velocity = entity.GetCurrentVelocity();
                velocity.x += delta.x;
                velocity.y += delta.y;
                entity.UpdateVelocity(velocity);
            }
            else
            {*/
                gameObject.transform.position += delta;
            //}
        }
    }

    void OnDrawGizmos()
    {
        if (!m_AlwaysShowGizmos)
            return;

        DrawHelperLines();
    }

    void OnDrawGizmosSelected()
    {
        DrawHelperLines();
    }

    private void DrawHelperLines()
    {
        Gizmos.color = Color.green;
        if(m_PointsContainer == null)
        {
            return;
        }

        if (m_AllTargetPoints.Count <= 0)
        {
            int allPoints = m_PointsContainer.transform.childCount;
            if(allPoints == 0)
            {
                return;
            }

            for (int i = 0; i < allPoints; i++)
            {
                if(i == m_startingPointIndex)
                {
                    Gizmos.color = Color.green;
                }
                else
                {
                    Gizmos.color = Color.cyan;
                }
                Gizmos.DrawSphere(m_PointsContainer.transform.GetChild(i).position, 0.1f);

                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(m_PointsContainer.transform.GetChild(i).position, m_PointsContainer.transform.GetChild((i + 1) % allPoints).position);
            }
        }
        else
        {
            for (int i = 0; i < m_AllTargetPoints.Count; i++)
            {
                if(i == m_startingPointIndex)
                {
                    Gizmos.color = Color.green;
                }
                else
                {
                    Gizmos.color = Color.cyan;
                }
                Gizmos.DrawSphere(m_AllTargetPoints[i], 0.1f);

                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(m_AllTargetPoints[i], m_AllTargetPoints[(i + 1) % m_AllTargetPoints.Count]);
            }
        }
    }


}