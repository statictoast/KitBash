using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public float m_CameraShakeDurationS = 0.25f;
    [Range(0, 1)]
    public float m_DampeningStartPercentage = 0.75f;
    public float m_ShakeMagnitude = 1.0f;

    private float m_ShakeTimeRemainingS = 0.0f;
    private float m_PerlinNoiseXCord = 0.0f;
    private float m_PerlinNoiseYCord = 0.0f;

    void Start()
    {
        
    }

    void Update()
    {
        if (m_ShakeTimeRemainingS > 0.0f)
        {
            Vector3 currentPosition = transform.position;

            float percentComplete = (m_CameraShakeDurationS - m_ShakeTimeRemainingS) / m_CameraShakeDurationS;

            // make range between [-1, 1]
            float xValue = Mathf.PerlinNoise(m_PerlinNoiseXCord / 2.3f, m_PerlinNoiseYCord / 2.3f) * 2.0f - 1.0f;
            float yValue = Mathf.PerlinNoise((2 - m_PerlinNoiseXCord) / 2.3f, (2 - m_PerlinNoiseYCord) / 2.3f) * 2.0f - 1.0f;

            float dampening = 1.0f;
            if (percentComplete >= m_DampeningStartPercentage)
            {
                // (max'-min')/(max-min)*(value-min)+min'
                dampening -= (1 / (1 - m_DampeningStartPercentage) * (percentComplete - m_DampeningStartPercentage));
            }

            xValue *= m_ShakeMagnitude * dampening;
            yValue *= m_ShakeMagnitude * dampening;

            transform.position = new Vector3(currentPosition.x + xValue, currentPosition.y + yValue, currentPosition.z);

            m_PerlinNoiseXCord += 12.3f;
            m_PerlinNoiseYCord += 4.2f;
            m_ShakeTimeRemainingS -= Time.deltaTime;
        }
    }

    public void StartShake()
    {
        m_ShakeTimeRemainingS = m_CameraShakeDurationS;
        m_PerlinNoiseXCord = 0;
        m_PerlinNoiseYCord = 0;
    }
}