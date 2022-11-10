using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

/// <summary>
/// Be aware this will not prevent a non singleton constructor
///   such as `T myT = new T();`
/// To prevent that, add `protected T () {}` to your singleton class.
/// 
/// As a note, this is made as MonoBehaviour because we need Coroutines.
/// </summary>
public class BaseManager<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T m_instance;
    protected static GameObject m_GameObject;

    private void Awake()
    {
        
    }

    public static T Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = (T)FindObjectOfType(typeof(T));

                if (FindObjectsOfType(typeof(T)).Length > 1)
                {
                    Debug.LogError("[Manager] Something went really wrong " +
                        " - there should never be more than 1 singleton!" +
                        " Reopening the scene might fix it.");
                    return m_instance;
                }

                if (m_instance == null)
                {
                    m_GameObject = new GameObject();
                    m_instance = m_GameObject.AddComponent<T>();
                    m_GameObject.name = "(Manager) " + typeof(T).ToString();


                    DontDestroyOnLoad(m_GameObject);

                    Debug.Log("[Manager] An instance of " + typeof(T) +
                        " is needed in the scene, so '" + m_GameObject +
                        "' was created with DontDestroyOnLoad.");
                }
                else
                {
                    Debug.Log("[Manager] Using instance already created: " +
                        m_instance.gameObject.name);
                }
            }

            return m_instance;
        }
    }

    virtual public void OnRestartLevel()
    {

    }

    virtual public void Init()
    {

    }

    public static bool DoesInstanceExist()
    {
        return Instance != null;
    }
}