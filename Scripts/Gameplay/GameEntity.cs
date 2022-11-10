using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameEntity : MonoBehaviour, IPausable
{
    protected int m_health;
    protected int m_maxHealth;
    protected Collider2D m_collider;
    protected Rigidbody2D m_rigidBody2D;
    protected SpriteRenderer m_spriteRenderer;
    protected Animator m_animator;
    protected bool m_simulated = true;
    protected bool m_paused = false;
    protected bool m_shouldUpdateFacing;

    private Vector2 m_velocity = Vector2.zero;
    private Vector2 m_prevVelocity = Vector2.zero;
    private float m_facing;
    private float m_previousFacing;
    protected Dictionary<string, Action> m_signals;

    private void Awake()
    {
        m_simulated = true;
        m_paused = false;
        m_shouldUpdateFacing = true;
        m_facing = 1f;
        m_previousFacing = 0f;
        m_signals = new Dictionary<string, Action>();
        EntityAwake();

        if(Application.isPlaying)
        {
            GameplayManager.Instance.RegisterEntity(this);
        }
    }

    protected virtual void EntityAwake()
    {
        m_collider = GetComponent<Collider2D>();
        m_rigidBody2D = GetComponent<Rigidbody2D>();
        m_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        m_animator = GetComponent<Animator>();
    }

    void Start()
    {
        EntityStart();
    }

    protected virtual void EntityStart()
    {

    }

    void Update()
    {
        if(m_paused)
        {
            return;
        }

        if(!m_simulated)
        {
            CheckForRestartSimulation();
            return;
        }

        EntityUpdate();
    }

    protected virtual void EntityUpdate()
    {

    }

    private void FixedUpdate()
    {
        if(m_paused)
        {
            return;
        }

        if(!m_simulated)
        {
            return;
        }

        m_prevVelocity = m_velocity;
        EntityFixedUpdate();
        if(m_rigidBody2D != null)
        {
            if(m_rigidBody2D.bodyType != RigidbodyType2D.Static)
            {
                m_rigidBody2D.velocity = m_velocity;
            }
        }
        else
        {
            transform.position += (new Vector3(m_velocity.x, m_velocity.y, 0) * Time.deltaTime);
        }
    }

    protected virtual void EntityFixedUpdate()
    {

    }

    public bool IsSimulated()
    {
        return m_simulated;
    }

    public bool IsPaused()
    {
        return m_paused;
    }

    protected virtual void CheckForRestartSimulation()
    {

    }

    public void SetSimulated(bool aSimulated)
    {
        if(m_simulated == aSimulated)
        {
            return;
        }

        m_simulated = aSimulated;

        if(aSimulated)
        {
            OnStartSimulation();
        }
        else
        {
            OnEndSimulation();
        }
    }

    virtual protected void OnStartSimulation()
    {
        if(m_rigidBody2D != null)
        {
            m_rigidBody2D.simulated = true;
        }

        if(m_collider != null)
        {
            m_collider.enabled = true;
        }

        if(m_spriteRenderer != null)
        {
            m_spriteRenderer.enabled = true;
        }

        if(m_animator != null)
        {
            m_animator.enabled = true;
        }

        m_facing = 1f;
        m_previousFacing = 0f;
        transform.localScale = new Vector3(m_facing, transform.localScale.y, transform.localScale.z);
    }

    virtual protected void OnEndSimulation()
    {
        m_velocity = Vector2.zero;
        if(m_rigidBody2D != null)
        {
            m_rigidBody2D.simulated = false;
        }

        if(m_collider != null)
        {
            m_collider.enabled = false;
        }

        if(m_spriteRenderer != null)
        {
            m_spriteRenderer.enabled = false;
        }

        if(m_animator != null)
        {
            m_animator.enabled = false;
        }
    }

    public virtual void SetPaused(bool aPaused)
    {
        if(m_paused == aPaused)
        {
            return;
        }

        m_paused = aPaused;

        if(m_paused)
        {
            OnStartPause();
        }
        else
        {
            OnEndPause();
        }
    }

    protected virtual void OnStartPause()
    {
        if(m_animator != null)
        {
            m_animator.enabled = false;
        }
    }

    protected virtual void OnEndPause()
    {
        if(m_animator != null)
        {
            m_animator.enabled = true;
        }
    }

    virtual protected void UpdateFacing()
    {
        if(!m_shouldUpdateFacing)
        {
            return;
        }

        Vector2 currentVelocity = GetCurrentVelocity();
        m_previousFacing = m_facing;
        //Determine heading and flip object
        if(currentVelocity.x != 0f)
        {
            m_facing = (currentVelocity.x < 0f) ? -1f : 1f;
        }

        if(m_previousFacing != m_facing)
        {
            transform.localScale = new Vector3(m_facing, transform.localScale.y, transform.localScale.z);
        }
    }

    protected void SetFacing(float aFacing)
    {
        if(m_previousFacing != aFacing)
        {
            m_previousFacing = m_facing;
            m_facing = aFacing;
            transform.localScale = new Vector3(m_facing, transform.localScale.y, transform.localScale.z);
        }
    }

    public float GetFacing()
    {
        return m_facing;
    }

    public void SetPosition(Vector2 aPos)
    {
        if(m_rigidBody2D)
        {
            m_rigidBody2D.position = aPos;
        }

        transform.position = aPos;
    }

    protected Collider2D GetEntityCollider()
    {
        return m_collider;
    }

    public Vector2 GetCurrentVelocity()
    {
        return m_velocity;
    }

    public Vector2 GetPreviousVelocity()
    {
        return m_prevVelocity;
    }

    public virtual void UpdateVelocity(Vector2 aNewVelocity)
    {
        m_velocity = aNewVelocity;
        UpdateFacing();
    }

    public Rigidbody2D GetRigidbody2D()
    {
        return m_rigidBody2D;
    }

    public Vector2 GetSize()
    {
        Vector2 returnVector = Vector2.zero;

        if(m_collider)
        {
            returnVector.x = m_collider.bounds.extents.x * 2;
            returnVector.y = m_collider.bounds.extents.y * 2;
        }

        return returnVector;
    }

    public Vector2 GetHalfSize()
    {
        Vector2 returnVector = Vector2.zero;

        if(m_collider)
        {
            returnVector.x = m_collider.bounds.extents.x;
            returnVector.y = m_collider.bounds.extents.y;
        }

        return returnVector;
    }

    public Vector2 GetGroundCenter()
    {
        Vector2 returnVector = transform.position;

        if(m_collider)
        {
            returnVector -= m_collider.offset;
            returnVector.y -= m_collider.bounds.extents.y;
        }

        return returnVector;
    }

    public Vector2 GetGroundCenterBack()
    {
        Vector2 returnVector = Vector2.zero;

        if(m_collider)
        {
            returnVector = m_collider.bounds.min;
            if(transform.localScale.x < 0.0f)
            {
                returnVector.x += m_collider.bounds.extents.x * 2.0f;
            }
        }

        return returnVector;
    }

    public Vector2 GetGroundCenterForward()
    {
        Vector2 returnVector = Vector2.zero;

        if(m_collider)
        {
            returnVector = m_collider.bounds.min;
            if(transform.localScale.x > 0.0f)
            {
                returnVector.x += (m_collider.bounds.extents.x * 2f);
            }
        }

        return returnVector;
    }

    public void ProcessSignal(string aSignal)
    {
        if(m_signals.TryGetValue(aSignal, out Action aciton))
        {
            aciton.Invoke();
        }
    }

    public GameObject PlayFX(GameObject aPrefab)
    {
        return GameFXManager.Instance.RequestPlayFX(aPrefab, transform.position, gameObject);
    }

    public bool IsInCameraBounds()
    {
        Plane[] cameraFrustums = GameplayManager.Instance.GetMainCameraFrustums();
        return GeometryUtility.TestPlanesAABB(cameraFrustums, m_collider.bounds);
    }

    public int GetCurrentHealth() { return m_health; }
    public int GetMaxHealth() { return m_maxHealth; }

    public void SetStartingHealth(int aNewHealth)
    {
        m_health = aNewHealth;
        m_maxHealth = m_health;
    }

    virtual public void ChangeHealth(int aDelta)
    {
        m_health += aDelta;
        m_health = Mathf.Clamp(m_health, 0, m_maxHealth);

        if(m_health == 0)
        {
            SetSimulated(false);
        }
    }

    virtual public void OnTakeDamage(int aDamage)
    {
        ChangeHealth(-aDamage);
    }
}
