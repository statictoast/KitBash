
public class CountdownTimer
{
    public delegate void CountdownTimerFinishCallback();

    private float duration;
    private float current;
    private bool active;
    private CountdownTimerFinishCallback finishedCallback;

    public CountdownTimer()
    {
        active = false;
    }

    public void SetDuration(float aDuration)
    {
        duration = aDuration;
    }

    public void SetFinishedCallback(CountdownTimerFinishCallback aCallback)
    {
        finishedCallback = aCallback;
    }

    public COUNTDOWN_TIMER_STATE GetState()
    {
        if(active)
        {
            return COUNTDOWN_TIMER_STATE.ACTIVE;
        }

        return COUNTDOWN_TIMER_STATE.DONE;
    }

    public bool IsDone()
    {
        return GetState() == COUNTDOWN_TIMER_STATE.DONE;
    }
    

    public bool IsActive()
    {
        return GetState() == COUNTDOWN_TIMER_STATE.ACTIVE;
    }

    public float GetPercentComplete()
    {
        return (duration - current) / duration;
    }

    public void Start(float aDuration)
    {
        duration = aDuration;
        current = aDuration;
        active = true;
    }

    public void Update(float aDeltaTime)
    {
        if(!active)
            return;

        current -= aDeltaTime;
        if(current <= 0f)
        {
            End();
        }
    }

    public void Stop(bool aEndCallback = false)
    {
        if(!active)
            return;

        active = false;
        current = 0f;

        if(aEndCallback)
        {
            finishedCallback?.Invoke();
        }
    }

    private void End()
    {
        if(!active)
            return;

        current = 0f;
        active = false;
        finishedCallback?.Invoke();
    }
}