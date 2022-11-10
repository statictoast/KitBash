using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagingPlatform : Platform
{
    public int m_contactDamage = 2;

    public override void OnGroundEntity(GameEntity aGrounder)
    {
        aGrounder.OnTakeDamage(m_contactDamage);
    }

    protected override void ProcessOnCollisionEnter(Collision2D collision)
    {
        base.ProcessOnCollisionEnter(collision);
        if(IsPlayerTouching())
        {
            GameEntity gameEntity = collision.gameObject.GetComponent<GameEntity>();
            DamageTouchingEntity(gameEntity);
        }
    }

    protected override void ProcessOnCollisionStay(Collision2D collision)
    {
        base.ProcessOnCollisionStay(collision);
        if(IsPlayerTouching())
        {
            DamageTouchingEntity(GameplayManager.Instance.GetPlayerEntity());
        }
    }

    protected void DamageTouchingEntity(GameEntity aToucher)
    {
        aToucher.OnTakeDamage(m_contactDamage);
    }
}
