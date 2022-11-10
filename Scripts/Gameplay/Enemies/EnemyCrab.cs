using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCrab : ProjectileEnemyEntity
{
    [Header("Crab")]
    public GameObject m_shieldFXPrefab;
    public float m_secondaryProjectileFireChance = 0.2f;
    protected bool m_fireSecondaryProjectile;
    protected GameObject m_currentShieldFX = null;

    protected override void EntityAwake()
    {
        base.EntityAwake();
        m_fireSecondaryProjectile = false;

        if(!m_attackingTarget && m_currentShieldFX == null)
        {
            m_currentShieldFX = GameFXManager.Instance.RequestPlayFX(m_shieldFXPrefab, gameObject);
        }
    }

    protected override void EntityUpdate()
    {
        base.EntityUpdate();
    }

    override protected void OnAttackAnimationComplete()
    {
        base.OnAttackAnimationComplete();
        if(m_currentShieldFX == null)
        {
            m_currentShieldFX = GameFXManager.Instance.RequestPlayFX(m_shieldFXPrefab, gameObject);
        }
    }

    protected override void StartAttack()
    {
        base.StartAttack();
        m_fireSecondaryProjectile = m_secondaryProjectileFireChance >= UnityEngine.Random.Range(0f, 1f);
        if(m_currentShieldFX)
        {
            m_currentShieldFX.GetComponent<FXLogic>().RequestEnd();
            m_currentShieldFX = null;
        }
    }

    public override void FireProjectile()
    {
        if(m_fireSecondaryProjectile)
        {
            int numPoints = m_projectileData.GetNumProjectileAttackDirections(1);
            for(int i = 0; i < numPoints; i++)
            {
                Vector3 fireDir = m_projectileData.GetTransformedProjectileDirection(1, i, transform);

                Vector2 direction = fireDir;
                float velocity = m_projectileData.GetProjectileVelocity(1);
                direction *= velocity;
                ProjectileEntity newProjectile = GetProjectile(1);
                newProjectile.FireProjectile(GetFirePosition(), direction);
                OnProjectileFired();
            }
            m_attackCooldownTimer.Start(m_attackCooldownDurationS);
        }
        else
        {
            base.FireProjectile();
        }
    }

    protected override Vector3 GetFirePosition()
    {
        return base.GetFirePosition();
    }

    protected override Vector2 GetProjectileDirection()
    {
        return m_projectileData.GetTransformedProjectileDirection(0, 0, transform);
    }

    protected override void OnStartSimulation()
    {
        base.OnStartSimulation();
        if(!m_attackingTarget && m_currentShieldFX == null)
        {
            m_currentShieldFX = GameFXManager.Instance.RequestPlayFX(m_shieldFXPrefab, gameObject);
        }
    }

    protected override void OnEndSimulation()
    {
        base.OnEndSimulation();
        m_attackingTarget = false;

        if(m_currentShieldFX)
        {
            m_currentShieldFX.GetComponent<FXLogic>().RequestEnd();
            m_currentShieldFX = null;
        }

        m_animator.Play("Crab_idle");
    }
}
