using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelDataRepositoryEditor : EditorWindow
{
    private string projectFilePath = "Assets/Resources/LevelDataRepository.asset";

    private static LevelDataRepositoryEditor editor;
    private LevelDataRepository m_data;
    private Vector2 scrollPos;

    [MenuItem("Window/Level Data Editor")]
    static void Init()
    {
        if(editor == null)
        {
            editor = (LevelDataRepositoryEditor)GetWindow(typeof(LevelDataRepositoryEditor));
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

        /*if(GUILayout.Button("Save data"))
        {
            SaveData();
        }*/

        EditorGUILayout.Space();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        SerializedObject so = new SerializedObject(m_data);

        SerializedProperty mainMenuProperty = so.FindProperty("m_mainMenuPath");
        SceneAsset oldMainMenuScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(mainMenuProperty.stringValue);
        SceneAsset newMainMenuScene = EditorGUILayout.ObjectField("Main Menu: ", oldMainMenuScene, typeof(SceneAsset), false) as SceneAsset;
        string newMainMenuPath = AssetDatabase.GetAssetPath(newMainMenuScene);
        if(newMainMenuPath != mainMenuProperty.stringValue)
        {
            mainMenuProperty.stringValue = newMainMenuPath;
        }

        SerializedProperty levelsProperty = so.FindProperty("m_playableLevelPaths");
        if(GUILayout.Button("Add Level", GUILayout.Width(150)))
        {
            levelsProperty.InsertArrayElementAtIndex(levelsProperty.arraySize);
            levelsProperty.GetArrayElementAtIndex(levelsProperty.arraySize - 1).stringValue = "";
        }

        int removeLevelIndex = -1;
        EditorGUI.indentLevel = 1;
        for(int i = 0; i < levelsProperty.arraySize; i++)
        {
            SceneAsset oldScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(levelsProperty.GetArrayElementAtIndex(i).stringValue);
            EditorGUILayout.BeginHorizontal();
            SceneAsset newScene = EditorGUILayout.ObjectField("Level " + (i+1), oldScene, typeof(SceneAsset), false) as SceneAsset;
            if(GUILayout.Button("X", GUILayout.Width(50)))
            {
                removeLevelIndex = i;
            }
            EditorGUILayout.EndHorizontal();
            string newPath = AssetDatabase.GetAssetPath(newScene);
            levelsProperty.GetArrayElementAtIndex(i).stringValue = newPath;
        }

        if(removeLevelIndex != -1)
        {
            levelsProperty.DeleteArrayElementAtIndex(removeLevelIndex);
        }

        so.ApplyModifiedProperties();

        EditorGUI.indentLevel = 0;

        EditorGUILayout.EndScrollView();

        if(GUI.changed)
        {
            EditorUtility.SetDirty(m_data);
        }
    }

    void OnDestroy()
    {
        SaveData();
    }

    private void SaveData()
    {
        AssetDatabase.SaveAssets();
        Debug.Log("Saved Level Data");
    }

    private void LoadData()
    {
        m_data = AssetDatabase.LoadAssetAtPath(projectFilePath, typeof(LevelDataRepository)) as LevelDataRepository;
        if(m_data == null)
        {
            m_data = ScriptableObject.CreateInstance(typeof(LevelDataRepository)) as LevelDataRepository;
            AssetDatabase.CreateAsset(m_data, projectFilePath);
            Debug.Log("Level Data not found, need to make some");
        }
    }
}
