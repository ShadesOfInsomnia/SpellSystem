using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.AI;
using System;
#if !VANILLA
using Invector;
using Invector.vItemManager;
using Invector.vCharacterController.AI;
#endif

namespace Shadex
{
    /// <summary>
    /// Magic control and other modifiers for the AI.
    /// </summary>
#if !VANILLA
    [vClassHeader("MAGIC AI", iconName = "inputIcon")]
    public class MagicAI : vMonoBehaviour
    {
#else
    public class MagicAI : MonoBehaviour {
#endif
        /// <summary>XP to give on death.</summary>
        [Tooltip("XP to give on death")]
        public int XPToGive = 100;

        /// <summary>Disable for non magic characters.</summary>
        [Tooltip("Disable for non magic characters")]
        public bool HasMagicAbilities = true;

        /// <summary>Minimum delay between launching another ranged attack.</summary>
        [Tooltip("Minimum delay between launching another ranged attack")]
        public float MinDelay = 5f;

        /// <summary>Maximum delay between launching another ranged attack.</summary>
        [Tooltip("Maximum delay between launching another ranged attack")]
        public float MaxDelay = 10f;

        /// <summary>Spawn point for the magic effect.</summary>
        [Tooltip("Spawn point for the magic effect")]
        public Transform MagicSpawnPoint;

        [Header("AI Item Manager")]
        /// <summary>Animator trigger to activate using a spell stored in the item manager.</summary>
        [Tooltip("Animator trigger to activate using a spell stored in the item manager")]
        public string AnimatorTrigger = "MagicAttack";

        [Header("Animator Random Attack Sets")]
        /// <summary>Animator trigger to randomly activate spell.</summary>
        [Tooltip("Animator trigger to randomly activate spell")]
        public string RandomTrigger = "AIMagicAttackSet1";

        /// <summary>Static mana cost of all magic attacks launched.</summary>
        [Tooltip("Static mana cost of all magic attacks launched")]
        public int StaticManaCost = 25;


        [Header("Birth and Death Spawns")]
        /// <summary>Name of the trigger to call on ai birth eg for spawn animation b4 the idle state.</summary>
        [Tooltip("Name of the trigger to call on ai birth eg for spawn animation b4 the idle state")]
        public string BirthStartTrigger = "BirthStarted";

        /// <summary>Delay before starting the spawn.</summary>
        [Tooltip("Delay before starting the spawn")]
        public float BirthSpawnDelay;

        /// <summary>List of game objects (Spells, Explosion, Teleport Particle, etc) to spawn when this ai is instantiated.</summary>
        [Tooltip("List of game objects (Spells, Explosion, Teleport Particle, etc) to spawn when this ai is instantiated")]
        public List<SpawnerOptionsDelayedSequence> SpawnOnBirth = new List<SpawnerOptionsDelayedSequence>();

        /// <summary>List of game objects (Spells, Explosion, Enemies, etc) to spawn when this ai is killed.</summary>
        [Tooltip("List of game objects (Spells, Explosion, Enemies, etc) to spawn when this ai is killed")]
        public List<SpawnerOptionsDelayedSequence> SpawnOnDeath = new List<SpawnerOptionsDelayedSequence>();

        /// <summary>List of game objects (Spells, Explosion, Enemies, etc) to spawn when this ai's dead body is removed from the game.</summary>
        [Tooltip("List of game objects (Spells, Explosion, Enemies, etc) to spawn when this ai's dead body is removed from the game")]
        public List<SpawnerOptionsDelayedSequence> SpawnOnBodyRemoval = new List<SpawnerOptionsDelayedSequence>();

        /// <summary>Process spawn lists in order sequential is the default, eg list element 0 all copies, then element 1 all copies.  Enable to process elm 0-x looped until no more to spawn.</summary>
        [Tooltip("Process spawn lists in order sequential is the default, eg list element 0 all copies, then element 1 all copies.  Enable to process elm 0-x looped until no more to spawn")]
        public bool NoneSequentialSpawns;

        [Header("ReAnimate")]
        /// <summary>Prefab for when a killed enemy is reanimated as a minion of the player.</summary>
        [Tooltip("Prefab for when a killed enemy is reanimated as a minion of the player")]
        public GameObject MinionPrefab;

        [Header("Way point Actions")]
        /// <summary>When enabled and ai is patrolling, update the animator with the way point id.</summary>
        [Tooltip("When enabled and ai is patrolling, update the animator with the way point id")]
        public bool WaypointActions = false;

        /// <summary>Way point action set id in use, passed to animator ActionSetIDParameter below.</summary>
        [Tooltip("Way point action set id in use, passed to animator ActionSetIDParameter below")]
        public int ActionSetID = 1;

        /// <summary>Animator parameter name (must be type int) to update with the way point id.</summary>
        [Tooltip("Animator parameter name (must be type int) to update with the way point id")]
        public string WaypointIDParameter = "WaypointID";

        /// <summary>Animator parameter name (must be type int) to update with the way point action set id.</summary>
        [Tooltip("Animator parameter name (must be type int) to update with the way point action set id")]
        public string ActionSetIDParameter = "WaypointActionSet";

        /// <summary>Name of the trigger to call after update of the way point id.</summary>
        [Tooltip("Name of the trigger to call after update of the way point id")]
        public string ActionTriggerParameter = "WaypointActionStart";

        /// <summary>Delegate to call mana used onto leveling system.</summary>
        public SetIntValue useMana; 

        // hidden from GUI
        [HideInInspector] public bool bMagicAttacking = false;  // ensure magic attack coroutine doesn't get doubled up
        [HideInInspector] public bool bPatrolling = false;  // state of invector ai

        // internal
        protected float fManaRemaining;  // keep track of available mana
        protected float fOriginal_maxDetectDistance, fOriginal_distanceToLostTarget, fOriginal_fieldOfView;   // store field of view on start
        protected List<MagicAIAvailableSpell> SpellsShortList;  // available inventory magic id's

#if !VANILLA
        protected v_AIController ai;
#endif
        protected NavMeshAgent agent;
        protected GameObject player;
        protected Animator animator;
        protected MagicAIItemManager inventory;
        protected Coroutine CoDelayedSpawn;

        /// <summary>
        /// Initialise listeners and component references.
        /// </summary>
        protected virtual void Start()
        {
            // initialise invector AI handling
            player = GlobalFuncs.FindPlayerInstance();
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            inventory = GetComponent<MagicAIItemManager>();
#if !VANILLA
            ai = GetComponent<v_AIController>();
            if (ai)
            {  // don't start if invector AI and all components are not present
                // invector ai event handlers
                ai.onReceiveDamage.AddListener(OnReceiveDamage_Chase);  // listen for damage to enable chase when out of FOV
                ai.onChase.AddListener(delegate { OnChase(); });  // listen for start of chase mode
                ai.onIdle.AddListener(delegate { OnIdle(); });  // listen for start of idle mode
                ai.onPatrol.AddListener(delegate { OnPatrol(); });  // listen for start of idle mode
                ai.onDead.AddListener(delegate { OnDead(); });  // listen for sudden death

                // store original FOV
                fOriginal_maxDetectDistance = ai.maxDetectDistance;
                fOriginal_distanceToLostTarget = ai.distanceToLostTarget;
                fOriginal_fieldOfView = ai.fieldOfView;
            }
#else
            // handle non invector events
#endif

            // add a listener to the leveling component if available
            CharacterBase levelingSystem = GetComponentInParent<CharacterBase>();
            if (levelingSystem)
            {
                useMana = new SetIntValue(levelingSystem.UseMana);
                levelingSystem.NotifyUpdateHUD += new CharacterBase.UpdateHUDHandler(UpdateHUDListener);
                levelingSystem.ForceUpdateHUD();
            }

#if !VANILLA
            // make short list of spells in inventory
            if (inventory && AnimatorTrigger.Length > 0 && HasMagicAbilities)
            {
                SpellsShortList = new List<MagicAIAvailableSpell>();  // init the list
                foreach (ItemReference ir in inventory.startItems)
                {  // process all start items
                    vItem item = inventory.itemListData.items.Find(t => t.id.Equals(ir.id));  // find the vItem by id
                    if (item.type == vItemType.Spell)
                    {  // only after spells
                        SpellsShortList.Add(new MagicAIAvailableSpell()
                        {
                            MagicID = item.attributes.Find(ia => ia.name.ToString() == "MagicID").value,
                            ManaCost = item.attributes.Find(ia => ia.name.ToString() == "ManaCost").value
                        });  // add to the short list
                    }
                }
                if (SpellsShortList.Count == 0)
                { // none found
                    SpellsShortList = null;  // reset back to null for fast checking
                    if (GlobalFuncs.DEBUGGING_MESSAGES)
                    {
                        Debug.Log("Warning spells NOT found in magic AI inventory");
                    }
                }
            }
#endif

            // birth animation & magic spawning
            if (BirthStartTrigger != "")
            {   // birth animation specified
                animator.SetTrigger(BirthStartTrigger);   // trigger the random magic state selector                
            }
            CoDelayedSpawn = StartCoroutine(GlobalFuncs.SpawnAllDelayed(SpawnOnBirth, BirthSpawnDelay, NoneSequentialSpawns, transform, null, 0, false, (gameObject.tag == "Enemy" ? SpawnTarget.Friend : SpawnTarget.Enemy)));  // trigger all birth spawns the the array            
        }

        /// <summary>
        /// Listen to the leveling system for attribute changes.
        /// </summary>
        /// <param name="cb">Callback reference to the leveling class.</param>
        /// <param name="e">Character stats that have been updated.</param>
        protected virtual void UpdateHUDListener(CharacterBase cb, CharacterUpdated e)
        {
#if !VANILLA
            if (ai)
            {
                ai.maxHealth = e.LifeMAX;
                ai.ChangeHealth((int)e.Life);
                fManaRemaining = e.Mana;
            }
#endif
        }


        /// <summary>
        /// Launch magic attack's randomly whilst chasing
        /// </summary>
        /// <returns>IEnumerator whilst delaying to coroutine.</returns>
        public virtual IEnumerator LongRangeMagicAttack()
        {
            UnityEngine.Random.InitState(System.Environment.TickCount);
            while (true)
            {  // keep attacking whilst chasing
                // wait for next targeting interval                
                yield return new WaitForSeconds(UnityEngine.Random.Range(MinDelay, MaxDelay));  // random delay

                if (bMagicAttacking)
                { // magic attack mode still enabled

                    if (SpellsShortList != null)
                    {  // use spells from AI inventory?
                        List<MagicAIAvailableSpell> ShortShortList = SpellsShortList.FindAll(s => s.ManaCost < fManaRemaining);  // grab all spells that ai has the mana for
                        if (ShortShortList.Count > 0)
                        {  // spells available?
                            //UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
                            int iNextSpell = UnityEngine.Random.Range(0, ShortShortList.Count);  // randomly choose the spell
                            animator.SetInteger("MagicID", ShortShortList[iNextSpell].MagicID);  // set the magic id
                            animator.SetTrigger(AnimatorTrigger);   // trigger the attack
                            useMana(ShortShortList[iNextSpell].ManaCost);  // update remaining mana
                        }
                    }
                    else
                    {  // not found, use random trigger
                        if (fManaRemaining > StaticManaCost)
                        { // enough mana to attack
#if !VANILLA
                            if (!ai.isAttacking)
                            {  // not already melee attacking
#else
                        if (true) {  // handle ai not already melee attacking here
#endif
                                if (GlobalFuncs.DEBUGGING_MESSAGES)
                                {
                                    Debug.Log(transform.name + " attacking with magic");
                                }
                                animator.SetTrigger(RandomTrigger);   // trigger the random magic state selector
                                useMana(StaticManaCost);  // update remaining mana
                            }
                        }
                    }

#if !VANILLA
                    // reset the FOV to original values once the AI has closed the distance to the player (allowing player to lose the AI
                    if (ai.maxDetectDistance != fOriginal_maxDetectDistance)
                    {  // still working on extended FOV
                        if (Vector3.Distance(player.transform.localPosition, transform.localPosition) < fOriginal_maxDetectDistance)
                        {  // within original FOV?
                            if (GlobalFuncs.DEBUGGING_MESSAGES)
                            {
                                Debug.Log(transform.name + " within range, resetting to original FOV");
                            }
                            ai.maxDetectDistance = fOriginal_maxDetectDistance;
                            ai.distanceToLostTarget = fOriginal_distanceToLostTarget;
                            ai.fieldOfView = fOriginal_fieldOfView;
                        }
                    }
#endif
                }
                else
                {  // no longer able to attack
                    yield break; // drop out
                }
            }
        }

        /// <summary>
        /// Pass the way point id's to the animator whilst patrolling.
        /// </summary>
        /// <returns>IEnumerator whilst delaying to coroutine.</returns>
        public virtual IEnumerator UpdateAnimatorWithWaypointID()
        {
            int iWaypointID = -1;
            while (true)
            {  // keep attacking whilst chasing
                // wait for next targeting interval
                yield return new WaitForSeconds(0.25f);  // check 4 times a second

                // test whether idle at a way point or in between
                if (bPatrolling)
                { // patrol mode still enabled
                    if (agent.pathStatus == NavMeshPathStatus.PathComplete)
                    {  // potential idle at a way point
                        int iWaypointIDCheck = -1;  // init way point check
#if !VANILLA
                        for (int w = 0; w < ai.pathArea.waypoints.Count; w++)
                        {  // check all way points
                            if (Vector3.Distance(transform.localPosition, ai.pathArea.waypoints[w].transform.localPosition) <= ai.patrollingStopDistance)
                            {  // within stop distance
                                iWaypointIDCheck = w;  // found it
                                break;  // work complete
                            }
                        }
#endif
                        if (iWaypointIDCheck > -1)
                        { // at a way point
                            if (iWaypointIDCheck != iWaypointID)
                            {  // way point id changed?
                                iWaypointID = iWaypointIDCheck;  // set at new way point
                                animator.SetInteger(ActionSetIDParameter, ActionSetID);  // update the animator action set id in case has been changed
                                animator.SetInteger(WaypointIDParameter, iWaypointID);  // update the animator waypointID
                                animator.SetTrigger(ActionTriggerParameter);  // cause the trigger 
                                if (GlobalFuncs.DEBUGGING_MESSAGES)
                                {
                                    Debug.Log(transform.name + " is at way point ID " + iWaypointID.ToString());
                                }
                            }
                        }
                        else
                        {
                            iWaypointID = -1;  // enforce the parameter back 
                            animator.SetInteger(WaypointIDParameter, -1);  // to default (ie not at way point)
                            animator.SetTrigger(ActionTriggerParameter);  // cause the trigger 
                        }
                    }
                    else
                    {  // on a new path
                        iWaypointID = -1;  // enforce the parameter back
                        animator.SetInteger(WaypointIDParameter, -1);  // to default (ie not at way point)
                        animator.SetTrigger(ActionTriggerParameter);  // cause the trigger 
                    }
                }
                else
                {  // no longer able to attack
                    animator.SetInteger(WaypointIDParameter, -1);  // enforce the parameter back to default (ie not at way point)
                    animator.SetTrigger(ActionTriggerParameter);  // cause the trigger 
                    yield break; // drop out
                }
            }
        }

        /// <summary>
        /// Handle invector AI when idling.
        /// </summary>
        public virtual void OnIdle()
        {
            if (GlobalFuncs.DEBUGGING_MESSAGES)
            {
                Debug.Log(transform.name + " change state to Idle");
            }
            bMagicAttacking = false;
            bPatrolling = false;
        }

        /// <summary>
        /// Handle invector AI when chasing.
        /// </summary>
        public virtual void OnChase()
        {
            if (GlobalFuncs.DEBUGGING_MESSAGES)
            {
                Debug.Log(transform.name + " change state to Chase");
            }
            bPatrolling = false;
            if (!bMagicAttacking && HasMagicAbilities)
            {  // is not already in magic attack mode and has magic
                bMagicAttacking = true;  // ensure no more spells lauched
                StartCoroutine(LongRangeMagicAttack());  // launch magic attack's randomly whilst chasing
            }
        }

        /// <summary>
        /// Handle invector AI when patrolling.
        /// </summary>
        public virtual void OnPatrol()
        {
            if (GlobalFuncs.DEBUGGING_MESSAGES)
            {
                Debug.Log(transform.name + " change state to Patrol");
            }
            bMagicAttacking = false;  // ensure no more spells launched
            if (WaypointActions)
            {  // update the animator with patrol way point id's
                bPatrolling = true;  // init dropout condition
#if !VANILLA
                if (ai.pathArea)
                {
#endif
                    StartCoroutine(UpdateAnimatorWithWaypointID());  // pass the way point id's to the animator whilst patrolling
#if !VANILLA
                }
#endif
            }
        }

        /// <summary>
        /// I have died, give XP to player and spawn any death spawns.
        /// </summary>
        public virtual void OnDead()
        {
            if (GlobalFuncs.DEBUGGING_MESSAGES)
            {
                Debug.Log(transform.name + " change state to Dead");
            }
            GlobalFuncs.GiveXPToPlayer(XPToGive);  // update player XP
            bMagicAttacking = false;  // ensure no more spells launched
            bPatrolling = false;  // ensure patrol actions stopped                       
            StartCoroutine(GlobalFuncs.SpawnAllDelayed(SpawnOnDeath, 0f, NoneSequentialSpawns, transform, null, 0, false, (gameObject.tag == "Enemy" ? SpawnTarget.Friend : SpawnTarget.Enemy)));  // trigger all death spawns the the array
            StartCoroutine(GlobalFuncs.SpawnAllDelayed(SpawnOnBodyRemoval, GlobalFuncs.EnemyDestroyDelay, NoneSequentialSpawns, transform, null, 0, false, (gameObject.tag == "Enemy" ? SpawnTarget.Friend : SpawnTarget.Enemy)));  // trigger all death spawns the the array
            Destroy(gameObject, GlobalFuncs.EnemyDestroyDelay);  // remove self when dead after global time limit
        }


        /// <summary>
        /// Enable increased target detection when damaged, even when target it out of range
        /// </summary>
        /// <param name="damage">Used to update the AI to look at the damage source.</param>
#if !VANILLA
        public virtual void OnReceiveDamage_Chase(vDamage damage)
        {
            if (ai.currentState != v_AIMotor.AIStates.Chase)
            {  // not already chasing     
                if (GlobalFuncs.DEBUGGING_MESSAGES)
                {
                    Debug.Log(transform.name + " change state to Chase (from long range attack)");
                }
                transform.LookAt(damage.sender.transform.localPosition);  // face damage source                
                if (player)
                {  // found player already
                    ai.maxDetectDistance = 5f + Vector3.Distance(transform.position, player.transform.position);  // extend detect range to include player                    
                    ai.distanceToLostTarget = ai.maxDetectDistance;  // extend lost target range
                    ai.fieldOfView = 180;  // increase FOV to ensure player detection
                }
            }
        }  
#endif
        /// <summary>
        /// On disable/death disable active coroutine.
        /// </summary>
        protected virtual void OnDisable()
        {
            if (CoDelayedSpawn != null)
            {
                StopCoroutine(CoDelayedSpawn);
                CoDelayedSpawn = null;
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Update the spawn array on new for 1st element defaults.
        /// </summary>
        protected virtual void OnValidate()
        {
            if (!Application.isPlaying)
            {
                foreach (SpawnerOptionsDelayedSequence s in SpawnOnBirth)
                {
                    s.New();
                }
                foreach (SpawnerOptionsDelayedSequence s in SpawnOnDeath)
                {
                    s.New();
                }
            }
        }  // force default values for new list members in the inspector
#endif
    }

    /// <summary>
    /// Available spell short list.
    /// </summary>
    public class MagicAIAvailableSpell
    {
        /// <summary>Magic ID of the spell.</summary>
        public int MagicID;

        /// <summary>Mana cost of the spell.</summary>
        public int ManaCost;
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
