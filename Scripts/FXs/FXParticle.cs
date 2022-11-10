using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FXParticle : FXLogic
{
    ParticleSystem m_particleSystem;
    bool endedEarly;

    protected override void FXAwake()
    {
        base.FXAwake();
        if(m_particleSystem == null)
        {
            m_particleSystem = GetComponent<ParticleSystem>();
        }
    }

    public override void FXStart()
    {
        base.FXStart();
        endedEarly = false;
        m_particleSystem.Play();
    }

    public override void FXStop()
    {
        base.FXStop();
        m_particleSystem.Stop();
    }

    public override void RequestEnd()
    {
        FXStop();
        endedEarly = true;
    }

    public override bool ShouldEnd()
    {
        return endedEarly || (m_particleSystem.time >= m_particleSystem.main.duration && m_particleSystem.particleCount == 0);
    }

    protected override void OnStartPause()
    {
        base.OnStartPause();
        m_particleSystem.Pause();
    }

    protected override void OnEndPause()
    {
        base.OnEndPause();
        m_particleSystem.Play();
    }
}
