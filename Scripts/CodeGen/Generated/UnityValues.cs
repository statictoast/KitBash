using UnityEditor;

public static class GameplayLayers
{


// The numbers here represent the actual value of the layer in Unity
public const int Default = 0;
public const int TransparentFX = 1;
public const int Ignore_Raycast = 2;
public const int Water = 4;
public const int UI = 5;
public const int Player = 8;
public const int Platform = 9;
public const int Enemy = 10;
public const int FX = 11;
public const int ProjectilePlayer = 12;
public const int ProjectileEnemy = 13;
public const int GameplayTrigger = 14;
public const int EnemyProp = 15;
public const int Hazards = 16;
public const int TriggerVolume = 17;

  public enum Values
  {
    DEFAULT = 0,
    TRANSPARENTFX = 1,
    IGNORE_RAYCAST = 2,
    WATER = 3,
    UI = 4,
    PLAYER = 5,
    PLATFORM = 6,
    ENEMY = 7,
    FX = 8,
    PROJECTILEPLAYER = 9,
    PROJECTILEENEMY = 10,
    GAMEPLAYTRIGGER = 11,
    ENEMYPROP = 12,
    HAZARDS = 13,
    TRIGGERVOLUME = 14,
  }
}


public static class GameplayTags
{
public static readonly string UNTAGGED = "Untagged";
public static readonly string RESPAWN = "Respawn";
public static readonly string FINISH = "Finish";
public static readonly string EDITORONLY = "EditorOnly";
public static readonly string MAINCAMERA = "MainCamera";
public static readonly string PLAYER = "Player";
public static readonly string GAMECONTROLLER = "GameController";
public static readonly string PLAYERPROJECTILE = "playerprojectile";

  public enum Values
  {
    UNTAGGED = 0,
    RESPAWN = 1,
    FINISH = 2,
    EDITORONLY = 3,
    MAINCAMERA = 4,
    PLAYER = 5,
    GAMECONTROLLER = 6,
    PLAYERPROJECTILE = 7,
  }
}
