using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameFXManager : BaseManager<GameFXManager>
{
    private List<FXLogic> m_activeFXs;
    private Dictionary<string, List<FXLogic>> m_pooledInactiveFXs;

    public override void Init()
    {
        m_activeFXs = new List<FXLogic>();
        m_pooledInactiveFXs = new Dictionary<string, List<FXLogic>>();
    }

    public override void OnRestartLevel()
    {
        for(int i = 0; i < m_activeFXs.Count; i++)
        {
            m_activeFXs[i].RequestEnd();
        }
        m_activeFXs.Clear();

        foreach(KeyValuePair<string, List<FXLogic>> keyPair in m_pooledInactiveFXs)
        {
            List<FXLogic> list = keyPair.Value;
            for(int i = 0; i < list.Count; i++)
            {
                Destroy(list[i]);
            }
        }
        m_pooledInactiveFXs.Clear();
    }

    public GameObject RequestPlayFX(GameObject aPrefab)
    {
        return RequestPlayFX(aPrefab, Vector3.zero, null);
    }

    public GameObject RequestPlayFX(GameObject aPrefab, GameObject aParent)
    {
        return RequestPlayFX(aPrefab, Vector3.zero, aParent);
    }

    public GameObject RequestPlayFX(GameObject aPrefab, Vector3 aStartingPos, GameObject aParent)
    {
        FXLogic prefabLogic = aPrefab.GetComponent<FXLogic>();

        if(prefabLogic == null)
        {
            Debug.LogError("[RequestPlayFX] attempted to play an FX but it does not have any FXLogic");
            return null;
        }

        string prefabName = aPrefab.name;
        FXLogic newFXLogic = null;

        List<FXLogic> inactiveFXs;
        m_pooledInactiveFXs.TryGetValue(prefabName, out inactiveFXs);
        if(inactiveFXs != null)
        {
            if(inactiveFXs.Count > 0)
            {
                newFXLogic = inactiveFXs[0];
                inactiveFXs.RemoveAt(0);
            }
        }

        if(newFXLogic == null)
        {
            GameObject newFX = Instantiate(aPrefab, aStartingPos, aPrefab.transform.rotation);
            newFXLogic = newFX.GetComponent<FXLogic>();

            if(newFXLogic == null)
            {
                Destroy(newFX);
                Debug.LogError("[RequestPlayFX] attempted to create a new FX with no FX but somehow passed check for FXLogic");
                return null;
            }

            newFXLogic.SetPrefabName(prefabName);
        }

        GameObject FXGameObject = newFXLogic.gameObject;

        switch(newFXLogic.m_attachType)
        {
            case FX_ATTACH_TYPE.POINT:
            {
                FXGameObject.transform.SetParent(null);
                Vector3 finalPosition = aStartingPos + newFXLogic.m_pointOffset;
                FXGameObject.transform.position = finalPosition;
            }
            break;
            case FX_ATTACH_TYPE.POINT_FACE_WITH_PARENT:
            {
                if(aParent == null)
                {
                    Destroy(FXGameObject);
                    Debug.LogError("[RequestPlayFX] attempted to use gameobject that does not exist for facing");
                    return null;
                }

                FXGameObject.transform.SetParent(null);
                FXGameObject.transform.position = aStartingPos;
                FXGameObject.transform.localScale = new Vector3(aParent.GetComponent<GameEntity>().GetFacing(), FXGameObject.transform.localScale.y, FXGameObject.transform.localScale.z);
            }
            break;
            case FX_ATTACH_TYPE.PARENT:
            {
                if(aParent == null)
                {
                    Destroy(FXGameObject);
                    Debug.LogError("[RequestPlayFX] attempted to parent FX to gameobject that does not exist");
                    return null;
                }

                FXGameObject.transform.SetParent(aParent.transform);
                FXGameObject.transform.localPosition = Vector3.zero;
            }
            break;
            case FX_ATTACH_TYPE.PARENT_WITH_OFFSET:
            {
                if(aParent == null)
                {
                    Destroy(FXGameObject);
                    Debug.LogError("[RequestPlayFX] attempted to parent FX to gameobject that does not exist");
                    return null;
                }

                FXGameObject.transform.SetParent(aParent.transform);
                FXGameObject.transform.localPosition = newFXLogic.m_pointOffset;
            }
            break;
        }

        m_activeFXs.Add(newFXLogic);
        newFXLogic.FXStart();
        return FXGameObject;
    }

    private void Update()
    {
        if(GameplayManager.Instance.IsGamePaused())
            return;

        for(int i = m_activeFXs.Count - 1; i >= 0; i--)
        {
            FXLogic fx = m_activeFXs[i];
            fx.UpdateFX();
            if(fx.ShouldEnd())
            {
                fx.OnTimeAliveFinished();

                string fxPrefabName = fx.GetPrefabName();
                List<FXLogic> fXLogics;
                m_pooledInactiveFXs.TryGetValue(fxPrefabName, out fXLogics);
                if(fXLogics == null)
                {
                    fXLogics = new List<FXLogic>();
                    m_pooledInactiveFXs.Add(fxPrefabName, fXLogics);
                }

                fXLogics.Add(fx);

                m_activeFXs.RemoveAt(i);
            }
        }
    }
}
