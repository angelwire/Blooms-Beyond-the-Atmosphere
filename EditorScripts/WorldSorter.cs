#if UNITY_EDITOR 
//created: 11-2-19
//updated: 11-2-19
//version: .01
//author Vance Howald
//Adds a button to sort the world inside WorldManager, this is needed because of how the script loads the world
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WorldManager))]
public class WorldSorter : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WorldManager wm = (WorldManager)target;
        if (GUILayout.Button("Sort World"))
        {
            wm.sortWorld();
        }
    }

}
#endif