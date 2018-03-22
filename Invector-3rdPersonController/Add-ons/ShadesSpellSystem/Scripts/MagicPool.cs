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
    /// Magic pooling, keeps a stack of spells ready for smoother FPS
    /// </summary>
    /// <remarks>
    /// Whilst this magic pool automatically fills via the spawner, prewarming it with 
    /// the most common spells will produce better frame rates.
    /// </remarks>
#if !VANILLA
    [vClassHeader("MAGIC POOL", iconName = "inputIcon")]
    public class MagicPool : vMonoBehaviour
    {
#else
    public class MagicPool : MonoBehaviour {
#endif
        /// <summary>Default minimum instances to load into the magic pool (for when not prewarming the pool).</summary>
        [Tooltip("Default minimum instances to load into the magic pool (for when not prewarming the pool)")]
        public int DefaultInitialSize = 3;

        /// <summary>Default maximum to retain in the pool whilst the scene is running (for when not prewarming the pool).</summary>
        [Tooltip("Default maximum to retain in the pool whilst the scene is running (for when not prewarming the pool)")]
        public int DefaultMaxSize = 50;

        /// <summary>Fill this pool with all magic spells (link to via the property id if prewarming the pool).</summary>
        [Tooltip("Fill this pool with all magic spells (link to via the property id if prewarming the pool)")]
        public List<MagicPoolItem> PooledMagic = new List<MagicPoolItem>();

        /// <summary>
        /// Set the pool to not destroy on scene load.
        /// </summary>
        private void Awake()
        {
            DontDestroyOnLoad(transform.gameObject);
        }

        /// <summary>
        /// Prewarm the pool
        /// </summary>
        void Start()
        {
            GlobalFuncs.mpi = this;  // store reference to this class for the spawner fast access
            for (int iSlot = 0; iSlot < PooledMagic.Count; iSlot++)
            {  // process all magic slots in the pool
                if (PooledMagic[iSlot].lgoInstances.Count == 0)
                {  // ensure not already filled via spell trigger
                    PooledMagic[iSlot].iSlotID = iSlot;  // set the index so that the internal class knows where in the array it is
                    PooledMagic[iSlot].PoolIsEmpty();  // fill up the pool for this slot         
                }
            }
        }  

        /// <summary>
        /// Gather a prefab from the magical pool when the pool id is known to a specific transform.
        /// </summary>
        /// <param name="Slot">Pool slot to gather from.</param>
        /// <param name="SpawnPoint">Transform to move the gathered game object to.</param>
        /// <returns></returns>
        public GameObject GatherFromPool(int Slot, Transform SpawnPoint)
        {
            if (GlobalFuncs.DEBUGGING_MESSAGES)
            {
                Debug.Log("Gathering magic from the known pool id " + (Slot + 1));
            }
            if (PooledMagic[Slot].lgoInstances.Count == 0)
            {  // all magic used from pool
                PooledMagic[Slot].PoolIsEmpty();  // restock
            }
            GameObject goInstance = PooledMagic[Slot].lgoInstances[0];  // take the pointer to the instance
            PooledMagic[Slot].lgoInstances.RemoveAt(0);  // remove from the available instances list
            goInstance.transform.parent = SpawnPoint;  // parent to the spawn point
            goInstance.transform.position = SpawnPoint.position;  // move to the spawn point
            goInstance.transform.rotation = SpawnPoint.rotation;  // and match its rotation
            return goInstance;  // return the clone instance            
        }

        /// <summary>
        /// Gather a prefab from the magical pool when the pool id is known to a specific position/rotation.
        /// </summary>
        /// <param name="Slot">Pool slot to gather from.</param>
        /// <param name="Position">Position to move the gathered game object to.</param>
        /// <param name="Rotation">Rotation to set the gathered game object to.</param>
        /// <returns></returns>
        public GameObject GatherFromPool(int Slot, Vector3 Position, Quaternion Rotation)
        {
            if (GlobalFuncs.DEBUGGING_MESSAGES)
            {
                Debug.Log("Gathering magic from the known pool id " + (Slot + 1));
            }
            if (PooledMagic[Slot].lgoInstances.Count == 0)
            {  // all magic used from pool
                PooledMagic[Slot].PoolIsEmpty();  // restock                
            }
            GameObject goInstance = PooledMagic[Slot].lgoInstances[0];  // take the pointer to the instance
            PooledMagic[Slot].lgoInstances.RemoveAt(0);  // remove from the available instances list
            goInstance.transform.parent = null;  // remove parent
            goInstance.transform.position = Position;  // direct set position
            goInstance.transform.rotation = Rotation;  // also rotation
            return goInstance;  // return the clone instance
        }

        /// <summary>
        /// Return the pool slot id for the specified prefab, creating a new pool slot if not found.
        /// </summary>
        /// <param name="Prefab">Prefab to search the pool for.</param>
        /// <param name="WhichTarget">Target filter to search the pool for.</param>
        /// <returns>ID of the pool slot.</returns>
        public int iFindPoolSlotID(GameObject Prefab, SpawnTarget WhichTarget)
        {
            MagicPoolItem mpiFound = PooledMagic.FirstOrDefault(i => i.Prefab == Prefab && i.Target == WhichTarget);  // search slots for the prefab
            if (mpiFound != null)
            {  // found?
                return mpiFound.iSlotID;  // return the slot index
            }
            else
            {  // nope
                PooledMagic.Add(new MagicPoolItem(PooledMagic.Count, Prefab, DefaultInitialSize, DefaultMaxSize, WhichTarget));  // create new slot
                return PooledMagic.Count - 1;  // return new slot index
            }
        }

        /// <summary>
        /// Gather from the magical pool when the pool slot id is unknown (or needs creating) by transform.
        /// </summary>
        /// <param name="Slot">Pool slot ID return by reference.</param>
        /// <param name="Prefab">Prefab to find.</param>
        /// <param name="SpawnPoint">Transform to move the gathered game object to.</param>
        /// <param name="WhichTarget">Target filter to search the pool for.</param>
        /// <returns>Game object gathered from the pool.</returns>
        public GameObject GatherFromPool(ref int Slot, GameObject Prefab, Transform SpawnPoint, SpawnTarget WhichTarget)
        {
            Slot = iFindPoolSlotID(Prefab, WhichTarget) + 1;
            if (GlobalFuncs.DEBUGGING_MESSAGES)
            {
                Debug.Log("Gathering magic from the pool id " + Slot + " for " + Prefab.name);
            }
            return GatherFromPool(Slot - 1, SpawnPoint);
        }

        /// <summary>
        /// Gather from the magical pool when the pool slot id is unknown (or needs creating) by direct position.
        /// </summary>
        /// <param name="Slot">Pool slot ID return by reference.</param>
        /// <param name="Prefab">Prefab to find.</param>
        /// <param name="Position">Position to move the gathered game object to.</param>
        /// <param name="Rotation">Rotation to set the gathered game object to.</param>
        /// <param name="WhichTarget">Target filter to search the pool for.</param>
        /// <returns>Game object gathered from the pool.</returns>
        public GameObject GatherFromPool(ref int Slot, GameObject Prefab, Vector3 Position, Quaternion Rotation, SpawnTarget WhichTarget)
        {
            Slot = iFindPoolSlotID(Prefab, WhichTarget) + 1;
            if (GlobalFuncs.DEBUGGING_MESSAGES)
            {
                Debug.Log("Gathering magic from the pool id " + Slot + " for " + Prefab.name);
            }
            return GatherFromPool(Slot - 1, Position, Rotation);
        }

        /// <summary>
        /// Return used magic fx to the slot it came from.
        /// </summary>
        /// <param name="Slot">ID of the pool slot to return to.</param>
        /// <param name="Instance">Game object instance to return to the pool slot.</param>
        public void ReturnToThePool(int Slot, GameObject Instance)
        {
            Instance.SetActive(false);  // disable fx
            Instance.transform.parent = PooledMagic[Slot - 1].goSlot.transform;  // re-parent
            PooledMagic[Slot - 1].lgoInstances.Add(Instance);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Force default values for new list members in the inspector.
        /// </summary>
        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                foreach (MagicPoolItem mpi in PooledMagic)
                {
                    mpi.New();
                }
            }
        }  
#endif
    }

    /// <summary>
    /// Pool slot item for the magical pool list.
    /// </summary>
    [Serializable]
    public class MagicPoolItem
    {
        /// <summary>Prefab to add to the magic pool.</summary>
        [Tooltip("Prefab to add to the magic pool")]
        public GameObject Prefab;

        /// <summary>Minimum to initially load into the magic pool on scene load.</summary>
        [Tooltip("Minimum to initially load into the magic pool on scene load")]
        public int InitialSize;

        /// <summary>Maximum to retain in the pool whilst the scene is running.</summary>
        [Tooltip("Maximum to retain in the pool whilst the scene is running")]
        public int MaxSize;

        /// <summary>Spell target, layers and tags are set when the pool is filled.</summary>
        [Tooltip("Spell target, layers and tags are set when the pool is filled")]
        public SpawnTarget Target;

        // internal
        [HideInInspector] public int iSlotID;  // slot index (for fast access)
        [HideInInspector] public GameObject goSlot;   // slot to store the pooled magic 
        [HideInInspector] public GameObject goMasterInstance;   // never destroyed, all instances are cloned from this 
        [HideInInspector] public List<GameObject> lgoInstances;  // current list of inactive spell instances

#if UNITY_EDITOR
        [HideInInspector] public bool bInitialised;

        /// <summary>
        /// Defaults.
        /// </summary>
        public void New()
        {
            if (!bInitialised)
            {
                bInitialised = true;
                InitialSize = 5;
                MaxSize = 50;
            }
        }
#endif

        /// <summary>
        /// Initialise a new pool slot.
        /// </summary>
        /// <param name="WhichSlotID">ID of this pool slot.</param>
        /// <param name="WhichPrefab">Prefab to instantiate.</param>
        /// <param name="WhichInitialSize">Minimum/initial pool size.</param>
        /// <param name="WhichMaxSize">Maximum pool size.</param>
        /// <param name="WhichTarget">Targeting to apply to the instance.</param>
        public MagicPoolItem(int WhichSlotID, GameObject WhichPrefab, int WhichInitialSize, int WhichMaxSize, SpawnTarget WhichTarget)
        {
            // initialise
            iSlotID = WhichSlotID;            
            Prefab = WhichPrefab;             
            InitialSize = WhichInitialSize;    
            MaxSize = WhichMaxSize;            
            Target = WhichTarget;

            // create initial instances
            BuildSlot();  
        }  

        /// <summary>
        /// Build the pool slot instances and settings.
        /// </summary>
        public void BuildSlot()
        {
            // create the slot
            string FriendlyTargetName = (Target == SpawnTarget.Any ? "" : (Target == SpawnTarget.Enemy ? "_Player" : "_AI"));
            goSlot = new GameObject();    // create new slot
            goSlot.name = "Slot(" + Prefab.name + ")" + FriendlyTargetName;  // name it
            goSlot.transform.parent = GlobalFuncs.mpi.transform;  // append the hierarchy beneath the pool slot

            // build initial for cloning            
            goMasterInstance = UnityEngine.Object.Instantiate(Prefab) as GameObject;  // create the master from the prefab
            goMasterInstance.SetActive(false);  // deactivate it
            goMasterInstance.transform.parent = goSlot.transform;  // keep it tidy in the hierarchy
            goMasterInstance.name = goMasterInstance.name.Replace("(Clone)", "") + FriendlyTargetName;

            // enforce pool return on root game object
            var mr = goMasterInstance.GetComponent<MagicPool_Return>();
            if (!mr)
            {  // not found create
                mr = goMasterInstance.AddComponent<MagicPool_Return>();
            }
            mr.PoolSlotID = iSlotID + 1;  // set slot id for return

            // set targeting, pool slot id on components
            GlobalFuncs.SetComponentTagsSlotsAndLayers(ref goMasterInstance, Target, iSlotID);

            // fill up the pool
            lgoInstances = new List<GameObject>();  // initialise the pool list
            PoolIsEmpty();  // create new disabled instances in the slot  

            if (GlobalFuncs.DEBUGGING_MESSAGES)
            {
                Debug.Log("Added slot to the pool for " + Prefab.name);
            }
        }

        /// <summary>
        /// Create a new clones of the master copy of the spell to refill the pool with
        /// </summary>
        public void PoolIsEmpty()
        {
            for (int i = 0; i < InitialSize; i++)
            {  // iterate till initial pool filled
                lgoInstances.Add(UnityEngine.Object.Instantiate(goMasterInstance));  // clone the master
                lgoInstances[lgoInstances.Count - 1].transform.parent = goSlot.transform; // keep it tidy in the hierarchy
                lgoInstances[lgoInstances.Count - 1].name = lgoInstances[lgoInstances.Count - 1].name.Replace("(Clone)", "");  // tidy name
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
