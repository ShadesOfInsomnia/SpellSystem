using UnityEngine;
using UnityEditor;
using System.Collections;
using Invector;
using System;

namespace Shadex
{
    /// <summary>
    /// Custom editor for the magic attack behavior inspector.
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MagicAttackBehavior), true)]
    public class MagicAttackBehaviorEditor : Editor
    {
        /// <summary>Invector skin.</summary>
        protected GUISkin skin;

        /// <summary>Unity skin.</summary>
        protected GUISkin defaultSkin;

        /// <summary>Logo for the script.</summary>
        protected Texture2D m_Logo;

        /// <summary>
        /// Occurs when the script is initially enabled, loads the skin.
        /// </summary>
        void OnEnable()
        {
            skin = Resources.Load("skin") as GUISkin;
            m_Logo = (Texture2D)Resources.Load("ammoIcon", typeof(Texture2D));
        }

        /// <summary>
        /// Occurs when the inspector GUI draws a frame, sets the skin to use invectors.
        /// </summary>
        public override void OnInspectorGUI()
        {        
            defaultSkin = GUI.skin;
            if (skin) GUI.skin = skin;
            GUILayout.BeginVertical("MAGIC ATTACK BEHAVIOUR", "window");
            GUILayout.Label(m_Logo, GUILayout.MaxHeight(25));
            base.OnInspectorGUI();
            GUILayout.EndVertical();
            GUI.skin = defaultSkin;
        }
    }
}

/* *****************************************************************************************************************************
 * Copyright        : 2017 Shades of Insomnia
 * Founding Members : Charles Page (Shade)
 *                  : Rob Alexander (Insomnia)
 * License          : Attribution-ShareAlike 4.0 International (CC BY-SA 4.0) https://creativecommons.org/licenses/by-sa/4.0/
 * *****************************************************************************************************************************
 * You are free to:
 *     Share        : copy and redistribute the material in any medium or format.
 *     Adapt        : remix, transform, and build upon the material for any purpose, even commercially. 
 *     
 * The licensor cannot revoke these freedoms as long as you follow the license terms.
 * 
 * Under the following terms:
 *     Attribution  : You must give appropriate credit, provide a link to the license, and indicate if changes were made. You may 
 *                    do so in any reasonable manner, but not in any way that suggests the licensor endorses you or your use.
 *     ShareAlike   : If you remix, transform, or build upon the material, you must distribute your contributions under the same 
 *                    license as the original. 
 *                  
 * You may not apply legal terms or technological measures that legally restrict others from doing anything the license permits. 
 * *****************************************************************************************************************************/
