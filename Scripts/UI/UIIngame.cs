using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIIngame : MonoBehaviour
{
    [SerializeField]
    private UIControl m_healthBar = null;
    [SerializeField]
    private UIControl m_fader = null;
    [SerializeField]
    private UIControl m_readyText = null;

    private CountdownTimer m_fadeOutReadyTextTimer;

    void Awake()
    {
        GameplayManager.Instance.RegisterIngameUI(this);
        m_readyText.SetShowing(false);
        m_fadeOutReadyTextTimer = new CountdownTimer();
    }

    private void Update()
    {
        if(m_fadeOutReadyTextTimer.IsActive())
        {
            m_fadeOutReadyTextTimer.Update(Time.deltaTime);
            m_readyText.SetAlpha(1f - m_fadeOutReadyTextTimer.GetPercentComplete());
        }
    }

    public void SetReadyTextShowing(bool aEnabled)
    {
        m_readyText.SetShowing(aEnabled);
    }

    public void FadeOutReadyText(float aTime)
    {
        m_fadeOutReadyTextTimer.Start(aTime);
    }

    public UIControl GetFader()
    {
        return m_fader;
    }

    public UIControl GetHealthBar()
    {
        return m_healthBar;
    }
}
