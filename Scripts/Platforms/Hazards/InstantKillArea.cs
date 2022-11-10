using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantKillArea : MonoBehaviour
{
    public bool m_checkInvulnerability = true;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        KillPlayer(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        KillPlayer(collider.gameObject);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        KillPlayer(collision.gameObject);
    }

    private void KillPlayer(GameObject collidedObject)
    {
        if(collidedObject.tag == GameplayTags.PLAYER)
        {
            PlayerEntity player = collidedObject.GetComponent<PlayerEntity>();
            if(m_checkInvulnerability && player.IsInvulnerable())
            {
                return;
            }

            player.InstantlyKillPlayer();
        }
    }
}
