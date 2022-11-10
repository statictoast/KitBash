using UnityEngine;
using UnityEngine.UI;

public class FillBar : UIControl
{
    [SerializeField]
    private Image m_fillBar = null;

    private float m_minValue = 0;
    private float m_maxValue = 0;
    private float m_startValue = 0;
    private float m_targetValue = 0;

    private float m_timeToTargetS = 0;
    private float m_timeRemainingS = 0;

    // Use this for initialization
    void Start ()
    {
        if (!m_fillBar)
        {
            Debug.LogError("no fill bar image set");
            return;
        }

        if(m_fillBar.type != Image.Type.Filled)
        {
            Debug.LogError("trying to use " + m_fillBar.type + " for a fill bar");
        }
    }
    
    // Update is called once per frame
    void Update ()
    {
        if(m_timeRemainingS < m_timeToTargetS)
        {
            float currentValue = Mathf.Lerp(m_startValue, m_targetValue, m_timeRemainingS / m_timeToTargetS);
            m_fillBar.fillAmount = currentValue / (m_maxValue - m_minValue);
            m_timeRemainingS += Time.deltaTime;
            if(m_timeRemainingS > m_timeToTargetS)
            {
                m_timeRemainingS = m_timeToTargetS;
            }
        }
    }

    public void FillInTime(float aTimeToTargetS)
    {
        m_timeToTargetS = aTimeToTargetS;
        m_startValue = m_minValue = 0;
        m_targetValue = m_maxValue = 1;
        m_timeRemainingS = 0;
    }

    public void AnimateBarFromPoint(float aMinValue, float aMaxValue, float aStartValue, float aTargetValue, float aTimeToTargetS)
    {
        m_minValue = aMinValue;
        m_maxValue = aMaxValue;
        m_startValue = aStartValue;
        m_targetValue = aTargetValue;
        m_timeToTargetS = aTimeToTargetS;
        m_timeRemainingS = 0;
    }

    public void SetBarFromPoint(float aMinValue, float aMaxValue, float aValue)
    {
        m_minValue = aMinValue;
        m_maxValue = aMaxValue;
        m_fillBar.fillAmount = aValue / (m_maxValue - m_minValue);
    }
}
