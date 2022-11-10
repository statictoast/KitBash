using System;
using System.Collections.Generic;
using UnityEngine;

public class PickupPool
{
    private GameObject m_prefab;
    private List<GameEntity> m_inactives;

    public PickupPool(GameObject aPrefab)
    {
        m_prefab = aPrefab;
        m_inactives = new List<GameEntity>();
    }

    public GameEntity AcquireObject()
    {
        if(m_inactives.Count > 0)
        {
            GameEntity oldEntity = m_inactives[0];
            m_inactives.RemoveAt(0);
            oldEntity.gameObject.SetActive(true);
            return oldEntity;
        }

        GameObject newObject = GameObject.Instantiate(m_prefab, Vector2.zero, Quaternion.identity);
        Pickup pickup = newObject.GetComponent<Pickup>();
        pickup.SetPickupEndedCallback(OnPickupEnded);
        return newObject.GetComponent<GameEntity>();
    }

    public void OnPickupEnded(Pickup aPickup)
    {
        aPickup.gameObject.SetActive(false);
        m_inactives.Add(aPickup);
    }
}


public class Pickup : GameEntity
{
    public PICKUP_TYPE m_type = PICKUP_TYPE.HEALTH;
    public int m_amount;

    protected Action<Pickup> m_onPickupEndedCallback;

    protected override void EntityUpdate()
    {
        base.EntityUpdate();
        if(!IsInCameraBounds())
        {
            EndPickup();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == GameplayTags.PLAYER)
        {
            GrantPickup();
        }
    }

    private void GrantPickup()
    {
        PlayerEntity player = GameplayManager.Instance.GetPlayerEntity();
        switch(m_type)
        {
            case PICKUP_TYPE.HEALTH:
            {
                player.ChangeHealth(m_amount);
            }
                break;
            case PICKUP_TYPE.WEAPON_ENERGY:
            {
                // TODO
            }
                break;
        }

        EndPickup();
    }

    private void EndPickup()
    {
        SetSimulated(false);
        m_onPickupEndedCallback?.Invoke(this);
    }

    public void SetPickupEndedCallback(Action<Pickup> aCallback)
    {
        m_onPickupEndedCallback = aCallback;
    }
}
