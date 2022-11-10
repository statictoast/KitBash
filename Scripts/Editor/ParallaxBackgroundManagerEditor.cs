using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ParallaxBackgroundManager))]
public class ParallaxBackgroundManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ParallaxBackgroundManager myTarget = (ParallaxBackgroundManager)target;

        bool inEditMode = myTarget.m_currentMode == ParallaxBackgroundManager.EditMode.EDIT;
        bool mode = EditorGUILayout.Toggle("Freeze Backgrounds", inEditMode);
        if(mode != inEditMode)
        {
            myTarget.m_currentMode = mode ? ParallaxBackgroundManager.EditMode.EDIT : ParallaxBackgroundManager.EditMode.PLAY;
        }

        GameObject lastRelativeTarget = myTarget.GetRelativeTarget();
        GameObject relativeTarget = (GameObject)EditorGUILayout.ObjectField("Target", lastRelativeTarget, typeof(GameObject), true);

        if(lastRelativeTarget != relativeTarget)
        {
            myTarget.SetRelativeTarget(relativeTarget);
        }
    }
}
