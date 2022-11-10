using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UIFader : UIControl
{
    [SerializeField]
    private Animator m_animator = null;
    private Action m_finishedCallback;

    private void Awake()
    {
        m_finishedCallback = null;
        EventManager.Instance.RegisterEvent(Events.EVENT_PLAYER_FINISHED_DYING, "fader", OnPlayerDied);
        EventManager.Instance.RegisterEvent(Events.EVENT_START_FADE_OUT, "fader", OnStartFadeOut);
        EventManager.Instance.RegisterEvent(Events.EVENT_START_FADE_IN, "fader", OnStartFadeIn);
    }

    public void StartFadeIn()
    {
        m_animator.SetTrigger("fadeIn");
    }

    private void OnFadeInComplete()
    {
        EventManager.Instance.TriggerEvent(Events.EVENT_FADE_IN_COMPLETE);
        m_finishedCallback?.Invoke();
        m_finishedCallback = null;
    }

    public void StartFadeOut()
    {
        m_animator.SetTrigger("fadeOut");
    }

    private void OnFadeOutComplete()
    {
        EventManager.Instance.TriggerEvent(Events.EVENT_FADE_OUT_COMPLETE);
        m_finishedCallback?.Invoke();
        m_finishedCallback = null;
    }

    private void OnPlayerDied(CallbackEvent aEvent)
    {
        PlayerFinishedDyingEvent newEvent = aEvent as PlayerFinishedDyingEvent;
        m_finishedCallback = newEvent.callback;
        StartFadeIn();
    }

    private void OnStartFadeOut(CallbackEvent aEvent)
    {
        StartFadeOutEvent newEvent = aEvent as StartFadeOutEvent;
        m_finishedCallback = newEvent.callback;
        StartFadeOut();
    }

    private void OnStartFadeIn(CallbackEvent aEvent)
    {
        StartFadeInEvent newEvent = aEvent as StartFadeInEvent;
        m_finishedCallback = newEvent.callback;
        StartFadeIn();
    }
}
