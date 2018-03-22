using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if !VANILLA
using Invector;
#endif

namespace Shadex
{
    /// <summary>
    /// Apply physics effects to a game object that will be enabled upon a condition.
    /// </summary>
#if !VANILLA
    [vClassHeader("MAGIC PROJECTILE PHYSICS", iconName = "triggerIcon")]
    public class MagicProjectilePhysics : vMonoBehaviour
    {
#else
    public class MagicProjectilePhysics : MonoBehaviour {
#endif
        /// <summary>Enable the effect.</summary>
        [Tooltip("Enable the effect")]
        public bool UseExplosion = true;

        /// <summary>Scale of the force applied to affected game objects.</summary>
        [Tooltip("Scale of the force applied to affected game objects")]
        public float power = 250f;

        /// <summary>Range to find game objects to apply the force to and width of the applied force sphere.</summary>
        [Tooltip("Range to find game objects to apply the force to and width of the applied force sphere")]
        public float radius = 5;

        /// <summary>Height of the applied force sphere.</summary>
        [Tooltip("Height of the applied force sphere")]
        public float height = 3f;

        /// <summary>Type of force to apply.</summary>
        [Tooltip("Type of force to apply")]
        public ForceMode forceMode;

        /// <summary>Affected layers, note the physics layers are appended to the target layers, autoset when launched from the animator/spawner.</summary>
        [Tooltip("Affected layers, note the physics layers are appended to the target layers, autoset when launched from the animator/spawner")]
        public SpawnTarget TargetLayers;


        /// <summary>
        /// Occurs when parent game object is activated, apply physics explosion to surroundings.
        /// </summary>
        void OnEnable()
        {  
            if (UseExplosion == true)  // enable explosion physics? 
            {
                // set tags & layers appropriately
                LayerMask applyForceLayer;
                switch (TargetLayers)
                {
                    case SpawnTarget.Friend:
                        applyForceLayer = (1 << GlobalFuncs.targetingLayerMaskFriend.value) | (1 << GlobalFuncs.targetingLayerMaskPhysics.value);
                        break;
                    case SpawnTarget.Enemy:
                        applyForceLayer = (1 << GlobalFuncs.targetingLayerMaskEnemy.value) | (1 << GlobalFuncs.targetingLayerMaskPhysics.value);
                        break;
                    default:
                        applyForceLayer = (1 << GlobalFuncs.targetingLayerMaskAll.value) | (1 << GlobalFuncs.targetingLayerMaskPhysics.value);
                        break;
                }

                // feel the force
                Collider[] colliders = Physics.OverlapSphere(transform.position, radius, applyForceLayer);  // find colliders within range
                foreach (Collider hit in colliders)
                {  // process all
                    Rigidbody rb = hit.GetComponent<Rigidbody>();  // does the collider have a rigid body attacked
                    if (rb)
                    {  // found rigid body
                        rb.AddExplosionForce(power, transform.position, radius, height, forceMode);  // add the force of the explosion 
                    }
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
