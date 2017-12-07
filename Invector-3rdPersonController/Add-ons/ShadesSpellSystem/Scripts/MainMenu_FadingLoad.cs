using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Invector;

namespace Shadex
{
    /// <summary>
    /// Fades in all levels loaded via event hook override.
    /// </summary>
    [vClassHeader("FADING LOADER", iconName = "triggerIcon")]
    public class MainMenu_FadingLoad : vMonoBehaviour
    {
        /// <summary>Speed of the fade to background and back again transition.</summary>
        [Tooltip("Transition speed")]
        public float fadeSpeed = 0.8f;

        /// <summary>Reference to the invector third person controller player camera.</summary>
        [Header("Class links")]
        [Tooltip("Invector TPC camera")]
        public vThirdPersonCamera PlayerCamera;

        /// <summary>Reference to the main menu component which hold most of the settings.</summary>
        [Tooltip("Shadex main menu component")]
        public MainMenu GameMainMenu;

        /// <summary>Level loading progress bar texture.</summary>
        [Header("Textures")]
        [Tooltip("Progress bar bar texture")]
        public Texture2D ProgressBar;

        /// <summary>Level loading progress bar background texture.</summary>
        [Tooltip("Progress bar background texture")]
        public Texture2D ProgressBarBackground;

        /// <summary>Full screen texture to fade to and back again whilst the level is loading.</summary>
        [Tooltip("Full screen texture to fade between the scenes")]
        public Texture2D fadeOutTexture;


        // internal
        private int drawDepth = -1000;
        private float alpha = 1.0f;
        private int fadeDir = -1;
        private AsyncOperation Async;

        /// <summary>
        /// Occurs when level is loading.
        /// </summary>
        void OnGUI()
        {
            // fade out/in the alpha value using a direction, a speed and Time.deltatime to convert the operation to seconds
            alpha += fadeDir * fadeSpeed * Time.fixedDeltaTime;

            // force (Clamp) the number between 0 and 1 because GUI.color uses alpha values between 0 and 1
            alpha = Mathf.Clamp01(alpha);

            // set color of our GUI (in this case our texture). All color values remain the same & the Alpha is set to the alpha variable
            GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, alpha);                // set the alpha value
            GUI.depth = drawDepth;                                                              // make the texture render on top (drawn last)
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), fadeOutTexture);       // draw the texture to fit the entire screen area

            if (Async != null)
            {
                // Progress Bar Size
                int Width = Screen.width / 3;
                int Heigth = 60;

                // Center in the screen
                int X = (Screen.width / 2) - (Width / 2);
                int Y = (Screen.height / 2) - (Heigth / 2);

                // Draw on screen
                GUI.depth = drawDepth;                                                              // make the texture render on top (drawn last)
                GUI.DrawTexture(new Rect(X, Y, Width, Heigth), ProgressBarBackground);          // draw the progress bar background
                GUI.DrawTexture(new Rect(X, Y, Width * Async.progress, Heigth), ProgressBar);       // draw the progress bar		

                GUIStyle gs = new GUIStyle();
                gs.fontSize = 40;
                gs.alignment = TextAnchor.MiddleCenter;

                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                GUI.Label(new Rect(X, Y, Width, Heigth), string.Format("{0:N0}%", Async.progress * 100), gs);
            }
        }

        /// <summary>
        /// Sets fadeDir to the direction parameter making the scene fade in if -1 and out if 1
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public float BeginFade(int direction)
        {
            fadeDir = direction;
            return fadeSpeed;
        }  // 

        /// <summary>
        /// Occurs when the scene changes, adds our level loading override to the unity scene loaded delegate.
        /// </summary>
        void OnEnable()
        {
            SceneManager.sceneLoaded += OnLevelFinishedLoading;
        }

        /// <summary>
        /// Occurs when a scene is loaded completely, assigns the player to the level spawn point and reactivates components.
        /// </summary>
        /// <param name="scene">Current scene that has finished loading.</param>
        /// <param name="mode">Scene load method (Additive or Singular).</param>
        public void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
        {
            // flag whether the lobby was just loaded (causes main menu to open auto)
            GameMainMenu.isNotLobby = (GameMainMenu.LobbySceneName != scene.name);
            GameMainMenu.SaveGameButton.SetActive(GameMainMenu.isNotLobby);
            GameMainMenu.LoadGameButton.SetActive(GameMainMenu.isNotLobby);
            GameMainMenu.ContinueButton.SetActive(!GameMainMenu.isNotLobby);

            // find the spawn point and update the player position
            GameObject psp = GameObject.Find("SpawnPoint");
            if (psp)
            {
                GameObject player = GlobalFuncs.FindPlayerInstance();
                //GameController.spawnPoint = psp.transform;
                player.transform.position = psp.transform.position;
                player.transform.rotation = psp.transform.rotation;
            }
            else
            {
                if (GameMainMenu.LobbySceneName != scene.name)
                {
                    Debug.Log("Spawn Point NOT Found");
                }
            }

            // start time and fade in from full screen texture
            PlayerCamera.Init();
            foreach (GameObject ui in GameMainMenu.UserInterface)
            {
                ui.SetActive(GameMainMenu.isNotLobby);
            }
            Time.timeScale = 1f;
            BeginFade(-1);
            Debug.Log("Load Level (" + scene.name + ") Complete");

            // scene change sound
            if (GameMainMenu.SourceOfAudio && GameMainMenu.PlayOnSpawn)
            {  // play audio?
                if (!GameMainMenu.SourceOfAudio.isPlaying)
                {  // ignore if already playing
                    GameMainMenu.SourceOfAudio.clip = (GameMainMenu.LobbySceneName != scene.name ? GameMainMenu.PlayOnSpawn : GameMainMenu.PlayOnDeath);  // set the clip
                    GameMainMenu.SourceOfAudio.Play();  // play the clip
                }
            }
        }

        /// <summary>
        /// Start loading the scene in async mode via coroutine.
        /// </summary>
        /// <param name="SceneName">Scene name to load.</param>
        /// <param name="WaitFor">Delay before loading the scene.</param>
        public void LoadSceneAsync(string SceneName, float WaitFor = 0.6f)
        {
            StartCoroutine(ChangeSceneAsync(SceneName, WaitFor));
        }  // Load scene with fade in/out effect asynchronously

        /// <summary>
        /// Scene changing async body to be called from a coroutine.
        /// </summary>
        /// <param name="SceneName">Scene name to load.</param>
        /// <param name="WaitFor">Delay before loading the scene.</param>
        /// <returns>Async iterator for the calling coroutine.</returns>
        IEnumerator ChangeSceneAsync(string SceneName, float WaitFor = 0.6f)
        {
            // wait for animation to stop playing
            yield return StartCoroutine(WaitForRealSeconds(WaitFor));

            // fade out the game and load a new scene
            BeginFade(1);
            Async = SceneManager.LoadSceneAsync(SceneName);
            yield return Async;
        }

        /// <summary>
        /// Wait for actual seconds for use with the load async coroutine.
        /// </summary>
        /// <param name="time">Real amount of seconds to wait.</param>
        /// <returns>Iterator until input time has passed.</returns>
        public static IEnumerator WaitForRealSeconds(float time)
        {
            float start = Time.realtimeSinceStartup;

            while (Time.realtimeSinceStartup < start + time)
            {
                yield return null;
            }
        }
    }
}

/* *****************************************************************************************************************************
* Copyright        : 2017 Shades of Insomnia
* Founding Members : Charles Page (Shade)
*                  : Rob Alexander (Insomnia)
* License          : Attribution-ShareAlike 4.0 International (CC BY-SA 4.0) https://creativecommons.org/licenses/by-sa/4.0/
* Thanks to        : Maurizio Lepora https://gist.github.com/leMaur/d99fd93a9812b76c535f319cb6b5478d
*                    Asbjørn Thirslund https://youtu.be/0HwZQt94uHQ
*                    for the base of this script
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
