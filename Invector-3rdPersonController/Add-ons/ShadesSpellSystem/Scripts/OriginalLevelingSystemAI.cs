using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector;

namespace Shadex
{
    /// <summary>
    /// The original leveling system for the AI.
    /// </summary>
    /// <remarks>
    /// This has been replaced with the CharacterBase/Instance classes.  
    /// Useful if your after a clean and simple character leveling system to extend.
    /// </remarks>
    [vClassHeader("ORIGINAL LEVELING SYSTEM (ENEMY AI)", iconName = "ladderIcon")]
    public class OriginalLevelingSystemAI : vMonoBehaviour
    {
        /// <summary>Amount of XP to give the linked player.</summary>
        public int XPToGive = 10;

        /// <summary>Destruction delay for the AI</summary>
        public float DelayToDestroy = 2.0f;

        /// <summary>Ensure is actually dead, prevents double coroutine run.</summary>
        public bool IsReallyDead = false;

        /// <summary>
        /// Waits for the dead flag to start the destroy routine.
        /// </summary>
        void Update()
        {
            if (IsReallyDead == true)
            {
                StartCoroutine(SDestroyEnemy());
            }
        }

        /// <summary>
        /// Link for the AI death invector event.
        /// </summary>
        public void GetXP()
        {
            OriginalLevelingSystem.XPCurrent += XPToGive;
            IsReallyDead = true;
            if (GlobalFuncs.DEBUGGING_MESSAGES)
            {
                Debug.Log("Enemy Died and Gave " + XPToGive.ToString() + " XP");
            }
        }

        /// <summary>
        /// Destroy the enemy after a delay.
        /// </summary>
        /// <returns>Coroutine wait time.</returns>
        public IEnumerator SDestroyEnemy()
        {
            yield return new WaitForSeconds(DelayToDestroy);
            Destroy(gameObject);
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
