using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System;
#if !VANILLA
using Invector;
using Invector.vCharacterController;
using Invector.vItemManager;
#endif
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Shadex
{
    /// <summary>
    /// Game main menu uGUI wrapper
    /// </summary>
#if !VANILLA
    [vClassHeader("MAINMENU", iconName = "inputIcon")]
    public class MainMenu : vMonoBehaviour
#else
    public class MainMenu : MonoBehaviour
#endif
    {
        /// <summary>Lobby (main menu scene name), note to use level loading/new game your scenes must be in the unity build list, including when running in the editor.</summary>
        [Header("Scenes")]
        [Tooltip("Lobby (main menu scene name), note to use level loading/new game your scenes must be in the unity build list, including when running in the editor")]
        public string LobbySceneName;

        /// <summary>Scene name to load when the new game menu option is selected.</summary>
        [Tooltip("Scene to load on a new game")]
        public string FirstSceneName;

#if !VANILLA
        /// <summary>Link to the inventory to prevent both menus keys clashing".</summary>
        [Tooltip("Link to the inventory to prevent both menus keys clashing")]
        public vInventory Inventory;
#endif

        /// <summary>Slight delay needed before lobby main menu is opened to load the lobby scene.</summary>
        [Header("Menu Options")]
        [Tooltip("Slight delay needed before lobby main menu is opened to load the lobby scene")]
        public float LobbyLoadDelay = 1f;

        /// <summary>Delay on death before reloading the most recent save.</summary>
        [Tooltip("Delay on death before reloading the most recent save")]
        public float DeathLoadDelay = 15f;

        /// <summary>UI to hide when in lobby, show when normal scene loaded.</summary>
        [Tooltip("UI to hide when in lobby, show when normal scene loaded")]
        public List<GameObject> UserInterface = new List<GameObject>();

        /// <summary>Timescale whilst menu is open, default is zero aka game fully paused.</summary>
        [Tooltip("Timescale whilst menu is open, default is zero aka game fully paused")]
        [Range(0, 1)]
        public float timeScaleWhileIsOpen = 0;

        /// <summary>Timescale to return to when the main menu is closed.</summary>
        [Tooltip("Timescale to return to when the main menu is closed")]
        public float originalTimeScale = 1f;

        /// <summary>Only disable if the main menu is being placed as a sub item of another object that is set to not destroy on load.</summary>
        [Tooltip("Only disable if the main menu is being placed as a sub item of another object that is set to not destroy on load")]
        public bool dontDestroyOnLoad = true;

        /// <summary>Audio clip to play on spawn.</summary>
        [Tooltip("Audio clip to play on spawn")]
        public AudioClip PlayOnSpawn;

        /// <summary>Audio clip to play on death.</summary>
        [Tooltip("Audio clip to play on death")]
        public AudioClip PlayOnDeath;

        /// <summary>Audio source to play the clip.</summary>
        [Tooltip("Audio source to play the clip")]
        public AudioSource SourceOfAudio;

        /// <summary>Main menu first window to be open.</summary>
        [Header("Main Menu Window")]
        [Tooltip("Main menu first window to be open")]
        public GameObject FirstWindow;

        /// <summary>Async fading scene loader.</summary>
        [Tooltip("Async fading scene loader")]
        public MainMenu_FadingLoad Fader;

        /// <summary>Load game button root, for hiding when no save games exist.</summary>
        [Tooltip("Load game button root, for hiding when no save games exist")]
        public GameObject LoadGameButton;

        /// <summary>Continue game button root, for swapping with save when in the lobby or hiding when no save games.</summary>
        [Tooltip("Continue game button root, for swapping with save when in the lobby or hiding when no save games")]
        public GameObject ContinueButton;

        /// <summary>Save game button root, for swapping with continue when in the lobby.</summary>
        [Tooltip("Save game button root, for swapping with continue when in the lobby")]
        public GameObject SaveGameButton;

        /// <summary>Save game success/fail on-screen message.</summary>
        [Tooltip("Save game success/fail on-screen message")]
        public Text SaveGameMessage;

        /// <summary>New character window game object.</summary>
        [Header("New Character Window")]
        [Tooltip("New character window game object")]
        public GameObject NewCharacterWindow;

        /// <summary>Character name text box control.</summary>
        [Tooltip("Character name text box control")]
        public InputField CharacterNameTextBox;

        /// <summary>Axis drop down list control.</summary>
        [Tooltip("Axis drop down list control")]
        public Dropdown AxisDropDown;

        /// <summary>Alignment drop down list control.</summary>
        [Tooltip("Alignment drop down list control")]
        public Dropdown AlignDropDown;

        /// <summary>Race drop down list control.</summary>
        [Tooltip("Race drop down list control")]
        public Dropdown RaceDropDown;

        /// <summary>Class drop down list control.</summary>
        [Tooltip("Class drop down list control")]
        public Dropdown ClassDropDown;

        /// <summary>Validation messages label control.</summary>
        [Tooltip("Validation messages label control")]
        public Text ValidationLabel;

        /// <summary>Load game window game object.</summary>
        [Header("Load Game Window")]
        [Tooltip("Load game window game object")]
        public GameObject LoadGameWindow;

        /// <summary>Character drop down list control.</summary>
        [Tooltip("Character drop down list control")]
        public Dropdown CharacterDropDown;

        /// <summary>Scrollable list of save games game object.</summary>
        [Tooltip("Scrollable list of save games game object")]
        public GameObject LoadGameScrollable;

        /// <summary>Scrollable list master item game object for cloning.</summary>
        [Tooltip("Scrollable list master item game object for cloning")]
        public GameObject LoadGameScrollItem;

#if !VANILLA
        /// <summary>Open menu input.</summary>
        [Header("Input Mapping")]
        public GenericInput openMenu = new GenericInput("Escape", "Start", "Start");

        /// <summary>Menu horizontal movement input.</summary>
        [Header("This fields will override the EventSystem Input")]
        public GenericInput horizontal = new GenericInput("Horizontal", "D-Pad Horizontal", "Horizontal");

        /// <summary>Menu vertical movement input.</summary>
        public GenericInput vertical = new GenericInput("Vertical", "D-Pad Vertical", "Vertical");

        /// <summary>Menu submit input.</summary>
        public GenericInput submit = new GenericInput("Return", "A", "A");

        /// <summary>Menu back/cancel input.</summary>
        public GenericInput cancel = new GenericInput("Backspace", "B", "B");
#endif
        // internal
        private StandaloneInputModule inputModule;
        [HideInInspector] public bool isOpen, lockInput, isNotLobby, updatedTimeScale;

        private CharacterBase LevelingSystem;
        private List<SimpleDataPair> AllCharacters;
        private List<SimpleDataPair> SaveGamesForSelectedCharacter;
        private List<GameObject> SaveGameCreatedButtonList = new List<GameObject>();
        private int SelectedSlotID, CurrentCharacter;
        [HideInInspector] public int LatestSaveGameID;
        [HideInInspector] public int LatestSaveSlotID;
        private bool ReAnimating;

        /// <summary>
        /// Initialise the game control menu.
        /// </summary>
        /// <remarks>
        /// Sets up the unity don't destroy and locks the main menu open when in the lobby scene.
        /// </remarks>
        void Start()
        {
            // input handling
            inputModule = FindObjectOfType<StandaloneInputModule>();

            // ensure this script is moved between scenes
            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            // force main menu open when scene is the lobby
            if (!isNotLobby)
            {
                // fill dropdown lists on new character
                AxisDropDown.ClearOptions();
                AxisDropDown.AddOptions(new List<string>(Enum.GetNames(typeof(BaseAxis))));
                AlignDropDown.ClearOptions();
                AlignDropDown.AddOptions(new List<string>(Enum.GetNames(typeof(BaseAlignment))));
                RaceDropDown.ClearOptions();
                RaceDropDown.AddOptions(new List<string>(Enum.GetNames(typeof(BaseRace))));
                ClassDropDown.ClearOptions();
                ClassDropDown.AddOptions(new List<string>(Enum.GetNames(typeof(BaseClass))));
                ValidationLabel.text = "";

                // show the continue button and cache the most recent save game
                GlobalFuncs.TheDatabase().FindMostRecentSaveGameID(ref LatestSaveSlotID, ref LatestSaveGameID);
                ContinueButton.SetActive(LatestSaveGameID > 0 && LatestSaveSlotID > 0);
                LoadGameButton.SetActive(LatestSaveGameID > 0 && LatestSaveSlotID > 0);

                // lock the main menu open
                StartCoroutine(forceMenuOpen());
            }
        }

        /// <summary>
        /// Handle inputs as defined in the settings.
        /// </summary>
        void LateUpdate()
        {
            if (lockInput) return;
            ControlWindowsInput();
        }

        /// <summary>
        /// Occurs when the new game menu option is clicked.
        /// </summary>
        public void NewGame()
        {
            if (CharacterNameTextBox.text.Length > 2)
            {
                ValidationLabel.text = "";
                if (FirstSceneName.Length > 0)
                {
                    // find the leveling system
                    if (!LevelingSystem)
                    {
                        GameObject player = GlobalFuncs.FindPlayerInstance();
                        if (player)
                        {
                            LevelingSystem = player.GetComponent<CharacterBase>();
                        }
                        else
                        {
                            if (GlobalFuncs.DEBUGGING_MESSAGES)
                            {
                                Debug.Log("Unable to find player");
                            }
                        }
                    }
                    if (!LevelingSystem)
                    {  // not found?
                        if (GlobalFuncs.DEBUGGING_MESSAGES)
                        {
                            Debug.Log("Leveling system not found on player");
                        }
                    }
                    else
                    {  // all good
                        // set the leveling system initial values
                        LevelingSystem.CurrentLevel = 1;
                        LevelingSystem.Name = CharacterNameTextBox.text;
                        LevelingSystem.CurrentAxis = (BaseAxis)AxisDropDown.value;
                        LevelingSystem.CurrentAlignment = (BaseAlignment)AlignDropDown.value;
                        LevelingSystem.CurrentRace = (BaseRace)RaceDropDown.value;
                        LevelingSystem.CurrentClass = (BaseClass)ClassDropDown.value;
                        LevelingSystem.ResetToDefaults();  // reset character to the selection
                        LevelingSystem.reCalcCore(true);  // recalc the core stats, max out health etc
                        CharacterNameTextBox.text = "";  // clear for mid game create new character

                        // create a new slot and first save game in the data layer
                        LevelingSystem.SaveSlotID = 0;
                        LevelingSystem.LastSaveGameID = GlobalFuncs.TheDatabase().SavePlayerState(ref LevelingSystem, FirstSceneName, -1);
                        GlobalFuncs.TheDatabase().ClearInventoryAndHUD();

                        // close the menu and load the scene
#if !VANILLA
                        Inventory.gameObject.SetActive(true);
#endif
                        NewCharacterWindow.SetActive(false);
                        Fader.BeginFade(1);
                        isOpen = false;
                        lockInput = false;
                        Fader.LoadSceneAsync(FirstSceneName);  // start the load with progress bar
                    }
                }
                else
                {
                    if (GlobalFuncs.DEBUGGING_MESSAGES)
                    {
                        Debug.Log("First scene name is not specified");
                    }
                }
            }
            else
            {
                ValidationLabel.text = "Missing Character Name..";
            }
        }

        /// <summary>
        /// Fills the load game GUI screen from the data layer
        /// </summary>
        public void ShowLoadGame()
        {
            // load the character list from the data layer           
            AllCharacters = GlobalFuncs.TheDatabase().GetCharacterSlotList();

            // build string array from the names 
            List<string> AllCharNames = new List<string>();
            foreach (SimpleDataPair sdp in AllCharacters)
            {
                string[] sdpSplit = sdp.Display.Split('~');
                AllCharNames.Add(sdpSplit[0] + " (" + sdpSplit[1].Split('T')[0] + ")");
            }

            // fill the dropdown
            CharacterDropDown.ClearOptions();
            CharacterDropDown.AddOptions(AllCharNames);

            // fill the save game list
            if (AllCharacters.Count > 0) ChangeCharacter(CurrentCharacter);

            // show the load game window
            FirstWindow.gameObject.SetActive(false);
            LoadGameWindow.SetActive(true);
        }

        /// <summary>
        /// Updates the list of save games from the character drop down selection
        /// </summary>
        /// <param name="SelectedIndex">Index of the character that has been selected.</param>
        public void ChangeCharacter(int SelectedIndex)
        {
            CurrentCharacter = SelectedIndex;

            // remove previous buttons
            if (SaveGameCreatedButtonList.Count > 0)
            {
                foreach (GameObject child in SaveGameCreatedButtonList)
                {
                    DestroyImmediate(child);
                }
            }

            // load all save games for this character from the datalayer
            SaveGameCreatedButtonList = new List<GameObject>();
            SelectedSlotID = AllCharacters[SelectedIndex].Value;
            //SaveGamesForSelectedCharacter = GlobalFuncs.TheDatabase().GetShortList("vSaveGames", "ID", new string[] { "CreatedOn", "Level", "XP", "SceneName" }, "CreatedOn", true, "SlotID", SelectedSlotID.ToString(), false, "^");
            SaveGamesForSelectedCharacter = GlobalFuncs.TheDatabase().GetSaveGameListForSlot(SelectedSlotID);

            // clone a button from the master for each save game
            foreach (SimpleDataPair sdp in SaveGamesForSelectedCharacter)
            {
                GameObject LoadButton = Instantiate(LoadGameScrollItem);
                LoadButton.transform.SetParent(LoadGameScrollable.transform);

                Text LoadButtonText = LoadButton.GetComponentInChildren<Text>();
                string[] sdpSplit = sdp.Display.Split('~');
                LoadButtonText.text = sdpSplit[0].Replace('T', ' ') + " (Lvl " + sdpSplit[1] + " XP " + sdpSplit[2] + ") " + sdpSplit[3];

                Button LoadActualButton = LoadButton.GetComponent<Button>();
                LoadActualButton.onClick.AddListener(() => { LoadGame(sdp.Value); });

                LoadButton.SetActive(true);
                SaveGameCreatedButtonList.Add(LoadButton);
            }
        }

        /// <summary>
        /// Loads the selected save game scene from when selected in the GUI.
        /// </summary>
        /// <param name="SaveGameID">ID of the save game to load from the data layer.</param>
        public void LoadGame(int SaveGameID)
        {
            // clear player state
            GlobalFuncs.TheDatabase().ClearInventoryAndHUD();

            // find the leveling system
            if (!LevelingSystem)
            {
                GameObject player = GlobalFuncs.FindPlayerInstance();
                if (player)
                {
                    LevelingSystem = player.GetComponent<CharacterBase>();
                }
                else
                {
                    if (GlobalFuncs.DEBUGGING_MESSAGES)
                    {
                        Debug.Log("Unable to find player");
                    }
                }
            }
            if (!LevelingSystem)
            {  // not found?
                if (GlobalFuncs.DEBUGGING_MESSAGES)
                {
                    Debug.Log("Levelling system not found on player");
                }
            }
            else
            {  // all good
                // load character from the data layer
                LevelingSystem.SaveSlotID = SelectedSlotID;
                LevelingSystem.LastSaveGameID = SaveGameID;
                string SceneName = GlobalFuncs.TheDatabase().LoadPlayerState(ref LevelingSystem, SaveGameID);

                // close the menu and load the scene
                if (SceneName != "")
                {
#if !VANILLA
                    Inventory.gameObject.SetActive(true);
#endif
                    LoadGameWindow.SetActive(false);
                    FirstWindow.gameObject.SetActive(false);
                    Fader.BeginFade(1);
                    isOpen = false;
                    lockInput = false;
                    Fader.LoadSceneAsync(SceneName);  // start the load with progress bar
                }
                else
                {
                    if (GlobalFuncs.DEBUGGING_MESSAGES)
                    {
                        Debug.Log("Failed to load save game ID " + SaveGameID);
                    }
                }
            }
        }

        /// <summary>
        /// Loads the most recent save game from the data layer.
        /// </summary>
        public void ContinueGame()
        {
#if !VANILLA
            Inventory.gameObject.SetActive(true);
#endif
            ContinueButton.SetActive(false);
            SaveGameButton.SetActive(true);
            SelectedSlotID = LatestSaveSlotID;
            LoadGame(LatestSaveGameID);
        }

        /// <summary>
        /// Save the current scene/character state to the data layer
        /// </summary>
        public void SaveGame()
        {
            if (!LevelingSystem)
            {
                GameObject player = GlobalFuncs.FindPlayerInstance();
                if (player)
                {
                    LevelingSystem = player.GetComponent<CharacterBase>();
                }
                else
                {
                    if (GlobalFuncs.DEBUGGING_MESSAGES)
                    {
                        Debug.Log("Unable to find player");
                    }
                }
            }
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
                LevelingSystem.LastSaveGameID = GlobalFuncs.TheDatabase().SavePlayerState(ref LevelingSystem, SceneManager.GetActiveScene().name, -1);
                LatestSaveGameID = LevelingSystem.LastSaveGameID;

                // hide the menu and enable time       
                foreach (GameObject ui in UserInterface)
                {
                    ui.SetActive(true);
                }
#if !VANILLA
                Inventory.gameObject.SetActive(true);
#endif
                FirstWindow.gameObject.SetActive(false);
                isOpen = false;
                lockInput = false;
                Time.timeScale = 1f;

                // confirm save to user
                SaveGameMessage.CrossFadeAlpha(1f, 0.01f, true);
                SaveGameMessage.text = "Save Complete..";
                SaveGameMessage.CrossFadeAlpha(0f, 3f, true);
            }
        }

        /// <summary>
        /// Occurs when the player dies
        /// </summary>
        /// <remarks>
        /// Loads the most recent save after the specified timeout.
        /// </remarks>
        public void OnReloadGame()
        {
            StartCoroutine(delayedContinue());
        }

        /// <summary>
        /// Load the most recent save with a delay.
        /// </summary>
        /// <returns>IEnumerator until specified delay has passed.</returns>
        public IEnumerator delayedContinue()
        {
            ReAnimating = true;  // disable main menu
#if !VANILLA
            Inventory.enabled = false; // and the inventory keys
#endif

            // disable leveling system (otherwise health regen reanimates the character)
            LevelingSystem.enabled = false;
#if !VANILLA
            var invectorController = GlobalFuncs.FindPlayerInstance().GetComponent<vCharacter>();
            invectorController.enabled = false;
#endif

            // disable UI
            foreach (GameObject ui in UserInterface)
            {
                ui.SetActive(false);
            }

            // death sound
            if (SourceOfAudio && PlayOnDeath)
            {  // play audio?
                if (!SourceOfAudio.isPlaying)
                {  // ignore if already playing
                    SourceOfAudio.clip = PlayOnDeath;  // set the clip
                    SourceOfAudio.Play();  // play the clip
                }
            }

            // wait
            yield return new WaitForSeconds(DeathLoadDelay);

            // restart the game from the last save
            LevelingSystem.enabled = true;
#if !VANILLA
            invectorController.enabled = true;
            Inventory.enabled = true;
#endif
            LevelingSystem.reCalcCore(true);
            ReAnimating = false;
            
            ContinueGame();
        }

        /// <summary>
        /// Occurs when the loaded scene is the lobby, forces menu open rather than player control.
        /// </summary>
        /// <returns>IEnumerator until the lobby load delay has passed.</returns>
        public IEnumerator forceMenuOpen()
        {
            yield return new WaitForSeconds(LobbyLoadDelay);

            FirstWindow.gameObject.SetActive(true);
            isOpen = true;

            if (!updatedTimeScale)
            {
                updatedTimeScale = true;
                originalTimeScale = Time.timeScale;
            }
            Time.timeScale = timeScaleWhileIsOpen;
#if !VANILLA
            Inventory.gameObject.SetActive(false);
#endif
        }

        /// <summary>
        /// Handle user input
        /// </summary>
        public virtual void ControlWindowsInput()
        {
            if (ReAnimating) return;  // dead so ignore keys
#if !VANILLA
            if (Inventory)
                if (Inventory.isOpen) return;  // ignore if the inventory is open
#endif

            // check menu open/back keys
#if !VANILLA
            if (openMenu.GetButtonDown())
#else
            if (Input.GetKeyDown("mainmenu"))
#endif
            {
                if (!isOpen)
                {
                    FirstWindow.gameObject.SetActive(true);
                    isOpen = true;
                    if (!updatedTimeScale)
                    {
                        updatedTimeScale = true;
                        originalTimeScale = Time.timeScale;
                    }
                    Time.timeScale = timeScaleWhileIsOpen;
#if !VANILLA
                    Inventory.gameObject.SetActive(false);
#endif
                    foreach (GameObject ui in UserInterface)
                    {
                        ui.SetActive(false);
                    }
                }
                else
                {
                    LoadGameWindow.SetActive(false);
                    NewCharacterWindow.SetActive(false);
                    if (LobbySceneName != SceneManager.GetActiveScene().name)
                    {
                        FirstWindow.gameObject.SetActive(!FirstWindow.gameObject.activeInHierarchy);
                        if (!FirstWindow.gameObject.activeInHierarchy)
                        {
                            isOpen = false;
#if !VANILLA
                            Inventory.gameObject.SetActive(true);
#endif
                            Time.timeScale = originalTimeScale;
                            foreach (GameObject ui in UserInterface)
                            {
                                ui.SetActive(true);
                            }
                        }
                    }
                    else
                    {
                        FirstWindow.gameObject.SetActive(true);
                    }
                }
            }
        }

        /// <summary>
        /// Swap input device when joystick is plugged/unplugged.
        /// </summary>
        void UpdateEventSystemInput()
        {
            if (inputModule)
            {
#if !VANILLA
                inputModule.horizontalAxis = horizontal.buttonName;
                inputModule.verticalAxis = vertical.buttonName;
                inputModule.submitButton = submit.buttonName;
                inputModule.cancelButton = cancel.buttonName;
#endif
            }
            else
            {
                inputModule = FindObjectOfType<StandaloneInputModule>();
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