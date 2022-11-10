using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField]
    private int m_id = 0;

    private void Awake()
    {
        GameplayManager.Instance.RegisterCheckpoint(this);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == GameplayTags.PLAYER)
        {
            GameplayManager.Instance.MarkNewCheckpoint(m_id);
        }
    }

    public int GetCheckpointId()
    {
        return m_id;
    }
}
