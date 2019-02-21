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
    /// Magic projectile options and actions.
    /// </summary>
#if !VANILLA
    [vClassHeader("MAGIC PROJECTILE", iconName = "ammoIcon")]
    public class MagicProjectile : vMonoBehaviour
    {
#else
    public class MagicProjectile : MonoBehaviour {
#endif        
        [Header("Movement")]
        /// <summary>Forward Speed of the projectile.</summary>
        [Tooltip("Forward Speed of the projectile")]
        public float ForwardSpeed = 5f;

        /// <summary>Adjusts target collide height.</summary>
        [Tooltip("Adjusts target collide height")]
        public float HeightAdjust = 1;

        /// <summary>List of child game objects to rotate whilst moving towards the target, eg magic hammers spinning.</summary>
        [Tooltip("List of child game objects to rotate whilst moving towards the target, eg magic hammers spinning")]
        public List<RotateChild> RotateChildren = new List<RotateChild>();

        [Header("Target Selection")]
        /// <summary>Target assignment, note this is auto updated when fired from the animator.</summary>        
        [Tooltip("Target assignment, note this is auto updated when fired from the animator")]
        public SpawnTarget Target;

        /// <summary>Is invector target lock enabled (overrides the heat seeking).</summary>
        [Tooltip("Is invector target lock enabled (overrides the heat seeking)")]
        public bool LockOnHasPriority = true;

        /// <summary>Follow moving target, if disabled then projectile will aim at last known position (if has target).</summary>
        [Tooltip("Follow moving target, if disabled then projectile will aim at last known position (if has target)")]
        public bool FollowTarget = false;

        /// <summary>Enable fire and forget heat (well tag) seeking projectile, any lock on takes priority over this.</summary>
        [Tooltip("Enable fire and forget heat (well tag) seeking projectile, any lock on takes priority over this")]
        public bool EnableHeatSeeking = false;

        /// <summary>Heat seeking target selection range, note when enabling chained magic effects this radius is used to find the next target.</summary>
        [Tooltip("Heat seeking target selection range, note when enabling chained magic effects this radius is used to find the next target")]
        public float HeatSeekRange = 20f;

        /// <summary>Heat seeking choose nearest (if not enabled then random target is chosen).</summary>
        [Tooltip("Heat seeking choose nearest (if not enabled then random target is chosen)")]
        public bool HeatSeekNearest = false;

        /// <summary>Chain projectile to another enemy, leave as zero for no chain, the Heat Seek Range is used to find the next target.</summary>
        [Tooltip("Chain projectile to another enemy, leave as zero for no chain, the Heat Seek Range is used to find the next target")]
        public int ChainEffectCount = 0;


        [Header("Destruction")]
        /// <summary>Spawn particles on collision.</summary>
        [Tooltip("Spawn particles on collision")]
        public List<SpawnerOptionsDelayedSequence> ParticlesOnCollision = new List<SpawnerOptionsDelayedSequence>();

        /// <summary>Process list in order sequential is the default, eg list element 0 all copies, then element 1 all copies.  Enable to process elm 0-x looped until no more to spawn.</summary>
        [Tooltip("Process list in order sequential is the default, eg list element 0 all copies, then element 1 all copies.  Enable to process elm 0-x looped until no more to spawn")]
        public bool NoneSequential;

        /// <summary>Delay before starting the collision spawns.</summary>
        [Tooltip("Delay before starting the collision spawns")]
        public float SpawnDelay;

        // hidden from GUI
        /// <summary>Pool slot ID, , set by the master pool instance.</summary>
        [HideInInspector] public int iPoolSlotID;

        /// <summary>Tags for targeting the spell, set by the master pool instance.</summary>
        [HideInInspector] public string[] HeatSeekTags = new string[] { "Enemy", "Boss" };

        /// <summary>Layers for targeting the spell, set by the master pool instance.</summary>
        [HideInInspector] public LayerMask HeatSeekLayers = (1 << 9) | (1 << 17);

        // internal
        private string sChainedToAlready;
        private Transform tSpellTarget;
        private Vector3 v3SpellTarget;
        private GameObject player;
        

        /// <summary>
        /// Occurs on activation, sets up the projectile for targeting.
        /// </summary>
        void OnEnable()
        {
            sChainedToAlready = "";  // reset chain list

            if (!player) player = GlobalFuncs.FindPlayerInstance();  // grab the player            
            if (player)
            {  // found?
                if (Target == SpawnTarget.Enemy)
                {  // is the source the player
                    if (LockOnHasPriority)
                    { // attempt to use the lock on target
                        tSpellTarget = GlobalFuncs.GetLockOn();  // take the target if any
                    }
                    if (!tSpellTarget && EnableHeatSeeking)
                    {  // no lock on and heat seeking mode enabled
                        tSpellTarget = GlobalFuncs.GetTargetWithinRange(transform.position, HeatSeekRange, HeatSeekLayers, HeatSeekTags, HeatSeekNearest, true, 1.5f, true);
                    }
                }
                else if (EnableHeatSeeking)
                {  // enemy spell
                    tSpellTarget = player.transform;  // that homes on the player
                }

                // update the local target seek
                if (tSpellTarget)
                { // target found?
                    v3SpellTarget = tSpellTarget.position;  // update the seek position
                    v3SpellTarget.y += HeightAdjust;  // update height (avoid hiting target feet)
                }
            }
        }

        /// <summary>
        /// Update the movement of the projectile, including internal movement.
        /// </summary>
        void Update()
        {
            // spinning child elements whilst moving forward
            if (RotateChildren.Count > 0)
            {  // have some elements to spin
                foreach (RotateChild rc in RotateChildren)
                {  // process all
                    if (rc.ChildGameObject)
                    {  // failsafe
                        if (rc.RotateAround)
                        {  // rotate around the center?
                            rc.ChildGameObject.RotateAround(Vector3.zero, rc.Speed, rc.RotateAroundAngle * Time.deltaTime);  // apply rotation
                        }
                        else
                        {  // nope spin instead
                            rc.ChildGameObject.Rotate(rc.Speed, rc.RelativeTo);  // apply spin
                        }
                    }
                }
            }

            // apply movement
            if (tSpellTarget)
            {
                if (FollowTarget)
                {  // follow target
                    v3SpellTarget = tSpellTarget.position;  // update the seek position
                    v3SpellTarget.y += HeightAdjust;  // update height (avoid hiting target feet)
                }
                //transform.LookAt(v3SpellTarget);  // face target                                                  
                //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(v3SpellTarget - transform.position), (ForwardSpeed / 3) * Time.deltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(v3SpellTarget - transform.position), ForwardSpeed * Time.deltaTime);
                transform.Translate(Vector3.forward * ForwardSpeed * Time.deltaTime);
            }
            else
            {
                transform.Translate(Vector3.forward * ForwardSpeed * Time.deltaTime);
            }
        }

        /// <summary>
        /// Occurs when the projectile collides with another collider.
        /// </summary>
        /// <param name="c">Collider that caused the collision event.</param>
        void OnCollisionEnter(Collision c)
        {
            if (((HeatSeekLayers.value | GlobalFuncs.targetingLayerMaskCollision.value) & 1 << c.gameObject.layer) == 1 << c.gameObject.layer)
            {  // valid collide target  
                bool bFound = false;
                if (ChainEffectCount > 0)
                {  // chain spell to another target?
                    List<Transform> listTargetsInRange = GlobalFuncs.FindAllTargetsWithinRange(transform.position, HeatSeekRange, HeatSeekLayers, HeatSeekTags, true, 1.5f, true);
                    if (listTargetsInRange.Count > 0)
                    {  // found more in range                        
                        sChainedToAlready += "-" + c.gameObject.name + "-";  // unique character names required, append name of current collision
                        foreach (Transform t in listTargetsInRange)
                        {
                            if (!sChainedToAlready.Contains("-" + t.gameObject.name + "-"))
                            {  // found new target
                                DestroyGameObjectAndSpawn md = GetComponent<DestroyGameObjectAndSpawn>();  //  attempt gray destroy n spawn
                                if (md)
                                {  // found 
                                    md.enabled = false;  // disable it
                                }
                                MagicPool_Return mr = GetComponent<MagicPool_Return>();  // attempt get the pool return component
                                if (mr)
                                {  // found
                                    mr.enabled = false;  // disable
                                    mr.enabled = true;  // re enable, resetting the count down
                                }
                                tSpellTarget = t;  // re target the projectile
                                v3SpellTarget = tSpellTarget.position;  // update position of the transform
                                FollowTarget = true;  // ensure moving target is followed
                                bFound = true;  // flag don't destroy
                                if (ParticlesOnCollision.Count > 0)
                                {  // has spawns?
                                    StartCoroutine(GlobalFuncs.SpawnAllDelayed(ParticlesOnCollision, SpawnDelay, NoneSequential, transform, null, 0, false, Target));  // spawn all but dont kill the projectile
                                }
                                ChainEffectCount -= 1;  // lower the chain count                                               
                                break;  // work complete
                            }
                        }
                    }
                }
                if (!bFound)
                {  // run out of chain-able enemies or chaining not enabled
                    if (ParticlesOnCollision.Count > 0)
                    {  // has spawns?
                        if (gameObject.activeInHierarchy)
                        {  // failsafe, not already returned to the pool
                            StartCoroutine(GlobalFuncs.SpawnAllDelayed(ParticlesOnCollision, SpawnDelay, NoneSequential, transform, gameObject, iPoolSlotID, false, Target));  // spawn all then kill the projectile
                        }
                    }
                    else
                    {
                        GlobalFuncs.ReturnToThePoolOrDestroy(iPoolSlotID, gameObject);  // kill or return projectile to the pool
                    }
                }
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Force default values for new list members in the inspector.
        /// </summary>
        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                foreach (RotateChild r in RotateChildren)
                {
                    r.New();
                }
                foreach (SpawnerOptionsDelayedSequence s in ParticlesOnCollision)
                {
                    s.New();
                }
            }
        } 
#endif
    }

    /// <summary>
    /// Rotate child projectile elements.
    /// </summary>
    [Serializable]
    public class RotateChild
    {
        /// <summary>Link to the element.</summary>
        [Tooltip("Link to the element")]
        public Transform ChildGameObject;

        /// <summary>Speed on each axis to rotate by.</summary>
        [Tooltip("Speed on each axis to rotate by")]
        public Vector3 Speed;

        /// <summary>Space in which to perform the rotation.</summary>
        [Tooltip("Space in which to perform the rotation")]
        public Space RelativeTo;

        /// <summary>Alt mode, rotates the child around the center.</summary>
        [Tooltip("Alt mode, rotates the child around the center")]
        public bool RotateAround;

        /// <summary>Angle at which to rotate around.</summary>
        [Tooltip("Angle at which to rotate around")]
        public float RotateAroundAngle;

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
                Speed = Vector3.up;
                RelativeTo = Space.Self;
                RotateAroundAngle = 20f;
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
