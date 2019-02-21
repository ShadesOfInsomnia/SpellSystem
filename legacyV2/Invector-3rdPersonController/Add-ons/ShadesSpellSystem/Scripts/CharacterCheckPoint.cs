using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if !VANILLA
using Invector;
#endif

namespace Shadex
{
    /// <summary>
    /// Level check point aka next level portal, includes auto save to the data layer.
    /// </summary>
    /// <remarks>
    /// Requires a collider attached with trigger enabled.
    /// </remarks>
#if !VANILLA
    [vClassHeader("CHECK POINT", iconName = "triggerIcon")]
    public class CharacterCheckPoint : vMonoBehaviour
    {
#else
    public class CharacterCheckPoint : MonoBehaviour {
#endif
        /// <summary>Scene name to load on collider enter, must be in level list in the build settings.</summary>
        [Tooltip("Scene name to load on collider enter, must be in level list in the build settings")]
        public string SceneToLoad;

        /// <summary>Delay before loading the scene.</summary>
        [Tooltip("Delay before loading the scene")]
        public float LoadDelay;

        /// <summary>Name of the animator trigger to call on enter collider, default is birth started trigger aka spawn animation, leave empty for no animation.</summary>
        [Tooltip("Name of the animator trigger to call on enter collider, default is birth started trigger aka spawn animation, leave empty for no animation")]
        public string TriggerOnEnter = "BirthStarted";

        /// <summary>Delay before starting the spawnables.</summary>
        [Tooltip("Delay before starting the spawnables")]
        public float SpawnDelay;

        /// <summary>Process spawn lists in order sequential is the default, eg list element 0 all copies, then element 1 all copies.  Enable to process elm 0-x looped until no more to spawn.</summary>
        [Tooltip("Process spawn lists in order sequential is the default, eg list element 0 all copies, then element 1 all copies.  Enable to process elm 0-x looped until no more to spawn")]
        public bool NoneSequentialSpawns;

        /// <summary>List of game objects (Spells, Explosion, Teleport Particle, etc) to spawn when collider entered.</summary>
        [Tooltip("List of game objects (Spells, Explosion, Teleport Particle, etc) to spawn when collider entered")]
        public List<SpawnerOptionsDelayedSequence> SpawnOnEnter = new List<SpawnerOptionsDelayedSequence>();

        // internal
        private CharacterBase LevelingSystem;

        /// <summary>
        /// Save the game and load the next scene when collider trigger entered by the player.
        /// </summary>
        /// <param name="other">Collider that has entered the trigger.</param>
        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag.Equals("Player"))
            {  // only player can trigger
                if (SceneToLoad != "")
                {  // fail safe
                    // spawn all
                    if (SpawnOnEnter.Count > 0)
                    {
                        StartCoroutine(GlobalFuncs.SpawnAllDelayed(SpawnOnEnter, SpawnDelay, NoneSequentialSpawns, transform, null, 0, false, SpawnTarget.Any));  // trigger all spawns the the array  
                    }

                    // save to data layer
                    LevelingSystem = other.gameObject.GetComponent<CharacterBase>();
                    if (!LevelingSystem)
                    {  // not found?
                        if (GlobalFuncs.DEBUGGING_MESSAGES)
                        {
                            Debug.Log("Leveling system not found on player");
                        }
                    }
                    else
                    {  // all good
                       // save the game
                        LevelingSystem.LastSaveGameID = GlobalFuncs.TheDatabase().SavePlayerState(ref LevelingSystem, SceneToLoad, -1);
                        MainMenu MainMenuSystem = GlobalFuncs.TheMainMenu();
                        if (MainMenuSystem)
                        {
                            MainMenuSystem.LatestSaveGameID = LevelingSystem.LastSaveGameID;

                            // confirm save to user
                            MainMenuSystem.SaveGameMessage.CrossFadeAlpha(1f, 0.01f, true);
                            MainMenuSystem.SaveGameMessage.text = "Save Complete..";
                            MainMenuSystem.SaveGameMessage.CrossFadeAlpha(0f, 3f, true);

                            // load the next scene
                            MainMenu_FadingLoad Fader = MainMenuSystem.gameObject.GetComponent<MainMenu_FadingLoad>();
                            if (Fader)
                            {
                                Fader.BeginFade(1);
                                Fader.LoadSceneAsync(SceneToLoad, LoadDelay);  // start the load with progress bar
                            }
                            else
                            {
                                if (GlobalFuncs.DEBUGGING_MESSAGES)
                                {
                                    Debug.Log("Main menu system NOT found in scene");
                                }
                            }
                        }
                        else
                        {
                            if (GlobalFuncs.DEBUGGING_MESSAGES)
                            {
                                Debug.Log("Main menu system NOT found in scene");
                            }
                        }
                    }
                }
                else
                {
                    if (GlobalFuncs.DEBUGGING_MESSAGES)
                    {
                        Debug.Log("Scene to load is NOT set");
                    }
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
