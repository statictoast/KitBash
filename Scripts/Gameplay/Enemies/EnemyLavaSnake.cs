using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLavaSnake : EnemyEntity
{
    [SerializeField]
    private float m_submergeDistance = 1f;
    [SerializeField]
    private float m_emergeTimeS = 1f;
    [SerializeField]
    private GameObject m_submergedParticleFX = null;
    private bool m_hasEmerged;
    private CountdownTimer m_emergeTimer;
    private Vector3 m_emergedLocation;
    private GameObject m_currentSubmergeParticleFX;

    protected override void SetupEnemy()
    {
        base.SetupEnemy();
        m_hasEmerged = false;
        m_emergedLocation = m_startingLocation;
        m_emergedLocation.y += m_submergeDistance;
        m_emergeTimer = new CountdownTimer();
        if(m_currentSubmergeParticleFX == null)
        {
            m_currentSubmergeParticleFX = GameFXManager.Instance.RequestPlayFX(m_submergedParticleFX, transform.position, gameObject);
        }
    }

    protected override void EntityUpdate()
    {
        base.EntityUpdate();
        if(m_emergeTimer.IsActive())
        {
            m_emergeTimer.Update(Time.deltaTime);
            Vector3 nextPos = Vector3.Lerp(m_startingLocation, m_emergedLocation, m_emergeTimer.GetPercentComplete());
            SetPosition(nextPos);
        }
    }

    protected override void OnStartSimulation()
    {
        base.OnStartSimulation();
        if(m_currentSubmergeParticleFX == null)
        {
            m_currentSubmergeParticleFX = GameFXManager.Instance.RequestPlayFX(m_submergedParticleFX, transform.position, gameObject);
        }
    }

    protected override void OnEndSimulation()
    {
        base.OnEndSimulation();
        m_hasEmerged = false;

        if(m_currentSubmergeParticleFX)
        {
            m_currentSubmergeParticleFX.GetComponent<FXParticle>().RequestEnd();
            m_currentSubmergeParticleFX = null;
        }
    }

    protected override void StartAttack()
    {
        base.StartAttack();
        if(!m_hasEmerged)
        {
            m_hasEmerged = true;
            m_emergeTimer.Start(m_emergeTimeS);
        }
    }
}
