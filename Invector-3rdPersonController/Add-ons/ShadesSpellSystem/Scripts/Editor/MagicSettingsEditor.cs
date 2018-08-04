using UnityEngine;
using UnityEngine.UI;
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
                vThirdPersonController controller = player.GetComponent<vThirdPersonController>();
                vMeleeManager melee = player.GetComponent<vMeleeManager>();
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

                // leveling system connections
                if (levelingSystem)
                {
                    // on use mana
                    settings.onUseMana = new UnityIntEvent();
                    UnityEventTools.AddPersistentListener(settings.onUseMana, levelingSystem.UseMana);
                    changeCount += 1;
                    lastUICheckResults += "Re-Linked leveling system to onUseMana\r\n";

                    // on receive damage
                    controller.onReceiveDamage = new OnReceiveDamage();
                    UnityEventTools.AddPersistentListener(controller.onReceiveDamage, levelingSystem.OnRecieveDamage);
                    changeCount += 1;
                    lastUICheckResults += "Re-Linked leveling system to onReceiveDamage\r\n";

                    // on send hit
                    if (melee)
                    {
                        melee.onDamageHit = new vOnHitEvent();
                        UnityEventTools.AddPersistentListener(melee.onDamageHit, levelingSystem.OnSendHit);
                        changeCount += 1;
                        lastUICheckResults += "Re-Linked leveling system to onSendHit\r\n";
                    }
                }
                else
                {
                    lastUICheckResults += "Optional Leveling system is missing\r\nonUseMana not handled\r\nonReceiveDamage not handled\r\nonSendHit not handled";
                }

                // check links between inventory ui and the player
                vItemManager itemManager = player.GetComponent<vItemManager>();
                if (!itemManager)
                {
                    lastUICheckResults += "vItemManager is MISSING\r\n";
                }
                else  // found, checking slot links
                {
                    if (!itemManager.inventoryPrefab)
                    {
                        lastUICheckResults += "Item manager found but inventory NOT set\r\n";
                    }
                    else
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
                    }

                    changeCount += 1;
                    lastUICheckResults += "Re-Linked inventory/UI display slots to the player\r\n";

                    // check use potion links
                    itemManager.onUseItem = new OnHandleItemEvent();
                    UnityEventTools.AddPersistentListener(itemManager.onUseItem, settings.UsePotion);

                    changeCount += 1;
                    lastUICheckResults += "Re-Linked item manager use potion to the player\r\n";
                }

                // link the HUD
                var HUD = GameObject.Find("HUD");
                if (HUD)
                {
                    // XP display
                    if (!settings.XPText)
                    {
                        var XPDisplay = HUD.transform.Find("XP");
                        if (XPDisplay)
                        {
                            settings.XPText = XPDisplay.gameObject.GetComponent<Text>();
                            changeCount += 1;
                            lastUICheckResults += "Linked XP Display UI\r\n";
                        }
                        else
                        {
                            lastUICheckResults += "XP Display UI NOT found\r\n";
                        }
                    }

                    // level reached
                    if (!settings.LevelText)
                    {
                        var LevelDisplay = HUD.transform.Find("Level");
                        if (LevelDisplay)
                        {
                            settings.LevelText = LevelDisplay.gameObject.GetComponent<Text>();
                            changeCount += 1;
                            lastUICheckResults += "Linked Level Display UI\r\n";
                        }
                        else
                        {
                            lastUICheckResults += "XP Level UI NOT found\r\n";
                        }
                    }

                    // level up
                    if (!settings.LevelUpText)
                    {
                        var LevelUpDisplay = HUD.transform.Find("Level up");
                        if (LevelUpDisplay)
                        {
                            settings.LevelUpText = LevelUpDisplay.gameObject.GetComponent<Text>();
                            changeCount += 1;
                            lastUICheckResults += "Linked Level Up Display UI\r\n";
                        }
                        else
                        {
                            lastUICheckResults += "Level Up UI NOT found\r\n";
                        }
                    }

                    // combo performed
                    if (!settings.ComboDisplayText)
                    {
                        var ComboDisplay = HUD.transform.Find("Combo");
                        if (ComboDisplay)
                        {
                            settings.ComboDisplayText = ComboDisplay.gameObject.GetComponent<Text>();
                            changeCount += 1;
                            lastUICheckResults += "Linked Combo Display UI\r\n";
                        }
                        else
                        {
                            lastUICheckResults += "Combo UI NOT found\r\n";
                        }
                    }

                    // available mana
                    if (!settings.ManaSlider)
                    {
                        var ManaDisplay = HUD.transform.Find("mana");
                        if (ManaDisplay)
                        {
                            settings.ManaSlider = ManaDisplay.gameObject.GetComponent<Slider>();
                            changeCount += 1;
                            lastUICheckResults += "Linked Mana Display UI\r\n";
                        }
                        else
                        {
                            lastUICheckResults += "Mana UI NOT found\r\n";
                        }
                    }
                }
                else
                {
                    lastUICheckResults += "Player HUD UI NOT found\r\n";
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
