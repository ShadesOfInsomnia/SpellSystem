using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
#if !VANILLA
using Invector;
using Invector.vItemManager;
#endif

namespace Shadex
{
    /// <summary>
    /// Character display window skill point to spend via button click.
    /// </summary>
#if !VANILLA
    [vClassHeader("CHARACTER SPEND SKILL POINT", iconName = "inputIcon")]
    public class CharacterSpendSkillPoint : vMonoBehaviour
    {
#else
    public class CharacterSpendSkillPoint : MonoBehaviour {
#endif
        /// <summary>Type of skill point to spend as defined in the BaseSkill enum.</summary>
        [Tooltip("Type of skill point to spend")]
        public BaseSkill Type;

        private CharacterDisplay DisplayUI;

        /// <summary>
        /// Find the character display class on load.
        /// </summary>
        void Start()
        {
            DisplayUI = GetComponentInParent<CharacterDisplay>();
        }

        /// <summary>
        /// Occurs when the spend skill point +/- buttons are pressed.
        /// </summary>
        /// <param name="eventData">Event sender, unused.</param>
        public void OnClick(BaseEventData eventData)
        {
            if (DisplayUI)
            {
                DisplayUI.SpendSkillPoint(Type);  // spend the skill point
            }
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
