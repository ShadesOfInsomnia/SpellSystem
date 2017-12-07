using UnityEngine;
using System.Collections.Generic;

namespace Shadex
{
    /// <summary>
    /// Attached to a spell animator state, causes hand effects and spawning of spells
    /// </summary>
    /// <remarks>
    /// Critical, causes the hand particle IK tracking and the spell instances to be
    /// spawned at specific points in the animation.
    /// </remarks>
    public class MagicAttackBehavior : StateMachineBehaviour
    {        
        [Header("Casting Hand (or foot) Particle (IK Required)")]
        /// <summary>Prefab of the particle system to attach to the limb.</summary>
        [Tooltip("Prefab of the particle system to attach to the limb")]
        public GameObject LimbParticleEffect;

        /// <summary>The limb that the LimbParticleEffect should follow.</summary>
        [Tooltip("The limb that the LimbParticleEffect should follow")]
        public AvatarIKGoal attackLimb;

        /// <summary>(Optional) The 2nd Prefab of the particle system to attach to second limb.</summary>
        [Tooltip("(Optional) The 2nd Prefab of the particle system to attach to second limb")]
        public GameObject LimbParticleEffect2;

        /// <summary>(Optional) The 2nd limb that the LimbParticleEffect should follow, if a one handed spell, set to the same limb as above.</summary>
        [Tooltip("(Optional) The 2nd limb that the LimbParticleEffect should follow, if a one handed spell, set to the same limb as above")]
        public AvatarIKGoal attackLimb2;

        /// <summary>Force no pooling of the limb effect, useful if the limb particles wont be reused in the scene.</summary>
        [Tooltip("Force no pooling of the limb effect, useful if the limb particles wont be reused in the scene")]
        public bool DoNotPoolLimbParticles;

        [Header("Spell Spawn Options")]
        /// <summary>For the charge animator state, destroys all spawned on state exit.</summary>
        [Tooltip("For the charge animator state, destroys all spawned on state exit")]
        public bool ChargeState;

        /// <summary>List of all prefabs to spawn within the animator time frame.</summary>
        [Tooltip("List of all prefabs to spawn within the animator time frame")]
        public List<SpawnerOptionsOverTime> SpawnOverTime = new List<SpawnerOptionsOverTime>();


        // internal
        GameObject goLimb1_ParticleInstance;
        int iLimbPoolSlotID1;
        GameObject goLimb2_ParticleInstance;
        int iLimbPoolSlotID2;
        Transform tMagicSpawn;
        bool bAI;

        /// <summary>
        /// Occurs when the animator enters the parent state, creates hand particles and targeting.
        /// </summary>
        /// <param name="animator">Reference to the parent animator.</param>
        /// <param name="stateInfo">Information about the state.</param>
        /// <param name="layerIndex">Index of the current animator layer.</param>
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // set the magic spawn point
            bAI = !(animator.gameObject.tag == "Player");
            if (!bAI)
            {
                tMagicSpawn = animator.GetComponentInChildren<MagicSettings>().MagicSpawnPoint;
            }
            else
            {
                tMagicSpawn = animator.GetComponentInChildren<MagicAI>().MagicSpawnPoint;
            }

            // handle the hand particle 
            if (LimbParticleEffect && !goLimb1_ParticleInstance)
            {   // enabled, present and not already active?
                if (GlobalFuncs.MAGICAL_POOL && !DoNotPoolLimbParticles)
                {  // pooling enabled?
                    if (iLimbPoolSlotID1 > 0)
                    {  // pool slot already known
                        goLimb1_ParticleInstance = GlobalFuncs.TheMagicalPool().GatherFromPool(iLimbPoolSlotID1 - 1, tMagicSpawn);  // load from the pool
                    }
                    else
                    {
                        goLimb1_ParticleInstance = GlobalFuncs.TheMagicalPool().GatherFromPool(ref iLimbPoolSlotID1, LimbParticleEffect, tMagicSpawn, SpawnTarget.Any);  // load from the pool
                    }
                    goLimb1_ParticleInstance.SetActive(true);  // enable as will be returned from the pool disabled
                }
                else
                {  // pool disabled
                    goLimb1_ParticleInstance = Instantiate(LimbParticleEffect);  // load from the prefab
                }
                if (attackLimb != attackLimb2)
                {  // dont create a second particle if both limbs the same
                    if (GlobalFuncs.MAGICAL_POOL && !DoNotPoolLimbParticles)
                    {  // pooling enabled?
                        if (LimbParticleEffect2)
                        { // does limb 2 has a different particle effect?
                            if (iLimbPoolSlotID2 > 0)
                            {  // pool slot already known
                                goLimb2_ParticleInstance = GlobalFuncs.TheMagicalPool().GatherFromPool(iLimbPoolSlotID2 - 1, tMagicSpawn);  // load from the pool
                            }
                            else
                            {
                                goLimb2_ParticleInstance = GlobalFuncs.TheMagicalPool().GatherFromPool(ref iLimbPoolSlotID2, LimbParticleEffect2, tMagicSpawn, SpawnTarget.Any);  // load from the pool
                            }
                        }
                        else
                        {  // nope
                            if (iLimbPoolSlotID1 > 0)
                            {  // pool slot already known
                                goLimb2_ParticleInstance = GlobalFuncs.TheMagicalPool().GatherFromPool(iLimbPoolSlotID1 - 1, tMagicSpawn);  // load from the pool
                            }
                            else
                            {
                                goLimb2_ParticleInstance = GlobalFuncs.TheMagicalPool().GatherFromPool(ref iLimbPoolSlotID1, LimbParticleEffect, tMagicSpawn, SpawnTarget.Any);  // load from the pool
                            }
                        }
                        goLimb2_ParticleInstance.SetActive(true);  // enable as will be returned from the pool disabled
                    }
                    else
                    {  // pool disabled
                        goLimb2_ParticleInstance = Instantiate((LimbParticleEffect2 ? LimbParticleEffect2 : LimbParticleEffect));  // load from the prefab
                    }
                }
            }

            // spawn all immediate if charge state
            if (ChargeState)
            {  // spawn over time all as long as not the charge state
                foreach (SpawnerOptionsOverTime sootSpawnMe in SpawnOverTime)
                {  // process all spawns immediate
                    if (sootSpawnMe.UseRootTransform)
                    {  // ground based effect?
                        sootSpawnMe.goSpawnedParticle = sootSpawnMe.Spawn(animator.rootPosition, Quaternion.identity, (bAI ? SpawnTarget.Friend : SpawnTarget.Enemy));  // spawn
                    }
                    else
                    {  // negative, use the magic spawn point
                        sootSpawnMe.goSpawnedParticle = sootSpawnMe.Spawn(tMagicSpawn, (bAI ? SpawnTarget.Friend : SpawnTarget.Enemy));  // spawn
                    }
                }
            }
        }

        /// <summary>
        /// Occurs as the animator leaves the state, cleans up instances, returning them to the pool.
        /// </summary>
        /// <param name="animator">Reference to the parent animator.</param>
        /// <param name="stateInfo">Information about the state.</param>
        /// <param name="layerIndex">Index of the current animator layer.</param>
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // handle limb particle effects
            if (goLimb1_ParticleInstance)
            {  // still have limb 1 particle?
                GlobalFuncs.ReturnToThePoolOrDestroy(iLimbPoolSlotID1, goLimb1_ParticleInstance);  // return to the pool, or destroy it if no pool
                goLimb1_ParticleInstance = null;  // ensure destroy the reference
            }
            if (goLimb2_ParticleInstance)
            {  // still have limb 2 particle?
                if (LimbParticleEffect2)
                {  // did limb2 use a seperate effect?
                    GlobalFuncs.ReturnToThePoolOrDestroy(iLimbPoolSlotID2, goLimb2_ParticleInstance);  // return to the pool, or destroy it if no pool
                }
                else
                {  // negative, return to the same pool as limb1
                    GlobalFuncs.ReturnToThePoolOrDestroy(iLimbPoolSlotID1, goLimb2_ParticleInstance);  // return to the pool, or destroy it if no pool
                }
                goLimb2_ParticleInstance = null;  // ensure destroy the reference
            }

            // remove spawned charged (non moving) particles
            if (ChargeState)
            {  // was this the charge animator state
                foreach (SpawnerOptionsOverTime sootSpawnMe in SpawnOverTime)
                {  // process all spawns
                    GlobalFuncs.ReturnToThePoolOrDestroy(sootSpawnMe.PoolSlotId, sootSpawnMe.goSpawnedParticle);  // destroy
                }
            }
        }

        /// <summary>
        /// Creates the spawner prefabs at the specific time intervals selected.
        /// </summary>
        /// <param name="animator">Reference to the parent animator.</param>
        /// <param name="stateInfo">Information about the state.</param>
        /// <param name="layerIndex">Index of the current animator layer.</param>
        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!ChargeState)
            {  // spawn over time all as long as not the charge state
                foreach (SpawnerOptionsOverTime sootSpawnMe in SpawnOverTime)
                {  // process all spawns at the specified frame of the animation
                    sootSpawnMe.Spawn(tMagicSpawn, stateInfo.normalizedTime % 1, animator, (bAI ? SpawnTarget.Friend : SpawnTarget.Enemy));  // spawn
                }
            }
        }  // spawning projectiles and particles over time

        /// <summary>
        /// Keep the hand particles aligned with the hand IK as they move.
        /// </summary>
        /// <remarks>
        /// MUST SET THE LAYER TO IK. GO TO THE ANIMATOR, CLICK LAYERS, CLICK THE IK BUTTON TO THE RIGHT OF THE LAYERS.
        /// </remarks>
        /// <param name="animator">Reference to the parent animator.</param>
        /// <param name="stateInfo">Information about the state.</param>
        /// <param name="layerIndex">Index of the current animator layer.</param>
        override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // OnStateExit may be called before the last OnStateIK so we need to check the LimbParticleEffect haven't been destroyed.
            if (!goLimb1_ParticleInstance) return;  // dropout if no limb particle

            // Find the position and rotation of the limb the LimbParticleEffect should follow.
            Vector3 limbPosition = animator.GetIKPosition(attackLimb);
            Quaternion limbRotation = animator.GetIKRotation(attackLimb);

            // Set the particle's position and rotation based on that limb.
            goLimb1_ParticleInstance.transform.position = limbPosition;
            goLimb1_ParticleInstance.transform.rotation = limbRotation;

            // handle the second limb if available
            if (attackLimb != attackLimb2)
            {
                // Find the position and rotation of the limb the LimbParticleEffect should follow.
                Vector3 limbPosition2 = animator.GetIKPosition(attackLimb2);
                Quaternion limbRotation2 = animator.GetIKRotation(attackLimb2);

                // Set the particle's position and rotation based on that limb.
                goLimb2_ParticleInstance.transform.position = limbPosition2;
                goLimb2_ParticleInstance.transform.rotation = limbRotation2;
            }
        }  // apply the hand effects

#if UNITY_EDITOR
        /// <summary>
        /// Apply defaults when first spawn list item is created
        /// </summary>
        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                foreach (SpawnerOptionsOverTime s in SpawnOverTime)
                {
                    s.New();
                }
            }
        }  // force default values for new list members in the inspector
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
