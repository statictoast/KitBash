using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWurm : ProjectileEnemyEntity
{
    protected override Vector2 GetProjectileDirection()
    {
        // TODO: make more accurate?
        Vector2 distanceToTarget = m_aggroTarget.transform.position - transform.TransformPoint(m_projectileData.GetStartingPosition(0));
        Vector2 direction = new Vector2((distanceToTarget.x * 0.6f) / m_attackRadius, 1);
        return direction.normalized;
    }
}
