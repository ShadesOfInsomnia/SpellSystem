using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System;
using UnityEngine.UI;
#if !VANILLA
using Invector;
using Invector.vItemManager;
#endif

namespace Shadex
{
    /// <summary>
    /// Abstract base class for the data layer.
    /// </summary>
    /// <remarks>
    /// Allows multiple data connectors to all have the same signature and be treated as the 
    /// same class despite having different implementations of the abstract methods.
    /// </remarks>
#if !VANILLA
    [vClassHeader("CHARACTER DATABASE", iconName = "ladderIcon")]
    public abstract class CharacterDataBase : vMonoBehaviour
    {
#else
    public abstract class CharacterDataBase : MonoBehaviour {
#endif
        /// <summary>Name of the database.</summary>
        public const string DATABASE_NAME = "ShadesSpellSystem";

        /// <summary>Relative path to the database.</summary>
        public const string LOCAL_DATABASE_PATH = "/Invector-3rdPersonController/Add-ons/ShadesSpellSystem";

#if !VANILLA
        /// <summary>Link to the in scene inventory game object.</summary>
        [Tooltip("Link to the in scene inventory game object")]
        public vInventory Inventory;

        /// <summary>Link to the invector item list asset for the full list of available items.</summary>
        [Tooltip("Link to the invector item list asset for the full list of available items")]
        public vItemListData ItemListData;

        // internal
        private vItemManager ItemManager;
#endif
        private MagicSettings Settings;
        //private vMeleeManager MeleeManager;
               
        /// <summary>
        /// Initialise component references
        /// </summary>
        public virtual void Start()
        {
            Settings = GetComponent<MagicSettings>();
#if !VANILLA
            //MeleeManager = GetComponent<vMeleeManager>();
            ItemManager = GetComponent<vItemManager>();
#endif
        }

        /// <summary>
        /// Check for database upgrades. 
        /// </summary>
        /// <param name="Clear">Empty the database of data.</param>
        public abstract void ValidateDatabase(bool Clear);

        /// <summary>
        /// Cleanup an close the data source.
        /// </summary>        
        protected abstract void CleanUp();

        /// <summary>
        /// Clear down the player/HUD before loading a save game.
        /// </summary>
        public void ClearInventoryAndHUD()
        {
#if !VANILLA  // clear inventory
            if (Inventory)
            {
                Inventory.items.Clear();

                // then clear equip display slots
                for (int i = 0; i < Inventory.changeEquipmentControllers.Count; i++)
                {
                    if (Inventory.changeEquipmentControllers[i].display.item)
                    {
                        Inventory.UnequipItem(Inventory.changeEquipmentControllers[i].equipArea, Inventory.changeEquipmentControllers[i].display.item);
                    }
                }

                // also clear equiped melee weapons & consumables
                for (int i = 0; i < Inventory.equipAreas.Length; i++)
                {
                    var validSlot = Inventory.equipAreas[i].ValidSlots;
                    for (int a = 0; a < validSlot.Count; a++)
                    {
                        try
                        {
                            validSlot[a].RemoveItem();
                        }
                        catch { }    // ensure nulls dont crash it
                    }
                }
            }
#endif
            // clear spell triggers
            if (Settings)
            {
                for (int s = 0; s < Settings.SpellsTriggers.Count; s++)
                {  // check all triggers
                    MagicSpellTrigger st = Settings.SpellsTriggers[s];
#if !VANILLA
                    if (st.Input.useInput)
                    {  // enabled
                        for (int i = 0; i < st.EquipSlots.Length; i++)
                        {  // check all equip slots
                            try
                            {
                                st.EquipSlots[i].RemoveItem();
                            }
                            catch { }    // ensure nulls dont crash it
                        }
                    }
#endif
                }
            }
        }

        /// <summary>
        /// Build the header data attributes that need saving/loading.
        /// </summary>
        /// <param name="LevelingSystem">Reference to the leveling system to build the data block from.</param>
        /// <param name="IncludeSlotData">Include slot data as well as save game data.</param>
        /// <returns>Header data attribute block.</returns>
        private List<SimpleDataHeader> BuildHeaderDataBlock(ref CharacterBase LevelingSystem, bool IncludeSlotData)
        {
            List<SimpleDataHeader> HeaderData = new List<SimpleDataHeader>();
            if (IncludeSlotData)
            {  // create save a new slot
                HeaderData.Add(new SimpleDataHeader() { Name = "CharacterName", Value = LevelingSystem.Name, Type = SimpleDataType.String, Level = SimpleDataLevel.Slot });
                HeaderData.Add(new SimpleDataHeader() { Name = "Axis", Value = ((int)LevelingSystem.CurrentAxis).ToString(), Type = SimpleDataType.Number, Level = SimpleDataLevel.Slot });
                HeaderData.Add(new SimpleDataHeader() { Name = "Alignment", Value = ((int)LevelingSystem.CurrentAlignment).ToString(), Type = SimpleDataType.Number, Level = SimpleDataLevel.Slot });
                HeaderData.Add(new SimpleDataHeader() { Name = "Race", Value = ((int)LevelingSystem.CurrentRace).ToString(), Type = SimpleDataType.Number, Level = SimpleDataLevel.Slot });
                HeaderData.Add(new SimpleDataHeader() { Name = "Class", Value = ((int)LevelingSystem.CurrentClass).ToString(), Type = SimpleDataType.Number, Level = SimpleDataLevel.Slot });
            }
            HeaderData.Add(new SimpleDataHeader() { Name = "Level", Value = LevelingSystem.CurrentLevel.ToString(), Type = SimpleDataType.Number, Level = SimpleDataLevel.SaveGame });
            HeaderData.Add(new SimpleDataHeader() { Name = "XP", Value = LevelingSystem.CurrentXP.ToString(), Type = SimpleDataType.Number, Level = SimpleDataLevel.SaveGame });
            HeaderData.Add(new SimpleDataHeader() { Name = "UnspentSkillPoints", Value = LevelingSystem.UnspentSkillPoints.ToString(), Type = SimpleDataType.Number, Level = SimpleDataLevel.SaveGame });
            return HeaderData;
        }  

        /// <summary>
        /// Load a save game from the abstract data layer.
        /// </summary>
        /// <param name="LevelingSystem">Leveling system to load into.</param>
        /// <param name="SaveGameID">ID of the save game to load.</param>
        /// <returns>Name of scene to load.</returns>
        public string LoadPlayerState(ref CharacterBase LevelingSystem, int SaveGameID)
        {
            try
            {
                // load the header values
                string SceneName = "EDITOR";
                List<SimpleDataHeader> HeaderData = BuildHeaderDataBlock(ref LevelingSystem, true);
                LoadHeader(SaveGameID, ref SceneName, ref HeaderData);

                // process the data into the leveling system
                foreach (SimpleDataHeader sdh in HeaderData)
                {
                    switch (sdh.Name)
                    {
                        case "CharacterName":
                            LevelingSystem.Name = sdh.Value;
                            break;
                        case "Axis":
                            LevelingSystem.CurrentAxis = (BaseAxis)Convert.ToInt32(sdh.Value);
                            break;
                        case "Alignment":
                            LevelingSystem.CurrentAlignment = (BaseAlignment)Convert.ToInt32(sdh.Value);
                            break;
                        case "Race":
                            LevelingSystem.CurrentRace = (BaseRace)Convert.ToInt32(sdh.Value);
                            break;
                        case "Class":
                            LevelingSystem.CurrentClass = (BaseClass)Convert.ToInt32(sdh.Value);
                            break;
                        case "Level":
                            LevelingSystem.CurrentLevel = Convert.ToInt32(sdh.Value);
                            break;
                        case "XP":
                            LevelingSystem.CurrentXP = Convert.ToInt32(sdh.Value);
                            break;
                        case "UnspentSkillPoints":
                            LevelingSystem.UnspentSkillPoints = Convert.ToInt32(sdh.Value);
                            break;
                    }
                }

                // load all attributes
                for (int i = 0; i < LevelingSystem.Skills.Count; i++)
                {
                    LevelingSystem.Skills[i].Value = LoadAttribute(SaveGameID, 100 + (int)LevelingSystem.Skills[i].Skill);
                }
                for (int i = 0; i < LevelingSystem.Resist.Count; i++)
                {
                    LevelingSystem.Resist[i].Value = LoadAttribute(SaveGameID, 200 + (int)LevelingSystem.Resist[i].Resist);
                }
                for (int i = 0; i < LevelingSystem.Collectables.Count; i++)
                {
                    LevelingSystem.Collectables[i].Value = LoadAttribute(SaveGameID, 300 + (int)LevelingSystem.Collectables[i].Type);
                }

#if !VANILLA
                // then load equip display slots
                for (int i = 0; i < Inventory.changeEquipmentControllers.Count; i++)
                {
                    int EquipedItemID = (int)LoadAttribute(SaveGameID, 600 + i);  // load from data layer
                    if (EquipedItemID > 0)
                    {
                        var reference = new ItemReference(EquipedItemID - 1);
                        reference.amount = 1;
                        reference.autoEquip = true;
                        reference.indexArea = i;
                        ItemManager.AddItem(reference, true);
                    }
                }

                // load inventory
                if (Inventory && ItemListData)
                {
                    Inventory.items.Clear();
                    foreach (vItem vi in ItemListData.items)
                    {
                        vi.amount = (int)LoadAttribute(SaveGameID, 1000 + vi.id);
                        if (vi.amount > 0)
                        {
                            Inventory.items.Add(vi);
                        }
                    }

                    // also load equipped melee weapons & consumables
                    for (int i = 0; i < Inventory.equipAreas.Length; i++)
                    {
                        var equipArea = Inventory.equipAreas[i];
                        var validSlot = equipArea.ValidSlots;
                        for (int a = 0; a < validSlot.Count; a++)
                        {
                            int EquipedItemID = (int)LoadAttribute(SaveGameID, 500 + (i * 10) + a);  // load from data layer
                            if (EquipedItemID > 0)
                            {  // found equipped item id
                                vItem vi = ItemListData.items.Find(e => e.id == (EquipedItemID - 1));  // search
                                if (vi)
                                {   // found the id in the item list data
                                    validSlot[a].AddItem(vi);  // add to slot
                                }
                            }
                        }
                    }
                    
                }
#endif

                // spell triggers
                if (Settings)
                {
                    for (int s = 0; s < Settings.SpellsTriggers.Count; s++)
                    {  // check all triggers
                        MagicSpellTrigger st = Settings.SpellsTriggers[s];
#if !VANILLA
                        if (st.Input.useInput)
                        {  // enabled
                            for (int i = 0; i < st.EquipSlots.Length; i++)
                            {  // check all equip slots for this trigger
                                int ID = (int)LoadAttribute(SaveGameID, 400 + (s * 10) + i);  // attempt load
                                if (ID > 0)
                                {  // found in data layer?
                                    vItem viSpell = ItemListData.items.Find(v => v.id == (ID - 1));  // attempt find vItem in collection, ID is -1 as LoadReterns 0 for not found
                                    if (viSpell)
                                    {  // spell item id found?
                                        st.EquipSlots[i].AddItem(viSpell);  // update inventory slot
                                        st.MagicId = viSpell.attributes.Find(ia => ia.name.ToString() == "MagicID").value;  // grab the magic id 
                                        st.ManaCost = viSpell.attributes.Find(ia => ia.name.ToString() == "ManaCost").value;  // grab the mana cost
                                        st.EquipDisplay.AddItem(viSpell);  // update the slot with the spell icon
                                    }
                                }
                            }
                        }
#endif
                    }
                }

                // order recalc
                LevelingSystem.reCalcCore(true);

                // return scene to load
                return SceneName;
            }
            catch (Exception ex)
            {
                if (GlobalFuncs.DEBUGGING_MESSAGES)
                {
                    Debug.Log("LoadPlayerState(" + LevelingSystem.Name + "," + SaveGameID.ToString() + ") " + ex.Message);
                }
                return "";
            }
            finally
            {  // ensure clean up happens, even on error
                CleanUp();
            }
        }

        /// <summary>
        /// Load the header data array
        /// </summary>
        /// <param name="SaveGameID">ID of the save game being loaded.</param>
        /// <param name="SceneName">Return the name of the scene to load.</param>
        /// <param name="Data">Return the character data in list form.</param>
        protected abstract void LoadHeader(int SaveGameID, ref string SceneName, ref List<SimpleDataHeader> Data);

        /// <summary>
        /// Load attribute value.
        /// </summary>
        /// <param name="SaveGameID">Save game ID containing the attribute.</param>
        /// <param name="ID">Attribute ID</param>
        /// <returns>Value of specified attribute.</returns>
        protected abstract float LoadAttribute(int SaveGameID, int ID);

        /// <summary>
        /// Save the player state and level to the abstract data layer.
        /// </summary>
        /// <param name="LevelingSystem">Leveling system to save.</param>
        /// <param name="SceneName">Name of the current scene.</param>
        /// <param name="SaveGameID">ID of the save game or zero for new.</param>
        /// <returns>SaveGame ID updated.</returns>
        public int SavePlayerState(ref CharacterBase LevelingSystem, string SceneName, int SaveGameID)
        {
            try
            {
                // create or update the save header                
                SaveHeader(ref LevelingSystem.SaveSlotID, ref SaveGameID, SceneName,
                    BuildHeaderDataBlock(ref LevelingSystem, (LevelingSystem.SaveSlotID == 0)));

                // save all attributes
                for (int i = 0; i < LevelingSystem.Skills.Count; i++)
                {
                    SaveAttribute(SaveGameID, 100 + (int)LevelingSystem.Skills[i].Skill, LevelingSystem.Skills[i].Value);
                }
                for (int i = 0; i < LevelingSystem.Resist.Count; i++)
                {
                    SaveAttribute(SaveGameID, 200 + (int)LevelingSystem.Resist[i].Resist, LevelingSystem.Resist[i].Value);
                }
                for (int i = 0; i < LevelingSystem.Collectables.Count; i++)
                {
                    SaveAttribute(SaveGameID, 300 + (int)LevelingSystem.Collectables[i].Type, LevelingSystem.Collectables[i].Value);
                }


#if !VANILLA    // save inventory
                if (Inventory)
                {
                    foreach (vItem vi in Inventory.items)
                    {
                        SaveAttribute(SaveGameID, 1000 + vi.id, vi.amount);
                    }

                    // also save equipped melee weapons & consumables
                    for (int i = 0; i < Inventory.equipAreas.Length; i++)
                    {
                        var equipArea = Inventory.equipAreas[i];
                        var validSlot = equipArea.ValidSlots;
                        for (int a = 0; a < validSlot.Count; a++)
                        {
                            if (validSlot[a].item != null && validSlot[a].item.type != vItemType.Spell)
                            {
                                SaveAttribute(SaveGameID, 500 + (i * 10) + a, validSlot[a].item.id + 1);  // save equiped vItem id as +1 
                            }
                        }
                    }

                    // then save equip display slots
                    for (int i = 0; i < Inventory.changeEquipmentControllers.Count; i++)
                    {
                        if (Inventory.changeEquipmentControllers[i].display.item)
                        {
                            SaveAttribute(SaveGameID, 600 + i, Inventory.changeEquipmentControllers[i].display.item.id + 1);  // save active equiped vItem id as +1 
                        }
                    }
                }

#endif
                // spell triggers
                if (Settings)
                {
                    for (int s = 0; s < Settings.SpellsTriggers.Count; s++)
                    {  // check all triggers
                        MagicSpellTrigger st = Settings.SpellsTriggers[s];
#if !VANILLA
                        if (st.Input.useInput)
                        {  // enabled
                            for (int i = 0; i < st.EquipSlots.Length; i++)
                            {  // check all equp slots for this trigger
                                if (st.EquipSlots[i])
                                {  // invector inventory 
                                    if (st.EquipSlots[i].item)
                                    {  // has an item
                                        SaveAttribute(SaveGameID, 400 + (s * 10) + i, st.EquipSlots[i].item.id + 1);  // save equiped vItem id as +1 
                                    }
                                }
                            }
                        }
#endif
                    }
                }

                // success
                if (GlobalFuncs.DEBUGGING_MESSAGES)
                {
                    Debug.Log("Save Completed..");
                }
                return SaveGameID;
            }
            catch (Exception ex)
            {
                if (GlobalFuncs.DEBUGGING_MESSAGES)
                {
                    Debug.Log("SavePlayerState(" + LevelingSystem.Name + "," + SceneName + "," + SaveGameID.ToString() + ") " + ex.Message);
                }
                return -1;
            }
            finally
            {  // ensure clean up happens, even on error
                CleanUp();
            }
        } 

        /// <summary>
        /// Save the header, creating slot if needed.
        /// </summary>
        /// <param name="SlotID">Returns the slotID if one was created.</param>
        /// <param name="SaveGameID">Returns the save game ID if one was created.</param>
        /// <param name="SceneName">Name of the scene saved.</param>
        /// <param name="Data">List of header attributes to save.</param>
        protected abstract void SaveHeader(ref int SlotID, ref int SaveGameID, string SceneName, List<SimpleDataHeader> Data);

        /// <summary>
        /// Save attribute ID/Value pair.
        /// </summary>
        /// <param name="SaveGameID">ID of the save game.</param>
        /// <param name="ID">Attribute ID being saved.</param>
        /// <param name="Value">Value of the attribute</param>
        protected abstract void SaveAttribute(int SaveGameID, int ID, float Value);

        /// <summary>
        /// Displays a GUI pop up using a non contiguous ID set of pairs from an unknown data source.
        /// </summary>
        /// <param name="Pairs">Key value pairs to pop up in the inspector.</param>
        /// <param name="SelectedID">Currently selected ID.</param>
        /// <param name="options">GUI layout options to use.</param>
        /// <returns></returns>
        public int SimpleDataGUIPopup(ref List<SimpleDataPair> Pairs, int SelectedID, params GUILayoutOption[] options)
        {
            int iSelected = 0;  // index of the currently selected ID

            // create the string array to popup from (and find the index of the ID
            string[] sLabels = new string[Pairs.Count];
            for (int i = 0; i < Pairs.Count; i++)
            {
                sLabels[i] = Pairs[i].Display;
                if (Pairs[i].Value == SelectedID)
                {
                    iSelected = i;  // found the selected ID
                }
            }

            // show the popup
            int iNewSelection = EditorGUILayout.Popup(iSelected, sLabels, options);

            // return the new ID 
            if (iNewSelection != iSelected)
            {  // new ID, find in the datatable
                return Pairs[iNewSelection].Value;
            }
            else
            { // no change, return orginal ID
                return SelectedID;
            }
        }  

        /// <summary>
        /// Returns ID of latest save for continue menu option, 0 if no save games found.
        /// </summary>
        /// <param name="SlotID">Returned SlotID</param>
        /// <param name="SaveGameID">Returned SaveGameID</param>
        public abstract void FindMostRecentSaveGameID(ref int SlotID, ref int SaveGameID);

        /// <summary>
        /// Returns list of available character slots.
        /// </summary>
        /// <returns>List of SlotID/CharacterName + Date pairs.</returns>
        public abstract List<SimpleDataPair> GetCharacterSlotList();

        /// <summary>
        /// Returns list of available save games for the selected slot.
        /// </summary>
        /// <param name="SelectedSlotID">Slot ID to filter the save game list by.</param>
        /// <returns>List of SaveGameID/SceneName + XP + Date pairs</returns>
        public abstract List<SimpleDataPair> GetSaveGameListForSlot(int SelectedSlotID);
    }

    /// <summary>
    /// Simple data type list.
    /// </summary>
    public enum SimpleDataType
    {
        String, Number
    }

    /// <summary>
    /// Simple data level list.
    /// </summary>
    public enum SimpleDataLevel
    {
        Slot, SaveGame
    }

    /// <summary>
    /// Simple data key value pair instance.
    /// </summary>
    public class SimpleDataPair
    {
        public int Value;
        public string Display;
    }

    /// <summary>
    /// Simple data header attribute instance.
    /// </summary>
    public class SimpleDataHeader
    {
        public string Name;
        public string Value;
        public SimpleDataType Type;
        public SimpleDataLevel Level;
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
