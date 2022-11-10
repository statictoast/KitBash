using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBee : ProjectileEnemyEntity
{
    protected override void OnAttackAnimationComplete()
    {
        base.OnAttackAnimationComplete();
        m_aiMovement = ENEMY_MOVEMENT_TYPE.MOVE_FOWARD;
    }

    protected override void CheckPlayerInAttackRange()
    {
        if(m_aiMovement == ENEMY_MOVEMENT_TYPE.STATIONARY)
        {
            return;
        }

        if(m_aggroTarget && !IsAttackOnCooldown())
        {
            Vector2 distanceToPlayer = m_aggroTarget.transform.position - transform.position;
            m_attackingTarget = Mathf.Abs(distanceToPlayer.x) <= m_attackRadius && Mathf.Abs(distanceToPlayer.y) < m_outOfRangeThreshold;
            if(m_attackingTarget)
            {
                m_aiMovement = ENEMY_MOVEMENT_TYPE.STATIONARY;
            }
        }
        else
        {
            m_attackingTarget = false;
        }
    }

    protected override Vector2 GetProjectileDirection()
    {
        Vector2 distanceToTarget = m_aggroTarget.transform.position - transform.TransformPoint(m_projectileData.GetStartingPosition(0));
        return distanceToTarget.normalized;
    }

    protected override void OnStartSimulation()
    {
        base.OnStartSimulation();
        m_aiMovement = ENEMY_MOVEMENT_TYPE.MOVE_FOWARD;
    }
}
