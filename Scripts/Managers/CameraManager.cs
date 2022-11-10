using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : BaseManager<CameraManager>
{
    private ICinemachineCamera m_activeCamera = null;
    private ICinemachineCamera m_requestedCamera = null;

    public override void Init()
    {
        
    }

    public override void OnRestartLevel()
    {
        m_activeCamera = null;
        m_requestedCamera = null;
    }

    private void Update()
    {
        if(m_requestedCamera != null && m_activeCamera != null)
        {
            ActivateCamera(m_requestedCamera);
            m_requestedCamera = null;
        }
    }

    public CinemachineBrain GetMainCameraCinemachineBrain()
    {
        return Camera.main.GetComponent<CinemachineBrain>();
    }

    public ICinemachineCamera GetActiveCinemachineCamera()
    {
        return m_activeCamera;
    }

    public void CenterCameraOnPlayer()
    {
        PlayerEntity player = GameplayManager.Instance.GetPlayerEntity();
        if(!player)
            return;

        Vector2 playerPos = player.transform.position;

        ICinemachineCamera camera = GetActiveCinemachineCamera();
        Vector2 cameraPos = camera.State.FinalPosition;
        CinemachineVirtualCamera vCam = camera as CinemachineVirtualCamera;
        if(vCam)
        {
            Vector2 trackedObjectOffset = vCam.GetCinemachineComponent<CinemachineFramingTransposer>().m_TrackedObjectOffset;
            cameraPos -= trackedObjectOffset;
        }

        camera.OnTargetObjectWarped(player.transform, playerPos - cameraPos);
    }

    public void ActivateCamera(ICinemachineCamera aNewCamera)
    {
        ICinemachineCamera currentCamera = GetActiveCinemachineCamera();
        if(currentCamera == null)
        {
            m_requestedCamera = aNewCamera;
            return;
        }

        if(currentCamera == aNewCamera)
        {
            return;
        }

        currentCamera.Priority = 10;
        aNewCamera.Priority = 100;
    }

    public void OnCinemachineCameraCut()
    {
        
    }

    public void OnCinemachineCameraActivated(ICinemachineCamera newCamera, ICinemachineCamera oldCamera)
    {
        m_activeCamera = newCamera;
        Debug.Log(m_activeCamera);
        CenterCameraOnPlayer();
        EventManager.Instance.TriggerEvent(Events.EVENT_ACTIVE_CAMERA_CHANGED, new ActiveCameraChangedEvent(newCamera));
    }
}
