using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if !VANILLA
using Invector;
#endif

namespace Shadex {
    /// <summary>
    /// Collectable pickup item on collider entrance.
    /// </summary>
    /// <remarks>
    /// Collider is required with trigger not set.
    /// </remarks>
#if !VANILLA
    [vClassHeader("Collectible Item", iconName = "triggerIcon")]
    public class CollectableItem : vMonoBehaviour {
#else
    public class CollectableItem : MonoBehaviour {
#endif
        /// <summary>Set by the pooling system for return to slot.</summary>
        [HideInInspector]
        public int iPoolSlotID;

        /// <summary>Type of Collectable.</summary>
        [Tooltip("Type of Collectable")]
        public BaseCollectable Type;

        /// <summary>Amount to collect.</summary>
        [Tooltip("Amount to collect")]
        public float Amount = 1f;

        /// <summary>Layers allowed to trigger the collider pickup.</summary>
        [Tooltip("Layers allowed to trigger the collider pickup")]
        public LayerMask TriggerLayers = 1 << 8;

        /// <summary>Audio clip to play on collection.</summary>
        [Tooltip("Audio clip to play on collection")]
        public AudioClip PlayOnCollection;

        /// <summary>Audio source to play the clip.</summary>
        [Tooltip("Audio source to play the clip")]
        public AudioSource SourceOfAudio;

        [Header("Spawn on Collection")]
        /// <summary>Force spawned object to face the triggering object.</summary>
        [Tooltip("Force spawned object to face the triggering object")]
        public bool ForceFaceTrigger;

        /// <summary>Delay before spawning list.</summary>
        [Tooltip("Delay before spawning list")]
        public float Delay;

        /// <summary>Process spawn list in order sequential is the default, eg list element 0 all copies, then element 1 all copies.  Enable to process elm 0-x looped until no more to spawn.</summary>
        [Tooltip("Process spawn list in order sequential is the default, eg list element 0 all copies, then element 1 all copies.  Enable to process elm 0-x looped until no more to spawn")]
        public bool NoneSequential;

        /// <summary>Target assignment, effects are different depending on spawns.</summary>
        [Tooltip("Target assignment, effects are different depending on spawns")]
        public SpawnTarget Target;

        /// <summary>Optional prefab's created upon collection, eg particle effect, spells, enemies, etc.</summary>
        [Tooltip("Optional prefab's created upon collection, eg particle effect, spells, enemies, etc")]
        public List<SpawnerOptionsDelayedSequence> PrefabsToSpawn = new List<SpawnerOptionsDelayedSequence>();

        // internal
        private Coroutine CoDelayedSpawn;

        /// <summary>
        /// Collectable trigger via collider.
        /// </summary>
        /// <param name="col">Collider that has triggered the potential picup</param>
        void OnCollisionEnter(Collision col) {
            if ((TriggerLayers.value & 1 << col.gameObject.layer) == 1 << col.gameObject.layer) {  // matching layer
                CharacterBase levellingSystem = col.gameObject.GetComponent<CharacterBase>();
                if (levellingSystem) {
                    levellingSystem.Collectables[(int)Type].Value += Amount;
                    levellingSystem.ForceUpdateHUD();

                    // play audio
                    if (SourceOfAudio && PlayOnCollection) {
                        SourceOfAudio.clip = PlayOnCollection;
                        SourceOfAudio.Play();
                    }

                    if (PrefabsToSpawn.Count > 0) {
                        CoDelayedSpawn = StartCoroutine(GlobalFuncs.SpawnAllDelayed(PrefabsToSpawn, Delay, NoneSequential, transform, gameObject, iPoolSlotID, ForceFaceTrigger, Target));     // call shared spawner loop    
                    } else { 
                        // remove collectable from the game
                        GlobalFuncs.ReturnToThePoolOrDestroy(iPoolSlotID, gameObject);
                    }
                }
            }
        }

        /// <summary>
        /// Kill the coroutine failsafe.
        /// </summary>
        void OnDisable() {
            if (CoDelayedSpawn != null) {
                StopCoroutine(CoDelayedSpawn);
                CoDelayedSpawn = null;
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
