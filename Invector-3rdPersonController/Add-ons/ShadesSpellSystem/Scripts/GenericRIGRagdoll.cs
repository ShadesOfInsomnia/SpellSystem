using Invector.vEventSystems;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Invector;
using Invector.vCharacterController.AI;

namespace Shadex
{
    /// <summary>
    /// A modified version of invector's ragdoll controller script that doesn't require human body parts
    /// </summary>
    /// <remarks>
    /// Deprecated, invector has since released generic ragdoll via template, this inherited script simply adds a head transform.
    /// </remarks>
    public class GenericRIGController : v_AIController
    {
        [Header("--- Non Humanoid ---")]
        public Transform HeadTransform;

        bool bStayDead = false;
        public bool UseRootMotion = true;

        protected override void Start()
        {
            base.Start();  // call invectors base start

            if (!head)
            {  // no head = generic rig
                head = HeadTransform;  // set the head component as invector's ai wont be able to for non humanoid                
            }
            if (!UseRootMotion)
            {
                animator.applyRootMotion = false;
            }
        }

        protected void Update()
        {
            // fix for the AI not playing the death animation
            if (!bStayDead)
            {  // not dead already
                if (isDead)
                {  // are we alive?
                    bStayDead = true;  // stop death checking
                    animator.CrossFade("Dead", 0.5f);  // apply death
                }
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
