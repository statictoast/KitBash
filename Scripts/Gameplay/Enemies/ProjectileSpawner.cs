using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileSpawner : TriggerableEntity, IProjectileFiringEntity
{
    [SerializeField]
    private ProjectileFiringEntityComponent m_projectileData = null;
    public PROJECTILE_SPAWNER_TYPE m_spawnerType = PROJECTILE_SPAWNER_TYPE.FREQUENCY;
    public float m_fireFrequncyS = 1f;
    public float m_initialDelayS = 0f;
    public float m_overrideMaxTimeAliveS = 0f;
    public float m_proximityFireCooldownS = 2f;
    private CountdownTimer m_frequencyFireCooldownTimer;
    private CountdownTimer m_proximityFireCooldownTimer;

    protected override void EntityAwake()
    {
        base.EntityAwake();
        m_proximityFireCooldownTimer = new CountdownTimer();
        m_frequencyFireCooldownTimer = new CountdownTimer();
        m_frequencyFireCooldownTimer.Start(m_initialDelayS);
        m_projectileData.Setup();
    }

    protected override void EntityUpdate()
    {
        base.EntityUpdate();

        if(m_spawnerType == PROJECTILE_SPAWNER_TYPE.FREQUENCY)
        {
            m_frequencyFireCooldownTimer.Update(Time.deltaTime);
            if(m_frequencyFireCooldownTimer.IsDone())
            {
                m_frequencyFireCooldownTimer.Start(m_fireFrequncyS);
                FireProjectile();
            }
        }
        
        m_proximityFireCooldownTimer.Update(Time.deltaTime);
    }

    private void FireProjectile()
    {
        Vector3 spawnPosition = transform.TransformPoint(m_projectileData.GetStartingPosition(0));
        int numPoints = m_projectileData.GetNumProjectileAttackDirections(0);
        for(int i = 0; i < numPoints; i++)
        {
            ProjectileEntity projectile = m_projectileData.AcquireProjectile(0);
            projectile.SetOwner(this);
            Vector3 direction = m_projectileData.GetTransformedProjectileDirection(0, i, transform);
            float velocity = m_projectileData.GetProjectileVelocity(0);
            direction *= velocity;
            if(m_overrideMaxTimeAliveS != 0f)
            {
                projectile.m_maxTimeAliveS = m_overrideMaxTimeAliveS;
            }
            projectile.SetDistanceTether(float.MaxValue);
            projectile.FireProjectile(spawnPosition, direction);
        }
    }

    public ProjectileFiringEntityComponent GetProjectileData()
    {
        return m_projectileData;
    }

    public override void OnTriggerHit()
    {
        if(m_spawnerType == PROJECTILE_SPAWNER_TYPE.PROXIMITY)
        {
            if(m_proximityFireCooldownTimer.IsDone())
            {
                m_proximityFireCooldownTimer.Start(m_proximityFireCooldownS);
                FireProjectile();
            }
        }
    }

    #region Debug Functions

    private void OnDrawGizmos()
    {
        if(m_projectileData == null && m_projectileData.GetNumProjectileAttacks() == 0)
            return;

        Gizmos.color = Color.blue;
        int numPoints = m_projectileData.GetNumProjectileAttackDirections(0);
        for(int i = 0; i < numPoints; i++)
        {
            Vector3 direction = m_projectileData.GetProjectilePoint(0, i);
            Gizmos.DrawLine(transform.position, transform.TransformPoint(direction));
        }
    }

    #endregion
}
