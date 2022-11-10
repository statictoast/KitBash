using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(YokuPlatformController))]
public class YokuPlatformControllerEditor : Editor
{
    private int m_CurrentInterval = 0;

    public override void OnInspectorGUI()
    {
        YokuPlatformController myTarget = (YokuPlatformController)target;

        myTarget.m_IntervalS = EditorGUILayout.FloatField("Interval Seconds: ", myTarget.m_IntervalS);
        EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
        myTarget.SetNumIntervals(EditorGUILayout.IntField("Intervals", myTarget.GetNumIntervals()));
        m_CurrentInterval = EditorGUILayout.IntSlider("Current", m_CurrentInterval, 0, myTarget.GetNumIntervals() - 1);
        EditorGUI.EndDisabledGroup();

        List<YokuPlatform> platforms = new List<YokuPlatform>(myTarget.GetComponentsInChildren<YokuPlatform>());

        if(myTarget.m_PlatformsByInterval == null)
        {
            myTarget.m_PlatformsByInterval = new List<YokuPlatformInterval>();
            EditorUtility.SetDirty(target);
        }

        while(myTarget.m_PlatformsByInterval.Count <= m_CurrentInterval)
        {
            myTarget.m_PlatformsByInterval.Add(new YokuPlatformInterval(platforms.Count));
            EditorUtility.SetDirty(target);
        }

        YokuPlatformInterval currentIntervalData = myTarget.m_PlatformsByInterval[m_CurrentInterval];
        if (!EditorApplication.isPlaying)
        {
            if (currentIntervalData.m_ActivePlatforms.Count != platforms.Count)
            {
                currentIntervalData.ResizeData(platforms.Count);
                EditorUtility.SetDirty(target);
            }
        }

        EditorGUILayout.Space();
        EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
        currentIntervalData.m_overrideIntervalTimeS = EditorGUILayout.FloatField("Override Time S: ", currentIntervalData.m_overrideIntervalTimeS);
        int i = 0;
        foreach (YokuPlatform platform in platforms)
        {
            bool previousValue = currentIntervalData.m_ActivePlatforms[i];
            bool newValue = EditorGUILayout.Toggle(platform.name, currentIntervalData.m_ActivePlatforms[i]);
            if(newValue != previousValue)
            {
                Undo.RecordObject(target, "Toggle Yoku Block");
                currentIntervalData.m_ActivePlatforms[i] = newValue;
                EditorUtility.SetDirty(target);
            }

            i++;
        }
        EditorGUI.EndDisabledGroup();
    }
}
