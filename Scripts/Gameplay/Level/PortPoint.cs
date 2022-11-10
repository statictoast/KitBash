using Cinemachine;
using UnityEngine;

public class PortPoint : MonoBehaviour
{
    [SerializeField]
    private int m_checkpointPointWarpNum = 0;
    [SerializeField]
    private CinemachineVirtualCamera m_camera = null;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == GameplayTags.PLAYER)
        {
            Vector2 pos = GameplayManager.Instance.GetCheckpointPosition(m_checkpointPointWarpNum);
            GameplayManager.Instance.TeleportPlayer(pos, m_camera);
        }
    }
}
