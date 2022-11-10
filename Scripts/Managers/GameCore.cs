using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class GameCore : BaseManager<GameCore>
{
    private string m_levelToLoad;
    bool m_waitingForLevelToLoad;
    bool m_restartingLevel;
    string m_restartingActiveCameraName = "";

    private void Awake()
    {
        m_waitingForLevelToLoad = false;
        m_restartingLevel = false;

        if(m_instance)
        {
            //Debug.LogError("[GameCore] Already had a GameCore object, need to delete this one.");
            Destroy(gameObject);
            return;
        }

        if(!DoesInstanceExist())
        {
            Debug.LogError("[Manager] GameCore does not exist when we asked for it!");
        }

        DontDestroyOnLoad(transform.gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;

        // make sure all singleton managers are created in the beginning
        if(!EventManager.DoesInstanceExist())
        {
            Debug.LogError("[Manager] EventManager does not exist when we asked for it!");
        }

        if (!InputManager.DoesInstanceExist())
        {
            Debug.LogError("[Manager] InputManager does not exist when we asked for it!");
        }

        if(GameFXManager.DoesInstanceExist())
        {
            GameFXManager.Instance.Init();
        }
        else
        {
            Debug.LogError("[Manager] GameFXManager does not exist when we asked for it!");
        }

        if (GameplayManager.DoesInstanceExist())
        {
            GameplayManager.Instance.Init();
        }
        else
        {
            Debug.LogError("[Manager] GameplayManager does not exist when we asked for it!");
        }

        if (CameraManager.DoesInstanceExist())
        {
            CameraManager.Instance.Init();
        }
        else
        {
            Debug.LogError("[Manager] CameraManager does not exist when we asked for it!");
        }
    }

    public void OnCinemachineCameraCut(CinemachineBrain aBrain)
    {
        CameraManager.Instance.OnCinemachineCameraCut();
    }

    public void OnCinemachineCameraActivated(ICinemachineCamera newCamera, ICinemachineCamera oldCamera)
    {
        CameraManager.Instance.OnCinemachineCameraActivated(newCamera, oldCamera);
    }

    public void SetupCinemachineHooks()
    {
        CinemachineBrain brain = CameraManager.Instance.GetMainCameraCinemachineBrain();
        if(brain)
        {
            brain.m_CameraCutEvent.AddListener(OnCinemachineCameraCut);
            brain.m_CameraActivatedEvent.AddListener(OnCinemachineCameraActivated);
        }
    }

    protected void OnSceneLoaded(Scene aScene, LoadSceneMode aSceneMode)
    {
        if(m_levelToLoad == null || m_levelToLoad.Contains(aScene.name))
        {
            m_waitingForLevelToLoad = false;
            SetupCinemachineHooks();
            if(m_restartingLevel)
            {
                GameplayManager.Instance.RestartAtCheckpoint();
                if(m_restartingActiveCameraName != "")
                {
                    GameObject camera = GameObject.Find(m_restartingActiveCameraName);
                    CameraManager.Instance.ActivateCamera(camera.GetComponent<ICinemachineCamera>());
                    m_restartingActiveCameraName = "";
                }
            }
            m_restartingLevel = false;
            GameplayManager.Instance.PlayerStartSequence();
        }
    }

    public void RestartLevel()
    {
        if(m_waitingForLevelToLoad)
        {
            return;
        }

        m_restartingLevel = true;
        m_restartingActiveCameraName = CameraManager.Instance.GetActiveCinemachineCamera().VirtualCameraGameObject.name;
        LoadLevel(SceneManager.GetActiveScene().name);
    }

    public void LoadLevel(string levelName)
    {
        if(m_waitingForLevelToLoad)
        {
            return;
        }

        EventManager.Instance.OnRestartLevel();
        InputManager.Instance.OnRestartLevel();
        GameFXManager.Instance.OnRestartLevel();
        GameplayManager.Instance.OnRestartLevel();
        CameraManager.Instance.OnRestartLevel();

        Physics2D.IgnoreLayerCollision(GameplayLayers.Player, GameplayLayers.Enemy, false);
        
        m_levelToLoad = levelName;
        m_waitingForLevelToLoad = true;
        SceneManager.LoadScene(levelName);
    }
}
