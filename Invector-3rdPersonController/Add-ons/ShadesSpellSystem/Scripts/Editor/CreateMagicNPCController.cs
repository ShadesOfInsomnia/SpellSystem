using UnityEngine;
using UnityEditor.Events;
using System.Collections.Generic;
using UnityEditor;
using Invector;
using Invector.vCharacterController;
using Invector.vItemManager;
using System;
using System.IO;
using Invector.vCharacterController.AI;
using Invector.vMelee;

namespace Shadex
{
    /// <summary>
    /// Upgrade an invector NPC character to a magic character
    /// </summary>
    public class CreateMagicNPCController : EditorWindow
    {
        GUISkin skin;
        Vector2 rect = new Vector2(480, 310);
        Vector2 scrool;
        Texture2D m_Logo;

        bool bLoaded;
        vItemListData ItemListData;
        GameObject HitDamageParticle;
        GameObject HealthUI;
        Transform HeadBone;
        GameObject LastSelection;
        List<BaseCondition> Conditions;

        List<string> LOD1BodyMeshNames;
        SkinnedMeshRenderer[] LOD1BodyMesh;
        int iWhichMesh = -1;
        GameObject LastScanned;

        /// <summary>
        /// Menu item
        /// </summary>
        [MenuItem("Invector/Shades Spell System/Create Magic NPC Controller", false, -1)]
        public static void CreateMagicNPCControllerMenu()
        {
            GetWindow<CreateMagicNPCController>();
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
            this.titleContent = new GUIContent("Shadex", null, "Create Magic NPC Controller");
            m_Logo = Resources.Load("icon_v2") as Texture2D;
            GUILayout.BeginVertical("Create Magic NPC Controller", "window");
            GUILayout.Label(m_Logo, GUILayout.MaxHeight(25));
            GUILayout.Space(5);

            // attempt grab the headbone on selected gameobject
            if (LastSelection != Selection.activeGameObject)
            {
                Animator animator = Selection.activeGameObject.GetComponent<Animator>();
                if (animator)
                {
                    HeadBone = animator.GetBoneTransform(HumanBodyBones.Head);
                }
                LastSelection = Selection.activeGameObject;
            }

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

                // spell system components, grab prefabs
                if (File.Exists(Application.dataPath + "/Invector-3rdPersonController/Add-ons/ShadesSpellSystem/Inventory/vMeleeMagic_ItemListData.asset"))
                {
                    ItemListData = AssetDatabase.LoadAssetAtPath("Assets/Invector-3rdPersonController/Add-ons/ShadesSpellSystem/Inventory/vMeleeMagic_ItemListData.asset", typeof(vItemListData)) as vItemListData;
                }
                if (File.Exists(Application.dataPath + "/Invector-3rdPersonController/Add-ons/ShadesSpellSystem/UI/enemyHealthUI.prefab"))
                {
                    HealthUI = AssetDatabase.LoadAssetAtPath("Assets/Invector-3rdPersonController/Add-ons/ShadesSpellSystem/UI/enemyHealthUI.prefab", typeof(GameObject)) as GameObject;
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
            ItemListData = EditorGUILayout.ObjectField("Item List Data: ", ItemListData, typeof(vItemListData), false) as vItemListData;
            HitDamageParticle = EditorGUILayout.ObjectField("Hit Damage Particle: ", HitDamageParticle, typeof(GameObject), false) as GameObject;
            HeadBone = EditorGUILayout.ObjectField("Head Bone: ", HeadBone, typeof(Transform), false) as Transform;
            for (int i = 0; i < Conditions.Count; i++)
            {
                Conditions[i].Display = EditorGUILayout.ObjectField(Conditions[i].Type.ToString() + " Condition", Conditions[i].Display, typeof(GameObject), false) as GameObject;
            }
            iWhichMesh = EditorGUILayout.Popup("LOD1 Body :", iWhichMesh, LOD1BodyMeshNames.ToArray());
            GUILayout.EndVertical();

            // create button if valid
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (ItemListData != null && HeadBone != null && LOD1BodyMesh != null)
            {
                if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<v_AIController>() != null)
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
                    EditorGUILayout.HelpBox("Please select the vAI to add this component", MessageType.Warning);
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
                // hit damage particle                                
                vHitDamageParticle hitDamageParticle = Selection.activeGameObject.GetComponent<vHitDamageParticle>();
                if (!hitDamageParticle)
                {
                    hitDamageParticle = Selection.activeGameObject.AddComponent<vHitDamageParticle>();
                }
                hitDamageParticle.defaultHitEffect = HitDamageParticle;

                // melee manager
                vMeleeManager meleeManager = Selection.activeGameObject.GetComponent<vMeleeManager>();
                if (!meleeManager)
                {
                    meleeManager = Selection.activeGameObject.AddComponent<vMeleeManager>();
                }
                meleeManager.hitProperties = new HitProperties();
                meleeManager.hitProperties.hitDamageTags[0] = "Player";
                
                // leveling system
                CharacterInstance levelingsystem = Selection.activeGameObject.GetComponent<CharacterInstance>();
                if (!levelingsystem)
                {
                    levelingsystem = Selection.activeGameObject.AddComponent<CharacterInstance>();
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

                // link the ai damage to the leveling system
                v_AIController vai = Selection.activeGameObject.GetComponent<v_AIController>();
                vai.onReceiveDamage = new vHealthController.OnReceiveDamage();
                UnityEventTools.AddPersistentListener(vai.onReceiveDamage, levelingsystem.OnRecieveDamage);

                // link the melee manager hits to the leveling system
                meleeManager.onDamageHit = new vOnHitEvent();
                UnityEventTools.AddPersistentListener(meleeManager.onDamageHit, levelingsystem.OnSendHit);

                // add the magic spawn point
                GameObject goMagicSpawn = new GameObject("Magic Spawn");
                goMagicSpawn.transform.SetParent(Selection.activeGameObject.transform);
                goMagicSpawn.transform.position = new Vector3(0f, 1.5f, 0.9f);

                // AI inventory
                MagicAIItemManager itemManager = Selection.activeGameObject.GetComponent<MagicAIItemManager>();
                if (!itemManager)
                {
                    itemManager = Selection.activeGameObject.AddComponent<MagicAIItemManager>();
                }
                itemManager.itemListData = ItemListData;
                itemManager.itemsFilter.Add(vItemType.Spell);

                // Magic AI
                MagicAI mai = Selection.activeGameObject.GetComponent<MagicAI>();
                if (!mai)
                {
                    mai = Selection.activeGameObject.AddComponent<MagicAI>();
                }
                mai.MagicSpawnPoint = goMagicSpawn.transform;

                // health/mana bars
                GameObject HealthUIinstance = (GameObject)Instantiate(HealthUI);
                HealthUIinstance.transform.SetParent(HeadBone);
                HealthUIinstance.transform.localPosition = HealthUI.transform.localPosition;
                HealthUIinstance.transform.localRotation = HealthUI.transform.localRotation;
                HealthUIinstance.transform.localScale = HealthUI.transform.localScale;

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
