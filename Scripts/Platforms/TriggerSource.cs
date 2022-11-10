using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerSource : GameEntity
{
    public TriggerableEntity m_triggerTarget;
    public TRIGGER_SOURCE_TYPE m_triggerType = TRIGGER_SOURCE_TYPE.PLAYER_HIT;

    public override void OnTakeDamage(int aDamage)
    {
        if(m_triggerType == TRIGGER_SOURCE_TYPE.PLAYER_HIT)
        {
            TriggerFire();
        }
    }

    private void TriggerFire()
    {
        m_triggerTarget?.OnTriggerHit();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(m_triggerType == TRIGGER_SOURCE_TYPE.TRIGGER_VOLUME)
        {
            TriggerFire();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(m_triggerType == TRIGGER_SOURCE_TYPE.TRIGGER_VOLUME)
        {
            TriggerFire();
        }
    }
}
