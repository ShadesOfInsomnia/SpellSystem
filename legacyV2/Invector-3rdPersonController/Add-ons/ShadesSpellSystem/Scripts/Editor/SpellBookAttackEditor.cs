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
    [CustomEditor(typeof(SpellBookAttack), true)]
    public class SpellBookAttackEditor : SpellBookBaseEditor
    {

        /// <summary>
        /// Occurs when the inspector GUI draws a frame, sets the skin to use invectors.
        /// </summary>
        public override void OnInspectorGUI()
        {        
            defaultSkin = GUI.skin;
            if (skin) GUI.skin = skin;

            SpellBookAttack cc = (SpellBookAttack)target;

            DisplaySpellSubDetail(cc.SpellOptions);

            GUI.skin = defaultSkin;
        }

        /// <summary>
        /// No header applied, this is empty to allow the spell book to share the same code.
        /// </summary>
        protected override void DisplaySpellHeader(SpellBookEntrySubType SubType)
        {
            GUILayout.EndHorizontal();
            GUILayout.Space(3);
        }

        /// <summary>
        /// Always true for the attack behavior, also allows sharing with the spell book.
        /// </summary>
        /// <returns>True.</returns>
        protected override bool IsExpanded()
        {
            return true;
        }

        /// <summary>
        /// No spell info output, this is empty to allow the spell book to share the same code.
        /// </summary>
        protected override void DisplaySpellInfo(SpellBookEntrySubType SubType)
        {

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
