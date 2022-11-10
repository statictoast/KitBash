using UnityEngine;

public class EasingUtils
{
    public static float EaseStart2(float t)
    {
        return t * t;
    }

    public static float EaseStart3(float t)
    {
        return t * t * t;
    }

    public static float EaseStart4(float t)
    {
        return t * t * t * t;
    }

    public static float EaseEndN(float t, float n)
    {
        return 1 - Mathf.Pow(1 - t, n);
    }

    public static float EaseEnd2(float t)
    {
        return EaseEndN(t, 2);
    }

    public static float EaseEnd3(float t)
    {
        return EaseEndN(t, 3);
    }

    public static float EaseEnd4(float t)
    {
        return EaseEndN(t, 4);
    }

    public static float EaseStartStop2(float t)
    {
        return Mix(t, 1 - t, t);
    }

    public static float EaseStartStop3(float t)
    {
        return Mix(EaseStart2(t), EaseEnd2(t), t);
    }

    public static float EaseStartStop4(float t)
    {
        return Mix(EaseStart3(t), EaseEnd3(t), t);
    }

    // helper functions
    private static float Mix(float partA, float partB, float weightB)
    {
        return ((1 - weightB) * partA) + (weightB * partB);
    }
}
