using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
#if !VANILLA
using Invector;
#endif

namespace Shadex
{
    /// <summary>
    /// Teleport's the parent object to another AI/player depending upon source.
    /// </summary>
    /// <remarks>
    /// Keep parent option MUST be enabled on the animator spawner options.
    /// </remarks>
#if !VANILLA
    [vClassHeader("MAGIC TELEPORT", iconName = "ammoIcon")]
    public class MagicTeleport : vMonoBehaviour
    {
#else
    public class MagicTeleport : MonoBehaviour {        
#endif
        /// <summary>Pool slot id for the teleport, only set if pre warming the pool.</summary>
        [Tooltip("Pool slot id for the teleport, only set if pre warming the pool")]
        public int PoolSlotID;

        /// <summary>Delay between particle and teleporting.</summary>
        [Header("Takeoff")]
        [Tooltip("Delay between particle and teleporting")]
        public float TeleportDelay = 1;

        /// <summary>Prefab for the teleport takeoff particle.</summary>
        [Tooltip("Prefab for the teleport particle")]
        public SpawnerOptionsDelayedSequence TakeOffParticle = new SpawnerOptionsDelayedSequence();

        /// <summary>Target assignment, note this is auto updated when fired from the animator.</summary>
        [Header("Landing")]
        [Tooltip("Target assignment, note this is auto updated when fired from the animator")]
        public SpawnTarget Target;

        /// <summary>Max Distance to find target.</summary>
        [Tooltip("Max Distance to find target")]
        public float MaxDistance = 25;

        /// <summary>Teleport to the nearest target.</summary>
        [Tooltip("Teleport to the nearest target")]
        public bool NearestTarget = true;

        /// <summary>Offset from the target after teleport.</summary>
        [Tooltip("Offset from the target after teleport")]
        public float Offset = -2;

        /// <summary>Height offset from the target after teleport.</summary>
        [Tooltip("Height offset from the target after teleport")]
        public float HeightAdjust = 0;

        /// <summary>Prefab for the teleport particle.</summary>
        [Tooltip("Prefab for the teleport particle")]
        public SpawnerOptionsDelayedSequence LandingParticle = new SpawnerOptionsDelayedSequence();

        // internal
        [HideInInspector] public string[] TargetTags;
        [HideInInspector] public LayerMask TargetLayers;
        [HideInInspector] public GameObject goTeleportMe;
        [HideInInspector] public Transform tSpellTarget;
        private Coroutine CoTeleport;

        /// <summary>
        /// Occurs when enabled by the magical pool, sets up the teleport.
        /// </summary>
        void OnEnable()
        {
            if (TargetTags != null)
            {  // ensure creating pool master copy doesnt activate it
                // set gameobject to teleport
                if (transform.parent)
                {  // if run via the vMagicAttackBehaviour class then set the KeepParent flag
                    goTeleportMe = transform.parent.transform.parent.gameObject;  // take the individual to teleport to, enemy or player
                    goTeleportMe.transform.parent = null;  // unparent
                }
                else
                {  // no parent so assume player as fallback
                    goTeleportMe = GlobalFuncs.FindPlayerInstance();  // find the player gameobject
                }

                // init the teleport
                if (goTeleportMe)
                {  // failsafe, should always be true
                    if (goTeleportMe.tag == "Player")
                    {  // am i the player
                        tSpellTarget = GlobalFuncs.GetLockOn();   // do i have a lock
                    }
                    if (!tSpellTarget)
                    {  // no lock or enemy ai
                        tSpellTarget = GlobalFuncs.GetTargetWithinRange(goTeleportMe.transform.position, MaxDistance, TargetLayers, TargetTags, NearestTarget, false, 0f, true);
                    }

                    // lets go, unless no target
                    if (tSpellTarget)
                    {  // valid?
                        CoTeleport = StartCoroutine(TeleportAtTarget());  // beam me up
                    }
                }
            }
        } 

        /// <summary>
        /// Ensure the coroutine that teleport's is stopped when returing to the pool.
        /// </summary>
        void OnDisable()
        {
            if (CoTeleport != null)
            {
                StopCoroutine(CoTeleport);
                CoTeleport = null;
            }
        }  

        /// <summary>
        /// Teleport processing, called via coroutine.
        /// </summary>
        /// <returns>IEnumerator for the delay between take off and landing.</returns>
        public IEnumerator TeleportAtTarget()
        {
            // Spawn the take off particle effect
            if (TakeOffParticle.Prefab)
            {  // if available
                TakeOffParticle.KeepParent = false;  // ensure leave the particle at the takeoff location
                if (TakeOffParticle.NumberToSpawn == 0) TakeOffParticle.NumberToSpawn = 1;  // failsafe
                TakeOffParticle.Spawn(goTeleportMe.transform, SpawnTarget.Any);  // pull fx from the pool
            }

            // Delay time between particle effect and Teleport
            yield return new WaitForSeconds(TeleportDelay);

            // move the individual to the telport location offset, facing the target      
            goTeleportMe.transform.position = tSpellTarget.position + tSpellTarget.forward * Offset;
            Vector3 v3Temp = tSpellTarget.position;
            v3Temp.y = tSpellTarget.position.y + HeightAdjust;
            goTeleportMe.transform.LookAt(v3Temp);

            // Spawn the landing off particle effect
            if (LandingParticle.Prefab)
            {  // if available
                if (TakeOffParticle.NumberToSpawn == 0) TakeOffParticle.NumberToSpawn = 1;  // failsafe
                LandingParticle.Spawn(goTeleportMe.transform, SpawnTarget.Any);  // pull fx from the pool
            }

            // clean up
            yield return new WaitForSeconds(TeleportDelay);
            GlobalFuncs.ReturnToThePoolOrDestroy(PoolSlotID, gameObject);  // kill or return teleport to the pool
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
