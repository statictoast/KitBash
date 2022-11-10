using Cinemachine;
using UnityEngine;

public class ActivateCameraTrigger : MonoBehaviour, ITriggerTarget
{
    [SerializeField]
    CinemachineVirtualCamera m_camera = null;

    public void OnTriggerHit()
    {
        if(m_camera == null)
        {
            return;
        }

        CameraManager.Instance.ActivateCamera(m_camera);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == GameplayTags.PLAYER)
        {
            OnTriggerHit();
        }
    }
}
