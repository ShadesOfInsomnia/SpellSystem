using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Invector.vCharacterController.AI;
#if !VANILLA
using Invector;
#endif

namespace Shadex
{
    /// <summary>
    /// ReAnimate the fallen spell.
    /// </summary>
    /// <remarks>
    /// Finds fallen enemies within a radius and reanimates them as friendly minions.
    /// Target characters must not have remove components flagged on the vAIController and have a minion prefab specified.
    /// </remarks>
#if !VANILLA
    [vClassHeader("MAGIC REANIMATE", iconName = "ammoIcon")]
    public class MagicReAnimate : vMonoBehaviour
    {
#else
    public class MagicReAnimate : MonoBehaviour {
#endif
        /// <summary>Tag of the reanimate particle.</summary>
        [Tooltip("Tag of the reanimate particle")]
        public string[] Tags = new string[] { "Ragdoll" };

        /// <summary>Layer of the reanimate particle.</summary>
        [Tooltip("Layer of the reanimate particle")]
        public LayerMask Layers = (1 << 18);

        /// <summary>Target selection range.</summary>
        [Tooltip("Target selection range")]
        public float Range = 20f;

        /// <summary>Ensure within line of sight.</summary>
        [Tooltip("Ensure within line of sight")]
        public bool LineOfSightCheck = false;

        /// <summary>Maximum to reanimate, zero = no maximum.</summary>
        [Tooltip("Maximum to reanimate, zero = no maximum")]
        public int MaxToReAnimate = 5;

        /// <summary>Delay before starting the spawn,if zero then no delay before reanimation.</summary>
        [Tooltip("Delay before starting the spawn,if zero then no delay before reanimation")]
        public float InitialDelay = 2f;

        /// <summary>Delay before destroying the particle, if zero then don't destroy.</summary>
        [Tooltip("Delay before destroying the particle, if zero then don't destroy")]
        public float DestroyDelay = 4f;


        /// <summary>
        /// Process all potential reanimate dead within the radius.
        /// </summary>
        /// <returns>IEnumerator whilst delaying.</returns>
        public IEnumerator Start()
        {
            if (InitialDelay > 0)
            {  // initial delay enabled
                yield return new WaitForSeconds(InitialDelay);  // wait
            }
            yield return new WaitForSeconds(InitialDelay);  // wait

            int iReAnimatedSoFar = 0;
            bool AllDone = false;
            GameObject goMinion;
            while (!AllDone)
            {  // scan loop for when all bones are tagged rather than just the parent
                AllDone = true;  // enable drop out
                if (MaxToReAnimate == 0 || iReAnimatedSoFar < MaxToReAnimate)
                {  // limit how many get reanimated?
                    List<Transform> ltTargetsInRange = GlobalFuncs.FindAllTargetsWithinRange(transform.localPosition, Range, Layers, Tags, LineOfSightCheck, 0f, false);  // search for deaders
                    if (ltTargetsInRange.Count > 0)
                    {  // deaders found?
                        foreach (Transform tPotentialDeader in ltTargetsInRange)
                        {  // process all transforms found
                            MagicAI mai = tPotentialDeader.GetComponentInParent<MagicAI>();  // attempt grab magic ai component
                            if (mai)
                            {  // found magic ai?
                                if (mai.MinionPrefab)
                                {  // valid is raise?
                                    if (GlobalFuncs.MAGICAL_POOL)
                                    {
                                        goMinion = GlobalFuncs.SpawnBasic(mai.MinionPrefab, 1, mai.transform, new RandomSphereOptions() { }, SpawnTarget.Any)[0];
                                    }
                                    else
                                    {
                                        goMinion = Instantiate(mai.MinionPrefab, mai.transform);  // attempt spawn from transform
                                        goMinion.transform.SetParent(null);  // unparent
                                    }
                                    var Agent = goMinion.GetComponent<NavMeshAgent>();
                                    if (Agent)
                                    {
                                        Agent.enabled = true;
                                    }
#if !VANILLA
                                    var vAI = goMinion.GetComponent<v_AICompanion>();
                                    if (vAI)
                                    {
                                        vAI.companion = GlobalFuncs.FindPlayerInstance().transform;
                                        vAI.Init();
                                        vAI.companionState = v_AICompanion.CompanionState.Follow;
                                        vAI.enabled = true;
                                    }
#endif
                                    DestroyImmediate(mai.transform.gameObject);  // kill original lying on the floor
                                    iReAnimatedSoFar += 1;  // we have raised another, mwahahaha
                                    AllDone = false;
                                    break;  // force rescan area
                                }
                            }
                        }
                    }
                }
            }
            if (DestroyDelay > 0)
            {  // destruction enabled
                yield return new WaitForSeconds(DestroyDelay);  // wait
                Destroy(gameObject);  // destruct
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
