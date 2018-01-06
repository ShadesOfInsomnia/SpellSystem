using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
#if !VANILLA
using Invector;
using Invector.CharacterController;
using Invector.ItemManager;
#endif

namespace Shadex
{
    /// <summary>
    /// Core control class for the magic system settings.
    /// </summary>
#if !VANILLA
    [vClassHeader("MAGIC SETTINGS", iconName = "inputIcon")]
    public class MagicSettings : vMonoBehaviour
    {
#else
    public class MagicSettings : MonoBehaviour {
#endif
        /// <summary>Enable debugging messages across all spell system classes.</summary>
        [Tooltip("Enable debugging messages")]
        public bool DebuggingMessages = false;

        /// <summary>Enable the magical pool for much smoother scene performance.</summary>
        [Tooltip("Enable the magical pool")]
        public bool PooledMagic = false;

        /// <summary>Global set dead enemy delay before destruction.</summary>
        [Tooltip("Global set dead enemy delay before destruction")]
        public float EnemyDestroyDelay = 10f;

        /// <summary>Global set dead enemy collectible drop radius.</summary>
        [Tooltip("Global set dead enemy collectible drop radius")]
        public float OnDeathDropRadius = 2.5f;

        /// <summary>Array of spell trigger keys and/or buttons.</summary>
        [Header("MagicCombat Inputs")]
        [Tooltip("Array of spell trigger keys and/or buttons")]
        public List<MagicSpellTrigger> SpellsTriggers = new List<MagicSpellTrigger>();

        /// <summary>Create a child empty game object, move to head height and forward then drop here.</summary>
        [Header("MagicCombat Output")]
        [Tooltip("Create a child empty game object, move to head height and forward then drop here")]
        public Transform MagicSpawnPoint;

        /// <summary>Set to the event that decreases the remaining mana, takes an int parameter.</summary>
        [Tooltip("Set to the event that decreases the remaining mana, takes an int parameter")]
        public UnityIntEvent onUseMana;

        /// <summary>XP held uGUI text display element.</summary>
        [Header("GUI Display")]
        [Tooltip("XP held uGUI text display element")]
        public Text XPText;

        /// <summary>Current character level uGUI text display element.</summary>
        [Tooltip("Current character level uGUI text display element")]
        public Text LevelText;

        /// <summary>Level up message uGUI text display element.</summary>
        [Tooltip("Level up message uGUI text display element")]
        public Text LevelUpText;

        /// <summary>Combo UI text display element.</summary>
        [Tooltip("Combo UI text display element")]
        public Text ComboDisplayText;

        /// <summary>Duration of the level up message fade in/out.</summary>
        [Tooltip("Duration of the level up message fade in/out")]
        public float FadeDuration = 3;

        /// <summary>Mana level slider uGUI element.</summary>
        [Tooltip("Mana level slider uGUI element")]
        public Slider ManaSlider;

        /// <summary>Friendly layers for spell targeting by player enemies.</summary>
        [Header("Targeting Layers")]
        [Tooltip("Friendly layers for spell targeting by player enemies")]
        public LayerMask FriendLayers = (1 << 8) | (1 << 10);

        /// <summary>Friendly tags for spell targeting by player enemies.</summary>
        [Tooltip("Friendly tags for spell targeting by player enemies")]
        public string[] FriendTags = new string[] { "Player", "CompanionAI" };

        /// <summary>UnFriendly layers for spell targeting by the player and allies.</summary>
        [Tooltip("UnFriendly layers for spell targeting by the player and allies")]
        public LayerMask EnemyLayers = (1 << 9) | (1 << 17);

        /// <summary>UnFriendly tags for spell targeting by player and allies.</summary>
        [Tooltip("UnFriendly tags for spell targeting by player and allies")]
        public string[] EnemyTags = new string[] { "Enemy", "Boss" };

        /// <summary>Inanimate collision layers that cause spell destruction, floors, walls etc.</summary>
        [Tooltip("Inanimate collision layers that cause spell destruction, floors, walls etc")]
        public LayerMask CollisionLayers = (1 << 0) | (1 << 12);

        /// <summary>Inanimate layers automatically affected by physics.</summary>
        [Tooltip("Inanimate layers automatically affected by physics")]
        public LayerMask PhysicsLayers = (1 << 0);

        /// <summary>Add mana return definition for linking to by the leveling system.</summary>
        public SetIntValue AddMana;

        /// <summary>Max out the mana level return definition for linking to by the leveling system.</summary>
        public CallFunction SetManaMAX;

        // internal component pointers
        protected Animator animator;
#if !VANILLA
        protected vThirdPersonController vThirdPerson;
#endif
        protected int iLastLevel;

        /// <summary>
        /// Initialise the spell system global settings
        /// </summary>
        /// <remarks>
        /// Includes inputs, class links targeting layers and more
        /// </remarks>
        protected virtual void Start()
        {
            iLastLevel = 1;
            animator = GetComponent<Animator>();
#if !VANILLA
            vThirdPerson = GetComponent<vThirdPersonController>();
#endif
            GlobalFuncs.DEBUGGING_MESSAGES = DebuggingMessages;
            GlobalFuncs.MAGICAL_POOL = PooledMagic;
            GlobalFuncs.EnemyDestroyDelay = EnemyDestroyDelay;
            GlobalFuncs.OnDeathDropRadius = OnDeathDropRadius;

            // init targeting
            //Test = CollisionLayers.value | EnemyLayers.value;
            GlobalFuncs.targetingLayerMaskAll = FriendLayers.value | EnemyLayers.value;
            GlobalFuncs.targetingLayerMaskFriend = FriendLayers;
            GlobalFuncs.targetingLayerMaskEnemy = EnemyLayers;
            GlobalFuncs.targetingTagsAll = FriendTags.Union(EnemyTags).ToArray();
            GlobalFuncs.targetingTagsFriend = FriendTags;
            GlobalFuncs.targetingTagsEnemy = EnemyTags;
            GlobalFuncs.targetingLayerMaskCollision = CollisionLayers;
            GlobalFuncs.targetingLayerMaskPhysics = PhysicsLayers;
            //Test = GlobalFuncs.targettingLayerMaskCollision.value | GlobalFuncs.targettingLayerMaskEnemy.value;

            // add a listener to the leveling component if available
            CharacterBase levelingSystem = GetComponent<CharacterBase>();
            if (levelingSystem)
            {
                levelingSystem.NotifyUpdateHUD += new CharacterBase.UpdateHUDHandler(UpdateHUDListener);
                GlobalFuncs.GiveXPToPlayer = new SetIntValue(levelingSystem.AddXP);
                AddMana = new SetIntValue(levelingSystem.AddMana);
                SetManaMAX = new CallFunction(levelingSystem.SetManaMAX);
                levelingSystem.ForceUpdateHUD();
            }
        }

        /// <summary>
        /// Listen for an update to the leveling system stats, forcing HUB update.
        /// </summary>
        /// <param name="cb">Link to an instance of the leveling system abstract class.</param>
        /// <param name="e">Character stats update contained within class properties.</param>
        protected virtual void UpdateHUDListener(CharacterBase cb, CharacterUpdated e)
        {
            XPText.text = e.XP.ToString();
            ManaSlider.maxValue = e.ManaMAX;
            ManaSlider.value = e.Mana;


#if !VANILLA
            if (vThirdPerson)
            {
                vThirdPerson.maxHealth = e.LifeMAX;
                vThirdPerson.currentHealth = e.Life;
                vThirdPerson.maxStamina = e.StaminaMAX;
            }
#endif
            // handle level up
            if (e.Level > iLastLevel)
            {
                iLastLevel = e.Level;
                LevelText.text = e.Level.ToString();
                LevelUpText.CrossFadeAlpha(1f, 0.01f, false);
                LevelUpText.text = "Congratulations, you reached level " + e.Level.ToString();
                LevelUpText.CrossFadeAlpha(0f, FadeDuration, false);
            }
        }  

        /// <summary>
        /// Handle user input into the spell triggers
        /// </summary>
        protected virtual void Update()
        {
            if (animator)
            {  // has animator
#if !VANILLA
                if (!vThirdPersonController.instance.isDead && animator.enabled)
                {  // not dead
#else
                if (animator.enabled) {  // add not dead condition here
#endif
                    // check all inputs
                    foreach (MagicSpellTrigger st in SpellsTriggers)
                    {  // check all triggers
                        if (st.MagicId > 0)
                        {  // has a spell assigned
#if !VANILLA
                            if (st.Input.useInput)
                            {  // input enabled?
#else
                            if (st.InputEnabled) {  // input enabled?
#endif 
                                if (st.ManaCost < ManaSlider.value)
                                {   // mana available?
#if !VANILLA
                                    if (st.Input.GetButtonDown())
                                    {  // input pressed down
#else
                                    if (Input.GetKeyDown(st.InputButton)) {  // input pressed down
#endif
                                        animator.SetInteger("MagicID", st.MagicId);  // set the animator magic ID to select the spell
                                        animator.SetTrigger(st.ChargeTrigger);   // trigger the Magic State                                     
                                    }
#if !VANILLA
                                    if (st.Input.GetButtonUp())
                                    {  // input released    
#else
                                    if (Input.GetKeyUp (st.InputButton)) {  // input released
#endif
                                        animator.SetInteger("MagicID", st.MagicId);  // set the animator magic ID to select the spell
                                        animator.SetTrigger(st.AnimatorTrigger);   // trigger the Magic State                                         
                                        onUseMana.Invoke(st.ManaCost);  // apply the mana cost to the levelling system
#if !VANILLA
                                    }
                                    else if (st.EquipSlots.Length > 1)
                                    {  // multi slots per display
                                        if (st.PreviousSlot.useInput)
                                        {  // previous slot key enabled?
                                            if (st.PreviousSlot.GetButtonUp())
                                            {  // previous slot button pressed
                                                do
                                                {
                                                    if (st.CurrentSlotId == 0)
                                                    {  // on the 1st slot
                                                        st.CurrentSlotId = st.EquipSlots.Length - 1;  // roll to the end of the list
                                                    }
                                                    else
                                                    {
                                                        st.CurrentSlotId -= 1;  // back one item
                                                    }
                                                } while (!st.EquipSlots[st.CurrentSlotId].item); // loop backwards until found the next slot with an item
                                                st.MagicId = st.EquipSlots[st.CurrentSlotId].item.attributes.Find(ia => ia.name.ToString() == "MagicID").value;  // update the magic id
                                                st.EquipDisplay.AddItem(st.EquipSlots[st.CurrentSlotId].item);  // update the slot with the spell icon
                                            }
                                        }
                                        if (st.NextSlot.useInput)
                                        {  // next slot key enabled?
                                            if (st.NextSlot.GetButtonUp())
                                            {  // next slot button pressed
                                                do
                                                {
                                                    if (st.CurrentSlotId == st.EquipSlots.Length - 1)
                                                    {  // on the last slot
                                                        st.CurrentSlotId = 0;  // roll to the end of the list
                                                    }
                                                    else
                                                    {
                                                        st.CurrentSlotId += 1;  // forward one item
                                                    }
                                                } while (!st.EquipSlots[st.CurrentSlotId].item); // loop forwards until found the next slot with an item
                                                st.MagicId = st.EquipSlots[st.CurrentSlotId].item.attributes.Find(ia => ia.name.ToString() == "MagicID").value;  // update the magic id
                                                st.EquipDisplay.AddItem(st.EquipSlots[st.CurrentSlotId].item);  // update the slot with the spell icon
                                            }
                                        }
                                    }
#else
                                    }
#endif
                                }
                            }
                        }
                    }
                }
            }
        }

#if !VANILLA
        /// <summary>
        /// Occurs when a spell is equipped by the invector inventory.
        /// </summary>
        /// <remarks>
        /// Updates the spell display slot in the HUD, updates the spell trigger and caches the mana cost of the spell.
        /// </remarks>
        /// <param name="viSpell">Spell that has been equipped as an inventory item.</param>
        public virtual void SpellEquiped(vItem viSpell)
        {
            var vAttribMagicID = viSpell.attributes.Find(ai => ai.name.ToString() == "MagicID");  // grab the magic id
            if (vAttribMagicID != null)
            {  // fail safe
                foreach (MagicSpellTrigger st in SpellsTriggers)
                {  // check all triggers
                    for (int i = 0; i < st.EquipSlots.Length; i++)
                    {  // check all equp slots for this trigger
                        if (st.EquipSlots[i])
                        {  // invector inventory 
                            if (st.EquipSlots[i].item)
                            {  // has an item                       
                                if (st.EquipSlots[i].item.attributes.Find(ai => ai.name.ToString() == "MagicID").value == vAttribMagicID.value)
                                {   // found 
                                    if (st.EquipDisplay)
                                    {  // do we have a display for this input
                                        st.MagicId = vAttribMagicID.value;  // grab the magic id 
                                        var vAttribManaCost = viSpell.attributes.Find(ai => ai.name.ToString() == "ManaCost");  // grab the mana cost
                                        if (vAttribManaCost != null)
                                        {  // fail safe
                                            st.ManaCost = viSpell.attributes.Find(ai => ai.name.ToString() == "ManaCost").value;  // grab the mana cost
                                        }
                                        else
                                        {
                                            st.ManaCost = 50;  // missing manacost, apply default
                                            if (DebuggingMessages)
                                            {
                                                Debug.Log(viSpell.name + " is missing attribute ManaCost, applying default");
                                            }
                                        }
                                        st.EquipDisplay.AddItem(viSpell);  // update the slot with the spell icon
                                        st.CurrentSlotId = i; // assign the spell just set as active
                                    }
                                    break;   // work complete
                                }
                            }
                        }
                    }
                }

                if (DebuggingMessages)
                {
                    Debug.Log("Equipped " + viSpell.name);
                }
            }
            else
            {
                if (DebuggingMessages)
                {
                    Debug.Log(viSpell.name + " is missing required attribute MagicID, unable to equip the spell");
                }
            }
        }

        /// <summary>
        /// Occurs when a spell is unequipped by the invector inventory.
        /// </summary>
        /// <remarks>
        /// Updates the spell display slot in the HUD, clears the spell trigger.
        /// </remarks>
        /// <param name="viSpell">Spell that has been unequipped as an inventory item.</param>
        public virtual void SpellUnEquiped(vItem viSpell)
        {
            var vAttribMagicID = viSpell.attributes.Find(ai => ai.name.ToString() == "MagicID");  // grab the magic id
            if (vAttribMagicID != null)
            {  // fail safe
                foreach (MagicSpellTrigger st in SpellsTriggers)
                {  // check all triggers
                    if (st.MagicId == vAttribMagicID.value)
                    {  // found?
                        st.EquipDisplay.RemoveItem();  // reset the slot to empty
                        st.MagicId = 0;  // no spell
                        break;
                    }
                }

                if (DebuggingMessages)
                {
                    Debug.Log("UnEquiped " + viSpell.name);
                }
            }
            else
            {
                if (DebuggingMessages)
                {
                    Debug.Log(viSpell.name + " is missing required attribute MagicID, unable to unequip the spell");
                }
            }
        }  

        /// <summary>
        /// Occurs when the invector inventory signals that a potion has been drunk.
        /// </summary>
        /// <remarks>
        /// Updates the leveling system with the new mana level from the potion drunk.
        /// </remarks>
        /// <param name="viDrinkMe"></param>
        public virtual void UsePotion(vItem viDrinkMe)
        {
            if (viDrinkMe.type == vItemType.Consumable)
            {  // ensure is a potion
                foreach (vItemAttribute viaAttrib in viDrinkMe.attributes)
                {  // check for valid attributes
                    switch (viaAttrib.name.ToString())
                    {  // naming is important
                        case "Mana":
                            if (AddMana != null) AddMana(viaAttrib.value);
                            break;
                        case "MaxMana":
                            if (SetManaMAX != null) SetManaMAX();
                            break;
                    }
                }
            }
        }

#endif
        /// <summary>
        /// Updates the HUD with the combo just performed.
        /// </summary>
        /// <param name="ComboDepth">Depth into the branch of the combo.</param>
        /// <param name="ComboName">Name of the combo.</param>
        public virtual void UpdateComboDisplay(int ComboDepth, string ComboName)
        {
            if (ComboDisplayText)
            {
                ComboDisplayText.CrossFadeAlpha(1f, 0.01f, false);
                ComboDisplayText.text = "Level " + ComboDepth.ToString() + " Combo " + ComboName;
                ComboDisplayText.CrossFadeAlpha(0f, 3, false);
            }
            else
            {
                if (GlobalFuncs.DEBUGGING_MESSAGES)
                    Debug.Log("Level " + ComboDepth.ToString() + " Combo " + ComboName);
            }            
        }

#if UNITY_EDITOR
        /// <summary>
        /// Force apply the class defaults when the first array item is added in the inspector.
        /// </summary>
        protected virtual void OnValidate()
        {
            if (!Application.isPlaying)
            {
                foreach (MagicSpellTrigger s in SpellsTriggers)
                {
                    s.New();
                }
            }
        }  
#endif
    }


    /// <summary>
    /// Spell trigger element for the trigger list array.
    /// </summary>
    [Serializable]
    public class MagicSpellTrigger
    {
        /// <summary>MagicID passed to the animator, set to 0 if using the inventory for no spell.</summary>
        [Tooltip("MagicID passed to the animator, set to 0 if using the inventory for no spell")]
        public int MagicId;

        /// <summary>Mana cost of the equipped spell, leave as 0 if using the inventory as set via attribute.</summary>
        [Tooltip("Mana cost of the equipped spell, leave as 0 if using the inventory as set via attribute")]
        public int ManaCost;

        /// <summary>Name of the animator trigger called to optionally charge the spell.</summary>
        [Tooltip("Name of the animator trigger called to optionally charge the spell")]
        public string ChargeTrigger;

        /// <summary>Name of the animator trigger called to trigger the spell.</summary>
        [Tooltip("Name of the animator trigger called to trigger the spell")]
        public string AnimatorTrigger;

#if !VANILLA
        /// <summary>Input key or button that triggers a spell.</summary>
        [Tooltip("Input key or button that triggers a spell")]
        public GenericInput Input;

        /// <summary>If using the invector inventory, drag the equip display to be used by this input.</summary>
        [Tooltip("If using the invector inventory, drag the equip display to be used by this input")]
        public vEquipmentDisplay EquipDisplay;

        /// <summary>If using the invector inventory, drag the equip slot to be triggered by this input.</summary>
        [Tooltip("If using the invector inventory, drag the equip slot to be triggered by this input")]
        public vEquipSlot[] EquipSlots;

        /// <summary>Input key or button that selects the previous equip slot item in the list (only valid when EquipSlots > 1).</summary>
        [Tooltip("Input key or button that selects the previous equip slot item in the list (only valid when EquipSlots > 1)")]
        public GenericInput PreviousSlot;

        /// <summary>Input key or button that selects the next equip slot item in the list (only valid when EquipSlots > 1).</summary>
        [Tooltip("Input key or button that selects the next equip slot item in the list (only valid when EquipSlots > 1)")]
        public GenericInput NextSlot;

        /// <summary>Default slot to display, leave as 0 (only valid when EquipSlots > 1).</summary>
        [Tooltip("Default slot to display, leave as 0 (only valid when EquipSlots > 1)")]
        public int CurrentSlotId;
#else
        /// <summary>Input enabled.</summary>
        [Tooltip("Input enabled")]
        public bool InputEnabled;

        /// <summary>Input key or button that triggers a spell.</summary>
        [Tooltip("Input key or button that triggers a spell")]
        public string InputButton;
#endif

#if UNITY_EDITOR        
        [HideInInspector] public bool bInitialised;

        /// <summary>
        /// Apply defaults
        /// </summary>
        public virtual void New()
        {
            if (!bInitialised)
            {
                bInitialised = true;
                ChargeTrigger = "MagicCharge";
                AnimatorTrigger = "MagicAttack";
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
