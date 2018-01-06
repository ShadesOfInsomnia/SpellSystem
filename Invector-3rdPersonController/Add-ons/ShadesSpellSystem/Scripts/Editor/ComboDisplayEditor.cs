using UnityEngine;
using UnityEditor;
using System.Collections;
using Invector;
using System;
using BehaviorDesigner.Runtime;

namespace Shadex
{
    /// <summary>
    /// Custom editor for the sentient intelligence CPU.
    /// </summary>
    /// <remarks>
    /// Draws the field of view, max detect collider gizmo's when enabled and running in the editor.
    /// </remarks>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ComboDisplay), true)]
    public class ComboDisplayEditor : Editor
    {
        GUISkin skin;
        GUISkin defaultSkin;

        void OnEnable()
        {
            skin = Resources.Load("skin") as GUISkin;
        }

        /// <summary>
        /// Occurs when the inspector GUI draws a frame, outputs the sphere sensor radius.
        /// </summary>
        public override void OnInspectorGUI()
        {
            defaultSkin = GUI.skin;
            if (skin) GUI.skin = skin;
            GUILayout.BeginVertical("COMBO DISPLAY", "window");
            GUILayout.Space(30);
            base.OnInspectorGUI();
            GUILayout.EndVertical();
            GUI.skin = defaultSkin;
        }
    }
}
