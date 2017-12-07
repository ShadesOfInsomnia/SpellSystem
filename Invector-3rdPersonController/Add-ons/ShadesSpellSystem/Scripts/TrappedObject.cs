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
    /// Turn any game object into a trap trigger that spawns.
    /// </summary>
    /// <remarks>
    /// When a game object within the specified layer triggers the trap (via proximity or collision), the array of spawns are instantiated.
    /// The global spawner options class is used to provide a large range of potential spawn options.
    /// </remarks>
#if !VANILLA
    [vClassHeader("TRAPPED OBJECT", iconName = "triggerIcon")]
    public class TrappedObject : vMonoBehaviour
    {
#else
    public class TrappedObject : MonoBehaviour {
#endif
        /// <summary>Enable proximity mode to react when a game object is within the proximity radius of the trapped object.</summary>
        [Header("-- Proximity Trap --")]
        [Tooltip("Enable proximity mode")]
        public bool ProximityEnter = false;

        /// <summary>Specify the proximity trigger distance radius.</summary>
        [Tooltip("Proximity trigger distance")]
        public float Proximity = 5f;

        /// <summary>Proximity distance check frequency.</summary>
        [Tooltip("Proximity distance check frequency")]
        public float CheckRate = 0.8f;

        /// <summary>Enable collision mode to react when a game object is within the attached collider of the trapped object.</summary>
        [Header("-- Collider Trap --")]
        [Tooltip("React to enter collider")]
        public bool ColliderEnter = false;

        /// <summary>Proximity distance check frequency.</summary>
        [Tooltip("Proximity distance check frequency")]
        public bool ResetTrap = false;

        /// <summary>Maximum number of times to reset the trap, if collider enter enabled.</summary>
        [Tooltip("Maximum number of times to reset the trap, if collider enter enabled")]
        public int MaxResetTrap = 3;

        /// <summary>Layers allowed to trigger the collider enter trap.</summary>
        [Tooltip("Layers allowed to trigger the collider enter trap")]
        public LayerMask TriggerLayers = 1 << 8;

        /// <summary>Target assignment, effects are different depending on spawns.</summary>
        [Header("-- Spawn Options --")]
        [Tooltip("Target assignment, effects are different depending on spawns")]
        public SpawnTarget Target;

        /// <summary>Force spawned object to face the triggering object.</summary>
        [Tooltip("Force spawned object to face the triggering object")]
        public bool ForceFaceTrigger;

        /// <summary>Delay before starting the spawn all loop once the trap has been triggered.</summary>
        [Tooltip("Delay before starting the spawn")]
        public float Delay;

        /// <summary>Process list in order sequential is the default, eg list element 0 all copies, then element 1 all copies.  Enable to process elm 0-x looped until no more to spawn.</summary>
        [Tooltip("Process list in order sequential is the default, eg list element 0 all copies, then element 1 all copies.  Enable to process elm 0-x looped until no more to spawn")]
        public bool NoneSequential;

        /// <summary>Standard spawner options list class, to be executed when the trap is triggered.</summary>
        [Tooltip("List of traps (enemies/spells) to spawn")]
        public List<SpawnerOptionsDelayedSequence> Traps = new List<SpawnerOptionsDelayedSequence>();

        // internal
        GameObject goPlayer;

        /// <summary>
        /// Find the player for proximity mode
        /// </summary>
        void Start()
        {
            if (ProximityEnter)
            {  // proximity trap?
                goPlayer = GlobalFuncs.FindPlayerInstance();
            }
        }  

        /// <summary>
        /// Less efficient than collision mode, check at the specified frequency whether the 
        /// </summary>
        void Update()
        {
            if (ProximityEnter)  // proximity trap enabled?
            {  
                if (Time.time > CheckRate)  // time for a distance check?          
                {            
                    if (Vector3.Distance(transform.position, goPlayer.transform.position) < Proximity)  // within range
                    {  
                        StartCoroutine(GlobalFuncs.SpawnAllDelayed(Traps, Delay, NoneSequential, transform, null, 0, ForceFaceTrigger, Target));  // trigger all traps in the array
                        ProximityEnter = false;  // disable the trap
                    }
                }
            }
        }  

        /// <summary>
        /// Occurs when the attached collider is entered within by another collider, layers are in play to filter the potential collision sources.
        /// </summary>
        /// <param name="col">The collider that triggered the trap</param>
        void OnCollisionEnter(Collision col)
        {
            if (ColliderEnter)  // collider trap?
            {  
                if ((TriggerLayers.value & 1 << col.gameObject.layer) == 1 << col.gameObject.layer)   // matching layer
                { 
                    if (!ResetTrap)   // not a one off trap
                    { 
                        ColliderEnter = false;  // disable trap reactivation
                    }
                    else  // reset trap enabled
                    {  
                        if (MaxResetTrap <= 1)  // has the number of times reset hasn't passed the max allowed?
                        {  
                            ColliderEnter = false;  // disable trap reactivation
                        }
                        else  // nope
                        {  
                            MaxResetTrap -= 1;  // reduce the reset count
                        }
                    }
                    StartCoroutine(GlobalFuncs.SpawnAllDelayed(Traps, Delay, NoneSequential, transform, null, 0, ForceFaceTrigger, Target));  // trigger all traps the array
                }
            }
        }

        /// <summary>
        /// Trigger the entire spawn trap array directly via delgate or script
        /// </summary>
        public void ManualTriggerTrap()
        {
            StartCoroutine(GlobalFuncs.SpawnAllDelayed(Traps, Delay, NoneSequential, transform, null, 0, ForceFaceTrigger, Target));  // trigger all traps the array
        }

#if UNITY_EDITOR
        /// <summary>
        /// Occurs when the unity inspector validates the properties.  Used to force default 
        /// values for new spawn list members in the inspector
        /// </summary>
        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                foreach (SpawnerOptionsDelayedSequence s in Traps)
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
