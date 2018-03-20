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
    /// Upgrade an invector player character to a magic character
    /// </summary>
    public class CreateMagicController : EditorWindow
    {
        GUISkin skin;
        Vector2 rect = new Vector2(480, 300);
        Vector2 scrool;
        Texture2D m_Logo;

        bool bLoaded;
        vInventory InventoryPrefab;
        vItemListData ItemListData;
        GameObject ItemCollectionDisplay;
        GameObject HitDamageParticle;
        GameObject UIBase;
        List<BaseCondition> Conditions;

        List<string> LOD1BodyMeshNames;
        SkinnedMeshRenderer[] LOD1BodyMesh;
        int iWhichMesh = -1;
        GameObject LastScanned;

        /// <summary>
        /// Menu item
        /// </summary>
        [MenuItem("Invector/Shades Spell System/Create Magic Controller", false, -1)]
        public static void CreateMagicControllerMenu()
        {
            GetWindow<CreateMagicController>();
        }

        /// <summary>
        /// Occurs per frame for the unity inspector GUI
        /// </summary>
        void OnGUI()
        {
            // header
            if (!skin) skin = Resources.Load("skin") as GUISkin;
            GUI.skin = skin;
            this.minSize = rect;
            this.titleContent = new GUIContent("Shadex", null, "Create Magic Controller");
            m_Logo = Resources.Load("icon_v2") as Texture2D;
            GUILayout.BeginVertical("Create Magic Controller", "window");
            GUILayout.Label(m_Logo, GUILayout.MaxHeight(25));
            GUILayout.Space(5);
            

            GUILayout.BeginVertical("box");
            if (!bLoaded)
            {
                // fill conditions list
                Conditions = new List<BaseCondition>();
                BaseDamage[] AllConditions = (BaseDamage[])Enum.GetValues(typeof(BaseDamage));
                for (int i = 0; i < AllConditions.Length; i++)
                {
                    if (AllConditions[i] != BaseDamage.Physical)
                    {
                        Conditions.Add(new BaseCondition() { Type = AllConditions[i], Display = AssetDatabase.LoadAssetAtPath("Assets/Invector-3rdPersonController/Add-ons/ShadesSpellSystem/Prefabs/Conditions/" + AllConditions[i].ToString() + ".prefab", typeof(GameObject)) as GameObject });
                    }
                }

                // standard invector components, grab prefabs
                if (File.Exists(Application.dataPath + "/Invector-3rdPersonController/Basic Locomotion/3D Models/Particles/DamageEffect/prefabs/bloodSplash.prefab"))
                {
                    HitDamageParticle = AssetDatabase.LoadAssetAtPath("Assets/Invector-3rdPersonController/Basic Locomotion/3D Models/Particles/DamageEffect/prefabs/bloodSplash.prefab", typeof(GameObject)) as GameObject;
                }
                if (File.Exists(Application.dataPath + "/Invector-3rdPersonController/ItemManager/Prefabs/ItemCollectionDisplay.prefab"))
                {
                    ItemCollectionDisplay = AssetDatabase.LoadAssetAtPath("Assets/Invector-3rdPersonController/ItemManager/Prefabs/ItemCollectionDisplay.prefab", typeof(GameObject)) as GameObject;
                }

                // spell system components, grab prefabs
                if (File.Exists(Application.dataPath + "/Invector-3rdPersonController/Add-ons/ShadesSpellSystem/Inventory/Inventory_MeleeMagic_Auto.prefab"))
                {
                    InventoryPrefab = AssetDatabase.LoadAssetAtPath("Assets/Invector-3rdPersonController/Add-ons/ShadesSpellSystem/Inventory/Inventory_MeleeMagic_Auto.prefab", typeof(vInventory)) as vInventory;
                }
                if (File.Exists(Application.dataPath + "/Invector-3rdPersonController/Add-ons/ShadesSpellSystem/Inventory/vMeleeMagic_ItemListData.asset"))
                {
                    ItemListData = AssetDatabase.LoadAssetAtPath("Assets/Invector-3rdPersonController/Add-ons/ShadesSpellSystem/Inventory/vMeleeMagic_ItemListData.asset", typeof(vItemListData)) as vItemListData;
                }

                // find the vUI root hierarchy object
                GameObject[] goRoots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
                foreach (GameObject go in goRoots)
                {
                    if (go.name == "vUI")
                    {
                        UIBase = go;
                        break;
                    }
                }


                // only run once
                bLoaded = true;
            }

            // rescan for skinned mesh renderers on active selection change
            if (Selection.activeGameObject != null)
            {
                if (LastScanned != Selection.activeGameObject)
                {
                    LastScanned = Selection.activeGameObject;
                    iWhichMesh = -1;
                    LOD1BodyMeshNames = new List<string>();
                    LOD1BodyMesh = null;
                    LOD1BodyMesh = Selection.activeGameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
                    if (LOD1BodyMesh != null)
                    {
                        for (int i = 0; i < LOD1BodyMesh.Length; i++)
                        {
                            LOD1BodyMeshNames.Add(LOD1BodyMesh[i].name);
                        }
                    }
                }
            }
            else
            {
                iWhichMesh = -1;
                LOD1BodyMeshNames = new List<string>();
            }
                

            // allow user to change the default component parts
            InventoryPrefab = EditorGUILayout.ObjectField("Inventory Prefab: ", InventoryPrefab, typeof(vInventory), false) as vInventory;
            ItemListData = EditorGUILayout.ObjectField("Item List Data: ", ItemListData, typeof(vItemListData), false) as vItemListData;
            ItemCollectionDisplay = EditorGUILayout.ObjectField("Item Collection Display: ", ItemCollectionDisplay, typeof(GameObject), false) as GameObject;
            HitDamageParticle = EditorGUILayout.ObjectField("Hit Damage Particle: ", HitDamageParticle, typeof(GameObject), false) as GameObject;
            UIBase = EditorGUILayout.ObjectField("UI Parent: ", UIBase, typeof(GameObject), false) as GameObject;
            for (int i = 0; i < Conditions.Count; i++)
            {
                Conditions[i].Display = EditorGUILayout.ObjectField(Conditions[i].Type.ToString() + " Condition", Conditions[i].Display, typeof(GameObject), false) as GameObject;
            }
            //LOD1BodyMesh = EditorGUILayout.ObjectField("LOD1 Body Mesh: ", LOD1BodyMesh, typeof(SkinnedMeshRenderer), false) as SkinnedMeshRenderer;
            iWhichMesh = EditorGUILayout.Popup("LOD1 Body :", iWhichMesh, LOD1BodyMeshNames.ToArray());


            // fail safe
            if (InventoryPrefab != null && InventoryPrefab.GetComponent<vInventory>() == null)
            {
                EditorGUILayout.HelpBox("Please select a Inventory Prefab that contains the vInventory script", MessageType.Warning);
            }
            GUILayout.EndVertical();

            // create button if valid
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (InventoryPrefab != null && ItemListData != null)
            {
                if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<vThirdPersonController>() != null)
                {
                    if (iWhichMesh > -1)
                    {
                        if (GUILayout.Button("Create"))
                            Create();
                    }
                    else
                        EditorGUILayout.HelpBox("Please select which skinned mesh renderer is the LOD1 body", MessageType.Warning);                    
                }
                else
                    EditorGUILayout.HelpBox("Please select the Player to add this component", MessageType.Warning);                
            }
            else
                EditorGUILayout.HelpBox("Please select the inventory prefab and item list data", MessageType.Warning);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        /// <summary>
        /// Create button event, adds all components.
        /// </summary>
        void Create()
        {
            if (Selection.activeGameObject != null)
            {  // fail safe
                // add melee manager for when shooter
                if (!Selection.activeGameObject.GetComponent<vMeleeManager>())
                {
                    Selection.activeGameObject.AddComponent<vMeleeManager>();
                }

                // inventory
                vItemManager itemManager = Selection.activeGameObject.GetComponent<vItemManager>();
                if (!itemManager)
                {
                    itemManager = Selection.activeGameObject.AddComponent<vItemManager>();
                    vItemManagerUtilities.CreateDefaultEquipPoints(itemManager, itemManager.GetComponent<vMeleeManager>());
                }
                itemManager.inventoryPrefab = InventoryPrefab;
                itemManager.itemListData = ItemListData;
                itemManager.itemsFilter.Add(vItemType.MeleeWeapon);
                itemManager.itemsFilter.Add(vItemType.Spell);

                // hit damage particle                                
                vHitDamageParticle hitDamageParticle = Selection.activeGameObject.GetComponent<vHitDamageParticle>();
                if (!hitDamageParticle)
                {
                    hitDamageParticle = Selection.activeGameObject.AddComponent<vHitDamageParticle>();
                }
                hitDamageParticle.defaultHitEffect = HitDamageParticle;

                // UI
                GameObject goItemCollectionDisplay = PrefabUtility.InstantiatePrefab(ItemCollectionDisplay) as GameObject;
                goItemCollectionDisplay.transform.SetParent(UIBase.transform);
                GameObject goInventoryPrefab = PrefabUtility.InstantiatePrefab(InventoryPrefab.gameObject) as GameObject;
                goInventoryPrefab.name = "Inventory_MeleeMagic_Auto";


                // leveling system
                CharacterInstance levelingsystem = Selection.activeGameObject.GetComponent<CharacterInstance>();
                if (!levelingsystem)
                {
                    levelingsystem = Selection.activeGameObject.AddComponent<CharacterInstance>();
                }

                // link the invector character damage event to the leveling system
                vThirdPersonController thirdp = Selection.activeGameObject.GetComponent<vThirdPersonController>();
                UnityEventTools.AddPersistentListener(thirdp.onReceiveDamage, levelingsystem.OnRecieveDamage);

                // link the melee manager hits to the leveling system
                vMeleeManager meleeM = Selection.activeGameObject.GetComponent<vMeleeManager>();
                if (meleeM)
                {
                    if (meleeM.onDamageHit == null)
                    {
                        meleeM.onDamageHit = new vOnHitEvent();
                    }
                    UnityEventTools.AddPersistentListener(meleeM.onDamageHit, levelingsystem.OnSendHit);
                }

                // add conditions and update particles to use the LOD 1 mesh
                levelingsystem.Conditions = new List<BaseCondition>();
                levelingsystem.Conditions.Add(new BaseCondition() { Type = BaseDamage.Physical });
                GameObject goConditionsRoot = new GameObject("Conditions");
                goConditionsRoot.transform.SetParent(Selection.activeGameObject.transform);
                goConditionsRoot.transform.position = new Vector3(0f, 0f, 0f);
                foreach (BaseCondition bc in Conditions)
                {
                    GameObject goCondition = null;
                    if (bc.Display)
                    {
                        // load the prefab
                        goCondition = PrefabUtility.InstantiatePrefab(bc.Display) as GameObject;
                        goCondition.transform.SetParent(goConditionsRoot.transform);
                        goCondition.transform.position = new Vector3(0f, 0f, 0f);

                        // update all particles to use the mesh renderer from LOD1
                        goCondition.SetActive(true);
                        ParticleSystem[] ConditionParticles = goCondition.GetComponentsInChildren<ParticleSystem>();
                        foreach (ParticleSystem p in ConditionParticles)
                        {
                            if (p.shape.enabled)
                            {
                                if (p.shape.shapeType == ParticleSystemShapeType.SkinnedMeshRenderer)
                                {
                                    ParticleSystem.ShapeModule editableShape = p.shape;
                                    editableShape.skinnedMeshRenderer = LOD1BodyMesh[iWhichMesh];
                                }
                            }
                        }
                        goCondition.SetActive(false);                        
                    }

                    // add to the levelling system
                    levelingsystem.Conditions.Add(new BaseCondition() { Type = bc.Type, Length = 0, Display = goCondition });
                }

                // add the magic spawn point
                GameObject goMagicSpawn = new GameObject("Magic Spawn");
                goMagicSpawn.transform.SetParent(Selection.activeGameObject.transform);
                goMagicSpawn.transform.position = new Vector3(0f, 1.5f, 0.9f);

                // magic input
                MagicSettings magicIO = Selection.activeGameObject.GetComponent<MagicSettings>();
                if (!magicIO)
                {
                    magicIO = Selection.activeGameObject.AddComponent<MagicSettings>();
                }
                magicIO.PooledMagic = true;
                magicIO.MagicSpawnPoint = goMagicSpawn.transform;
                magicIO.onUseMana = new UnityIntEvent();
                UnityEventTools.AddPersistentListener(magicIO.onUseMana, levelingsystem.UseMana);

                
#if !VANILLA    // set spell triggers F1-F5
                GameObject goInventoryWindow = goInventoryPrefab.transform.Find("InventoryWindow").gameObject;  // grab inventory window
                GameObject goEquipmentInventory = goInventoryWindow.transform.Find("EquipmentInventory").gameObject;  // and the equip slot parent
                goEquipmentInventory.SetActive(true);  // enable for component search
                int iNext = 1;
                vEquipSlot[] allSlots = goInventoryPrefab.GetComponentsInChildren<vEquipSlot>();
                foreach (vEquipSlot slot in allSlots)
                {
                    if (slot.transform.parent.parent.name == "EquipMentArea_Spells")
                    {  // is a spell inventory area
                        MagicSpellTrigger trigger = new MagicSpellTrigger();  // create the trigger
                        trigger.EquipSlots = new vEquipSlot[] { slot };  // set the inventory slot
                        trigger.Input = new GenericInput("F" + iNext.ToString(), null, null);  // set the input key
                        trigger.Input.useInput = true;  // enable
                        vEquipmentDisplay[] allDisplays = goInventoryPrefab.GetComponentsInChildren<vEquipmentDisplay>();  // find all displays
                        foreach (vEquipmentDisplay disp in allDisplays)
                        {  // check all
                            if (disp.gameObject.name == slot.gameObject.name.Replace("EquipSlot ", "EquipDisplay_Spell "))
                            {  // found matching name?
                                trigger.EquipDisplay = disp;  // success, apply
                                UnityEventTools.AddPersistentListener(slot.onAddItem, magicIO.SpellEquiped);  // listen for spells equiped
                                UnityEventTools.AddPersistentListener(slot.onRemoveItem, magicIO.SpellUnEquiped);  // and unequiped
                                break;  // drop out
                            }
                        }
                        magicIO.SpellsTriggers.Add(trigger);  // add the trigger
                        iNext += 1;  // next please
                    }
                }
                goEquipmentInventory.SetActive(false);  // deactivate the inventory display
#endif

                // link the UI further
                Transform tHUD = UIBase.transform.Find("HUD");
                magicIO.XPText = tHUD.Find("XP").GetComponent<UnityEngine.UI.Text>();
                magicIO.LevelText = tHUD.Find("Level").GetComponent<UnityEngine.UI.Text>();
                magicIO.LevelUpText = tHUD.Find("Level up").GetComponent<UnityEngine.UI.Text>();
                magicIO.ManaSlider = tHUD.Find("mana").GetComponent<UnityEngine.UI.Slider>();

#if !VANILLA
                itemManager.onUseItem = new OnHandleItemEvent();
                UnityEventTools.AddPersistentListener(itemManager.onUseItem, magicIO.UsePotion);

                // also lock input when inventory open
                itemManager.onOpenCloseInventory = new OnOpenCloseInventory();
                vMeleeCombatInput MeleeInput = Selection.activeGameObject.GetComponent<vMeleeCombatInput>();
                if (MeleeInput) UnityEventTools.AddPersistentListener(itemManager.onOpenCloseInventory, MeleeInput.SetLockMeleeInput);
#endif
                // work complete
                this.Close();
            }
            else
                Debug.Log("Please select the Player to add these components.");
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
