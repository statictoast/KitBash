using UnityEditor;
using UnityEngine;
using System;

public class GlobalGameDataRepositoryEditor : EditorWindow
{
    private string projectFilePath = "Assets/Resources/GlobalGameDataRepository.asset";

    private static GlobalGameDataRepositoryEditor editor;
    private GlobalGameDataRepository m_data;
    private Vector2 scrollPos;

    [MenuItem("Window/Global Game Data Editor")]
    static void Init()
    {
        if(editor == null)
        {
            editor = (GlobalGameDataRepositoryEditor)GetWindow(typeof(GlobalGameDataRepositoryEditor));
        }
        editor.Show();
    }

    void OnGUI()
    {
        if(m_data == null)
        {
            LoadData();
        }

        if(m_data == null)
        {
            // something bad has happened, don't render anything
            return;
        }

        if(GUILayout.Button("Save data"))
        {
            SaveData();
        }

        EditorGUILayout.Space();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Enemy Death FX: ", EditorStyles.wordWrappedLabel);
        m_data.m_enemyDeathFX = EditorGUILayout.ObjectField(m_data.m_enemyDeathFX, typeof(GameObject), true) as GameObject;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Health Pickup: ", EditorStyles.wordWrappedLabel);
        m_data.m_healthPickupPrefab = EditorGUILayout.ObjectField(m_data.m_healthPickupPrefab, typeof(GameObject), true) as GameObject;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();

        if(GUI.changed)
        {
            EditorUtility.SetDirty(m_data);
        }

        /*if(GUILayout.Button("Add New Attack", GUILayout.Width(200)))
        {
            m_attackData.AddNewAttack();
        }

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Filtered Attacks:", EditorStyles.wordWrappedLabel);
        m_filteredAttacks = EditorGUILayout.TextField(m_filteredAttacks);
        EditorGUILayout.EndHorizontal();

        string[] selectedMissions = null;
        if(m_filteredAttacks != "")
        {
            selectedMissions = m_filteredAttacks.Split(',');
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        EditorGUILayout.Space();

        if(selectedMissions != null && selectedMissions.Length > 0)
        {
            for(int i = 0; i < selectedMissions.Length; i++)
            {
                if(selectedMissions[i] == "")
                {
                    continue;
                }

                try
                {
                    int attackUID = int.Parse(selectedMissions[i]);
                    ShowAttack(m_attackData.GetAttack(attackUID));
                }
                catch(Exception e)
                {
                    m_filteredAttacks = "";
                }
            }
        }
        else
        {
            foreach(Attack atk in m_attackData.m_AllAttacks)
            {
                ShowAttack(atk);
            }
        }

        EditorGUILayout.EndScrollView();

        if(GUI.changed)
        {
            EditorUtility.SetDirty(m_attackData);
        }
    }

    private void ShowAttack(Attack aAttack)
    {
        if(aAttack == null)
        {
            Debug.Log("Tried to edit an attack but it does not exist");
            return;
        }

        GUILayout.Label("Attack ID: " + aAttack.m_uid, EditorStyles.boldLabel);

        aAttack.m_actionType = (PLAYER_ACTION_TYPES)EditorGUILayout.EnumPopup("Attack Type:", aAttack.m_actionType, GUILayout.Width(400));

        if(aAttack.m_actionType != PLAYER_ACTION_TYPES.PROJECTILE)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Animation: ", EditorStyles.wordWrappedLabel);
            aAttack.m_animationClip = EditorGUILayout.ObjectField(aAttack.m_animationClip, typeof(AnimationClip), true) as AnimationClip;
            EditorGUILayout.EndHorizontal();


            aAttack.m_linkedAttackData = EditorGUILayout.IntField("Linked Attack Id: ", aAttack.m_linkedAttackData, GUILayout.Width(200));

            if(aAttack.m_linkedAttackData > Attack.cNoLinkedAttackData)
            {
                bool attackExists = m_attackData.GetAttack(aAttack.m_linkedAttackData) != null;
                if(!attackExists)
                {
                    aAttack.m_linkedAttackData = Attack.cNoLinkedAttackData;
                    Debug.LogWarning("[AttackDataEditor] attempted to link attack that doesn't exist");
                }
            }

            aAttack.m_direction = (ACTION_DIRECTION)EditorGUILayout.EnumPopup("Attack Direction:", aAttack.m_direction, GUILayout.Width(400));
            aAttack.m_priority = (PLAYER_ACTION_PRIORITY)EditorGUILayout.EnumPopup("Priority:", aAttack.m_priority, GUILayout.Width(400));


            EditorGUILayout.BeginHorizontal();
            aAttack.m_powerGain = EditorGUILayout.FloatField("Power Gain: ", aAttack.m_powerGain, GUILayout.Width(200));
            aAttack.m_powerCost = EditorGUILayout.FloatField("Power Cost: ", aAttack.m_powerCost, GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();


            aAttack.m_moveForwardMagnitude = EditorGUILayout.FloatField("Forward Magnitude: ", aAttack.m_moveForwardMagnitude, GUILayout.Width(200));
        }

        GUILayout.Label("Hitbox Data", EditorStyles.boldLabel);
        aAttack.m_hitBoxData.damage = EditorGUILayout.IntField("Damage: ", aAttack.m_hitBoxData.damage, GUILayout.Width(200));
        if(aAttack.m_hitBoxData.damage < 0)
        {
            aAttack.m_hitBoxData.damage = 0;
        }

        aAttack.m_hitBoxData.hitStunDamage = EditorGUILayout.FloatField("Hitstun Damage: ", aAttack.m_hitBoxData.hitStunDamage, GUILayout.Width(200));
        if(aAttack.m_hitBoxData.hitStunDamage < 0.0f)
        {
            aAttack.m_hitBoxData.hitStunDamage = 0.0f;
        }

        if(aAttack.m_actionType != PLAYER_ACTION_TYPES.PROJECTILE)
        {
            aAttack.m_cooldownAfterAttackS = EditorGUILayout.FloatField("Cooldown After Attack(S): ", aAttack.m_cooldownAfterAttackS, GUILayout.Width(200));
            if(aAttack.m_cooldownAfterAttackS < 0.0f)
            {
                aAttack.m_cooldownAfterAttackS = 0.0f;
            }
            aAttack.m_airOnly = EditorGUILayout.Toggle("Air Only: ", aAttack.m_airOnly, GUILayout.Width(250));
        }
        aAttack.m_hitBoxData.oneHitPerTarget = EditorGUILayout.Toggle("One Hit Per Target: ", aAttack.m_hitBoxData.oneHitPerTarget, GUILayout.Width(250));

        if(aAttack.m_hitBoxData.launchData == null)
        {
            if(GUILayout.Button("Add Launch Data", GUILayout.Width(250)))
            {
                aAttack.m_hitBoxData.launchData = new LaunchData();
            }
        }
        else
        {
            GUILayout.Label("Launch Data", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            aAttack.m_hitBoxData.launchData.applyToAttacker = EditorGUILayout.Toggle("Apply to Attacker: ", aAttack.m_hitBoxData.launchData.applyToAttacker, GUILayout.Width(250));
            aAttack.m_hitBoxData.launchData.applyAttackerFacing = EditorGUILayout.Toggle("Apply Attacker Facing: ", aAttack.m_hitBoxData.launchData.applyAttackerFacing, GUILayout.Width(250));
            EditorGUILayout.EndHorizontal();

            aAttack.m_hitBoxData.launchData.velocity = EditorGUILayout.Vector2Field("Velocity: ", aAttack.m_hitBoxData.launchData.velocity, GUILayout.Width(200));
            aAttack.m_hitBoxData.launchData.gravity = EditorGUILayout.FloatField("Gravity: ", aAttack.m_hitBoxData.launchData.gravity, GUILayout.Width(200));
        }

        GUILayout.Label("Projectile Data", EditorStyles.boldLabel);
        aAttack.m_spawnsProjctiles = EditorGUILayout.Toggle("Spawns Projectile: ", aAttack.m_spawnsProjctiles, GUILayout.Width(250));
        if(aAttack.m_spawnsProjctiles)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Projectile Prefab: ", EditorStyles.wordWrappedLabel);
            aAttack.m_projectilePrefab = EditorGUILayout.ObjectField(aAttack.m_projectilePrefab, typeof(GameObject), true) as GameObject;
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            aAttack.m_projectilePrefab = null;
        }

        GUILayout.Label("Status Effect Data", EditorStyles.boldLabel);
        aAttack.m_addStatusEffect = EditorGUILayout.Toggle("Adds Status Effect: ", aAttack.m_addStatusEffect, GUILayout.Width(250));
        if(aAttack.m_addStatusEffect)
        {
            aAttack.m_statusEffectId = EditorGUILayout.IntField("Status Effect Id: ", aAttack.m_statusEffectId, GUILayout.Width(200));
            aAttack.m_statusEffectTarget = (ATTACK_STATUS_EFFECT_TARGET)EditorGUILayout.EnumPopup("Target:", aAttack.m_statusEffectTarget, GUILayout.Width(400));
        }
        else
        {
            aAttack.m_statusEffectId = -1;
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();*/
    }

    void OnDestroy()
    {
        SaveData();
    }

    private void SaveData()
    {
        AssetDatabase.SaveAssets();
        Debug.Log("Saved Global Game Data");
    }

    private void LoadData()
    {
        m_data = AssetDatabase.LoadAssetAtPath(projectFilePath, typeof(GlobalGameDataRepository)) as GlobalGameDataRepository;
        if(m_data == null)
        {
            m_data = ScriptableObject.CreateInstance(typeof(GlobalGameDataRepository)) as GlobalGameDataRepository;
            AssetDatabase.CreateAsset(m_data, projectFilePath);
            Debug.Log("Global Game Data not found, need to make some");
        }
    }
}