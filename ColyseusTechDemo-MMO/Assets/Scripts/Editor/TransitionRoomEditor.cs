using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TransitionRoom))]
public class TransitionRoomEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (Application.isPlaying == false)
        {
            return;
        }

        GUILayout.Space(40);

        TransitionRoom tR = target as TransitionRoom;

        if (GUILayout.Button("Open Entry Door"))
        {
            tR.OpenDoor(true, null);
        }
        else if (GUILayout.Button("Close Entry Door"))
        {
            tR.CloseDoor(true, null);
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Open Exit Door"))
        {
            tR.OpenDoor(false, null);
        }
        else if (GUILayout.Button("Close Exit Door"))
        {
            tR.CloseDoor(false, null);
        }
    }
}
