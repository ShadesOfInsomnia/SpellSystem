using UnityEngine;
using System;

namespace Shadex
{
    /// <summary>
    /// Friend or foe targeting selection
    /// </summary>
    /// <remarks>
    /// Used in many places within the spell system, in many cases auto assigned by the animator or the pooling system based 
    /// upon the tags of the game object that instantiated it.
    /// </remarks>
    public enum SpawnTarget
    {
        Any, Friend, Enemy
    }

    /// <summary>
    /// Centralised spawning of prefabs with options.
    /// </summary>
    /// <remarks>
    /// Critical class that is used by most of the other scripts in some form, allows pooling and a common set of options for all instances when
    /// game objects (often spells) need spawning.  Usually used in an array list format for easy serialising by the inspector.
    /// </remarks>
    [Serializable]
    public class SpawnerOptions
    {
        /// <summary>Target assignment, effects are different depending on spawns, note spells this is auto updated when fired from the animator.</summary>
        [Tooltip("Target assignment, effects are different depending on spawns, note spells this is auto updated when fired from the animator")]
        public SpawnTarget Target;

        /// <summary>Disable the use of pooling for this spawn, useful when the prefab being spawned will not be spawned often in the scene.</summary>
        [Tooltip("Disable the use of pooling for this spawn, useful when the prefab being spawned will not be spawned often in the scene")]
        public bool DoNotPool;

        /// <summary>Element ID+1 of the magic pool slot list (it is ID+1 as when zero means pool slot id not yet known), specify the pool slot element ID+1 here when prewarming the pool.</summary>
        [Tooltip("Element ID+1 of the magic pool slot list (it is ID+1 as when zero means pool slot id not yet known), specify the pool slot element ID+1 here when prewarming the pool")]
        public int PoolSlotId;

        /// <summary>Prefab of the projectile (or enemy) to instantiate.</summary>
        [Tooltip("Prefab of the projectile (or enemy) to instantiate")]
        public GameObject Prefab;

        /// <summary>Offset from the spawn point to create the prefab instance.</summary>
        [Tooltip("Offset from the spawn point to create the prefab instance")]
        public Vector3 Offset;

        /// <summary>Angle from the spawn point to create the prefab instance.</summary>
        [Tooltip("Angle from the spawn point to create the prefab instance")]
        public Vector3 Angle;

        /// <summary>Rotate spawned object randomly.</summary>
        [Tooltip("Rotate spawned object randomly")]
        public RandomRotateOptions RandomRotate;

        /// <summary>Spawn within a sphere randomly options.</summary>
        [Tooltip("Spawn within a sphere randomly options")]
        public RandomSphereOptions RandomSphere;

        /// <summary>Set to greater than zero to check that the radius centered upon the spawn point is empty.</summary>
        [Tooltip("Set to greater than zero to check that the radius centered upon the spawn point is empty")]
        public float EmptySpaceRadius;

        /// <summary>Only enable for teleport so that it can know the source object to transport, or any spells that use the SpellSizeByLevel script.</summary>
        [Tooltip("Only enable for teleport so that it can know the source object to transport, or any spells that use the SpellSizeByLevel script")]
        public bool KeepParent;

        /// <summary>When to kill the instance of the prefab, zero = no destruction.</summary>
        [Tooltip("When to kill the instance of the prefab, zero = no destruction")]
        public float DestructionTimeOut;

        /// <summary>Apply physical force to the specified radius.</summary>
        [Tooltip("Apply physical force to the specified radius")]
        public PhysicsOptions PhysicsForceOptions;

        /// <summary>Audio clip to play when this spawn occurs.</summary>
        [Tooltip("Audio clip to play on spawn")]
        public AudioClip PlayOnSpawn;

        /// <summary>Attached audio source to play the clip on.</summary>
        [Tooltip("Audio source to play the clip")]
        public AudioSource SourceOfAudio;

#if UNITY_EDITOR
        [HideInInspector] public bool bInitialised;

        /// <summary>
        /// Default values
        /// </summary>
        public virtual void New()
        {
            if (!bInitialised)
            {
                bInitialised = true;
                DestructionTimeOut = 2;
                PhysicsForceOptions = new PhysicsOptions();
                PhysicsForceOptions.Radius = 10f;
                PhysicsForceOptions.Force = 20f;
                PhysicsForceOptions.UpwardsForce = 3f;
                PhysicsForceOptions.Mode = ForceMode.Force;
                RandomSphere = new RandomSphereOptions();
                RandomSphere.IncludeX = true;
                RandomSphere.IncludeY = true;
                RandomSphere.IncludeZ = true;
                RandomRotate = new RandomRotateOptions();
            }
        }  
#endif
        /// <summary>
        /// Spawn specified prefab from a transform.
        /// </summary>
        /// <param name="SpawnPoint">Transform to instantiate the prefab at.</param>
        /// <param name="WhichTarget">Targeting selection to pass to the instance.</param>
        /// <returns>The instance of the game object that was instantiated.</returns>
        public GameObject Spawn(Transform SpawnPoint, SpawnTarget WhichTarget)
        {
            Target = WhichTarget;
            GameObject goInstance;
            if (GlobalFuncs.MAGICAL_POOL && !DoNotPool)
            {
                if (PoolSlotId > 0)  // pool slot already found
                {
                    goInstance = GlobalFuncs.TheMagicalPool().GatherFromPool(PoolSlotId - 1, SpawnPoint);  // load from the pool
                }
                else
                {
                    goInstance = GlobalFuncs.TheMagicalPool().GatherFromPool(ref PoolSlotId, Prefab, SpawnPoint, WhichTarget);  // load from the pool
                }
            }
            else
            {
                goInstance = UnityEngine.Object.Instantiate(Prefab, SpawnPoint);  // load from the prefab
                goInstance.SetActive(false);  // disable
                GlobalFuncs.SetComponentTagsSlotsAndLayers(ref goInstance, WhichTarget, -1);  // update the components
            }
            SetSpawnOptions(ref goInstance, PhysicsForceOptions);  // set the options            
            return goInstance;  // pass the spawned object back for potential modification
        } 

        /// <summary>
        /// Spawn to a specific position, rotation
        /// </summary>
        /// <param name="Position">Position to spawn to.</param>
        /// <param name="Rotation">Direction of rotation to spawn to.</param>
        /// <param name="WhichTarget">Target layers to pass to the spawned game object.</param>
        /// <returns>The instance of the game object that was instantiated.</returns>
        public GameObject Spawn(Vector3 Position, Quaternion Rotation, SpawnTarget WhichTarget)
        {
            Target = WhichTarget;
            GameObject goInstance;
            if (GlobalFuncs.MAGICAL_POOL && !DoNotPool)
            {
                if (PoolSlotId > 0)  // pool slot already found
                {
                    goInstance = GlobalFuncs.TheMagicalPool().GatherFromPool(PoolSlotId - 1, Position, Rotation);  // load from the pool
                }
                else
                {
                    goInstance = GlobalFuncs.TheMagicalPool().GatherFromPool(ref PoolSlotId, Prefab, Position, Rotation, WhichTarget);  // load from the pool
                }
            }
            else
            {
                goInstance = UnityEngine.Object.Instantiate(Prefab, Position, Rotation);  // load from the prefab
                goInstance.SetActive(false);  // disable
                GlobalFuncs.SetComponentTagsSlotsAndLayers(ref goInstance, WhichTarget, -1);  // update the components
            }
            SetSpawnOptions(ref goInstance, PhysicsForceOptions);  // set the options    
            return goInstance;  // pass the spawned object back for potential modification
        }  

        /// <summary>
        /// Check whether the prefab being spawned is within an empty space.
        /// </summary>
        /// <param name="Instance">Game object to check whether its current position contains any other game object.</param>
        /// <returns>Success as a boolean.</returns>
        public bool CheckSpaceIsEmpty(GameObject Instance)
        {
            if (EmptySpaceRadius > 0f)  // check whether the spawn space is empty?
            {
                Collider[] colliders = Physics.OverlapSphere(Instance.transform.position, EmptySpaceRadius, GlobalFuncs.targetingLayerMaskAll);  // find all colliders within the space check radius
                bool bSpaceIsEmpty = true;  // initialise flag
                foreach (Collider hit in colliders)  // process all found
                {
                    if (hit.gameObject != Instance)  // not self?
                    {
                        bSpaceIsEmpty = true;  // flag 
                        if (GlobalFuncs.DEBUGGING_MESSAGES)
                        {
                            Debug.Log(hit.name + " is in the space wanted by " + Instance.name);
                        }
                        break;  // process no more
                    }
                }
                return bSpaceIsEmpty;    // invert the not empty space flag
            }
            return true;  // pass check if radius not specified
        }

        /// <summary>
        /// Apply the spawn settings to the instantiated game object
        /// </summary>
        /// <param name="Instance"></param>
        /// <param name="PhysicsInstance"></param>
        protected void SetSpawnOptions(ref GameObject Instance, PhysicsOptions PhysicsInstance)
        {
            Instance.SetActive(false);  // ensure not active
            if (!KeepParent)  // some spells such as teleport DO NOT want this setting
            {
                Instance.transform.parent = null;  // clear
            }
            if (Offset != Vector3.zero)  // offset needed?
            {
                Instance.transform.position = Instance.transform.position + Offset;  // add in the offset
            }
            if (Angle != Vector3.zero)  // angular offset needed?
            {
                Instance.transform.localRotation = Quaternion.Euler(Angle) * Instance.transform.localRotation;  // alter the angle
            }
            RandomSphere.Apply(ref Instance);  // spawn randomly within sphere radius?
            RandomRotate.Apply(ref Instance);  // apply random rotation to the enabled axis's
            PhysicsInstance.Apply(Instance, Target);  // explode with force?
            if (CheckSpaceIsEmpty(Instance))  // spawned within an empty space checked ok?
            {
                if (DestructionTimeOut > 0)  // max length of instance enabled
                {
                    if (GlobalFuncs.MAGICAL_POOL && !DoNotPool)  // pool enabled?
                    {
                        MagicPool_Return mr = Instance.GetComponent<MagicPool_Return>();  // attempt get the pool return component
                        mr.Delay = DestructionTimeOut;  // set the return to pool timeout
                    }
                    else
                    {
                        UnityEngine.Object.Destroy(Instance, DestructionTimeOut);  // set to destroy in 5,4,3...
                    }
                }
            }
            else  // space not empty
            {
                GlobalFuncs.ReturnToThePoolOrDestroy(PoolSlotId, Instance);  // send back to the pool (if enabled)
            }

            // all options set activate
            Instance.SetActive(true);
        }  // set the spawn options
    } 

    /// <summary>
    /// Random position to a game object within the specified sphere radius.
    /// </summary>
    [Serializable]
    public class RandomSphereOptions
    {
        /// <summary>Set to greater than zero to spawn randomly within a sphere radius.</summary>
        [Tooltip("Set to greater than zero to spawn randomly within a sphere radius")]
        public float Radius;

        /// <summary>Include axis X when spawning randomly within sphere radius.</summary>
        [Tooltip("Include axis X when spawning randomly within sphere radius")]
        public bool IncludeX;

        /// <summary>Include axis Y when spawning randomly within sphere radius.</summary>
        [Tooltip("Include axis Y when spawning randomly within sphere radius")]
        public bool IncludeY;

        /// <summary>Include axis Z when spawning randomly within sphere radius.</summary>
        [Tooltip("Include axis Z when spawning randomly within sphere radius")]
        public bool IncludeZ;

        /// <summary>
        /// Apply random position to a game object within the specified radius.
        /// </summary>
        /// <param name="Instance">Game object instance to randomise the position of within a sphere.</param>
        public void Apply(ref GameObject Instance)
        {
            if (Radius > 0f)  // randomise position within a radius enabled?
            {
                UnityEngine.Random.InitState(System.Environment.TickCount);  // reseed
                Vector3 v3NewPosition = Instance.transform.position + (UnityEngine.Random.insideUnitSphere * Radius);  // alter the position
                if (!IncludeX)  // don't include the X plane
                {
                    v3NewPosition.x = Instance.transform.position.x;  // reset 
                }
                if (!IncludeY)  // don't include the Y plane
                {
                    v3NewPosition.y = Instance.transform.position.y;  // reset 
                }
                if (!IncludeZ)  // don't include the Z plane
                {
                    v3NewPosition.z = Instance.transform.position.z;  // reset 
                }
                Instance.transform.position = v3NewPosition;  // update the transform
            }
        }
    }

    /// <summary>
    /// Apply random rotation to a game object.
    /// </summary>
    [Serializable]
    public class RandomRotateOptions
    {
        /// <summary>Apply random rotation on the X axis.</summary>
        [Tooltip("Apply random rotation on the X axis")]
        public bool IncludeX;

        /// <summary>Apply random rotation on the Y axis.</summary>
        [Tooltip("Apply random rotation on the Y axis")]
        public bool IncludeY;

        /// <summary>Apply random rotation on the Z axis.</summary>
        [Tooltip("Apply random rotation on the Z axis")]
        public bool IncludeZ;

        /// <summary>
        /// Apply the rotation to the specified axis's of the instance
        /// </summary>
        /// <param name="Instance">Game object instance to apply the rotation to.</param>
        public void Apply(ref GameObject Instance)
        {
            // seed and randomise the rotation
            UnityEngine.Random.InitState(System.Environment.TickCount);
            Quaternion qNewRotation = Instance.transform.rotation;

            // apply to the included axis
            if (IncludeX)
            {
                qNewRotation.x = UnityEngine.Random.Range(0, 360);
            }
            if (IncludeY)
            {
                qNewRotation.y = UnityEngine.Random.Range(0, 360);
            }
            if (IncludeZ)
            {
                qNewRotation.z = UnityEngine.Random.Range(0, 360);
            }

            // apply to the game object instance
            Instance.transform.rotation = qNewRotation;
        }
    }

    /// <summary>
    /// Explosion physics force options.
    /// </summary>
    [Serializable]
    public class PhysicsOptions
    {
        /// <summary>Enable physics explosion localised on the spawn point (or animator root transform for particles).</summary>
        [Tooltip("Enable physics explosion localised on the spawn point (or animator root transform for particles)")]
        public bool Enabled;

        /// <summary>The radius of the sphere within which the explosion has its effect.</summary>
        [Tooltip("The radius of the sphere within which the explosion has its effect")]
        public float Radius;

        /// <summary>The force of the explosion (which may be modified by distance).</summary>
        [Tooltip("The force of the explosion (which may be modified by distance)")]
        public float Force;

        /// <summary>Adjustment to the apparent position of the explosion to make it seem to lift objects.</summary>
        [Tooltip("Adjustment to the apparent position of the explosion to make it seem to lift objects.")]
        public float UpwardsForce;

        /// <summary>The method used to apply the force to its targets.</summary>
        [Tooltip("The method used to apply the force to its targets")]
        public ForceMode Mode;

        /// <summary>
        /// Apply the specified force to surrounding game objects that match the targeting layers
        /// </summary>
        /// <param name="Instance">Game object to center the application of force radius on.</param>
        /// <param name="Target">Target game objects to apply force to.</param>
        public void Apply(GameObject Instance, SpawnTarget Target)
        {
            if (Enabled)  // physics enabled?
            {
                // find all colliders within the blast radius
                LayerMask TargetLayers;
                switch (Target)
                {
                    case SpawnTarget.Friend:
                        TargetLayers = (1 << GlobalFuncs.targetingLayerMaskFriend.value) | (1 << GlobalFuncs.targetingLayerMaskPhysics);
                        break;
                    case SpawnTarget.Enemy:
                        TargetLayers = (1 << GlobalFuncs.targetingLayerMaskEnemy.value) | (1 << GlobalFuncs.targetingLayerMaskPhysics);
                        break;
                    default:
                        TargetLayers = (1 << GlobalFuncs.targetingLayerMaskAll.value) | (1 << GlobalFuncs.targetingLayerMaskPhysics);
                        break;
                }
                Collider[] colliders = Physics.OverlapSphere(Instance.transform.position, Radius, TargetLayers);

                // process all found
                foreach (Collider hit in colliders)
                {
                    Rigidbody rb = hit.GetComponent<Rigidbody>();  // attempt grab the rigid body
                    if (rb)  // found a rigid body to apply force to?
                    {
                        rb.AddExplosionForce(Force, Instance.transform.position, Radius, UpwardsForce, Mode);  // explode
                    }
                }
            }
        }
    }  

    /// <summary>
    /// For the animator state update to call as the casting animation plays.
    /// </summary>
    [Serializable]
    public class SpawnerOptionsOverTime : SpawnerOptions
    {
        /// <summary>Start time range to spawn instances over.</summary>
        [Tooltip("Start time range to spawn instances over")]
        public float SpawnStartTime;

        /// <summary>End time range to spawn instances over.</summary>
        [Tooltip("End time range to spawn instances over")]
        public float SpawnEndTime;

        /// <summary>Number of instances to spawn averaged between the time range.</summary>
        [Tooltip("Number of instances to spawn averaged between the time range")]
        public int NumberToSpawn;

        /// <summary>Use the animator root transform rather than the magic spawn point.</summary>
        [Tooltip("Use the animator root transform rather than the magic spawn point")]
        public bool UseRootTransform;


        // internal
        float fSpawnRate;
        [HideInInspector] public int SpawnedSoFar = 999;
        [HideInInspector] public GameObject goSpawnedParticle;
        [HideInInspector] public bool ShowAdvancedOptions;

#if UNITY_EDITOR
        /// <summary>
        /// Default values
        /// </summary>
        public override void New()
        {
            if (!bInitialised)
            {
                base.New();
                SpawnStartTime = 0.5f;
                SpawnEndTime = 0.5f;
                NumberToSpawn = 1;
            }
        }  
#endif
        /// <summary>
        /// Spawn from a transform at a specific time, called repeatedly during an animator state
        /// </summary>
        /// <param name="SpawnPoint"></param>
        /// <param name="Time"></param>
        /// <param name="Animator"></param>
        /// <param name="WhichTarget"></param>
        /// <returns>Reference to the instance of the instantiated game object</returns>
        public GameObject Spawn(Transform SpawnPoint, float Time, Animator Animator, SpawnTarget WhichTarget)
        {
            if (Time < 0.1 && SpawnedSoFar >= NumberToSpawn)  // at the start of the animation and not already reset the counter
            {
                SpawnedSoFar = 1;  // reset (1 rather than zero to avoid undesired instant spawn
                fSpawnRate = ((SpawnEndTime - SpawnStartTime) / NumberToSpawn);  // cache 
            }
            else if (SpawnedSoFar < NumberToSpawn + 1)  // still more instances to spawn          
            {
                if (Time > (SpawnStartTime + (fSpawnRate * SpawnedSoFar)))  // time for the next spawn
                {
                    SpawnedSoFar += 1;  // count number spawned   

                    if (Prefab)  // only spawn if the prefab is set
                    { 
                        if (UseRootTransform)  // spawn from the animator base      
                        {
                            goSpawnedParticle = Spawn(Animator.rootPosition, Quaternion.identity, WhichTarget);
                        }
                        else  // use the magic spawn point
                        {
                            goSpawnedParticle = Spawn(SpawnPoint, WhichTarget);  // create the spawn instance
                        }
                    }

                    if (SourceOfAudio && PlayOnSpawn) // play audio?
                    {  
                        if (!SourceOfAudio.isPlaying)  // ignore if already playing
                        {  
                            SourceOfAudio.clip = PlayOnSpawn;  // set the clip
                            SourceOfAudio.Play();  // play the clip
                        }
                    }

                    return goSpawnedParticle;  // return a reference to the instance
                }
            }
            return null;  // drop out clean
        } 

    }  

    /// <summary>
    /// For the enumerator based co-routines to call spawns with potential delays.
    /// </summary>
    [Serializable]
    public class SpawnerOptionsDelayedSequence : SpawnerOptions
    {
        /// <summary>Delay till spawning the next in the list.</summary>
        [Tooltip("Delay till spawning the next in the list")]
        public float DelayTillNext;

        /// <summary>Number of instances of the prefab to spawn.</summary>
        [Tooltip("Number of instances to spawn")]
        public int NumberToSpawn;



#if UNITY_EDITOR
        /// <summary>
        /// Default values
        /// </summary>
        public override void New()
        {
            if (!bInitialised)
            {
                base.New();
                NumberToSpawn = 1;
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
