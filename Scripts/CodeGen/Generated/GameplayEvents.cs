using UnityEngine;
using System;
using Cinemachine;

struct Events
{
public static readonly string EVENT_STICKY_WALL_COLLISION = "stickywallcollision";
public static readonly string EVENT_PLAYER_FINISHED_DYING = "playerfinisheddying";
public static readonly string EVENT_PLAYER_DIED = "playerdied";
public static readonly string EVENT_MOUSE_STATE_CHANGED = "mousestatechanged";
public static readonly string EVENT_PROJECTILE_HIT_ENTITY = "projectilehitentity";
public static readonly string EVENT_PLAYER_HEALTH_SET = "playerhealthset";
public static readonly string EVENT_PLAYER_HEALTH_CHANGE = "playerhealthchange";
public static readonly string EVENT_START_FADE_OUT = "startfadeout";
public static readonly string EVENT_FADE_OUT_COMPLETE = "fadeoutcomplete";
public static readonly string EVENT_START_FADE_IN = "startfadein";
public static readonly string EVENT_FADE_IN_COMPLETE = "fadeincomplete";
public static readonly string EVENT_ACTIVE_CAMERA_CHANGED = "activecamerachanged";

}

public class StickyWallCollisionEvent : CallbackEvent
{
    public bool isContacting;
    public StickyWallCollisionEvent(bool aisContacting)
    {
        isContacting = aisContacting;
    }

}

public class PlayerFinishedDyingEvent : CallbackEvent
{
    public Action callback;
    public PlayerFinishedDyingEvent(Action acallback)
    {
        callback = acallback;
    }

}

public class MouseStateChangedEvent : CallbackEvent
{
    public int mouseIndex;
    public INPUT_STATE prevState;
    public INPUT_STATE newState;
    public MouseStateChangedEvent(int amouseIndex, INPUT_STATE aprevState, INPUT_STATE anewState)
    {
        mouseIndex = amouseIndex;
        prevState = aprevState;
        newState = anewState;
    }

}

public class ProjectileHitEntityEvent : CallbackEvent
{
    public GameObject projectile;
    public GameObject entity;
    public ProjectileHitEntityEvent(GameObject aprojectile, GameObject aentity)
    {
        projectile = aprojectile;
        entity = aentity;
    }

}

public class PlayerHealthSetEvent : CallbackEvent
{
    public int health;
    public PlayerHealthSetEvent(int ahealth)
    {
        health = ahealth;
    }

}

public class PlayerHealthChangeEvent : CallbackEvent
{
    public int oldHealth;
    public int newHealth;
    public PlayerHealthChangeEvent(int aoldHealth, int anewHealth)
    {
        oldHealth = aoldHealth;
        newHealth = anewHealth;
    }

}

public class StartFadeOutEvent : CallbackEvent
{
    public Action callback;
    public StartFadeOutEvent(Action acallback)
    {
        callback = acallback;
    }

}

public class StartFadeInEvent : CallbackEvent
{
    public Action callback;
    public StartFadeInEvent(Action acallback)
    {
        callback = acallback;
    }

}

public class ActiveCameraChangedEvent : CallbackEvent
{
    public ICinemachineCamera newCamera;
    public ActiveCameraChangedEvent(ICinemachineCamera anewCamera)
    {
        newCamera = anewCamera;
    }

}

