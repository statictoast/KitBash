using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHealthBar : UIControl
{
    public Image m_topHealthPip;
    public Image m_bottomHealthPip;
    public GameObject m_pipPrefab;

    private int m_numPips;
    private List<GameObject> m_spawnedPips;

    private void Awake()
    {
        EventManager.Instance.RegisterEvent(Events.EVENT_PLAYER_HEALTH_SET, "healthBar", OnPlayerHealthSet);
        EventManager.Instance.RegisterEvent(Events.EVENT_PLAYER_HEALTH_CHANGE, "healthBar", OnPlayerHealthChange);
    }

    // Start is called before the first frame update
    void Start()
    {
        m_spawnedPips = new List<GameObject>();

        float pipHeight = m_topHealthPip.preferredHeight + 1.5f;
        m_numPips = Mathf.CeilToInt((m_topHealthPip.rectTransform.anchoredPosition.y + m_bottomHealthPip.rectTransform.rect.height - m_bottomHealthPip.rectTransform.anchoredPosition.y) / pipHeight);

        for(int i = 0; i < m_numPips; i++)
        {
            GameObject pip = Instantiate(m_pipPrefab);
            pip.transform.SetParent(transform);
            RectTransform rt = pip.GetComponent<Image>().GetComponent<RectTransform>();
            rt.pivot = m_topHealthPip.rectTransform.pivot;
            rt.localScale = m_topHealthPip.rectTransform.localScale;
            rt.anchorMin = m_topHealthPip.rectTransform.anchorMin;
            rt.anchorMax = m_topHealthPip.rectTransform.anchorMax;
            rt.anchoredPosition = new Vector2(m_topHealthPip.rectTransform.anchoredPosition.x, (-i * (pipHeight)) + m_topHealthPip.rectTransform.anchoredPosition.y);

            m_spawnedPips.Add(pip);
        }

        m_topHealthPip.gameObject.SetActive(false);
        m_bottomHealthPip.gameObject.SetActive(false);
    }

    private void OnPlayerHealthSet(CallbackEvent aCallbackEvent)
    {
        PlayerHealthSetEvent healthSetEvent = aCallbackEvent as PlayerHealthSetEvent;
        UpdatePipsOnHealth(healthSetEvent.health);
    }

    private void OnPlayerHealthChange(CallbackEvent aCallbackEvent)
    {
        PlayerHealthChangeEvent healthChangeEvent = aCallbackEvent as PlayerHealthChangeEvent;
        UpdatePipsOnHealth(healthChangeEvent.newHealth);
    }

    private void UpdatePipsOnHealth(int aHealth)
    {
        for(int i = 0; i < m_spawnedPips.Count; i++)
        {
            m_spawnedPips[i].SetActive((m_spawnedPips.Count - i) <= aHealth);
        }
    }
}
