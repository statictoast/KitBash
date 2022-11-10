using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileEnemyEntity : EnemyEntity, IProjectileFiringEntity
{
    [Header("Projectile")]
    [SerializeField]
    protected ProjectileFiringEntityComponent m_projectileData;
    public float m_attackCooldownDurationS = 3f;

    protected override void EntityAwake()
    {
        base.EntityAwake();
        m_projectileData.Setup();
    }

    virtual public void FireProjectile()
    {
        if(m_aggroTarget == null)
        {
            Debug.Log("tried to fire projectile but had no target");
            return;
        }

        Vector2 direction = GetProjectileDirection();
        float velocity = m_projectileData.GetProjectileVelocity(0);
        direction *= velocity;
        ProjectileEntity newProjectile = GetProjectile(0);
        newProjectile.FireProjectile(GetFirePosition(), direction);
        m_attackCooldownTimer.Start(m_attackCooldownDurationS);
        OnProjectileFired();
    }

    virtual protected ProjectileEntity GetProjectile(int aIndex)
    {
        return m_projectileData.AcquireProjectile(aIndex);
    }

    virtual protected Vector3 GetFirePosition()
    {
        return transform.TransformPoint(m_projectileData.GetStartingPosition(0));
    }

    virtual protected Vector2 GetProjectileDirection()
    {
        Vector3 fireDir = m_projectileData.GetTransformedProjectileDirection(0, 0, transform);
        Vector2 direction = fireDir;
        return direction;
    }

    virtual protected void OnProjectileFired()
    {

    }

    virtual protected void OnAttackAnimationComplete()
    {
        SetAttackingTarget(false);
    }

    public ProjectileFiringEntityComponent GetProjectileData()
    {
        return m_projectileData;
    }
}
