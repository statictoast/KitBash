using System;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool
{
    private GameObject m_prefab;
    private List<ProjectileEntity> m_inactiveProjectiles;
    private List<ProjectileEntity> m_activeProjectiles;

    public ProjectilePool(GameObject aPrefab)
    {
        m_prefab = aPrefab;
        m_inactiveProjectiles = new List<ProjectileEntity>();
        m_activeProjectiles = new List<ProjectileEntity>();
    }

    public ProjectileEntity AcquireObject()
    {
        if(m_inactiveProjectiles.Count > 0)
        {
            ProjectileEntity oldProjectile = m_inactiveProjectiles[0];
            m_inactiveProjectiles.RemoveAt(0);
            m_activeProjectiles.Add(oldProjectile);
            oldProjectile.gameObject.SetActive(true);
            return oldProjectile;
        }

        GameObject newProjectile = GameObject.Instantiate(m_prefab, Vector2.zero, Quaternion.identity);
        ProjectileEntity projectileRule = newProjectile.GetComponent<ProjectileEntity>();
        m_activeProjectiles.Add(projectileRule);
        projectileRule.SetOnEndProjectileCallback(OnProjectileEnded);
        return newProjectile.GetComponent<ProjectileEntity>();
    }

    public void OnProjectileEnded(ProjectileEntity aProjectile)
    {
        aProjectile.gameObject.SetActive(false);
        m_activeProjectiles.Remove(aProjectile);
        m_inactiveProjectiles.Add(aProjectile);
    }

    public int GetNumActive()
    {
        return m_activeProjectiles.Count;
    }

    public int GetNumInactive()
    {
        return m_inactiveProjectiles.Count;
    }
}

public class ProjectileEntity : GameEntity
{
    public static float cRequireManualCollisionsThreshold = 32.5f;

    public LayerMask m_hitables;
    public int m_damage = 1;
    public float m_maxTimeAliveS = 10.0f;
    public bool m_oneHitPerTarget = true;
    public bool m_endOutsideCamera = false;
    public bool m_endOnlyOnNonLethalHit = false;
    public bool m_faceVelocityDirection = false;

    [Header("Gravity")]
    public bool m_affectedByGravity = false;
    public float m_gravity = 9f;
    public float m_terminalVelocity = 40f;

    public GameObject m_onHitFX;

    protected GameEntity m_owner;
    protected Action<ProjectileEntity> m_onProjectileEndedCallback;
    protected float m_maxDistanceFromOwner = 15.0f;
    protected float m_currentTimeAliveS;
    protected List<GameObject> m_hitGameObjects;


    protected override void EntityAwake()
    {
        base.EntityAwake();
        m_shouldUpdateFacing = false;
        m_currentTimeAliveS = 0.0f;
        UpdateVelocity(Vector2.zero);
        m_owner = null;
        m_onProjectileEndedCallback = null;
        m_hitGameObjects = new List<GameObject>();
    }

    protected override void EntityUpdate()
    {
        m_currentTimeAliveS += Time.deltaTime;
        if(ShouldDestroyProjectile())
        {
            EndProjectile();
        }
    }

    protected override void EntityFixedUpdate()
    {
        Vector2 velocity = GetCurrentVelocity();

        /*if(Math.Abs(velocity.x) >= cRequireManualCollisionsThreshold || Math.Abs(velocity.y) >= cRequireManualCollisionsThreshold)
        {
            Vector2 endPos = transform.position; // TODO: if more than this velocity ever impacts a projectile, we will need to update this logic
            //m_colliderController.PerformManualSweep(transform.position, endPos + (velocity * Time.deltaTime));
        }*/

        UpdateProjectileGravity();
    }

    protected override void UpdateFacing()
    {
        if(m_affectedByGravity)
        {
            return;
        }

        base.UpdateFacing();
    }

    public void SetOnEndProjectileCallback(Action<ProjectileEntity> aCallback)
    {
        m_onProjectileEndedCallback = aCallback;
    }

    public void SetOwner(GameEntity aOwner)
    {
        if(m_owner)
        {
            // TODO: probably need to do something here
        }

        m_owner = aOwner;
    }

    public void SetDistanceTether(float aDistance)
    {
        m_maxDistanceFromOwner = aDistance;
    }

    virtual public void OnProjectileHit()
    {
        EndProjectile();
    }

    /**
    * <summary> 
    * Starts the projectile from starting position along it's path using its defined velocity
    * </summary>
    * <param name="aStartingPos">position in world space were the projectile starts from</param>
    * <param name="aDirection">Used to flip x/y velocity. Data velocity should always be defined as firing top left. x,y values should be normalized</param>
    */
    virtual public void FireProjectile(Vector3 aStartingPos, Vector2 aVelocity)
    {
        SetSimulated(true);
        m_currentTimeAliveS = 0f;
        transform.position = aStartingPos;
        Vector2 velocity = Vector2.zero;
        velocity.x = aVelocity.x;
        velocity.y = aVelocity.y;
        UpdateVelocity(velocity);
        m_hitGameObjects.Clear();

        if(m_faceVelocityDirection)
        {
            float rawAngleR = Mathf.Atan2(aVelocity.y, aVelocity.x);
            float finalAngleD = Mathf.Rad2Deg * rawAngleR;
            transform.transform.rotation = Quaternion.Euler(0, 0, finalAngleD);
        }
    }

    virtual public void EndProjectile()
    {
        SetSimulated(false);
        m_onProjectileEndedCallback?.Invoke(this);
    }

    public bool ShouldDestroyProjectile()
    {
        if(m_owner)
        {
            Vector3 distance = m_owner.transform.position - transform.position;
            if(distance.magnitude > m_maxDistanceFromOwner)
            {
                return true;
            }
        }

        if(m_currentTimeAliveS > m_maxTimeAliveS)
        {
            return true;
        }

        if(m_endOutsideCamera)
        {
            if(!IsInCameraBounds())
            {
                return true;
            }
        }

        return false;
    }

    protected void UpdateProjectileGravity()
    {
        if(!m_affectedByGravity)
        {
            return;
        }

        Vector2 velocity = GetCurrentVelocity();


        float gravityToAdd = m_gravity * Time.deltaTime;

        velocity.y -= gravityToAdd;

        float terminalVelocity = m_terminalVelocity;
        if(velocity.y < -terminalVelocity)
        {
            velocity.y = -terminalVelocity;
        }

        UpdateVelocity(velocity);

        // maybe put this somewhere else?
        float rawAngleR = Mathf.Atan2(velocity.y, velocity.x);
        float finalAngleD = Mathf.Rad2Deg * rawAngleR;
        transform.transform.rotation = Quaternion.Euler(0, 0, finalAngleD);
    }

    protected bool HasHitTargetBefore(GameObject aGameObject)
    {
        foreach(GameObject alreadyHitObject in m_hitGameObjects)
        {
            if(alreadyHitObject == aGameObject)
            {
                return true;
            }
        }

        return false;
    }

    protected void PerformHitCheck(Collider2D collision)
    {
        if(!IsSimulated())
        {
            return;
        }

        GameObject collisionGameObject = collision.gameObject;
        /*if(((1 << collisionGameObject.layer) & m_hitables.value) == 0)
        {
            return;
        }*/

        bool hasHitBefore = HasHitTargetBefore(collisionGameObject);

        if(hasHitBefore)
        {
            if(m_oneHitPerTarget)
            {
                return;
            }
        }
        else
        {
            m_hitGameObjects.Add(collisionGameObject);
        }
        
        bool shouldEndProjectile = true;
        GameEntity damageableUnit = collisionGameObject.GetComponent<GameEntity>();
        if(damageableUnit != null)
        {
            bool wasSimulated = damageableUnit.IsSimulated();
            damageableUnit.OnTakeDamage(m_damage);

            if(m_endOnlyOnNonLethalHit && wasSimulated)
            {
                shouldEndProjectile = damageableUnit.IsSimulated();
            }
        }
        
        if(m_onHitFX)
        {
            GameFXManager.Instance.RequestPlayFX(m_onHitFX, transform.position, null);
        }

        if(shouldEndProjectile)
        {
            EndProjectile();
        }
        EventManager.Instance.TriggerEvent(Events.EVENT_PROJECTILE_HIT_ENTITY, new ProjectileHitEntityEvent(gameObject, collisionGameObject));
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        PerformHitCheck(collision);
    }

    protected void OnTriggerExit2D(Collider2D collision)
    {
        
    }

    protected void OnTriggerStay2D(Collider2D collision)
    {
        PerformHitCheck(collision);
    }
}
