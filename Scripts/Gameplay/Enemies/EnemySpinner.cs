using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpinner : EnemyEntity
{
    [Header("Spinner")]
    public float m_stunDurationS = 2f;
    public float m_defaultMovementSpeed = 1f;
    public float m_playerOnPlatformMovementSpeed = 2f;
    public float m_switchToDefaultSpeedTimeS = 0.5f;
    protected CountdownTimer m_stunTimer;
    protected CountdownTimer m_swtichToDefaultSpeedTimer;
    protected bool m_previousPlayerWasTouchingPlatform;

    protected override void EntityAwake()
    {
        base.EntityAwake();
        m_stunTimer = new CountdownTimer();
        m_swtichToDefaultSpeedTimer = new CountdownTimer();
        m_previousPlayerWasTouchingPlatform = false;
    }

    protected override void EntityUpdate()
    {
        m_stunTimer.Update(Time.deltaTime);
        if(m_stunTimer.IsDone())
        {
            m_aiMovement = ENEMY_MOVEMENT_TYPE.STAY_ON_PLATFORM;
        }

        m_swtichToDefaultSpeedTimer.Update(Time.deltaTime);
        if(m_swtichToDefaultSpeedTimer.IsDone())
        {
            m_movementSpeed = m_defaultMovementSpeed;
        }

        if(m_startingPlatform.collider != null)
        {
            Platform platform = m_startingPlatform.collider.GetComponent<Platform>();
            if(platform)
            {
                bool isPlayerTouching = platform.IsPlayerTouching();
                if(m_previousPlayerWasTouchingPlatform && !isPlayerTouching)
                {
                    m_swtichToDefaultSpeedTimer.Start(m_switchToDefaultSpeedTimeS);
                    m_movementSpeed = m_playerOnPlatformMovementSpeed;
                }
                else if(isPlayerTouching && m_swtichToDefaultSpeedTimer.IsDone())
                {
                    m_movementSpeed = m_playerOnPlatformMovementSpeed;
                }

                m_previousPlayerWasTouchingPlatform = isPlayerTouching;
            }
        }

        base.EntityUpdate();
    }

    public override void OnTakeDamage(int aDamage)
    {
        if(!m_stunTimer.IsActive())
        {
            m_stunTimer.Start(m_stunDurationS);
            m_aiMovement = ENEMY_MOVEMENT_TYPE.STATIONARY;
            m_animator.SetTrigger("stunned");
        }
    }
}
