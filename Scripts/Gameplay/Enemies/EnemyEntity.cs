using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEntity : GameEntity
{
    public bool m_alwaysShowGizmos = false;

    [Header("Combat")]
    public int m_startingHealth = 1;
    public int m_contactDamage = 1;
    public bool m_mustFaceTargetToAttack = false;
    protected bool m_attackingTarget;
    protected CountdownTimer m_attackCooldownTimer;

    [Header("Movement")]
    public float m_movementSpeed = 2f;
    public ENEMY_STARTING_DIRECTION m_startingDirection = ENEMY_STARTING_DIRECTION.LEFT;
    protected Vector3 m_spawnPosition;
    protected float m_movementHeading = 1f;
    protected RaycastHit2D m_startingPlatform;
    protected RaycastHit2D m_currentPlatform;
    [SerializeField]
    protected Vector3[] m_patrolBounds = new Vector3[2] { Vector3.zero, Vector3.zero };

    [Header("AI Logic")]
    public ENEMY_MOVEMENT_TYPE m_aiMovement = ENEMY_MOVEMENT_TYPE.STATIONARY;
    public bool m_canLeash = false;
    public AGGRO_DISTANCE_TYPE m_leashDistanceType = AGGRO_DISTANCE_TYPE.RADIAL;
    public float m_leashRadius = 4.0f;
    public AGGRO_DISTANCE_TYPE m_attackDistanceType = AGGRO_DISTANCE_TYPE.RADIAL;
    public float m_attackRadius = 0.5f;
    public bool m_faceTarget = false;
    public float m_outOfRangeThreshold = 3f;
    protected GameObject m_aggroTarget;

    [Header("Lifecycle")]
    protected Vector3 m_startingLocation;
    protected bool m_hasExitedCameraBoundsWhileDead;
    protected bool m_hasBeenSeen;

    private const float cMaxDistanceFromSpawnSqr = 200f;

    protected override void EntityAwake()
    {
        base.EntityAwake();
        m_spawnPosition = transform.position;
        m_hasExitedCameraBoundsWhileDead = false;
        m_startingLocation = transform.position;
        m_hasBeenSeen = false;
        m_attackCooldownTimer = new CountdownTimer();
        m_attackCooldownTimer.SetFinishedCallback(() => { OnAttackCooldownComplete(); });
        AIAwake();
    }

    protected void AIAwake()
    {
        switch(m_aiMovement)
        {
            case ENEMY_MOVEMENT_TYPE.STATIONARY:
            {

            }
            break;
            case ENEMY_MOVEMENT_TYPE.MOVE_FOWARD:
            {

            }
            break;
            case ENEMY_MOVEMENT_TYPE.STAY_ON_PLATFORM:
            {
                Vector2 unitSize = GetSize();
                float groundCheckLength = unitSize.y / 2.0f;
                Vector2 unitGroundCenter = GetGroundCenter();
                m_startingPlatform = Physics2D.Raycast(unitGroundCenter, Vector2.down, groundCheckLength, 1 << GameplayLayers.Platform);
            }
            break;
            case ENEMY_MOVEMENT_TYPE.FOLLOW_PLAYER:
            {

            }
            break;
        }
    }

    protected override void EntityStart()
    {
        SetupEnemy();
    }

    protected virtual void SetupEnemy()
    {
        SetStartingHealth(m_startingHealth);
        m_aggroTarget = null;
        m_attackingTarget = false;
        m_movementHeading = GetStartingDirectionHeading();
        m_attackCooldownTimer.Stop();
        m_currentPlatform = new RaycastHit2D();

        SetFacing(m_movementHeading);
    }

    protected float GetStartingDirectionHeading()
    {
        switch(m_startingDirection)
        {
            case ENEMY_STARTING_DIRECTION.LEFT:
                return -1f;
            case ENEMY_STARTING_DIRECTION.RIGHT:
                return 1f;
        }

        return 1f;
    }

    protected override void EntityUpdate()
    {
        if(GetCurrentVelocity().sqrMagnitude > 0.02f)
        {
            CheckPlayerDistanceThreshold();
        }

        UpdateAI();

        m_attackCooldownTimer.Update(Time.deltaTime);

        m_animator.SetFloat("movementSpeedX", Mathf.Abs(GetCurrentVelocity().x));
        m_animator.SetFloat("movementSpeedY", Mathf.Abs(GetCurrentVelocity().y));
        m_animator.SetBool("hasTarget", m_aggroTarget != null);
        m_animator.SetBool("attackTarget", m_attackingTarget);
    }

    protected override void EntityFixedUpdate()
    {
        FixedUpdateAI();
    }

    protected void UpdateAI()
    {
        if(m_aggroTarget == null)
        {
            m_aggroTarget = GameplayManager.Instance.GetPlayerObject();
        }

        CheckHasBeenSeen();
        CheckPlayerInAttackRange();

        switch(m_aiMovement)
        {
            case ENEMY_MOVEMENT_TYPE.STATIONARY:
            {
                if(m_faceTarget && m_aggroTarget)
                {
                    //TODO: this is wrong
                    Vector2 distanceToPlayer = m_aggroTarget.transform.position - transform.position;
                    m_movementHeading = distanceToPlayer.x > 0f ? 1f : -1f;
                    transform.localScale = new Vector3(m_movementHeading, transform.localScale.y, transform.localScale.z);
                }
            }
                break;
            case ENEMY_MOVEMENT_TYPE.MOVE_FOWARD:
            {

            }
                break;
            case ENEMY_MOVEMENT_TYPE.STAY_ON_PLATFORM:
            {

            }
                break;
            case ENEMY_MOVEMENT_TYPE.FOLLOW_PLAYER:
            {

            }
                break;
        }
    }

    protected void FixedUpdateAI()
    {
        if(!m_hasBeenSeen)
        {
            UpdateVelocity(Vector2.zero);
            return;
        }
        
        CheckShouldLeash();

        switch(m_aiMovement)
        {
            case ENEMY_MOVEMENT_TYPE.STATIONARY:
            {
                UpdateVelocity(Vector2.zero);
            }
                break;
            case ENEMY_MOVEMENT_TYPE.MOVE_FOWARD:
            {
                MoveForward();
            }
                break;
            case ENEMY_MOVEMENT_TYPE.STAY_ON_PLATFORM:
            {
                UpdateCurrentPlatform();
                MoveForward();
            }
                break;
            case ENEMY_MOVEMENT_TYPE.FOLLOW_PLAYER:
            {
                MoveTowardsTarget();
            }
                break;
            case ENEMY_MOVEMENT_TYPE.RETURN_TO_START:
            {
                MoveTowardsStart();
            }
                break;
            case ENEMY_MOVEMENT_TYPE.PATROL:
            {
                CheckPatrolOutOfBounds();
                MoveForward();
            }
                break;
        }
    }

    protected override void CheckForRestartSimulation()
    {
        if(IsInCameraBounds())
        {
            if(m_hasExitedCameraBoundsWhileDead)
            {
                SetSimulated(true);
            }
        }
        else
        {
            m_hasExitedCameraBoundsWhileDead = true;
        }
    }

    protected override void OnStartSimulation()
    {
        base.OnStartSimulation();
        m_hasBeenSeen = false;
        SetFacing(m_movementHeading);
        m_hasExitedCameraBoundsWhileDead = false;
    }

    protected override void OnEndSimulation()
    {
        base.OnEndSimulation();
        if(m_health <= 0)
        {
            GameObject deathFXPrefab = GameplayManager.Instance.GetGlobalGameData().m_enemyDeathFX;
            GameFXManager.Instance.RequestPlayFX(deathFXPrefab, transform.position, gameObject);

            GameplayManager.Instance.DeterminePickupDrop(transform.position);
        }

        SetPosition(m_startingLocation);
        SetupEnemy();
    }

    protected bool IsAttackOnCooldown()
    {
        return m_attackCooldownTimer.IsActive();
    }

    virtual protected void OnAttackCooldownComplete()
    {

    }

    virtual protected void CheckShouldLeash()
    {
        if(m_aggroTarget == null)
        {
            return;
        }

        Vector2 distanceToPlayer = m_aggroTarget.transform.position - transform.position;
        switch(m_leashDistanceType)
        {
            case AGGRO_DISTANCE_TYPE.RADIAL:
            {
                if(distanceToPlayer.magnitude > m_leashRadius)
                {
                    StartLeash();
                }
            }
            break;
            case AGGRO_DISTANCE_TYPE.X_ONLY:
            {
                if(Mathf.Abs(distanceToPlayer.x) > m_leashRadius)
                {
                    StartLeash();
                }
            }
            break;
            case AGGRO_DISTANCE_TYPE.Y_ONLY:
            {
                if(Mathf.Abs(distanceToPlayer.y) > m_leashRadius)
                {
                    StartLeash();
                }
            }
            break;
            case AGGRO_DISTANCE_TYPE.BOX:
            {

            }
            break;
        }
    }

    virtual protected void StartLeash()
    {

    }

    virtual protected void CheckPlayerInAttackRange()
    {
        if(IsAttackOnCooldown() || m_attackingTarget)
        {
            return;
        }

        bool shouldStartAttack = false;
        if(m_aggroTarget)
        {
            Vector2 distanceToPlayer = m_aggroTarget.transform.position - transform.position;
            switch(m_attackDistanceType)
            {
                case AGGRO_DISTANCE_TYPE.RADIAL:
                {
                    shouldStartAttack = distanceToPlayer.magnitude <= m_attackRadius;
                }
                break;
                case AGGRO_DISTANCE_TYPE.X_ONLY:
                {
                    shouldStartAttack = Mathf.Abs(distanceToPlayer.x) <= m_attackRadius && Mathf.Abs(distanceToPlayer.y) < m_outOfRangeThreshold;
                }
                break;
                case AGGRO_DISTANCE_TYPE.Y_ONLY:
                {
                    shouldStartAttack = Mathf.Abs(distanceToPlayer.y) <= m_attackRadius && Mathf.Abs(distanceToPlayer.x) < m_outOfRangeThreshold;
                }
                break;
                case AGGRO_DISTANCE_TYPE.BOX:
                {

                }
                break;
            }

            if(shouldStartAttack && m_mustFaceTargetToAttack)
            {
                shouldStartAttack = (GetFacing() > 0f && distanceToPlayer.x > 0f) || (GetFacing() < 0f && distanceToPlayer.x < 0f);
            }
        }

        // here we can only determine if we should start attacking
        // this event should be consumed once the attack finishes so we can
        // continue the cycle once again
        if(shouldStartAttack)
        {
            StartAttack();
        }
    }

    virtual protected void StartAttack()
    {
        SetAttackingTarget(true);
    }

    protected void SetAttackingTarget(bool aAttack)
    {
        if(m_attackingTarget != aAttack)
        {
            m_attackingTarget = aAttack;
        }
    }

    protected void MoveTowardsTarget()
    {
        if(m_aggroTarget)
        {
            Vector2 velocity = GetCurrentVelocity();
            Vector2 distanceToPlayer = m_aggroTarget.transform.position - transform.position;
            velocity = distanceToPlayer.normalized * m_movementSpeed;
            UpdateVelocity(velocity);
        }
        else
        {
            UpdateVelocity(Vector2.zero);
        }
    }

    protected void MoveTowardsStart()
    {
        Vector2 velocity = GetCurrentVelocity();
        Vector2 distanceToStart = m_startingLocation - transform.position;
        if(distanceToStart.sqrMagnitude < 0.02f)
        {
            UpdateVelocity(Vector2.zero);
            OnReachedStart();
        }
        else
        {
            velocity = distanceToStart.normalized * m_movementSpeed;
            UpdateVelocity(velocity);
        }
    }

    protected void CheckPatrolOutOfBounds()
    {
        bool switchHeading = false;
        Vector2 distanceToLeftBound = transform.position - m_patrolBounds[0];
        switchHeading = distanceToLeftBound.x < 0;

        Vector2 distanceToRightBound = transform.position - m_patrolBounds[1];
        switchHeading |= distanceToRightBound.x > 0;

        if(switchHeading)
        {
            m_movementHeading *= -1f;
        }
    }

    virtual protected void OnReachedStart()
    {
        
    }

    protected void MoveForward()
    {
        Vector2 velocity = GetCurrentVelocity();
        if(m_currentPlatform.collider == null)
        {
            velocity.x = m_movementHeading * m_movementSpeed;
        }
        else
        {
            velocity.x = m_movementHeading * m_movementSpeed * m_currentPlatform.normal.y * m_movementSpeed;
            velocity.y = m_movementHeading * m_movementSpeed * -m_currentPlatform.normal.x * m_movementSpeed;
        }
        UpdateVelocity(velocity);
    }

    protected void UpdateCurrentPlatform()
    {
        RaycastHit2D lastHit = m_currentPlatform;
        Vector2 unitSize = GetSize();
        float groundCheckLength = unitSize.y / 2.0f;
        Vector2 unitGroundCenter = GetGroundCenter();
        m_currentPlatform = Physics2D.Raycast(unitGroundCenter, Vector2.down, groundCheckLength, 1 << GameplayLayers.Platform);
        if(lastHit.collider != m_currentPlatform.collider)
        {
            OnNewPlatformHit();
        }
    }

    protected void OnNewPlatformHit()
    {
        switch(m_aiMovement)
        {
            case ENEMY_MOVEMENT_TYPE.STATIONARY:
            {

            }
            break;
            case ENEMY_MOVEMENT_TYPE.MOVE_FOWARD:
            {

            }
            break;
            case ENEMY_MOVEMENT_TYPE.STAY_ON_PLATFORM:
            {
                if(m_currentPlatform.collider != m_startingPlatform.collider)
                {
                    m_movementHeading *= -1f;
                }
            }
            break;
            case ENEMY_MOVEMENT_TYPE.FOLLOW_PLAYER:
            {
                
            }
            break;
        }
    }

    protected void CheckPlayerDistanceThreshold()
    {
        Vector2 distanceFromSpawn = transform.position - m_spawnPosition;
        if(distanceFromSpawn.sqrMagnitude > cMaxDistanceFromSpawnSqr)
        {
            if(!IsInCameraBounds())
            {
                SetSimulated(false);
            }
        }
    }

    protected void CheckHasBeenSeen()
    {
        if(m_hasBeenSeen)
            return;

        m_hasBeenSeen = IsInCameraBounds();
    }

    virtual public void OnContactDamagePlayer()
    {

    }

    public Vector3 GetPatrolPoint(int aIndex)
    {
        if(aIndex < 2 || aIndex >= 0)
        {
            return m_patrolBounds[aIndex];
        }

        return Vector3.zero;
    }

    public void SetPatrolPoint(int aIndex, Vector3 aPos)
    {
        if(aIndex < 2 || aIndex >= 0)
        {
            m_patrolBounds[aIndex] = aPos;
        }
    }

    #region Debug Functions

    void OnDrawGizmosSelected()
    {
        DrawEnemyDebugLines();
    }

    private void OnDrawGizmos()
    {
        if(!m_alwaysShowGizmos)
            return;

        DrawEnemyDebugLines();
    }

    private void DrawEnemyDebugLines()
    {
        if(!m_simulated)
        {
            return;
        }

        if(m_canLeash)
        {
            Gizmos.color = Color.yellow;
            switch(m_leashDistanceType)
            {
                case AGGRO_DISTANCE_TYPE.RADIAL:
                {
                    Gizmos.DrawWireSphere(transform.position, m_leashRadius);
                }
                break;
                case AGGRO_DISTANCE_TYPE.X_ONLY:
                {
                    Gizmos.DrawWireCube(transform.position, new Vector3(m_leashRadius * 2f, m_outOfRangeThreshold, m_leashRadius));
                }
                break;
                case AGGRO_DISTANCE_TYPE.Y_ONLY:
                {
                    Gizmos.DrawWireCube(transform.position, new Vector3(m_outOfRangeThreshold, m_leashRadius * 2f, m_leashRadius));
                }
                break;
                case AGGRO_DISTANCE_TYPE.BOX:
                {

                }
                break;
            }
        }

        Gizmos.color = Color.red;
        switch(m_attackDistanceType)
        {
            case AGGRO_DISTANCE_TYPE.RADIAL:
            {
                Gizmos.DrawWireSphere(transform.position, m_attackRadius);
            }
            break;
            case AGGRO_DISTANCE_TYPE.X_ONLY:
            {
                Gizmos.DrawWireCube(transform.position, new Vector3(m_attackRadius * 2f, m_outOfRangeThreshold, m_attackRadius));
            }
            break;
            case AGGRO_DISTANCE_TYPE.Y_ONLY:
            {
                Gizmos.DrawWireCube(transform.position, new Vector3(m_outOfRangeThreshold, m_attackRadius * 2f, m_attackRadius));
            }
            break;
            case AGGRO_DISTANCE_TYPE.BOX:
            {

            }
            break;
        }
    }

    #endregion
}
