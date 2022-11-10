using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class GameplayManager : BaseManager<GameplayManager>
{
    private GameObject m_playerObject;
    private GlobalGameDataRepository m_globalDataDataRepo;
    private Plane[] m_mainCameraFrustums;
    private List<GameEntity> m_entities;
    private List<IPausable> m_pausables; // gameobjects that are not entities but still need to be paused
    private UIIngame m_ingameUI;
    private bool m_gamePaused;

    [Header("Pickups")]
    private Dictionary<PICKUP_TYPE, PickupPool> m_pickupPools;

    [Header("Level")]
    private int m_currentCheckpointId;
    private List<Checkpoint> m_checkpoints;

    public override void Init()
    {
        m_pickupPools = new Dictionary<PICKUP_TYPE, PickupPool>();
        m_entities = new List<GameEntity>();
        m_pausables = new List<IPausable>();
        m_checkpoints = new List<Checkpoint>();
        m_currentCheckpointId = 0;

        m_globalDataDataRepo = (GlobalGameDataRepository)Resources.Load("GlobalGameDataRepository");
    }

    public override void OnRestartLevel()
    {
        m_pickupPools.Clear();
        m_entities.Clear();
        m_pausables.Clear();
        m_checkpoints.Clear();
        m_ingameUI = null;
        m_playerObject = null;
    }

    private void Update()
    {
        Camera mainCam = Camera.main;
        m_mainCameraFrustums = GeometryUtility.CalculateFrustumPlanes(mainCam);

        // Debug
        Keyboard keyboard = InputSystem.GetDevice<Keyboard>();
        if(keyboard.escapeKey.isPressed)
        {
            LevelDataRepository m_levels = (LevelDataRepository)Resources.Load("LevelDataRepository");
            GameCore.Instance.LoadLevel(m_levels.m_mainMenuPath);
            m_currentCheckpointId = 0;
        }
    }

    public void RegisterEntity(GameEntity aEntity)
    {
        m_entities.Add(aEntity);
    }

    public void RegisterPausable(IPausable aPausable)
    {
        m_pausables.Add(aPausable);
    }

    public void RegisterIngameUI(UIIngame aIngameUI)
    {
        m_ingameUI = aIngameUI;
    }

    public GameObject GetPlayerObject()
    {
        if(m_playerObject == null)
        {
            m_playerObject = FindObjectOfType<PlayerEntity>().gameObject;
        }
        return m_playerObject;
    }

    public PlayerEntity GetPlayerEntity()
    {
        if(m_playerObject == null)
        {
            PlayerEntity player = FindObjectOfType<PlayerEntity>();
            if(player)
            {
                m_playerObject = player.gameObject;
            }
            else
            {
                return null;
            }
        }
        return m_playerObject.GetComponent<PlayerEntity>();
    }

    public Plane[] GetMainCameraFrustums()
    {
        return m_mainCameraFrustums;
    }

    public GlobalGameDataRepository GetGlobalGameData()
    {
        return m_globalDataDataRepo;
    }

    public bool IsGamePaused()
    {
        return m_gamePaused;
    }

    #region Pickups

    public void DeterminePickupDrop(Vector3 aPosition)
    {
        PlayerEntity player = GetPlayerEntity();
        int currentHealth = player.GetCurrentHealth();
        int maxHealth = player.GetMaxHealth();
        float healthPercent = currentHealth / (float)maxHealth;
        float healthDropPercent = 1f - healthPercent;
        float randNum = Random.Range(0f, 1f);
        
        if(healthDropPercent >= randNum)
        {
            SpawnPickup(PICKUP_TYPE.HEALTH, aPosition);
        }
    }

    public void SpawnPickup(PICKUP_TYPE aType, Vector3 aPosition)
    {
        PickupPool pool;
        if(!m_pickupPools.TryGetValue(aType, out pool))
        {
            pool = new PickupPool(GetPickupPrefabByType(aType));
            m_pickupPools.Add(aType, pool);
        }

        GameEntity entity = pool.AcquireObject();
        entity.SetSimulated(true);
        entity.transform.position = aPosition;
    }

    private GameObject GetPickupPrefabByType(PICKUP_TYPE aType)
    {
        switch(aType)
        {
            case PICKUP_TYPE.HEALTH:
            {
                return m_globalDataDataRepo.m_healthPickupPrefab;
            }
            case PICKUP_TYPE.WEAPON_ENERGY:
            {

            }
                break;
        }

        return m_globalDataDataRepo.m_healthPickupPrefab;
    }

    #endregion // Pickups

    #region Level

    public void SetPauseAll(bool aPause)
    {
        if(m_gamePaused == aPause)
        {
            return;
        }

        m_gamePaused = aPause;

        foreach(GameEntity entity in m_entities)
        {
            entity.SetPaused(aPause);
        }

        foreach(IPausable pausable in m_pausables)
        {
            pausable.SetPaused(aPause);
        }
    }

    public void PlayerStartSequence()
    {
        PlayerEntity player = GetPlayerEntity();
        if(player == null)
            return;

        SetPauseAll(true);
        GetPlayerEntity().PrepareForSpawnIn();
        EventManager.Instance.TriggerEvent(Events.EVENT_START_FADE_OUT, new StartFadeOutEvent(OnFadeInPlayerStartComplete));
        m_ingameUI.SetReadyTextShowing(true);
    }

    private void OnFadeInPlayerStartComplete()
    {
        GetPlayerEntity().SpawnIn();
        m_ingameUI.FadeOutReadyText(m_globalDataDataRepo.m_levelStartReadyFadeOutTime);
    }

    public void PlayerSpawnInComplete()
    {
        SetPauseAll(false);
    }

    public void TeleportPlayer(Vector2 aPos, CinemachineVirtualCamera aNewCamera)
    {
        PlayerEntity player = GetPlayerEntity();
        player.OnTeleport();
        Vector2 startPos = player.transform.position;
        player.SetPosition(aPos);
        player.PlaceOnGround();
        if(aNewCamera)
        {
            CameraManager.Instance.ActivateCamera(aNewCamera);
        }
        //ICinemachineCamera camera = CameraManager.Instance.GetActiveCinemachineCamera();
        //camera.OnTargetObjectWarped(player.transform, aPos - startPos);
    }

    public void RegisterCheckpoint(Checkpoint aCheckPoint)
    {
        m_checkpoints.Add(aCheckPoint);
    }

    public void MarkNewCheckpoint(int aId)
    {
        m_currentCheckpointId = aId;
    }

    public void RestartAtCheckpoint()
    {
        PlayerEntity player = GetPlayerEntity();
        player.SetPosition(GetCurrentCheckpointPosition());
        player.PlaceOnGround();
    }

    private Vector3 GetCurrentCheckpointPosition()
    {
        return GetCheckpointPosition(m_currentCheckpointId);
    }

    public Vector3 GetCheckpointPosition(int aId)
    {
        foreach(Checkpoint checkpoint in m_checkpoints)
        {
            if(checkpoint.GetCheckpointId() == aId)
            {
                return checkpoint.transform.position;
            }
        }
        return Vector3.zero;
    }

    #endregion // Level
}