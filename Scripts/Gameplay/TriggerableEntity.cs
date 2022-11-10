using UnityEngine;

public class TriggerableEntity : GameEntity, ITriggerTarget
{
    virtual public void OnTriggerHit()
    {
        // must be overridden in derived classes
        Debug.Assert(false);
    }
}
