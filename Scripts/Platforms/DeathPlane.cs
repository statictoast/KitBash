using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathPlane : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == GameplayTags.PLAYER)
        {
            other.GetComponent<PlayerEntity>().InstantlyKillPlayer();
        }
    }
}
