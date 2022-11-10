using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ProjectileData
{
    public GameObject projectilePrefab;
    public Vector3 startingPosition;
    public Vector3[] attackDirections;
    public float velocity = 0f;
    public bool attackDirectionCalculated = false;
}

[Serializable]
public class ProjectileFiringEntityComponent
{
    [SerializeField]
    protected List<ProjectileData> m_projectileDatas;
    protected Dictionary<int, ProjectilePool> m_projectilePools;

    public ProjectileFiringEntityComponent()
    {
        m_projectileDatas = new List<ProjectileData>();
    }

    public void Setup()
    {
        m_projectilePools = new Dictionary<int, ProjectilePool>();
        for(int i = 0; i < m_projectileDatas.Count; i++)
        {
            ProjectileData projectileData = m_projectileDatas[i];
            if(projectileData.projectilePrefab != null)
            {
                m_projectilePools.Add(i, new ProjectilePool(projectileData.projectilePrefab));
            }
        }
    }

    public Vector3 GetStartingPosition(int aIndex)
    {
        if(aIndex < m_projectileDatas.Count)
        {
            return m_projectileDatas[aIndex].startingPosition;
        }

        return Vector3.zero;
    }

    public float GetProjectileVelocity(int aIndex)
    {
        if(aIndex < m_projectileDatas.Count)
        {
            return m_projectileDatas[aIndex].velocity;
        }

        return 0f;
    }

    public Vector3 GetProjectilePoint(int aIndex, int aDirectionIndex)
    {
        if(aIndex < m_projectileDatas.Count && aDirectionIndex < m_projectileDatas[aIndex].attackDirections.Length)
        {
            return m_projectileDatas[aIndex].attackDirections[aDirectionIndex];
        }

        return Vector3.zero;
    }

    public Vector3 GetTransformedProjectileDirection(int aIndex, int aDirectionIndex, Transform aTransform)
    {
        if(aIndex < m_projectileDatas.Count && aDirectionIndex < m_projectileDatas[aIndex].attackDirections.Length)
        {
            Vector3 dir = m_projectileDatas[aIndex].attackDirections[aDirectionIndex];
            Vector3 startingPos = GetStartingPosition(aIndex);
            Vector3 finalDir = aTransform.TransformPoint(dir) - aTransform.TransformPoint(startingPos);
            return finalDir.normalized;
        }

        return Vector3.zero;
    }

    public ProjectileEntity AcquireProjectile(int aIndex)
    {
        if(aIndex < m_projectileDatas.Count)
        {
            return m_projectilePools[aIndex].AcquireObject();
        }

        return null;
    }

    public int GetTotalNumActiveProjectiles()
    {
        int numActive = 0;
        foreach(KeyValuePair<int, ProjectilePool> projectilePool in m_projectilePools)
        {
            numActive += projectilePool.Value.GetNumActive();
        }

        return numActive;
    }

    public int GetNumActiveProjectiles(int aIndex)
    {
        ProjectilePool pool;
        if(m_projectilePools.TryGetValue(aIndex, out pool))
        {
            return pool.GetNumActive();
        }

        return 0;
    }

    #region Inspector Only

    public void ClearAll()
    {
        m_projectileDatas.Clear();
    }

    public int GetNumProjectileAttacks()
    {
        return m_projectileDatas.Count;
    }

    public int GetNumProjectileAttackDirections(int aIndex)
    {
        if(aIndex < m_projectileDatas.Count)
        {
            return m_projectileDatas[aIndex].attackDirections.Length;
        }

        return 0;
    }

    public void AddNewProjectileData(Vector3 aStartingPos)
    {
        ProjectileData newData = new ProjectileData
        {
            startingPosition = aStartingPos,
            attackDirections = new Vector3[1] { new Vector3(1f, 0f, 0f) },
            projectilePrefab = null,
            velocity = 1f
        };

        m_projectileDatas.Add(newData);
    }

    public void AddProjectilePoint(int aIndex, Vector3 aPos)
    {
        Vector3[] directions = m_projectileDatas[aIndex].attackDirections;
        Array.Resize(ref directions, directions.Length + 1);
        directions[directions.Length - 1] = aPos;
        m_projectileDatas[aIndex].attackDirections = directions; // TODO: do we need this?
    }

    public void RemoveProjectilePoint(int aIndex, int aDirectionIndex)
    {
        if(aIndex < m_projectileDatas.Count && aDirectionIndex < m_projectileDatas[aIndex].attackDirections.Length)
        {
            Vector3[] newDirections= new Vector3[0];
            Vector3[] oldDirections = m_projectileDatas[aIndex].attackDirections;
            Array.Resize(ref newDirections, oldDirections.Length - 1);
            int count = 0;
            for(int i = 0; i < oldDirections.Length; i++)
            {
                if(i != aDirectionIndex)
                {
                    newDirections[count] = oldDirections[i];
                    count++;
                }
            }

            m_projectileDatas[aIndex].attackDirections = newDirections;
        }
    }

    public void SetPojectilePoint(int aIndex, int aDirectionIndex, Vector3 aPos)
    {
        if(aIndex < m_projectileDatas.Count && aDirectionIndex < m_projectileDatas[aDirectionIndex].attackDirections.Length)
        {
            m_projectileDatas[aIndex].attackDirections[aDirectionIndex] = aPos;
        }
    }

    public void SetPojectileStart(int aIndex, Vector3 aPos)
    {
        if(aIndex < m_projectileDatas.Count)
        {
            m_projectileDatas[aIndex].startingPosition = aPos;
        }
    }

    public void SetProjectileVelocity(int aIndex, float aVelocity)
    {
        if(aIndex < m_projectileDatas.Count)
        {
            m_projectileDatas[aIndex].velocity = aVelocity;
        }
    }

    public void SetProjectilePrefab(int aIndex, GameObject aPrefab)
    {
        if(aIndex < m_projectileDatas.Count)
        {
            m_projectileDatas[aIndex].projectilePrefab = aPrefab;
        }
    }

    public GameObject GetProjectilePrefab(int aIndex)
    {
        if(aIndex < m_projectileDatas.Count)
        {
            return m_projectileDatas[aIndex].projectilePrefab;
        }

        return null;
    }

    public void SetAttackDirectionCalculated(int aIndex, bool aCalculated)
    {
        if(aIndex < m_projectileDatas.Count)
        {
            m_projectileDatas[aIndex].attackDirectionCalculated = aCalculated;
        }
    }

    public bool GetAttackDirectionCalculated(int aIndex)
    {
        if(aIndex < m_projectileDatas.Count)
        {
            return m_projectileDatas[aIndex].attackDirectionCalculated;
        }

        return false;
    }

    #endregion
}
