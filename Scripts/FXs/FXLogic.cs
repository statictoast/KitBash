using UnityEngine;

public class FXLogic : MonoBehaviour, IPausable
{
    protected CountdownTimer m_timer;
    protected bool m_alive;
    protected string m_prefabName;
    protected Animator m_animator;
    protected bool m_paused;

    public FX_ATTACH_TYPE m_attachType = FX_ATTACH_TYPE.POINT;
    public Vector3 m_pointOffset = Vector3.zero;

    private void Awake()
    {
        FXAwake();
    }

    virtual protected void FXAwake()
    {
        if(Application.isPlaying)
        {
            GameplayManager.Instance.RegisterPausable(this);
        }
    }

    virtual public void FXStart()
    {
        m_paused = false;
        m_alive = true;
        gameObject.SetActive(true);

        if(m_animator == null)
        {
            m_animator = GetComponent<Animator>();
        }

        if(m_timer == null)
        {
            m_timer = new CountdownTimer();
        }
    }

    virtual public void FXStop()
    {
        m_alive = false;
        gameObject.SetActive(false);
    }

    virtual public void UpdateFX()
    {
        if(m_alive)
        {
            m_timer.Update(Time.deltaTime);
        }
    }

    virtual public void OnTimeAliveFinished()
    {
        FXStop();
    }

    virtual public bool ShouldEnd()
    {
        return false;
    }

    virtual public void RequestEnd()
    {
        Debug.LogWarning("[FXLogic] can't request end on base FX logic");
    }

    public void SetPrefabName(string aPrefabName)
    {
        m_prefabName = aPrefabName;
    }

    public string GetPrefabName()
    {
        return m_prefabName;
    }

    public void SendParentSignal(string aSignal)
    {
        GameEntity parent = GetComponentInParent<GameEntity>();
        if(parent)
        {
            parent.ProcessSignal(aSignal);
        }
    }

    public void SetPaused(bool aPaused)
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

    virtual protected void OnStartPause()
    {
        if(m_animator != null)
        {
            m_animator.enabled = false;
        }
    }

    virtual protected void OnEndPause()
    {
        if(m_animator != null)
        {
            m_animator.enabled = true;
        }
    }

    public bool IsPaused()
    {
        return m_paused;
    }
}
