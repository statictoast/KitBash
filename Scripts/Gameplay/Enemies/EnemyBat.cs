using UnityEngine;

public class EnemyBat : EnemyEntity
{
    protected override void EntityUpdate()
    {
        base.EntityUpdate();
        m_animator.SetInteger("movementType", (int)m_aiMovement);
    }

    protected override void StartAttack()
    {
        base.StartAttack();
        if(m_aiMovement != ENEMY_MOVEMENT_TYPE.RETURN_TO_START)
        {
            m_aiMovement = ENEMY_MOVEMENT_TYPE.FOLLOW_PLAYER;
        }
    }

    protected override void StartLeash()
    {
        base.StartLeash();
        m_aiMovement = ENEMY_MOVEMENT_TYPE.RETURN_TO_START;
    }

    protected override void OnReachedStart()
    {
        base.OnReachedStart();
        SetAttackingTarget(false);
        m_aiMovement = ENEMY_MOVEMENT_TYPE.STATIONARY;
    }

    public override void OnContactDamagePlayer()
    {
        base.OnContactDamagePlayer();
        m_aiMovement = ENEMY_MOVEMENT_TYPE.RETURN_TO_START;
    }
}
