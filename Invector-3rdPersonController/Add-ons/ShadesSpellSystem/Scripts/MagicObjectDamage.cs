using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if !VANILLA
using Invector;
#endif

namespace Shadex
{
    /// <summary>
    /// Stores elemental damage for the leveling system damage mitigation.
    /// </summary>
#if !VANILLA
    [vClassHeader("MAGIC DAMAGE", iconName = "triggerIcon")]
    public class MagicObjectDamage : vMonoBehaviour
    {
#else
    public class MagicObjectDamage : MonoBehaviour {
#endif
        /// <summary>Elemental magic to apply alongside the base physical damage from the invector damage script.</summary>
        [Tooltip("Elemental magic to apply alongside the base physical damage from the invector damage script")]
        public List<MagicDamageOverTime> Damage = new List<MagicDamageOverTime>();

        /// <summary>Apply damage within a specified radius on hit, set to greater than zero to enable.</summary>
        [Tooltip("Apply damage within a specified radius on hit, set to greater than zero to enable")]
        public float AOERadius;

        /// <summary>Radius target assignment, note this is auto updated when fired from the animator.</summary>
        [Tooltip("Radius target assignment, note this is auto updated when fired from the animator")]
        public SpawnTarget AOETarget;
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
