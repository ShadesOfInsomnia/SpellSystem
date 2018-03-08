using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor.Events;
#endif
using System.Linq;
using System;

namespace Shadex
{
    public delegate void CallFunction();
    public delegate int GetIntValue();
    public delegate float GetFloatValue();
    public delegate void SetIntValue(int iValue);
    public delegate void SetBoolValue(bool bValue);

    [System.Serializable]
    public class UnityIntEvent : UnityEvent<int> { }

    /// <summary>
    /// Static global functions for the spell system.
    /// </summary>
    public class GlobalFuncs : MonoBehaviour
    {
        /// <summary>Enable to output debugging messages in the console, set in magic settings.</summary>
        public static bool DEBUGGING_MESSAGES = true;

        /// <summary>Enable automatic pooling via the spawner options class, set in magic settings.</summary>
        public static bool MAGICAL_POOL = true;

        /// <summary>Length to destroy dead enemy AI, set in magic settings.</summary>
        public static float EnemyDestroyDelay = 10f;

        /// <summary>Link to leveling system player XP, set automatically by the leveling system.</summary>
        public static SetIntValue GiveXPToPlayer;

        /// <summary>Radius to spawn the collectibles on AI death, set in magic settings.</summary>
        public static float OnDeathDropRadius = 2.5f;

        /// <summary>Collectibles for the prefabs, set on the player leveling system for all chars, set in player leveling system.</summary>
        public static List<BaseCollection> Collectables;

        /// <summary>Master targeting tags for all, set in magic settings.</summary>
        public static string[] targetingTagsAll;

        /// <summary>Master targeting tags for friends and the player, set in magic settings.</summary>
        public static string[] targetingTagsFriend;

        /// <summary>Master targeting tags for enemies, set in magic settings.</summary>
        public static string[] targetingTagsEnemy;

        /// <summary>Master targeting layers for all, set in magic settings.</summary>
        public static LayerMask targetingLayerMaskAll;

        /// <summary>Master targeting layers for friends and the player, set in magic settings.</summary>
        public static LayerMask targetingLayerMaskFriend;

        /// <summary>Master targeting layers for enemies, set in magic settings.</summary>
        public static LayerMask targetingLayerMaskEnemy;

        /// <summary>Master targeting layers for collisions, set in magic settings.</summary>
        public static LayerMask targetingLayerMaskCollision;

        /// <summary>Master targeting layers for physics, set in magic settings.</summary>
        public static LayerMask targetingLayerMaskPhysics;

        /// <summary>Reference to the Magic Pool, set by itself or the accessor function.</summary>
        public static MagicPool mpi;

        /// <summary>Cached reference to the character database.</summary>
        public static CharacterDataBase cDB;

        /// <summary>Cached reference to the main menu.</summary>
        public static MainMenu mainM;

        /// <summary>Cached reference to the main menu.</summary>
        public static MagicSettings mSet;

        /// <summary>
        /// Find the character database component.
        /// </summary>
        /// <returns>Active data layer class instance.</returns>
        public static CharacterDataBase TheDatabase()
        {
            if (!cDB)
            {
                GameObject player = FindPlayerInstance();
                if (player)
                {
                    cDB = player.GetComponent<CharacterDataBase>();
                }
            }
            return cDB;
        }

        /// <summary>
        /// Find the character database component.
        /// </summary>
        /// <returns>Active magic settings class instance.</returns>
        public static MagicSettings TheMagicalSettings()
        {
            if (!mSet)
            {
                GameObject player = FindPlayerInstance();
                if (player)
                {
                    mSet = player.GetComponent<MagicSettings>();
                }
            }
            return mSet;
        }

        /// <summary>
        /// Find the main menu component.
        /// </summary>
        /// <returns>Active main menu class instance.</returns>
        public static MainMenu TheMainMenu()
        {
            if (!mainM)
            {
                GameObject goMM = GameObject.Find("MainMenu");
                if (goMM)
                {
                    mainM = goMM.GetComponent<MainMenu>();
                }
            }
            return mainM;
        }  
       
        /// <summary>
        /// Find or create the magic pool component.
        /// </summary>
        /// <returns>Active magic pool class instance.</returns>
        public static MagicPool TheMagicalPool()
        {
            if (!mpi)
            {  // has this not been found already?
                GameObject goMpi = GameObject.Find("TheMagicalPool");
                if (!goMpi)
                {  // does it not exist?
                    goMpi = new GameObject("TheMagicalPool");  // create 
                    mpi = goMpi.AddComponent<MagicPool>();  // add the pool component and retain reference
                }
                else
                {  // found magic pool game object
                    mpi = goMpi.GetComponent<MagicPool>();  // grab reference to the magic pool
                    if (!mpi)
                    {  // not present
                        mpi = goMpi.AddComponent<MagicPool>();  // add the pool component and retain reference
                    }
                }
            }
            return mpi;  // return to the user
        }

        /// <summary>
        /// Destroy game object, returning to the magical pool if enabled.
        /// </summary>
        /// <param name="PoolSlotID">Pool slot ID or zero when pooling is disabled.</param>
        /// <param name="Instance">Game object instance to return or destroy.</param>
        public static void ReturnToThePoolOrDestroy(int PoolSlotID, GameObject Instance)
        {
            if (MAGICAL_POOL && PoolSlotID > 0)
            {  // pool enabled and slot id valid
                mpi.ReturnToThePool(PoolSlotID, Instance);
            }
            else
            {  // nope
                Destroy(Instance);   // normal destruction
            }
        }

        /// <summary>
        /// Find the player object, via invector, or if not ready via tag search fall back.
        /// </summary>
        /// <returns>Player game object.</returns>
        public static GameObject FindPlayerInstance()
        {
#if !VANILLA
            if (Invector.CharacterController.vThirdPersonController.instance)
            {  // valid?
                return Invector.CharacterController.vThirdPersonController.instance.gameObject;  // attempt grab player
            }
            else
            {  // instance not viable yet
#endif
                GameObject[] goPotentialPlayers = GameObject.FindGameObjectsWithTag("Player");  // find all tagged player
                foreach (GameObject goMaybePlayer in goPotentialPlayers)
                {  // check all results
                    if (goMaybePlayer.GetComponent<Animator>())
                    {  // has animator
                        return goMaybePlayer;  // then found
                    }
                }
#if !VANILLA
            }
#endif
            return null;  // player not found
        }  

        /// <summary>
        /// Find the player lock on if enabled
        /// </summary>
        /// <returns>Transform that is currently locked onto.</returns>
        public static Transform GetLockOn()
        {
            GameObject player = FindPlayerInstance();  // get the player instance from invector
            if (player)
            {  // valid?
#if !VANILLA
                vLockOn lockon = player.GetComponent<vLockOn>();  // grab the lock on component
                if (lockon)
                {  // found?
                    return lockon.currentTarget;  // send the current target back
                }
                else
                {  // fraid not
                    return null;  // no lock on 
                }
#else
                return null;   // none invector lock on code here
#endif
            }
            else
            {  // player not found
                return null;  // no lock on
            }
        }

        /// <summary>
        /// Find singular target within a radius.
        /// </summary>
        /// <param name="Center">Center of the sphere cast.</param>
        /// <param name="Range">Range to search within.</param>
        /// <param name="Layers">Layer filter.</param>
        /// <param name="Tags">Tag filter.</param>
        /// <param name="Nearest">Return the nearest.</param>
        /// <param name="CheckVisible">Ensure target is visible.</param>
        /// <param name="HeightAdjust">Height adjustment for search.</param>
        /// <param name="Alive">Ensure target is alive.</param>
        /// <returns>Transform of the selected target.</returns>
        public static Transform GetTargetWithinRange(Vector3 Center, float Range, LayerMask Layers, string[] Tags, bool Nearest, bool CheckVisible, float HeightAdjust, bool Alive)
        {
            // find targets within sphere range by tag/layer
            Transform tTarget = null;
            List<Transform> listTargetsInRange = FindAllTargetsWithinRange(Center, Range, Layers, Tags, CheckVisible, HeightAdjust, Alive);

            // find valid target from shortlist
            if (listTargetsInRange.Count > 0)
            {  // found some
                if (listTargetsInRange.Count > 1)
                {  // more than 1
                    if (!Nearest)
                    {  // random select
                        tTarget = listTargetsInRange[UnityEngine.Random.Range(0, listTargetsInRange.Count)];
                    }
                    else
                    {  // find nearest
                        Transform tMin = null;
                        float minDist = Mathf.Infinity;
                        Vector3 currentPos = Center;
                        foreach (Transform t in listTargetsInRange)
                        {
                            float dist = Vector3.Distance(t.position, currentPos);
                            if (dist < minDist)
                            {
                                tMin = t;
                                minDist = dist;
                            }
                        }
                        tTarget = tMin;
                    }
                }
                else
                {  // only 1 target in range, select it
                    tTarget = listTargetsInRange[0];
                }
            }

            // work complete
            if (DEBUGGING_MESSAGES)
            {
                if (tTarget)
                {
                    Debug.Log("Targeting " + tTarget.name);
                }
            }
            return tTarget;
        }

        /// <summary>
        /// Find list of all targets within a radius.
        /// </summary>
        /// <param name="Center">Center of the sphere cast.</param>
        /// <param name="Range">Range to search within.</param>
        /// <param name="Layers">Layer filter.</param>
        /// <param name="Tags">Tag filter.</param>
        /// <param name="Nearest">Return the nearest.</param>
        /// <param name="CheckVisible">Ensure target is visible.</param>
        /// <param name="HeightAdjust">Height adjustment for search.</param>
        /// <param name="Alive">Ensure target is alive.</param>
        /// <returns>Transform of the selected target.</returns>
        public static List<Transform> FindAllTargetsWithinRange(Vector3 Center, float Range, LayerMask Layers, string[] Tags, bool CheckVisible, float HeightAdjust, bool Alive)
        {
            // adjust center height for line of sight check
            Vector3 v3AdjustedCenter = Center;
            v3AdjustedCenter.y = v3AdjustedCenter.y + HeightAdjust;

            // find targets within sphere range by tag/layer
            List<Transform> listTargetsInRange = new List<Transform>();  // empty list
            Collider[] cTargets = Physics.OverlapSphere(Center, Range, Layers.value);  // who is close
            if (cTargets != null)
            {  // found some?
                for (int i = 0; i < cTargets.Length; i++)
                {
                    if (Tags.Contains(cTargets[i].transform.tag))
                    {  // tag matches
                        bool bValid = false;  // start valid check
                        if (!Alive)
                        {  // looking for no life?
                            bValid = true;  // all good then
                        }
                        else
                        {  // must be alive?
                            Animator aTemp = cTargets[i].transform.GetComponent<Animator>();  // grab animator
                            if (aTemp)
                            { // ensure only the actual character is returned
                                if (aTemp.enabled)
                                {  // and are not dead
                                    bValid = true;  // also all good
                                }
                            }
                        }
                        if (bValid)
                        {  // target either alive or we don't mind if not
                            if (CheckVisible)
                            {  // ensure visible via linecast    
                                RaycastHit rhHit;  // raycast result store
                                if (Physics.Linecast(v3AdjustedCenter, cTargets[i].transform.position, out rhHit) && rhHit.transform == cTargets[i].transform)
                                {
                                    listTargetsInRange.Add(cTargets[i].transform);  // ok add to list 
                                    if (GlobalFuncs.DEBUGGING_MESSAGES)
                                    {
                                        Debug.Log("Potential Target " + cTargets[i].transform.name);
                                    }
                                }
                            }
                            else
                            {  // targets can be round a corner
                                listTargetsInRange.Add(cTargets[i].transform);  // ok add to list 
                                if (GlobalFuncs.DEBUGGING_MESSAGES)
                                {
                                    Debug.Log("Potential Target " + cTargets[i].transform.name);
                                }
                            }
                        }
                    }
                }
            }
            return listTargetsInRange;   // work complete
        }  

        /// <summary>
        /// Spawn a list of prefabs via a derived spawner options list.
        /// </summary>
        /// <param name="Spawns">List of the prefabs to spawn.</param>
        /// <param name="Delay">Initial delay.</param>
        /// <param name="NoneSequential">Spawn copies of each in the spawn array in a blocks until no more</param>
        /// <param name="SpawnPoint">Transform to spawn at.</param>
        /// <param name="DestroyWhenFinished">Destroy parent when spawning complete.</param>
        /// <param name="DestroyPoolSlotID">Pool slot to return the parent to if parent was pooled.</param>
        /// <param name="FaceSpawnPoint">Force spawned game objects to face the spawn point.</param>
        /// <param name="WhichTarget">Targeting selection.</param>
        /// <returns></returns>
        public static IEnumerator SpawnAllDelayed(List<SpawnerOptionsDelayedSequence> Spawns, float Delay, bool NoneSequential,
            Transform SpawnPoint, GameObject DestroyWhenFinished, int DestroyPoolSlotID, bool FaceSpawnPoint, SpawnTarget WhichTarget)
        {

            // wait for x seconds before starting
            if (Delay > 0)
            {  // not instant?
                yield return new WaitForSeconds(Delay);  // wait
            }

            // spawn the list
            if (NoneSequential)
            { // process the list instances elm 0-x looped until no more to spawn
                bool bFoundMore = true;
                int iNext = 0;
                while (bFoundMore)
                {  // process until no more
                    bFoundMore = false;  // exit loop
                    iNext += 1;  // number to spawn test increment
                    foreach (SpawnerOptionsDelayedSequence soSpawnMe in Spawns)
                    {  // process all spawnables
                        if (soSpawnMe.NumberToSpawn >= iNext)
                        { // spawn another instance
                            bFoundMore = true;  // loop again
                            if (FaceSpawnPoint)
                            {  // rotate to face spawn point
                                GameObject goSpawned = soSpawnMe.Spawn(SpawnPoint, WhichTarget);  // create instance
                                goSpawned.transform.LookAt(SpawnPoint);  // rotate to source
                            }
                            else
                            {  // nope, just spawn
                                soSpawnMe.Spawn(SpawnPoint, WhichTarget);  // create instance
                            }
                            if (soSpawnMe.SourceOfAudio && soSpawnMe.PlayOnSpawn)
                            {  // play audio?
                                if (!soSpawnMe.SourceOfAudio.isPlaying)
                                {  // ignore if already playing
                                    soSpawnMe.SourceOfAudio.clip = soSpawnMe.PlayOnSpawn;  // set the clip
                                    soSpawnMe.SourceOfAudio.Play();  // play the clip
                                }
                            }
                            if (soSpawnMe.DelayTillNext > 0)
                            {  // delay enabled?
                                yield return new WaitForSeconds(soSpawnMe.DelayTillNext);  // wait
                            }
                        }
                    }
                }
            }
            else
            {   // sequential
                foreach (SpawnerOptionsDelayedSequence soSpawnMe in Spawns)
                {  // process all spawnables
                    for (int i = 0; i < soSpawnMe.NumberToSpawn; i++)
                    {   // process desired number
                        if (FaceSpawnPoint)
                        {  // rotate to face spawn point
                            GameObject goSpawned = soSpawnMe.Spawn(SpawnPoint, WhichTarget);  // create instance
                            goSpawned.transform.LookAt(SpawnPoint);  // rotate to source
                        }
                        else
                        {  // nope, just spawn
                            soSpawnMe.Spawn(SpawnPoint, WhichTarget);  // create instance
                        }
                        if (soSpawnMe.DelayTillNext > 0)
                        {  // delay enabled?
                            yield return new WaitForSeconds(soSpawnMe.DelayTillNext);  // wait
                        }
                    }
                }
            }

            // optional destruction
            if (DestroyWhenFinished)
            {  // destruction prefab specified?
                ReturnToThePoolOrDestroy(DestroyPoolSlotID, DestroyWhenFinished);  // attempt pooled returned
            }
        }  // trigger all spawns in the array     

        /// <summary>
        /// Spawn with the minimum options aka basic.
        /// </summary>
        /// <param name="Prefab">Prefab to spawn.</param>
        /// <param name="HowMany">Number of instances.</param>
        /// <param name="Where">Transform to center the spawns at.</param>
        /// <param name="Radius">Random radius options.</param>
        /// <param name="Target">Spawn targeting.</param>
        /// <returns></returns>
        public static GameObject SpawnBasic(GameObject Prefab, int HowMany, Transform Where, RandomSphereOptions Radius, SpawnTarget Target)
        {
            var soSpawnMe = new SpawnerOptionsDelayedSequence();
            soSpawnMe.Prefab = Prefab;
            soSpawnMe.NumberToSpawn = HowMany;
            soSpawnMe.RandomSphere = Radius;
            soSpawnMe.PhysicsForceOptions = new PhysicsOptions();
            soSpawnMe.RandomRotate = new RandomRotateOptions();
            return soSpawnMe.Spawn(Where, Target);
        }

        /// <summary>
        /// Use to set the targeting on various components before making active
        /// </summary>
        /// <remarks>
        /// Called per spell when pooling is disabled however when pooling is enabled
        /// a master instance is created for cloning of each spell and the targeting is only set once
        /// </remarks>
        /// <param name="MasterInstance"></param>
        /// <param name="Target"></param>
        /// <param name="SlotID"></param>
        public static void SetComponentTagsSlotsAndLayers(ref GameObject MasterInstance, SpawnTarget Target, int SlotID)
        {
            // grab target tag list
            string[] TargetTags;
            LayerMask TargetLayers;
            string[] InverseTargetTags;
            LayerMask InverseTargetLayers;
            switch (Target)
            {
                case SpawnTarget.Friend:
                    TargetTags = targetingTagsFriend;
                    TargetLayers = targetingLayerMaskFriend;
                    InverseTargetTags = targetingTagsEnemy;
                    InverseTargetLayers = targetingLayerMaskEnemy;
                    break;
                case SpawnTarget.Enemy:
                    TargetTags = targetingTagsEnemy;
                    TargetLayers = targetingLayerMaskEnemy;
                    InverseTargetTags = targetingTagsFriend;
                    InverseTargetLayers = targetingLayerMaskFriend;
                    break;
                default:
                    InverseTargetTags = TargetTags = targetingTagsAll;
                    InverseTargetLayers = TargetLayers = targetingLayerMaskAll;
                    break;
            }


            // set magic projectile target, tags, layers
            MagicProjectile[] allmProjectile = MasterInstance.GetComponentsInChildren<MagicProjectile>();
            foreach (MagicProjectile mProjectile in allmProjectile)
            {
                mProjectile.Target = Target;
                mProjectile.iPoolSlotID = SlotID + 1;
                switch (Target)
                {
                    case SpawnTarget.Friend:
                        mProjectile.HeatSeekTags = targetingTagsFriend;
                        mProjectile.HeatSeekLayers = targetingLayerMaskFriend;
                        break;
                    case SpawnTarget.Enemy:
                        mProjectile.HeatSeekTags = targetingTagsEnemy;
                        mProjectile.HeatSeekLayers = targetingLayerMaskEnemy;
                        break;
                    default:
                        mProjectile.HeatSeekTags = targetingTagsAll;
                        mProjectile.HeatSeekLayers = targetingLayerMaskAll;
                        break;
                }
            }

            // target and slot for teleport
            MagicTeleport mt = MasterInstance.GetComponentInChildren<MagicTeleport>();
            if (mt)
            {
                mt.Target = Target;
                mt.PoolSlotID = SlotID + 1;
                mt.TargetLayers = TargetLayers;
                mt.TargetTags = TargetTags;
            }

            // vobjectdamage  target tags
            vObjectDamage[] allVDamage = MasterInstance.GetComponentsInChildren<vObjectDamage>();
            foreach (vObjectDamage vd in allVDamage)
            {
                vd.tags = TargetTags.ToList();
            }

            // targeting for physics 
            MagicProjectilePhysics[] allmPhysics = MasterInstance.GetComponentsInChildren<MagicProjectilePhysics>();
            foreach (MagicProjectilePhysics mpp in allmPhysics)
            {
                mpp.TargetLayers = Target;
            }

            // and heal
            MagicHeal[] allHeal = MasterInstance.GetComponentsInChildren<MagicHeal>();
            foreach (MagicHeal mh in allHeal)
            {
                mh.HealTargetLayers = InverseTargetLayers;
                mh.HealTargetTags = InverseTargetTags;
            }


            // same for magic damage
            MagicObjectDamage[] allmDamage = MasterInstance.GetComponentsInChildren<MagicObjectDamage>();
            foreach (MagicObjectDamage md in allmDamage)
            {
                md.AOETarget = Target;
            }

            // slot for DestroyAndSpawn
            DestroyGameObjectAndSpawn[] allmDOS = MasterInstance.GetComponentsInChildren<DestroyGameObjectAndSpawn>();
            foreach (DestroyGameObjectAndSpawn dos in allmDOS)
            {
                dos.iPoolSlotID = SlotID + 1;
                dos.Target = Target;
            }

            // slot for DestroyAndSpawn
            DelayedSpawn[] allmDS = MasterInstance.GetComponentsInChildren<DelayedSpawn>();
            foreach (DelayedSpawn ds in allmDS)
            {
                ds.iPoolSlotID = SlotID + 1;
            }

            // same for collectible
            CollectableItem[] allmCI = MasterInstance.GetComponentsInChildren<CollectableItem>();
            foreach (CollectableItem ci in allmCI)
            {
                ci.iPoolSlotID = SlotID + 1;
            }


        }

        /// <summary>
        /// Shallow copy one class to another.
        /// </summary>
        /// <param name="dst">Destination class.</param>
        /// <param name="src">Source class.</param>
        public static void DuckCopyShallow(object dst, object src)
        {
            var srcT = src.GetType();
            var dstT = dst.GetType();
            foreach (var f in srcT.GetFields())
            {
                var dstF = dstT.GetField(f.Name);
                if (dstF == null)
                    continue;
                dstF.SetValue(dst, f.GetValue(src));
            }

            foreach (var f in srcT.GetProperties())
            {
                var dstF = dstT.GetProperty(f.Name);
                if (dstF == null)
                    continue;

                dstF.SetValue(dst, f.GetValue(src, null), null);
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
