using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRoboTank : ProjectileEnemyEntity
{
    protected override void StartAttack()
    {
        base.StartAttack();
        m_aiMovement = ENEMY_MOVEMENT_TYPE.STATIONARY;
    }

    protected override void OnAttackAnimationComplete()
    {
        base.OnAttackAnimationComplete();
        m_aiMovement = ENEMY_MOVEMENT_TYPE.PATROL;
    }
}
