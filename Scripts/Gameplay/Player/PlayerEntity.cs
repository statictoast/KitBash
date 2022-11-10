using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEntity : PlatformerUnit, IProjectileFiringEntity
{
    [Header("Movement")]
    public float m_runSpeed = 3.0f;
    public float m_dashSpeed = 5.0f;
    public float m_lossOfControlSpeed = 1.0f;
    public float m_maxDashTimeS = 1.0f;
    public float m_accelerationModifier = 0.5f;
    public float m_delayWallSlideHangTimeS = 0.33f;
    public GameObject m_dashFX;
    public GameObject m_wallSlideFX;
    public GameObject m_chargingFX;
    public PhysicsMaterial2D m_zeroFriction;
    public PhysicsMaterial2D m_fullFriction;
    private float m_currentDashTimeS;
    private bool m_dashing;
    private bool m_startDash;
    private bool m_dashHold;
    private bool m_endDash;
    private bool m_wallDashQueued;
    private float m_currentDashDirection;
    private float m_currentDelayWallSlideHangTimeS;
    private GameObject m_currentWallSlideFXObject;
    private GameObject m_currentCharingFX;

    [Header("Jumping")]
    public float m_maxJumpHeight = 2f;
    public float m_gravity = 9f;
    public float m_terminalVelocity = 40f;
    public float m_wallSlideDampeningFactor = 0.8f;
    public float m_wallSlideTerminalVelocity = 20f;
    public float m_wallSlideJumpOppositeForce = 4f;
    public float m_wallSlideJumpIgnoreInputTimeS = 0.2f;
    public int m_jumpCoyoteTimeF = 6;
    private float m_initialVelocity = 0f;
    private float m_timeToApex = 0f;
    private float m_jumpHeldTime = 0f;
    private float m_wallSlideJumpCurrentIgnoreInputTimeS = 0f;
    private float m_lastJumpCommandTimeS;
    private int m_currentJumpLedgeTimeF;
    private bool m_jumping;
    private bool m_jumpedWhileDashing;
    private bool m_wallSliding;
    private bool m_startJump;
    private bool m_jumpHeld;
    private bool m_jumpReleased;


    [Header("Attack")]
    public int m_maxProjectilesOnScreen = 3;
    public float m_timeToMinAttackChargeS = 0.1f;
    public float m_timeToMaxAttackChargeS = 1f;
    public float m_basicAttackSpawnVariance = 0.1f;
    private bool m_attacking;
    private bool m_attackingBlocked;
    private float m_attackCharge;
    PLAYER_PROJECTILE_TYPE m_currentProjectileType;
    [SerializeField]
    private ProjectileFiringEntityComponent m_projectileData = null;

    [Header("Gameplay")]
    public int m_startingHealth = 10;
    public float m_invulnerabilityTimeS = 2.0f;
    public float m_contactDamageLoCTimeS = 1.0f;
    public GameObject m_spawnInFXPrefab;
    private float m_currentInvulnerabilityTimeS;
    private float m_contactLoCDirection;
    private bool m_invulnerable;
    private float m_currentContactDamageLoCTimeS;

    protected override void EntityAwake()
    {
        base.EntityAwake();
        m_initialVelocity = Mathf.Sqrt(2 * m_gravity * m_maxJumpHeight);
        m_timeToApex = m_initialVelocity / m_gravity;
        m_startJump = m_jumpHeld = m_jumpReleased = false;
        m_startDash = m_dashHold = m_endDash = false;
        m_jumping = false;
        m_attacking = false;
        m_attackingBlocked = false;
        m_dashing = false;
        m_wallDashQueued = false;
        m_jumpedWhileDashing = false;
        m_wallSliding = false;
        m_currentDashTimeS = 0f;
        m_currentDashDirection = 1f;
        m_attackCharge = 0f;
        m_lastJumpCommandTimeS = 0f;
        m_wallSlideJumpCurrentIgnoreInputTimeS = 0f;
        m_currentJumpLedgeTimeF = 0;
        m_currentProjectileType = PLAYER_PROJECTILE_TYPE.BASIC;
        m_currentInvulnerabilityTimeS = 0f;
        m_invulnerable = false;
        m_currentContactDamageLoCTimeS = 0f;
        m_currentDelayWallSlideHangTimeS = 0f;

        m_signals.Add("spawnIn", OnFXSpawnIn);
        m_signals.Add("spawnInComplete", OnFXSpawnInComplete);

        m_projectileData.Setup();

        SetStartingHealth(m_startingHealth);
    }

    protected override void EntityStart()
    {
        base.EntityStart();
        EventManager.Instance.TriggerEvent(Events.EVENT_PLAYER_HEALTH_SET, new PlayerHealthSetEvent(m_health));
    }

    protected override void EntityUpdate()
    {
        base.EntityUpdate();

        if(HasLossOfControl())
        {
            m_currentContactDamageLoCTimeS -= Time.deltaTime;
            if(m_currentContactDamageLoCTimeS <= 0f)
            {
                m_currentContactDamageLoCTimeS = 0f;
            }
        }

        if(m_invulnerable)
        {
            m_currentInvulnerabilityTimeS -= Time.deltaTime;
            if(m_currentInvulnerabilityTimeS <= 0f)
            {
                EndInvulnerable();
            }
        }

        if(HasJumpLedgeForgivenessTime())
        {
            m_currentJumpLedgeTimeF -= 1;
            if(m_currentJumpLedgeTimeF <= 0)
            {
                m_currentJumpLedgeTimeF = 0;
            }
        }

        if(m_currentDelayWallSlideHangTimeS > 0f)
        {
            m_currentDelayWallSlideHangTimeS -= Time.deltaTime;
            if(m_currentDelayWallSlideHangTimeS <= 0f)
            {
                EndWallSlide();
                m_currentDelayWallSlideHangTimeS = 0f;
            }
        }

        HandlePlayerInput();
        HandlePlayerAttack();

        m_animator.SetBool("isDashing", m_dashing);
        m_animator.SetBool("isWallSliding", m_wallSliding);
        m_animator.SetBool("isLossOfControl", HasLossOfControl());

        m_collisionState.Reset();
    }

    private void HandlePlayerAttack()
    {
        bool attackDown = InputManager.Instance.HasActionThisFrame(InputActions.BasicAttackDown);
        bool attackHeld = InputManager.Instance.HasActionThisFrame(InputActions.BasicAttackHeld);
        bool attackReleased = InputManager.Instance.HasActionThisFrame(InputActions.BasicAttackReleased);

        if(attackReleased)
        {
            bool shouldFireCharge = false;
            if(m_attackCharge >= m_timeToMaxAttackChargeS)
            {
                m_currentProjectileType = PLAYER_PROJECTILE_TYPE.MAX_CHARGE;
                shouldFireCharge = true;
            }
            else if(m_attackCharge >= m_timeToMinAttackChargeS)
            {
                m_currentProjectileType = PLAYER_PROJECTILE_TYPE.MIN_CHARGE;
                shouldFireCharge = true;
            }

            if(m_attackingBlocked)
            {
                shouldFireCharge = false;
            }

            if(shouldFireCharge)
            {
                if(m_attacking)
                {
                    int nameHash = m_animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
                    m_animator.Play(nameHash, 0, 0f);
                }
                else
                {
                    m_attacking = true;
                    m_attackingBlocked = true;
                    m_animator.SetTrigger("attackTrigger");
                }
            }

            if(m_currentCharingFX != null)
            {
                m_currentCharingFX.GetComponent<FXLogic>().RequestEnd();
                m_currentCharingFX = null;
            }

            m_attackCharge = 0f;
        }

        if(attackDown && !m_attackingBlocked)
        {
            int numProjectilesOnScreen = GetNumProjectilesActive();
            if(numProjectilesOnScreen < m_maxProjectilesOnScreen)
            {
                m_currentProjectileType = PLAYER_PROJECTILE_TYPE.BASIC;
                if(m_attacking)
                {
                    // attacking as fast as you can press the button
                    int nameHash = m_animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
                    m_animator.Play(nameHash, 0, 0f);
                    m_attackCharge = 0f;
                }
                else
                {
                    m_attacking = true;
                    m_attackingBlocked = true;
                    m_animator.SetTrigger("attackTrigger");
                }
            }
        }

        if(attackHeld)
        {
            m_attackCharge += Time.deltaTime;
            if(m_attackCharge >= m_timeToMinAttackChargeS && m_currentCharingFX == null)
            {
                m_currentCharingFX = GameFXManager.Instance.RequestPlayFX(m_chargingFX, gameObject);
            }
        }
    }

    public void SpawnAttackProjectile()
    {
        int currentProjectileIndex = (int)m_currentProjectileType;
        Vector3 spawnPosition = transform.TransformPoint(m_projectileData.GetStartingPosition(currentProjectileIndex));
        int numPoints = m_projectileData.GetNumProjectileAttackDirections(currentProjectileIndex);
        for(int i = 0; i < numPoints; i++)
        {
            ProjectileEntity projectile = m_projectileData.AcquireProjectile(currentProjectileIndex);
            projectile.SetOwner(this);
            Vector3 direction = m_projectileData.GetTransformedProjectileDirection(currentProjectileIndex, i, transform);
            float velocity = m_projectileData.GetProjectileVelocity(currentProjectileIndex);
            direction *= velocity;

            if(m_currentProjectileType == PLAYER_PROJECTILE_TYPE.BASIC)
            {
                spawnPosition.y += Random.Range(-m_basicAttackSpawnVariance, m_basicAttackSpawnVariance);
            }
            projectile.FireProjectile(spawnPosition, direction);
        }

        m_attackingBlocked = false;
    }

    private int GetNumProjectilesActive()
    {
        return m_projectileData.GetTotalNumActiveProjectiles();
    }

    public void CompleteAttack()
    {
        m_attacking = false;
        if(m_currentCharingFX != null)
        {
            m_currentCharingFX.GetComponent<FXLogic>().RequestEnd();
            m_currentCharingFX = null;
        }
        m_currentProjectileType = PLAYER_PROJECTILE_TYPE.BASIC;
    }

    protected override void UpdateFacing()
    {
        if(HasLossOfControl())
        {
            return;
        }

        base.UpdateFacing();
    }

    protected override void EntityFixedUpdate()
    {
        base.EntityFixedUpdate();

        if(BecameGroundedThisFixedUpdate())
        {
            m_jumpedWhileDashing = false;
            m_wallDashQueued = false;
            m_wallSlideJumpCurrentIgnoreInputTimeS = 0f;
            GroundEntity();
        }

        if(WasGroundedLastFixedUpdate() && !IsGrounded())
        {
            m_currentJumpLedgeTimeF = m_jumpCoyoteTimeF;
        }

        HandlePlayerDash();
        HandlePlayerMovement();
        HandlePlayerJump();
        HandlePlayerGravity();
    }

    private void HandlePlayerMovement()
    {
        if(Time.time < m_wallSlideJumpCurrentIgnoreInputTimeS)
        {
            return;
        }

        Vector2 velocity = GetCurrentVelocity();
        Vector2 previousVelocity = GetPreviousVelocity();

        float heading = m_dashing ? m_currentDashDirection : InputManager.Instance.Heading.x;
        if(HasLossOfControl())
        {
            velocity.x = m_contactLoCDirection * -1f * m_lossOfControlSpeed;
            UpdateVelocity(velocity);
            return;
        }

        RaycastHit2D ground = GetCurrentPlatform();
        RaycastHit2D wall = GetCurrentWall();
        bool headingOutsideDeadzone = Mathf.Abs(heading) > 0.1f;
        if(wall.collider != null && !IsGrounded() && !m_jumping && headingOutsideDeadzone && velocity.y <= 0.02f)
        {
            float playerToWall = wall.point.x - transform.position.x;
            bool wallIsRight = playerToWall > 0f;
            bool headingIsRight = heading > 0f;
            if(wallIsRight == headingIsRight)
            {
                if(!m_wallSliding)
                {
                    m_wallSliding = true;
                    m_jumpedWhileDashing = false;
                    CleanupWallSlideFX();
                    velocity.x = 0f;
                    velocity.y = 0f;
                }
            }
            else
            {
                EndWallSlide();
                m_currentJumpLedgeTimeF = m_jumpCoyoteTimeF;
            }
        }
        else if(!headingOutsideDeadzone && m_wallSliding && m_currentDelayWallSlideHangTimeS == 0f)
        {
            m_currentDelayWallSlideHangTimeS = m_delayWallSlideHangTimeS;
            m_currentJumpLedgeTimeF = m_jumpCoyoteTimeF;
        }
        else
        {
            if(m_wallSliding && m_currentDelayWallSlideHangTimeS == 0f)
            {
                EndWallSlide();
            }
        }

        float movementSpeed = m_dashing || m_jumpedWhileDashing ? m_dashSpeed : m_runSpeed;
        float movementModifier = 1.0f; //m_unit.GetUnitStatus().GetMoveSpeedModifier();
        if(previousVelocity.x == 0f && IsGrounded())
        {
            movementModifier *= m_accelerationModifier;
        }

        m_rigidBody2D.sharedMaterial = m_zeroFriction;

        if(ground.collider == null || !IsGrounded())
        {
            velocity.x = heading * movementSpeed * movementModifier;
        }
        else if(!m_jumping)
        {
            float groundNormalAngle = Vector2.Angle(ground.normal, Vector2.up);
            if(Mathf.Abs(groundNormalAngle) > 0.01f && heading == 0f)
            {
                m_rigidBody2D.sharedMaterial = m_fullFriction;
            }

            velocity.x = heading * movementSpeed * ground.normal.y * movementModifier;
            velocity.y = heading * movementSpeed * -ground.normal.x * movementModifier;
            //Debug.DrawLine(transform.position, transform.position + new Vector3(ground.normal.y, -ground.normal.x, 0), Color.cyan, 1f);
        }

        //Debug.DrawLine(transform.position, transform.position + new Vector3(velocity.x, velocity.y, 0), Color.cyan, 1f);

        UpdateVelocity(velocity);
    }

    private void EndWallSlide()
    {
        m_wallSliding = false;
        CleanupWallSlideFX();
    }

    private void HandlePlayerInput()
    {
        if(!HasLossOfControl())
        {
            bool jumpDown = InputManager.Instance.HasActionThisFrame(InputActions.JumpDown);
            bool jumpHeld = InputManager.Instance.HasActionThisFrame(InputActions.JumpHeld);

            if(m_jumping && InputManager.Instance.HasAction(InputActions.JumpReleased, m_lastJumpCommandTimeS))
            {
                m_jumpReleased = true;
                m_lastJumpCommandTimeS = Time.time;
            }

            m_jumpHeld = jumpHeld;

            if(jumpDown)
            {
                m_startJump = IsGrounded() || m_wallSliding || HasJumpLedgeForgivenessTime();
                m_lastJumpCommandTimeS = Time.time;
            }

            bool dodgeDown = InputManager.Instance.HasActionThisFrame(InputActions.DodgeDown);
            bool dodgeHeld = InputManager.Instance.HasActionThisFrame(InputActions.DodgeHeld);
            bool dodgeReleased = InputManager.Instance.HasActionThisFrame(InputActions.DodgeReleased);

            if(m_dashing)
            {
                m_dashHold = dodgeHeld;
                m_endDash = dodgeReleased;
            }

            if(dodgeDown)
            {
                m_startDash = !m_dashing;
            }
        }
    }

    private void HandlePlayerJump()
    {
        if(HasLossOfControl())
        {
            return;
        }

        if(m_jumping)
        {
            m_jumpHeldTime += Time.deltaTime;
            if(m_jumpReleased)
            {
                EndJump();
            }
            else
            {
                if(m_jumpHeldTime >= m_timeToApex)
                {
                    EndJump();
                }
            }
        }
        else
        {
            if(m_startJump)
            {
                m_startJump = false;
                Vector2 velocity = GetCurrentVelocity();
                m_jumping = true;
                m_attacking = false;
                m_jumpedWhileDashing = m_dashing || m_wallDashQueued;
                EndDash();
                velocity.y = m_initialVelocity;
                if(m_wallSliding)
                {
                    if(!m_wallDashQueued)
                    {
                        velocity.x = GetFacing() * -1f * m_wallSlideJumpOppositeForce;
                        m_wallSlideJumpCurrentIgnoreInputTimeS = Time.time + m_wallSlideJumpIgnoreInputTimeS;
                    }
                }
                EndWallSlide();
                m_wallDashQueued = false;
                m_jumpHeldTime = 0f;
                UpdateVelocity(velocity);
            }
        }

    }

    private void EndJump()
    {
        if(IsGrounded())
        {
            m_jumpedWhileDashing = false;
            EndWallSlide();
        }
        Vector2 velocity = GetCurrentVelocity();
        m_jumping = false;
        m_jumpReleased = false;
        m_jumpHeld = false;
        velocity.y = 0;
        UpdateVelocity(velocity);
    }

    private bool HasJumpLedgeForgivenessTime()
    {
        return m_currentJumpLedgeTimeF > 0;
    }

    private void HandlePlayerGravity()
    {
        Vector2 velocity = GetCurrentVelocity();

        
        float gravityToAdd = m_gravity * Time.deltaTime;
        if(m_wallSliding)
        {
            gravityToAdd *= m_wallSlideDampeningFactor;
        }

        velocity.y -= gravityToAdd;

        float terminalVelocity = m_wallSliding ? m_wallSlideTerminalVelocity : m_terminalVelocity;
        if(velocity.y < -terminalVelocity)
        {
            velocity.y = -terminalVelocity;
        }
        

        UpdateVelocity(velocity);
    }

    private void HandlePlayerDash()
    {
        if(HasLossOfControl())
        {
            return;
        }

        if(m_wallSliding)
        {
            if(m_endDash)
            {
                m_wallDashQueued = false;
                m_endDash = false;
            }
        }

        if(m_dashing)
        {
            if(GetFacing() != m_currentDashDirection)
            {
                EndDash();
            }
            else if(m_jumping || m_endDash || (!IsGrounded() && InputManager.Instance.Heading.x == 0f))
            {
                EndDash();
            }
            else if(m_dashHold)
            {
                m_currentDashTimeS += Time.deltaTime;
                if(m_currentDashTimeS >= m_maxDashTimeS)
                {
                    EndDash();
                }
            }
        }
        else
        {
            if(m_startDash)
            {
                m_startDash = false;
                if(IsGrounded())
                {
                    Vector3 position = new Vector3(0.6f * -1f * GetFacing(), 0.3f, 0f);
                    position += transform.position;
                    GameFXManager.Instance.RequestPlayFX(m_dashFX, position, gameObject);
                    //GameFXManager.Instance.RequestPlayFX(m_dashFX, new Vector3(-0.6f, 0.3f, 0f), gameObject);
                    m_dashing = true;
                    m_currentDashTimeS = 0f;
                    m_currentDashDirection = GetFacing();
                }
                else if(m_wallSliding)
                {
                    m_wallDashQueued = true;
                }
            }
        }
    }

    private void EndDash()
    {
        m_dashing = false;
        m_endDash = false;
        m_dashHold = false;
    }

    private void CleanupWallSlideFX()
    {
        if(m_currentWallSlideFXObject != null)
        {
            m_currentWallSlideFXObject.GetComponent<FXLogic>().RequestEnd();
            m_currentWallSlideFXObject = null;
        }
    }

    public void OnTeleport()
    {
        EndDash();
        EndWallSlide();
        EndJump();
        m_jumpedWhileDashing = false;
        m_attackingBlocked = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.layer == GameplayLayers.Enemy)
        {
            OnCollideWithEnemy(collision.gameObject);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        
    }

    protected override void OnEndSimulation()
    {
        m_rigidBody2D.simulated = false;
        m_collider.enabled = false;
        UpdateVelocity(Vector2.zero);
        m_animator.SetTrigger("die");
    }

    protected void OnDeathComplete()
    {
        EventManager.Instance.TriggerEvent(Events.EVENT_PLAYER_FINISHED_DYING, new PlayerFinishedDyingEvent(FadeFinishedCallback));
    }

    private void FadeFinishedCallback()
    {
        GameCore.Instance.RestartLevel();
    }

    public void InstantlyKillPlayer()
    {
        SetSimulated(false);
    }

    public override void ChangeHealth(int aDelta)
    {
        int oldHealth = m_health;
        base.ChangeHealth(aDelta);
        EventManager.Instance.TriggerEvent(Events.EVENT_PLAYER_HEALTH_CHANGE, new PlayerHealthChangeEvent(oldHealth, m_health));
    }

    public override void OnTakeDamage(int aDamage)
    {
        if(m_invulnerable)
        {
            return;
        }

        CompleteAttack();
        EndJump();
        EndDash();
        m_jumpedWhileDashing = false;
        m_attackingBlocked = false;
        EndWallSlide();
        StartInvulnerable(m_invulnerabilityTimeS);
        base.OnTakeDamage(aDamage);
        m_currentContactDamageLoCTimeS = m_contactDamageLoCTimeS;
        m_animator.SetTrigger("loseControl");
    }

    private void OnCollideWithEnemy(GameObject aEnemy)
    {
        EnemyEntity eEntity = aEnemy.GetComponent<EnemyEntity>();
        if(eEntity == null)
        {
            Debug.LogWarning("[OnCollideWithEnemy] gameobject is layer enemy but does not have enemy script.");
            return;
        }

        eEntity.OnContactDamagePlayer();
        OnTakeDamage(eEntity.m_contactDamage);
    }

    private void StartInvulnerable(float aDuration)
    {
        m_currentInvulnerabilityTimeS = aDuration;
        m_contactLoCDirection = GetFacing();
        m_invulnerable = true;
        m_animator.Play("Invulnerability", 1);
        Physics2D.IgnoreLayerCollision(GameplayLayers.Player, GameplayLayers.Enemy, true);
    }

    private void EndInvulnerable()
    {
        m_invulnerable = false;

        // Make sure if we were to come out of invuln that we are not in an enemy, and if so, we are going to take another hit and repeat the cycle
        Collider2D hitEnemy = Physics2D.OverlapBox(transform.position, GetSize(), 0f, m_enemiesMask, 0f, 100f);
        if(hitEnemy)
        {
            OnCollideWithEnemy(hitEnemy.gameObject);
        }
        else
        {
            m_animator.Play("Empty", 1);
            Physics2D.IgnoreLayerCollision(GameplayLayers.Player, GameplayLayers.Enemy, false);
        }
    }

    public bool IsInvulnerable()
    {
        return m_invulnerable;
    }

    private bool HasLossOfControl()
    {
        return m_currentContactDamageLoCTimeS > 0f;
    }

    public ProjectileFiringEntityComponent GetProjectileData()
    {
        return m_projectileData;
    }

    #region NonInteractions

    public void PrepareForSpawnIn()
    {
        m_spriteRenderer.enabled = false;
    }

    public void SpawnIn()
    {
        PlayFX(m_spawnInFXPrefab);
    }

    private void OnFXSpawnIn()
    {
        m_spriteRenderer.enabled = true;
    }

    private void OnFXSpawnInComplete()
    {
        GameplayManager.Instance.PlayerSpawnInComplete();
    }

    #endregion
}
