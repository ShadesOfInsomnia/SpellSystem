using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
#if !VANILLA
using Invector;
#endif

namespace Shadex
{
    /// <summary>
    /// Starts the standard spawning loop with a delay on enable.
    /// </summary>
#if !VANILLA
    [vClassHeader("DELAYED SPAWN", iconName = "triggerIcon")]
    public class DelayedSpawn : vMonoBehaviour
    {
#else
    public class DelayedSpawn : MonoBehaviour {
#endif
        /// <summary>Delay before starting the spawn.</summary>
        [Tooltip("Delay before starting the spawn")]
        public float Delay;

        /// <summary>Force spawned object to face the triggering object.</summary>
        [Tooltip("Force spawned object to face the triggering object")]
        public bool ForceFaceTrigger;

        /// <summary>Process list in order sequential is the default, eg list element 0 all copies, then element 1 all copies.  Enable to process elm 0-x looped until no more to spawn.</summary>
        [Tooltip("Process list in order sequential is the default, eg list element 0 all copies, then element 1 all copies.  Enable to process elm 0-x looped until no more to spawn")]
        public bool NoneSequential;

        /// <summary>Target assignment, effects are different depending on spawns.</summary>
        [Tooltip("Target assignment, effects are different depending on spawns")]
        public SpawnTarget Target;

        /// <summary>List of gameobjects (enemies/spells) to spawn.</summary>
        [Tooltip("List of gameobjects (enemies/spells) to spawn")]  // do not set keep parent on any spawns
        public List<SpawnerOptionsDelayedSequence> SpawnMe = new List<SpawnerOptionsDelayedSequence>();

        /// <summary>Set by the pooling system for return to slot.</summary>
        [HideInInspector] public int iPoolSlotID;

        // internal
        private Coroutine CoDelayedSpawn;

        /// <summary>
        /// Simple run the spawn loop with delays (from other spawner's).
        /// </summary>
        void Start()
        {
            CoDelayedSpawn = StartCoroutine(GlobalFuncs.SpawnAllDelayed(SpawnMe, Delay, NoneSequential, transform, gameObject, iPoolSlotID, ForceFaceTrigger, Target));  // trigger all traps the the array
        }

        /// <summary>
        /// Kill the coroutine failsafe.
        /// </summary>
        void OnDisable()
        {
            if (CoDelayedSpawn != null)
            {
                StopCoroutine(CoDelayedSpawn);
                CoDelayedSpawn = null;
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Force default values for new list members in the inspector
        /// </summary>
        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                foreach (SpawnerOptionsDelayedSequence s in SpawnMe)
                {
                    s.New();
                }
            }
        }  
#endif
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
