﻿using UnityEngine;
#if !VANILLA
using Invector;
#endif

namespace Shadex
{
    /// <summary>
    /// Rotate parent game object around an angle
    /// </summary>
#if !VANILLA
    [vClassHeader("GAMEOBJECT ROTATE AROUND", iconName = "triggerIcon")]
    public class GameObjectRotateAround : vMonoBehaviour
    {
#else
    public class GameObjectRotateAround : MonoBehaviour {
#endif
        /// <summary>Angle of rotation.</summary>
        public float RotationAngle = 20;

        /// <summary>
        /// Apply the rotation once per frame.
        /// </summary>
        void Update()
        {
            transform.RotateAround(Vector3.zero, Vector3.up, RotationAngle * Time.deltaTime);
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
