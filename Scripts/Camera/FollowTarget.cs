using UnityEngine;

[ExecuteInEditMode]
public class FollowTarget : MonoBehaviour 
{
    const int MAX_FOLLOW_STRENGTH = 100;
    const float FOLLOW_DISTANCE_EPSILON = 0.009f;

    public Transform m_Target;
    public bool m_LockZAxis = false;
    [Range(0, MAX_FOLLOW_STRENGTH)]
    public int m_FollowStrength = MAX_FOLLOW_STRENGTH;
    float m_SavedZAxis;

    void Start()
    {
        m_SavedZAxis = transform.position.z;
    }

    // Update is called once per frame
    void Update ()
    {
        if(!m_Target)
        {
            return;
        }

        Vector3 targetPos = m_Target.position;

        Vector3 delta = targetPos - transform.position;

        if (delta.sqrMagnitude < FOLLOW_DISTANCE_EPSILON)
        {
            transform.position = targetPos;
            return;
        }

        float followStrength = m_FollowStrength / (float)MAX_FOLLOW_STRENGTH;
        Vector3 nextPos = new Vector3(transform.position.x + (delta.x * followStrength),
                                      transform.position.y + delta.y * followStrength,
                                      m_LockZAxis ? m_SavedZAxis : transform.position.z + delta.z * followStrength);

        transform.position = nextPos;
    }
}
