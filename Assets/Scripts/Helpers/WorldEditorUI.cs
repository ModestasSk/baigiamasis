using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor (typeof (Generator))]
public class WorldEditorUI : Editor
{
    public override void OnInspectorGUI()
    {
        Generator gen = (Generator)target;
        if (DrawDefaultInspector())
        {
            if (gen.autoDraw)
            {
                gen.GenerateTerrain();
            }
        }

        gen.DisplayView(gen.drawMode);

        if (GUILayout.Button("Build")){
            gen.GenerateTerrain();
        }
    }
}
