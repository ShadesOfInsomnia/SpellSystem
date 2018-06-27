using UnityEngine;
using UnityEditor.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Invector;
using Invector.vCharacterController;
using Invector.vItemManager;
using System;
using System.IO;
using Invector.vMelee;

namespace Shadex
{
    /// <summary>
    /// Custom editor for the magic settings, adds check player to UI links feature.
    /// </summary>
    //[CanEditMultipleObjects]
    [CustomEditor(typeof(MagicSettings), true)]
    public class MagicSettingsEditor : Editor
    {
        GUISkin skin;
        GUISkin defaultSkin;
        string lastUICheckResults;

        void OnEnable()
        {
            skin = Resources.Load("skin") as GUISkin;
            lastUICheckResults = "";
        }

        /// <summary>
        /// Add a check UI links validator to the default inspector window.
        /// </summary>
        public override void OnInspectorGUI()
        {
            defaultSkin = GUI.skin;
            if (skin) GUI.skin = skin;
            GUILayout.BeginVertical("MAGIC SETTINGS", "window");
            GUILayout.Space(30);

            // validate the interface to player links
            if (GUILayout.Button("Check UI->Player Links", GUILayout.ExpandWidth(true)))
            {
                // setup
                GameObject player = Selection.activeGameObject;
                MagicSettings settings = player.GetComponent<MagicSettings>();
                CharacterBase levelingSystem = player.GetComponent<CharacterBase>();
                lastUICheckResults = "";
                int changeCount = 0;

                // check magic spawn point
                if (!settings.MagicSpawnPoint)
                {
                    Transform tMagicSpawn = player.transform.Find("Magic Spawn");
                    if (!tMagicSpawn)
                    {
                        GameObject goMagicSpawn = new GameObject("Magic Spawn");
                        goMagicSpawn.transform.SetParent(player.transform);
                        goMagicSpawn.transform.position = new Vector3(0f, 1.5f, 0.9f);
                        settings.MagicSpawnPoint = goMagicSpawn.transform;
                    }
                    else
                    {
                        settings.MagicSpawnPoint = tMagicSpawn;
                    }
                    
                    changeCount += 1;
                    lastUICheckResults += "Added Magic Spawn Point\r\n";                
                }

                // leveling system on use mana connection
                if (levelingSystem)
                {
                    settings.onUseMana = new UnityIntEvent();
                    UnityEventTools.AddPersistentListener(settings.onUseMana, levelingSystem.UseMana);
                    changeCount += 1;
                    lastUICheckResults += "Re-Linked leveling system to onUseMana\r\n";
                }
                else
                {
                    lastUICheckResults += "Optional Leveling system is missing, onUseMana not handled\r\n";
                }

                // check links between inventory ui and the player
                vItemManager itemManager = player.GetComponent<vItemManager>();
                if (!itemManager)
                {
                    lastUICheckResults += "vItemManager is MISSING\r\n";
                }
                else  // found, checking slot links
                {
                    GameObject goInventoryWindow = itemManager.inventoryPrefab.transform.Find("InventoryWindow").gameObject;  // grab inventory window
                    GameObject goEquipmentInventory = goInventoryWindow.transform.Find("EquipmentInventory").gameObject;  // and the equip slot parent
                    goEquipmentInventory.SetActive(true);  // enable for component search
                    int iNext = 1;
                    vEquipSlot[] allSlots = itemManager.inventoryPrefab.transform.GetComponentsInChildren<vEquipSlot>();
                    settings.SpellsTriggers.Clear();
                    foreach (vEquipSlot slot in allSlots)
                    {
                        if (slot.transform.parent.parent.name == "EquipMentArea_Spells")  // is a spell inventory area
                        {
                            slot.onAddItem = new OnHandleItemEvent();
                            slot.onRemoveItem = new OnHandleItemEvent();
                            MagicSpellTrigger trigger = new MagicSpellTrigger();  // create the trigger
                            trigger.EquipSlots = new vEquipSlot[] { slot };  // set the inventory slot
                            trigger.Input = new GenericInput("F" + iNext.ToString(), null, null);  // set the input key
                            trigger.Input.useInput = true;  // enable
                            vEquipmentDisplay[] allDisplays = itemManager.inventoryPrefab.transform.GetComponentsInChildren<vEquipmentDisplay>();  // find all displays
                            foreach (vEquipmentDisplay disp in allDisplays)  // check all
                            {  
                                if (disp.gameObject.name == slot.gameObject.name.Replace("EquipSlot ", "EquipDisplay_Spell "))  // found matching name?
                                {                                    
                                    trigger.EquipDisplay = disp;  // success, apply
                                    UnityEventTools.AddPersistentListener(slot.onAddItem, settings.SpellEquiped);  // listen for spells equiped
                                    UnityEventTools.AddPersistentListener(slot.onRemoveItem, settings.SpellUnEquiped);  // and unequiped
                                    break;  // drop out
                                }
                            }
                            settings.SpellsTriggers.Add(trigger);  // add the trigger
                            iNext += 1;  // next please
                        }
                    }
                    goEquipmentInventory.SetActive(false);  // deactivate the inventory display

                    changeCount += 1;
                    lastUICheckResults += "Re-Linked inventory/UI display slots to the player\r\n";

                    // check use potion links
                    itemManager.onUseItem = new OnHandleItemEvent();
                    UnityEventTools.AddPersistentListener(itemManager.onUseItem, settings.UsePotion);

                    changeCount += 1;
                    lastUICheckResults += "Re-Linked item manager use potion to the player\r\n";
                }

                // finish up
                lastUICheckResults += "All done " + changeCount.ToString() + " changes applied\r\n\r\n";

            }
            GUILayout.Label(lastUICheckResults, GUILayout.ExpandWidth(true));

            // output the base inspector window
            GUI.skin = defaultSkin;
            base.OnInspectorGUI();
            GUI.skin = skin;
            GUILayout.EndVertical();
            GUI.skin = defaultSkin;
        }
    }
}
