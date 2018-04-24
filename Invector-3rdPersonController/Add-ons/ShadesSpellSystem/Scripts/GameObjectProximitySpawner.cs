using UnityEngine;
#if !VANILLA
using Invector;
#endif

namespace Shadex
{
    /// <summary>
    /// Spawn a game object withing a random sphere when triggered via proximity to player.
    /// </summary>
    /// <remarks>
    /// Whilst superseded by the proximity options on the spawner class this clean spawner
    /// is a clean example of proximity trap game object spawning.
    /// </remarks>
#if !VANILLA
    [vClassHeader("GAMEOBJECT PROXIMITY SPAWNER", iconName = "triggerIcon")]
    public class GameObjectProximitySpawner : vMonoBehaviour
    {
#else
    public class GameObjectProximitySpawner : MonoBehaviour {
#endif
        /// <summary>Game object to spawn.</summary>
        public GameObject ObjectToSpawn;

        /// <summary>Number of game objects to spawn.</summary>
        public int NumberToSpawn;

        /// <summary>Player proximity to trigger.</summary>
        public float Proximity;

        /// <summary>Center of the spawn area.</summary>
        public Transform SpawnArea;

        // internal
        private float NextCheck;        
        private float CheckRate;        
        private Transform MyTransform;
        private Transform PlayerTransform;        
        private Vector3 SpawnPosition;
        
        /// <summary>
        /// Initialise.
        /// </summary>
        void Start()
        {
            MyTransform = transform;
            PlayerTransform = GlobalFuncs.FindPlayerInstance().transform;
            CheckRate = Random.Range(0.8f, 1.2f); ;
        }

        /// <summary>
        /// Check for player proximity.
        /// </summary>
        void Update()
        {
            if (Time.time > NextCheck)
            {
                NextCheck = Time.time + CheckRate;
                if (Vector3.Distance(MyTransform.position, PlayerTransform.position) < Proximity)
                {
                    for (int i = 0; i < NumberToSpawn; i++)
                    {
                        SpawnPosition = SpawnArea.position + Random.insideUnitSphere * 5;       //Randomly spawn inside sphere collider
                        Instantiate(ObjectToSpawn, SpawnPosition, MyTransform.rotation);
                    }
                    this.enabled = false;
                }
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
