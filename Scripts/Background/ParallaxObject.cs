using Cinemachine;
using UnityEngine;

public class ParallaxObject : MonoBehaviour
{
    [SerializeField]
    protected Vector2 m_parallaxMagnitudes = Vector2.zero;
    [SerializeField]
    protected CinemachineVirtualCamera m_targetCamera;
    protected Vector3 m_previousPosition;
    protected bool m_active;

    void Awake()
    {
        m_active = false;
        m_previousPosition = Camera.main.transform.position;
        EventManager.Instance.RegisterEvent(Events.EVENT_ACTIVE_CAMERA_CHANGED, gameObject.name, OnCameraChanged);
    }

    private void Update()
    {
        if(m_targetCamera == null || !m_active)
        {
            return;
        }

        Vector3 newPos = Camera.main.transform.position;
        Vector3 movementDelta = newPos - m_previousPosition;
        UpdateParallax(movementDelta);
        m_previousPosition = newPos;
    }

    private void FixedUpdate()
    {
        
    }

    private void LateUpdate()
    {
        
    }

    public void UpdateParallax(Vector3 aDelta)
    {
        Vector3 newPosition = transform.position + (Vector3)(aDelta * m_parallaxMagnitudes);
        newPosition.z = transform.position.z;
        transform.position = newPosition;
    }

    private void OnCameraChanged(CallbackEvent aEventData)
    {
        ActiveCameraChangedEvent eventData = aEventData as ActiveCameraChangedEvent;
        m_active = (CinemachineVirtualCamera)eventData.newCamera == m_targetCamera;
        if(m_active)
        {
            m_previousPosition = Camera.main.transform.position;
        }
    }
}
