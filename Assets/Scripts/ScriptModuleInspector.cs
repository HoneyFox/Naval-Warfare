using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(ScriptModule))]
public class ScriptModuleInspector : Editor
{
    protected ScriptModule module;

    void RegisterUndo()
    {
        Undo.RegisterUndo(target, "Script Change");
        EditorUtility.SetDirty(target);
    }

    public override void OnInspectorGUI ()
    {
        base.OnInspectorGUI();
        GUILayout.BeginVertical();
        module = target as ScriptModule;
        string text = EditorGUILayout.TextArea(module.initCodes, GUI.skin.textArea, GUILayout.Height(100f));
        if (module.initCodes != text)
        {
            RegisterUndo();
            module.initCodes = text;
        }

        string text2 = EditorGUILayout.TextArea(module.fixedUpdateCodes, GUI.skin.textArea, GUILayout.Height(400f));
        if (module.fixedUpdateCodes != text2)
        {
            RegisterUndo();
            module.fixedUpdateCodes = text2;
        }
        GUILayout.EndVertical();
    }
}
#endif