using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrumblePlatform : Platform
{
    public float m_durationToCrumbleS = 1f;
    public float m_respawnTimeS = 5f;
    public float m_spirteShakePower = 0.05f;
    [SerializeField]
    protected GameObject m_leftSprite;
    [SerializeField]
    protected GameObject m_rightSprite;
    [SerializeField]
    protected GameObject m_crumbleParticleFXPrefab;
    protected CountdownTimer m_crumbleTimer;
    protected CountdownTimer m_respawnTimer;
    protected int m_shakeDirection = 1;

    protected override void EntityAwake()
    {
        base.EntityAwake();
        m_crumbleTimer = new CountdownTimer();
        m_crumbleTimer.SetFinishedCallback(OnCrumbleTimerComplete);
        m_respawnTimer = new CountdownTimer();
        m_respawnTimer.SetFinishedCallback(OnRepsawnTimerComplete);
    }

    protected override void EntityUpdate()
    {
        base.EntityUpdate();

        if(m_crumbleTimer.IsActive())
        {
            m_crumbleTimer.Update(Time.deltaTime);
            Vector3 position = m_leftSprite.transform.position;
            position.x += m_spirteShakePower * m_shakeDirection;
            m_leftSprite.transform.position = position;
            position = m_rightSprite.transform.position;
            position.x += m_spirteShakePower * m_shakeDirection;
            m_rightSprite.transform.position = position;
            m_shakeDirection *= -1;
        }
    }

    protected override void CheckForRestartSimulation()
    {
        base.CheckForRestartSimulation();
        m_respawnTimer.Update(Time.deltaTime);
    }

    public override void OnGroundEntity(GameEntity aGrounder)
    {
        base.OnGroundEntity(aGrounder);
        if(aGrounder.tag == GameplayTags.PLAYER && !m_crumbleTimer.IsActive())
        {
            m_crumbleTimer.Start(m_durationToCrumbleS);
            GameFXManager.Instance.RequestPlayFX(m_crumbleParticleFXPrefab, transform.position, gameObject);
        }
    }

    protected override void OnEndSimulation()
    {
        base.OnEndSimulation();
        m_leftSprite.GetComponent<SpriteRenderer>().enabled = false;
        m_rightSprite.GetComponent<SpriteRenderer>().enabled = false;
    }

    protected override void OnStartSimulation()
    {
        base.OnStartSimulation();
        m_leftSprite.GetComponent<SpriteRenderer>().enabled = true;
        m_rightSprite.GetComponent<SpriteRenderer>().enabled = true;
    }

    private void OnCrumbleTimerComplete()
    {
        SetSimulated(false);
        m_respawnTimer.Start(m_respawnTimeS);
    }

    private void OnRepsawnTimerComplete()
    {
        SetSimulated(true);
    }
}
