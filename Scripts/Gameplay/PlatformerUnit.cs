using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformerUnit : GameEntity
{
    private static float cRayDistance = 0.08f;

    public LayerMask m_platformMask = 0;
    public LayerMask m_enemiesMask = 0;
    public float m_boundsTightening = 0.8f;
    private RaycastHit2D m_collisionRayCast; // used multiple times in the same function so we don't have to keep allocating memory for it
    private float m_previousGroundingDistance = 0f;

    public class UnitCollisionState
    {
        public bool right;
        public bool left;
        public bool above;
        public bool below;
        public RaycastHit2D currentPlatform;
        public RaycastHit2D currentWall;
        public bool becameGroundedThisFrame;
        public bool becameGroundedThisFixedFrame;
        public bool wasGroundedLastFrame;
        public bool wasGroundedLastFixedFrame;
        public bool currentPlatformChanged;

        public UnitCollisionState()
        {
            Reset();
            FixedReset();
        }

        public UnitCollisionState(UnitCollisionState aOther)
        {
            right = aOther.right;
            left = aOther.left;
            above = aOther.above;
            below = aOther.below;
            becameGroundedThisFrame = aOther.becameGroundedThisFrame;
            becameGroundedThisFixedFrame = aOther.becameGroundedThisFixedFrame;
            wasGroundedLastFrame = aOther.wasGroundedLastFrame;
            wasGroundedLastFixedFrame = aOther.wasGroundedLastFixedFrame;
            currentPlatform = aOther.currentPlatform;
            currentWall = aOther.currentWall;
            currentPlatformChanged = aOther.currentPlatformChanged;
        }

        public bool HasCollision()
        {
            return below || right || left || above;
        }

        public void FixedReset()
        {
            right = left = above = below = becameGroundedThisFixedFrame = wasGroundedLastFixedFrame = currentPlatformChanged = false;
        }

        public void Reset()
        {
            becameGroundedThisFrame = wasGroundedLastFrame = false;
        }

        public override string ToString()
        {
            return string.Format("[UnitCollisionState] r: {0}, l: {1}, a: {2}, b: {3}, wasGroundedLastFrame: {4}, becameGroundedThisFrame: {5}",
                                 right, left, above, below, wasGroundedLastFixedFrame, becameGroundedThisFixedFrame);
        }
    }

    protected UnitCollisionState m_collisionState;

    private float m_minGravity;
    private float m_maxGravity;

    protected override void EntityStart()
    {
        m_collisionState = new UnitCollisionState();
    }

    protected override void EntityUpdate()
    {
        m_animator.SetFloat("movementSpeedX", Mathf.Abs(GetCurrentVelocity().x));
        m_animator.SetFloat("movementSpeedY", Mathf.Abs(GetCurrentVelocity().y));
        m_animator.SetBool("isGrounded", m_collisionState.below);
    }

    protected override void EntityFixedUpdate()
    {
        UpdateCollisionState();
    }

    protected void UpdateCollisionState()
    {
        UnitCollisionState previousState = new UnitCollisionState(m_collisionState);
        m_collisionState.FixedReset();
        m_collisionState.wasGroundedLastFixedFrame = previousState.below;
        if(m_collisionState.wasGroundedLastFixedFrame)
        {
            m_collisionState.wasGroundedLastFrame = true;
        }

        Bounds playerBounds = GetEntityCollider().bounds;

        Vector2 currentVelocity = GetCurrentVelocity();
        // Horizontal Check
        bool isGoingRight = GetFacing() > 0f;
        Vector2 horizontalRayDirection = isGoingRight ? Vector2.right : -Vector2.right;
        bool foundHorizontalHit = false;
        float boundsXSide = isGoingRight ? playerBounds.max.x : playerBounds.min.x;

        Vector2 position = new Vector2(boundsXSide, playerBounds.max.y);
        foundHorizontalHit = DoCollisionCheck(position, horizontalRayDirection, cRayDistance) || foundHorizontalHit;

        position = new Vector2(boundsXSide, playerBounds.min.y);
        foundHorizontalHit = DoCollisionCheck(position, horizontalRayDirection, cRayDistance) || foundHorizontalHit;

        if(isGoingRight)
        {
            m_collisionState.right = true;
        }
        else
        {
            m_collisionState.left = true;
        }

        // TODO: do we just want to box cast this?
        // "Feet" area
        position = new Vector2(boundsXSide, playerBounds.min.y + (playerBounds.extents.y / 2f));
        Debug.DrawRay(position, horizontalRayDirection * cRayDistance * 2, Color.white);
        m_collisionState.currentWall = Physics2D.Raycast(position, horizontalRayDirection, cRayDistance, m_platformMask);
        if(m_collisionState.currentWall.collider == null)
        {
            // "Head" area
            position = new Vector2(boundsXSide, playerBounds.max.y - (playerBounds.extents.y / 2f));
            Debug.DrawRay(position, horizontalRayDirection * cRayDistance * 2, Color.white);
            m_collisionState.currentWall = Physics2D.Raycast(position, horizontalRayDirection, cRayDistance, m_platformMask);
            if(m_collisionState.currentWall.collider == null)
            {
                // "Chest" area
                position = new Vector2(boundsXSide, playerBounds.min.y + playerBounds.extents.y);
                Debug.DrawRay(position, horizontalRayDirection * cRayDistance * 2, Color.white);
                m_collisionState.currentWall = Physics2D.Raycast(position, horizontalRayDirection, cRayDistance, m_platformMask);
            }
        }

        // Vertical Check - Up
        bool foundVerticalHit = false;
        Vector2 verticalRayDirection = Vector2.up;
        float boundsYSide = playerBounds.max.y;

        position = new Vector2(playerBounds.min.x, boundsYSide);
        foundVerticalHit = DoCollisionCheck(position, verticalRayDirection, cRayDistance) || foundVerticalHit;

        position = new Vector2(playerBounds.max.x, boundsYSide);
        foundVerticalHit = DoCollisionCheck(position, verticalRayDirection, cRayDistance) || foundVerticalHit;

        if(foundVerticalHit)
        {
            m_collisionState.above = true;
        }

        // Vertical Check - Down
        foundVerticalHit = false;
        verticalRayDirection = -Vector2.up;
        boundsYSide = playerBounds.min.y;

        position = new Vector2(playerBounds.min.x, boundsYSide);
        foundVerticalHit = DoCollisionCheck(position, verticalRayDirection, cRayDistance);

        position = new Vector2(playerBounds.min.x + playerBounds.extents.x, boundsYSide);
        foundVerticalHit = foundVerticalHit || DoCollisionCheck(position, verticalRayDirection, cRayDistance);

        position = new Vector2(playerBounds.max.x, boundsYSide);
        foundVerticalHit = foundVerticalHit || DoCollisionCheck(position, verticalRayDirection, cRayDistance);

        if(foundVerticalHit)
        {
            m_collisionState.below = true;
        }

        if(!m_collisionState.wasGroundedLastFixedFrame && m_collisionState.below)
        {
            m_collisionState.becameGroundedThisFixedFrame = true;
            m_collisionState.becameGroundedThisFrame = true;
        }

        // Platform checking
        Vector2 unitSize = GetSize();
        float groundCheckLength = unitSize.y / 2.0f;
        Vector2 unitGroundCenter = GetGroundCenter();
        unitGroundCenter.y += 0.1f;
        DrawRay(unitGroundCenter, Vector2.down * groundCheckLength, Color.white);
        m_collisionState.currentPlatform = Physics2D.Raycast(unitGroundCenter, Vector2.down, groundCheckLength, m_platformMask);

        // Back Foot
        if(m_collisionState.currentPlatform.collider == null)
        {
            unitGroundCenter = GetGroundCenterBack();
            unitGroundCenter.y += 0.1f;
            DrawRay(unitGroundCenter, Vector2.down * groundCheckLength, Color.white);
            m_collisionRayCast = Physics2D.Raycast(unitGroundCenter, Vector2.down, groundCheckLength, m_platformMask);
            m_collisionState.currentPlatform = m_collisionRayCast;

            // Front Foot
            if(m_collisionState.currentPlatform.collider == null)
            {
                unitGroundCenter = GetGroundCenterForward();
                unitGroundCenter.y += 0.1f;
                DrawRay(unitGroundCenter, Vector2.down * groundCheckLength, Color.white);
                m_collisionRayCast = Physics2D.Raycast(unitGroundCenter, Vector2.down, groundCheckLength, m_platformMask);
                m_collisionState.currentPlatform = m_collisionRayCast;
            }
        }

        if(previousState.currentPlatform.collider != m_collisionState.currentPlatform.collider && m_collisionState.currentPlatform.collider != null)
        {
            m_collisionState.currentPlatformChanged = true;
            m_previousGroundingDistance = float.MaxValue;
        }
    }

    private bool DoCollisionCheck(Vector2 aPosition, Vector2 aDirection, float aDistance)
    {
        m_collisionRayCast = Physics2D.Raycast(aPosition, aDirection, aDistance, m_platformMask);
        DrawRay(aPosition, aDirection * aDistance, Color.red);
        return m_collisionRayCast.collider != null;
    }

    public bool GroundEntity()
    {
        if(!IsGrounded())
        {
            return false;
        }

        RaycastHit2D ground = GetCurrentPlatform();
        if(ground.collider == null)
        {
            Debug.Break();
            return false;
        }
        //testpos = ground.point;
        float deltaGroundingDistance = m_previousGroundingDistance == float.MaxValue ? 100f : ground.distance - m_previousGroundingDistance;
        if(Mathf.Abs(deltaGroundingDistance) <= 0.0001f)
        {
            return true;
        }

        Platform currentPlatform = ground.collider.gameObject.GetComponent<Platform>();
        float groundAngle = Vector2.Angle(ground.normal, Vector2.up);

        Vector2 currentPosition = new Vector2(transform.position.x, transform.position.y);
        Vector2 playerFeetWorld = GetGroundCenter();
        Vector2 groundWorld = ground.point;
        Vector3 distance = playerFeetWorld - groundWorld;
        distance.x = 0f;
        Vector2 finalPos = transform.position - distance;
        m_rigidBody2D.MovePosition(finalPos);

        m_previousGroundingDistance = ground.distance;
        currentPlatform.OnGroundEntity(this);

        return true;
    }

    public void PlaceOnGround()
    {
        Vector2 unitGroundCenter = GetGroundCenter();
        RaycastHit2D ground = Physics2D.Raycast(unitGroundCenter, Vector2.down, 10f, m_platformMask);
        if(ground.collider != null)
        {
            Platform platform = ground.collider.gameObject.GetComponent<Platform>();
            if(platform != null)
            {
                Vector2 currentPosition = new Vector2(transform.position.x, transform.position.y);
                Vector2 groundWorld = ground.point;
                Vector3 distance = currentPosition - groundWorld - GetHalfSize();
                distance.x = 0f;
                Vector2 finalPos = transform.position - distance;
                transform.position = finalPos;
                platform.OnGroundEntity(this);
            }
        }
    }

    public bool IsTopping()
    {
        return m_collisionState.above;
    }

    public bool IsGrounded()
    {
        return m_collisionState.below;
    }

    public bool BecameGroundedThisUpdate()
    {
        return m_collisionState.becameGroundedThisFrame;
    }

    public bool BecameGroundedThisFixedUpdate()
    {
        return m_collisionState.becameGroundedThisFixedFrame;
    }

    public bool WasGroundedLastUpdate()
    {
        return m_collisionState.wasGroundedLastFrame;
    }

    public bool WasGroundedLastFixedUpdate()
    {
        return m_collisionState.wasGroundedLastFixedFrame;
    }

    public RaycastHit2D GetCurrentPlatform()
    {
        return m_collisionState.currentPlatform;
    }

    public RaycastHit2D GetCurrentWall()
    {
        return m_collisionState.currentWall;
    }

#region Debug
    void DrawRay(Vector3 start, Vector3 dir, Color color)
    {
        Debug.DrawRay(start, dir, color);
    }

    private Vector3 testpos = Vector3.zero;
    private void OnDrawGizmos()
    {
        //Gizmos.DrawSphere(testpos, 0.01f);
    }
    #endregion
}
