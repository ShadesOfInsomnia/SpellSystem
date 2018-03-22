using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Invector.vItemManager;
using System.Collections.Generic;
using UnityEditor.Animations;
using Invector.vMelee;

namespace Shadex
{
    /// <summary>
    /// Custom editor for the spell book, allowing a common set of inventory spells to be applied to multiple animators.
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SpellBook), true)]
    public class SpellBookEditor : SpellBookBaseEditor 
    {
        /// <summary>Defaults loaded?</summary>
        protected bool bLoaded;

        /// <summary>Validation messages.</summary>
        protected string validationMessage = "";

        /// <summary>Number of changes made.</summary>
        protected int ChangesMade;

        /// <summary>Cache a reference to the animator.</summary>
        protected Animator TheAnimator;

        /// <summary>Health message temp for each spell.</summary>
        protected string HealthMessage = "";

        /// <summary>Spell detail temp for each spell.</summary>
        protected SpellBookListEntry SpellDetail;

        /// <summary>MagicID temp for each spell.</summary>
        protected int MagicID = 0;

        /// <summary>ManaCost temp for each spell.</summary>
        protected int ManaCost = 0;

        /// <summary>Filter the spells by damage type.</summary>
        protected bool DamageFilterEnabled;

        /// <summary>Index of the filter.</summary>
        protected BaseDamage DamageFilterIndex;
        

        /// <summary>
        /// Override default inspector with the custom spell book UI
        /// </summary>
        public override void OnInspectorGUI()
        {        
            // set the skin and header
            defaultSkin = GUI.skin;
            if (skin) GUI.skin = skin;
            
            GUILayout.BeginVertical("SPELL BOOK", "window");
            GUILayout.Label(m_Logo, GUILayout.MaxHeight(25));
            
            // cache reference to the base class
            SpellBook cc = (SpellBook)target;
            TheAnimator = cc.GetComponent<Animator>();

            // load defaults
            if (!bLoaded) LoadDefaults(cc);

            // prevent controller validation if minimum field values not set
            if (ValidateFields(cc))
            {
                // run the animator controller validation?
                if (GUILayout.Button("Validate Controller(s)"))
                {
                    ChangesMade = -1;
                    foreach (SpellBookApplyTo anim in cc.ApplyTo)
                    {
                        ValidateController(cc, anim);
                    }
                }
            }
            else  // show why unable to validate the controller
            {
                EditorGUILayout.HelpBox(validationMessage, MessageType.Warning);
            }

            // show number of changes made
            if (ChangesMade != -1)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox(ChangesMade.ToString() + " changes made, validation complete", MessageType.Info);
                GUILayout.EndHorizontal();
            }

            // allow edit of the defaults
            InputFields(cc);
            
            // process current inventory
            EditorGUILayout.LabelField("Available Spells", EditorStyles.boldLabel);
            cc.itemListData = EditorGUILayout.ObjectField("Inventory: ", cc.itemListData, typeof(vItemListData), false) as vItemListData;
            if (cc.itemListData)
            {
                // allow edit of item list data
                if (GUILayout.Button("Edit Items in List"))
                {
                    vItemListWindow.CreateWindow(cc.itemListData);
                }

                // filter by damage type
                GUILayout.BeginHorizontal();
                DamageFilterEnabled = EditorGUILayout.Toggle("Damage Type Filter:", DamageFilterEnabled);
                DamageFilterIndex = (BaseDamage)EditorGUILayout.EnumPopup(DamageFilterIndex, GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();

                // process all inventory items
                for (int i = 0; i < cc.itemListData.items.Count; i++)
                {
                    // only interested in the spells
                    if (cc.itemListData.items[i].type == vItemType.Spell)
                    {
                        // custom edit of spell options for the behavior state
                        DisplaySpellItem(cc, i);
                    }                    
                }
            }

            // end the container, revert the skin
            GUILayout.EndVertical();
            GUI.skin = defaultSkin;
        }

        /// <summary>
        /// Spell book item options, spawns etc.
        /// </summary>
        /// <param name="cc">Reference to the parent spell book class.</param>
        /// <param name="i">Loop index.</param>
        public virtual void DisplaySpellItem(SpellBook cc, int i)
        {
            // check all attributes are present
            CheckAttributes(cc, i);

            // find the spell details
            SpellDetail = cc.Spells.Find(s => s.MagicID == MagicID);
            if (SpellDetail == null)  // not found
            {
                // create new empty entry
                SpellDetail = new SpellBookListEntry();
                SpellDetail.AllowMovement = false;
                SpellDetail.MagicID = MagicID;
                SpellDetail.ClipSpellCast = cc.ClipSpellCast;
                SpellDetail.ClipSpellChargeInit = cc.ClipSpellChargeInit;
                SpellDetail.ClipSpellChargeHold = cc.ClipSpellChargeHold;
                SpellDetail.ClipSpellChargeRelease = cc.ClipSpellChargeRelease;
                SpellDetail.SpeedCast = 1;
                SpellDetail.SpeedCharge = 1;
                SpellDetail.SpeedHold = 1;
                SpellDetail.SpeedRelease = 1;
                
                SpellDetail.MeleeHand = SpellBookHands.None;
                SpellDetail.meleeAttackType = vAttackType.Unarmed;
                SpellDetail.reactionID = 1;
                SpellDetail.recoilID = 1;
                SpellDetail.damageMultiplier = 1;
                SpellDetail.allowMovementAt = 0.9f;
                SpellDetail.endDamage = 0.9f;
                SpellDetail.startDamage = 0.05f;

                SpellDetail.SpellOptions = new SpellBookEntry();
                SpellDetail.SpellOptions.attackLimb = AvatarIKGoal.LeftHand;
                SpellDetail.SpellOptions.attackLimb2 = AvatarIKGoal.RightHand;
                SpellDetail.SpellOptions.SpawnOverTime = new List<SpawnerOptionsOverTime>();
                SpellDetail.SpellOptions.LimbParticleEffect = cc.DefaultHandParticle;
                SpellDetail.SpellOptions.LimbParticleEffect2 = cc.DefaultHandParticle;                
                SpellDetail.SpellOptions.Icon = cc.itemListData.items[i].icon;
                SpellDetail.SpellOptions.SpellName = cc.itemListData.items[i].name;
                SpellDetail.SpellOptions.SubType = SpellBookEntrySubType.Casting;

                SpellDetail.SpellOptionsCharge = new SpellBookEntry();
                SpellDetail.SpellOptionsCharge.attackLimb = AvatarIKGoal.LeftHand;
                SpellDetail.SpellOptionsCharge.attackLimb2 = AvatarIKGoal.RightHand;
                SpellDetail.SpellOptionsCharge.SpawnOverTime = new List<SpawnerOptionsOverTime>();
                SpellDetail.SpellOptionsCharge.LimbParticleEffect = cc.DefaultHandParticle;
                SpellDetail.SpellOptionsCharge.LimbParticleEffect2 = cc.DefaultHandParticle;
                SpellDetail.SpellOptionsCharge.Icon = cc.itemListData.items[i].icon;
                SpellDetail.SpellOptionsCharge.SpellName = cc.itemListData.items[i].name;
                SpellDetail.SpellOptionsCharge.SubType = SpellBookEntrySubType.Charge;

                SpellDetail.SpellOptionsHold = new SpellBookEntry();
                SpellDetail.SpellOptionsHold.attackLimb = AvatarIKGoal.LeftHand;
                SpellDetail.SpellOptionsHold.attackLimb2 = AvatarIKGoal.RightHand;
                SpellDetail.SpellOptionsHold.SpawnOverTime = new List<SpawnerOptionsOverTime>();
                SpellDetail.SpellOptionsHold.LimbParticleEffect = cc.DefaultHandParticle;
                SpellDetail.SpellOptionsHold.LimbParticleEffect2 = cc.DefaultHandParticle;
                SpellDetail.SpellOptionsHold.Icon = cc.itemListData.items[i].icon;
                SpellDetail.SpellOptionsHold.SpellName = cc.itemListData.items[i].name;
                SpellDetail.SpellOptionsHold.SubType = SpellBookEntrySubType.Hold;

                SpellDetail.SpellOptionsRelease = new SpellBookEntry();
                SpellDetail.SpellOptionsRelease.attackLimb = AvatarIKGoal.LeftHand;
                SpellDetail.SpellOptionsRelease.attackLimb2 = AvatarIKGoal.RightHand;
                SpellDetail.SpellOptionsRelease.SpawnOverTime = new List<SpawnerOptionsOverTime>();
                SpellDetail.SpellOptionsRelease.LimbParticleEffect = cc.DefaultHandParticle;
                SpellDetail.SpellOptionsRelease.LimbParticleEffect2 = cc.DefaultHandParticle;
                SpellDetail.SpellOptionsRelease.Icon = cc.itemListData.items[i].icon;
                SpellDetail.SpellOptionsRelease.SpellName = cc.itemListData.items[i].name;
                SpellDetail.SpellOptionsRelease.SubType = SpellBookEntrySubType.Release;

                cc.Spells.Add(SpellDetail);
            }

            // display the shared spell options GUI            
            if (!DamageFilterEnabled || (DamageFilterEnabled && SpellDetail.DamageType == DamageFilterIndex))
            {
                if (!SpellDetail.Charge)
                {
                    DisplaySpellSubDetail(SpellDetail.SpellOptions);
                }
                else  // charge hold release
                {
                    DisplaySpellSubDetail(SpellDetail.SpellOptionsCharge);
                    if (SpellDetail.Expanded)
                    {
                        DisplaySpellSubDetail(SpellDetail.SpellOptionsHold);
                        DisplaySpellSubDetail(SpellDetail.SpellOptionsRelease);
                    }
                }
            }
        }

        /// <summary>
        /// Spell header info used to create the animator state.
        /// </summary>
        protected override void DisplaySpellHeader(SpellBookEntrySubType SubType)
        {
            // expand button
            if (SpellDetail != null && SubType != SpellBookEntrySubType.Hold && SubType != SpellBookEntrySubType.Release)
            {
                if (GUILayout.Button((SpellDetail.Expanded ? "<<" : ">>"), GUILayout.Width(30)))
                {
                    SpellDetail.Expanded = !SpellDetail.Expanded;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            // validation messages
            if (HealthMessage != "")
            {
                if (SpellDetail == null)
                    EditorGUILayout.HelpBox(HealthMessage, MessageType.Error);
                else if (!SpellDetail.Expanded)
                    EditorGUILayout.HelpBox(HealthMessage, MessageType.Warning);
            }

            // type of damage for the filter
            if (SpellDetail.Expanded)
            {
                SpellDetail.DamageType = (BaseDamage)EditorGUILayout.EnumPopup("Base Damage Type: ", SpellDetail.DamageType, GUILayout.ExpandWidth(true));
            }
        }

        /// <summary>
        /// Is the spell expanded, true/false for the spell book but always true for the spell attack behavior.
        /// </summary>
        /// <returns>Whether expanded.</returns>
        protected override bool IsExpanded()
        {
            if (SpellDetail == null)
                return false;
            else
                return SpellDetail.Expanded;
        }

        /// <summary>
        /// Information specific to the spell book regarding the spell
        /// </summary>
        protected override void DisplaySpellInfo(SpellBookEntrySubType SubType)
        {
            // output attributes
            DisplayAttributesInfo(SubType);

            // chargeable spell?
            EditorGUILayout.LabelField("Casting Animations/Layers", EditorStyles.boldLabel);            
            if (SubType == SpellBookEntrySubType.Casting || SubType == SpellBookEntrySubType.Charge)
            {
                GUILayout.BeginHorizontal();
                SpellDetail.Charge = EditorGUILayout.Toggle("Charge Release:", SpellDetail.Charge);
                SpellDetail.AllowMovement = EditorGUILayout.Toggle("Allow Move:", SpellDetail.AllowMovement);
                GUILayout.EndHorizontal();
            }

            // animation selector
            switch (SubType) {
                case SpellBookEntrySubType.Charge:
                    SpellDetail.ClipSpellChargeInit = EditorGUILayout.ObjectField("Charge Init: ", SpellDetail.ClipSpellChargeInit, typeof(AnimationClip), false) as AnimationClip;

                    GUILayout.BeginHorizontal();
                    SpellDetail.MirrorCharge = EditorGUILayout.Toggle("Mirror:", SpellDetail.MirrorCharge);
                    GUILayout.Label("Speed:", GUILayout.Width(50));
                    SpellDetail.SpeedCharge = EditorGUILayout.FloatField(SpellDetail.SpeedCharge);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    SpellDetail.FootIKCharge = EditorGUILayout.Toggle("Foot IK:", SpellDetail.FootIKCharge);
                    GUILayout.Label("Offset:", GUILayout.Width(50));
                    SpellDetail.CycleOffsetCharge = EditorGUILayout.FloatField(SpellDetail.CycleOffsetCharge);
                    GUILayout.EndHorizontal();
                    break;
                case SpellBookEntrySubType.Hold:
                    SpellDetail.ClipSpellChargeHold = EditorGUILayout.ObjectField("Charge Hold: ", SpellDetail.ClipSpellChargeHold, typeof(AnimationClip), false) as AnimationClip;

                    GUILayout.BeginHorizontal();
                    SpellDetail.MirrorHold = EditorGUILayout.Toggle("Mirror:", SpellDetail.MirrorHold);
                    GUILayout.Label("Speed:", GUILayout.Width(50));
                    SpellDetail.SpeedHold = EditorGUILayout.FloatField(SpellDetail.SpeedHold);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    SpellDetail.FootIKHold = EditorGUILayout.Toggle("Foot IK:", SpellDetail.FootIKHold);
                    GUILayout.Label("Offset:", GUILayout.Width(50));
                    SpellDetail.CycleOffsetHold = EditorGUILayout.FloatField(SpellDetail.CycleOffsetHold);
                    GUILayout.EndHorizontal();
                    break;
                case SpellBookEntrySubType.Release:
                    SpellDetail.ClipSpellChargeRelease = EditorGUILayout.ObjectField("Charge Release: ", SpellDetail.ClipSpellChargeRelease, typeof(AnimationClip), false) as AnimationClip;

                    GUILayout.BeginHorizontal();
                    SpellDetail.MirrorRelease = EditorGUILayout.Toggle("Mirror:", SpellDetail.MirrorRelease);
                    GUILayout.Label("Speed:", GUILayout.Width(50));
                    SpellDetail.SpeedRelease = EditorGUILayout.FloatField(SpellDetail.SpeedRelease);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    SpellDetail.FootIKRelease = EditorGUILayout.Toggle("Foot IK:", SpellDetail.FootIKRelease);
                    GUILayout.Label("Offset:", GUILayout.Width(50));
                    SpellDetail.CycleOffsetRelease = EditorGUILayout.FloatField(SpellDetail.CycleOffsetRelease);
                    GUILayout.EndHorizontal();
                    break;
                default:  // casting
                    SpellDetail.ClipSpellCast = EditorGUILayout.ObjectField("Spell Cast: ", SpellDetail.ClipSpellCast, typeof(AnimationClip), false) as AnimationClip;

                    GUILayout.BeginHorizontal();
                    SpellDetail.MirrorCast = EditorGUILayout.Toggle("Mirror:", SpellDetail.MirrorCast);
                    GUILayout.Label("Speed:", GUILayout.Width(50));
                    SpellDetail.SpeedCast = EditorGUILayout.FloatField(SpellDetail.SpeedCast);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    SpellDetail.FootIKCast = EditorGUILayout.Toggle("Foot IK:", SpellDetail.FootIKCast);
                    GUILayout.Label("Offset:", GUILayout.Width(50));
                    SpellDetail.CycleOffsetCast = EditorGUILayout.FloatField(SpellDetail.CycleOffsetCast);
                    GUILayout.EndHorizontal();
                    break;
            }

            // Melee damage options
            if (SubType == SpellBookEntrySubType.Casting || SubType == SpellBookEntrySubType.Release)
            {
                // which hand has melee damage enabled if any
                EditorGUILayout.LabelField("Melee Damage?", EditorStyles.boldLabel);
                SpellDetail.MeleeHand = (SpellBookHands)EditorGUILayout.EnumPopup("Which Hand:", SpellDetail.MeleeHand, GUILayout.ExpandWidth(true));

                // melee damage options
                if (SpellDetail.MeleeHand != SpellBookHands.None)
                {
                    // attack name / activate ragdoll
                    GUILayout.BeginHorizontal();
                    SpellDetail.ignoreDefense = EditorGUILayout.Toggle("Ignore Defense:", SpellDetail.ignoreDefense);
                    GUILayout.Label("Name:", GUILayout.Width(50));
                    SpellDetail.attackName = EditorGUILayout.TextField(SpellDetail.attackName);
                    GUILayout.EndHorizontal();

                    // armed or unarmed
                    SpellDetail.meleeAttackType = (vAttackType)EditorGUILayout.EnumPopup("Type:", SpellDetail.meleeAttackType, GUILayout.ExpandWidth(true));

                    // damage start/end time
                    GUILayout.BeginHorizontal();
                    SpellDetail.startDamage = EditorGUILayout.FloatField("Damage Start:", SpellDetail.startDamage);
                    GUILayout.Label("End:", GUILayout.Width(50));
                    SpellDetail.endDamage = EditorGUILayout.FloatField(SpellDetail.endDamage);
                    GUILayout.EndHorizontal();

                    // allow move / damage multiplier
                    GUILayout.BeginHorizontal();
                    SpellDetail.allowMovementAt = EditorGUILayout.FloatField("Allow Movement:", SpellDetail.allowMovementAt);
                    GUILayout.Label("Dmg*:", GUILayout.Width(50));
                    SpellDetail.damageMultiplier = EditorGUILayout.IntField(SpellDetail.damageMultiplier);
                    GUILayout.EndHorizontal();

                    // recoil/reaction ID's
                    GUILayout.BeginHorizontal();
                    SpellDetail.recoilID = EditorGUILayout.IntField("Recoil ID:", SpellDetail.recoilID);
                    GUILayout.Label("Roll ID:", GUILayout.Width(50));
                    SpellDetail.reactionID = EditorGUILayout.IntField(SpellDetail.reactionID);
                    GUILayout.EndHorizontal();

                    // attack name / activate ragdoll
                    GUILayout.BeginHorizontal();
                    SpellDetail.activeRagdoll = EditorGUILayout.Toggle("Activate Ragdoll:", SpellDetail.activeRagdoll);
                    SpellDetail.resetAttackTrigger = EditorGUILayout.Toggle("Reset Attack Trigger:", SpellDetail.resetAttackTrigger);
                    GUILayout.EndHorizontal();
                }
            }
        }        

        /// <summary>
        /// Check attributes are present for sanity.
        /// </summary>
        protected virtual void CheckAttributes(SpellBook cc, int i)
        {
            // grabs its magic id
            HealthMessage = "";
            MagicID = 0;
            var vAttribMagicID = cc.itemListData.items[i].attributes.Find(ai => ai.name.ToString() == "MagicID");
            if (vAttribMagicID != null)
            {
                MagicID = vAttribMagicID.value;
            }
            if (MagicID == 0)
            {
                if (HealthMessage != "") HealthMessage += "\n";
                HealthMessage += "Missing MagicID Attribute";
            }

            // grabs its mana cost
            var vAttribManaCost = cc.itemListData.items[i].attributes.Find(ai => ai.name.ToString() == "ManaCost");
            if (vAttribManaCost != null)
            {
                ManaCost = vAttribManaCost.value;
            }
            if (ManaCost == 0)
            {
                if (HealthMessage != "") HealthMessage += "\n";
                HealthMessage += "Missing ManaCost Attribute";
            }            
        }

        /// <summary>
        /// Output the attribute in
        /// </summary>
        /// <param name="SubType"></param>
        protected virtual void DisplayAttributesInfo(SpellBookEntrySubType SubType)
        {
            if (SubType == SpellBookEntrySubType.Casting || SubType == SpellBookEntrySubType.Charge)
            {
                GUILayout.BeginHorizontal("box");
                EditorGUILayout.HelpBox(
                    "Magic ID   " + MagicID.ToString() + "\n" +
                    "Mana Cost " + (ManaCost == 0 ? "???" : ManaCost.ToString()),
                    (ManaCost == 0 ? MessageType.Warning : MessageType.Info));
                GUILayout.EndHorizontal();
            }
        }
                
        /// <summary>
        /// Load the input field defaults.
        /// </summary>
        protected virtual void LoadDefaults(SpellBook cc)
        {
            ChangesMade = -1;

            // animation clips
            cc.ClipSpellCast = AssetDatabase.LoadAssetAtPath("Assets/Invector-3rdPersonController/Add-ons/ShadesSpellSystem/Sauce/Standing_1H_Magic_Attack_01.fbx", typeof(AnimationClip)) as AnimationClip;
            cc.ClipSpellChargeInit = AssetDatabase.LoadAssetAtPath("Assets/Invector-3rdPersonController/Add-ons/ShadesSpellSystem/Sauce/Charge1.fbx", typeof(AnimationClip)) as AnimationClip;
            cc.ClipSpellChargeHold = AssetDatabase.LoadAssetAtPath("Assets/Invector-3rdPersonController/Add-ons/ShadesSpellSystem/Sauce/Charge2.fbx", typeof(AnimationClip)) as AnimationClip;
            cc.ClipSpellChargeRelease = AssetDatabase.LoadAssetAtPath("Assets/Invector-3rdPersonController/Add-ons/ShadesSpellSystem/Sauce/Charge3.fbx", typeof(AnimationClip)) as AnimationClip;
            cc.ClipSpawn = AssetDatabase.LoadAssetAtPath("Assets/Invector-3rdPersonController/Basic Locomotion/3D Models/Animations/Actions/hard_landing.fbx", typeof(AnimationClip)) as AnimationClip;
            cc.DefaultHandParticle = AssetDatabase.LoadAssetAtPath("Assets/Invector-3rdPersonController/Add-ons/ShadesSpellSystem/Spells/Custom Hand Effects/Hand Effect Fire.prefab", typeof(GameObject)) as GameObject;

            // only run once
            bLoaded = true;
        }

        /// <summary>
        /// Validate the input fields.
        /// </summary>
        /// <returns>Success when fields are filled out.</returns>
        protected virtual bool ValidateFields(SpellBook cc)
        {
            if (cc.ApplyTo.Count == 0)
            {
                validationMessage = "Please add an animator controller";
                return false;
            }
            else if (cc.ApplyTo.Count(a => a.Controller == null) > 0)
            {
                validationMessage = "Please specify an animator controller or remove from the list";
                return false;
            }
            else if (cc.ClipSpellCast == null)
            {
                validationMessage = "Please select the default cast clip";
                return false;
            }
            else if (cc.ClipSpellChargeInit == null)
            {
                validationMessage = "Please select the default charge init clip";
                return false;
            }
            else if (cc.ClipSpellChargeHold == null)
            {
                validationMessage = "Please select the default charge hold clip";
                return false;
            }
            else if (cc.ClipSpellChargeRelease == null)
            {
                validationMessage = "Please select the default charge release clip";
                return false;
            }
            else if (cc.ClipSpawn == null)
            {
                validationMessage = "Please select the default spawn clip";
                return false;
            }
            else if (cc.DefaultHandParticle == null)
            {
                validationMessage = "Please select the default hand particle clip";
                return false;
            }
            else if (cc.itemListData == null)
            {
                validationMessage = "Please select the invector inventory";
                return false;
            }

            // all good
            return true;
        }

        /// <summary>
        /// Input fields for animator controller validation.
        /// </summary>
        protected virtual void InputFields(SpellBook cc)
        {
            // add a new animator controller
            EditorGUILayout.LabelField("Animators", EditorStyles.boldLabel);
            if (GUILayout.Button("Add Controller"))
            {
                cc.ApplyTo.Add(new SpellBookApplyTo() { ClipSpawn = cc.ClipSpawn });
                cc.ApplyToExpanded = true;
            }

            // list of animators to validate
            if (!cc.ApplyToExpanded)
            {
                // expand
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(">>", GUILayout.Width(30)))
                {
                    cc.ApplyToExpanded = true;
                }

                // number of animator controllers
                EditorGUILayout.HelpBox(
                    "Found " + cc.ApplyTo.Count(a => a.Controller != null).ToString() + " out of " + cc.ApplyTo.Count.ToString() + " animator controller(s)\nready for validation", 
                    (cc.ApplyTo.Count(a => a.Controller == null) == 0 ? MessageType.Info : MessageType.Warning)
                );                
                GUILayout.EndHorizontal();
            }
            else  // show the full details
            {
                foreach (SpellBookApplyTo anim in cc.ApplyTo)
                {
                    GUILayout.BeginVertical("box");
                    GUILayout.BeginHorizontal();
                    var NewAnimatorController = EditorGUILayout.ObjectField("Controller: ", anim.Controller, typeof(AnimatorController), false) as AnimatorController;
                    if (anim.Controller != NewAnimatorController)
                    {
                        // reset settings
                        anim.Controller = NewAnimatorController;
                        anim.MagicLayerFixedIndex = 0;
                        anim.MagicLayerMoveIndex = 0;
                        anim.AllLayerNames = new List<string>();
                        ChangesMade = -1;

                        // build list of layers
                        if (anim.Controller != null)
                        {
                            for (int layer = 0; layer < anim.Controller.layers.Length; layer++)
                            {
                                anim.AllLayerNames.Add(anim.Controller.layers[layer].name);
                                if (anim.Controller.layers[layer].name == "FullBody") anim.MagicLayerFixedIndex = layer;
                                else if (anim.Controller.layers[layer].name == "UpperBody") anim.MagicLayerMoveIndex = layer;
                            }
                        }
                    }
                    if (GUILayout.Button("X", GUILayout.Width(30)))
                    {
                        cc.ApplyTo.Remove(anim);
                        break;
                    }
                    GUILayout.EndHorizontal();

                    // allow choice of layers
                    if (anim.Controller != null)
                    {
                        EditorGUILayout.LabelField("Layers", EditorStyles.boldLabel);
                        anim.MagicLayerFixedIndex = EditorGUILayout.Popup("Full Body :", anim.MagicLayerFixedIndex, anim.AllLayerNames.ToArray());
                        if (!anim.Controller.layers[anim.MagicLayerFixedIndex].iKPass)
                        {
                            EditorGUILayout.HelpBox("IK needs setting on this layer!", MessageType.Error);
                        }
                        anim.MagicLayerMoveIndex = EditorGUILayout.Popup("Upper Body :", anim.MagicLayerMoveIndex, anim.AllLayerNames.ToArray());
                        if (!anim.Controller.layers[anim.MagicLayerMoveIndex].iKPass)
                        {
                            EditorGUILayout.HelpBox("IK needs setting on this layer!", MessageType.Error);
                        }
                    }

                    // spawn clip
                    EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
                    anim.ClipSpawn = EditorGUILayout.ObjectField("Spawn: ", anim.ClipSpawn, typeof(AnimationClip), false) as AnimationClip;

                    // don't overwrite clips  (for non humans)
                    GUILayout.BeginHorizontal();
                    anim.DontSetAnimationClips = EditorGUILayout.Toggle("Do NOT Set Clips: ", anim.DontSetAnimationClips);

                    // write reflect skill inputs
                    anim.IncludeReflectSkills = EditorGUILayout.Toggle("Reflect Skills: ", anim.IncludeReflectSkills);
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                }

                // contract to short detail
                if (GUILayout.Button("<<"))
                {
                    cc.ApplyToExpanded = false;
                }
            }

            // defaults
            EditorGUILayout.LabelField("Defaults", EditorStyles.boldLabel);
            cc.ClipSpawn = EditorGUILayout.ObjectField("Spawn: ", cc.ClipSpawn, typeof(AnimationClip), false) as AnimationClip;
            cc.ClipSpellCast = EditorGUILayout.ObjectField("Spell Cast: ", cc.ClipSpellCast, typeof(AnimationClip), false) as AnimationClip;
            cc.ClipSpellChargeInit = EditorGUILayout.ObjectField("Charge Init: ", cc.ClipSpellChargeInit, typeof(AnimationClip), false) as AnimationClip;
            cc.ClipSpellChargeHold = EditorGUILayout.ObjectField("Charge Hold: ", cc.ClipSpellChargeHold, typeof(AnimationClip), false) as AnimationClip;
            cc.ClipSpellChargeRelease = EditorGUILayout.ObjectField("Charge Release: ", cc.ClipSpellChargeRelease, typeof(AnimationClip), false) as AnimationClip;
            cc.DefaultHandParticle = EditorGUILayout.ObjectField("Hand Particle: ", cc.DefaultHandParticle, typeof(GameObject), false) as GameObject;
        }

        /// <summary>
        /// Validate the current animator controller
        /// </summary>
        public virtual void ValidateController(SpellBook cc, SpellBookApplyTo anim)
        {
            ChangesMade = 0;

            // birth started trigger
            if (anim.Controller.parameters.Count(a => a.name == "BirthStarted") == 0)
            {
                anim.Controller.AddParameter("BirthStarted", AnimatorControllerParameterType.Trigger);
                ChangesMade += 1;
            }
            
            // birth started state
            ChangesMade += ValidateState(
                anim.Controller.layers[0].stateMachine, anim.Controller.layers[0].stateMachine,
                "I am born", anim.ClipSpawn, anim.DontSetAnimationClips, false, 1f, false, 0f,
                true, null, new AnimatorCondition[] {
                    new AnimatorCondition() { parameter = "BirthStarted", mode = AnimatorConditionMode.If, threshold = 0 }
                },
                null, FindStateMachine("Locomotion", anim.Controller.layers[0].stateMachine), null, null, false, true);


            // magic attack triggers and ID
            if (anim.Controller.parameters.Count(a => a.name == "MagicAttack") == 0)
            {
                anim.Controller.AddParameter("MagicAttack", AnimatorControllerParameterType.Trigger);
                ChangesMade += 1;
            }
            if (anim.Controller.parameters.Count(a => a.name == "MagicCharge") == 0)
            {
                anim.Controller.AddParameter("MagicCharge", AnimatorControllerParameterType.Trigger);
                ChangesMade += 1;
            }
            if (anim.Controller.parameters.Count(a => a.name == "MagicID") == 0)
            {
                anim.Controller.AddParameter("MagicID", AnimatorControllerParameterType.Int);
                ChangesMade += 1;
            }


            // check layers have IK
            anim.Controller.layers[anim.MagicLayerFixedIndex].iKPass = true;
            anim.Controller.layers[anim.MagicLayerMoveIndex].iKPass = true;
            

            // magic container state machine on full body            
            AnimatorStateMachine MagicContainerFixed = FindStateMachine("Magic", anim.Controller.layers[anim.MagicLayerFixedIndex].stateMachine);
            if (MagicContainerFixed == null)
            {
                MagicContainerFixed = anim.Controller.layers[anim.MagicLayerFixedIndex].stateMachine.AddStateMachine("Magic");
                ChangesMade += 1;
            }
            TidyStateMachineChildren(MagicContainerFixed);


            // magic container state machine on upper body
            AnimatorStateMachine MagicContainerMove = FindStateMachine("Magic", anim.Controller.layers[anim.MagicLayerMoveIndex].stateMachine);
            if (MagicContainerMove == null)
            {
                MagicContainerMove = anim.Controller.layers[anim.MagicLayerMoveIndex].stateMachine.AddStateMachine("Magic");
                ChangesMade += 1;
            }
            TidyStateMachineChildren(MagicContainerMove);

            // find destination magic null state on full body
            AnimatorStateMachine AttacksContainer = FindStateMachine("Attacks", anim.Controller.layers[anim.MagicLayerFixedIndex].stateMachine);
            AnimatorStateMachine NullFixed = null;
            if (AttacksContainer != null)
            {
                NullFixed = FindStateMachine("Null", AttacksContainer);
                if (NullFixed == null) // not found
                {
                    Console.Write("CRITICAL ERROR, attacks->null state on fixed casting layer is MISSING!");
                    return;
                }
            }
            else  // not found
            {
                Console.Write("CRITICAL ERROR, attacks state on fixed casting layer is MISSING!");
                return;
            }

            // find destination magic null state on upper body
            AnimatorStateMachine NullMove = FindStateMachine("Null", anim.Controller.layers[anim.MagicLayerMoveIndex].stateMachine);
            if (NullMove == null) // not found
            {
                Console.Write("CRITICAL ERROR, null state on move casting layer is MISSING!");
                return;
            }

            // check all magic spells are inside the state
            foreach (vItem vi in cc.itemListData.items)
            {
                // only interested in the spells
                if (vi.type == vItemType.Spell)
                {
                    // magic ID is not optional
                    var vAttribMagicID = vi.attributes.Find(ai => ai.name.ToString() == "MagicID");
                    if (vAttribMagicID != null)
                    {
                        // find the spell options
                        SpellBookListEntry SpellDetail = cc.Spells.Find(s => s.MagicID == vAttribMagicID.value);
                        if (SpellDetail != null)
                        {
                            // melee attack behavior
                            vMeleeAttackControl MeleeAttack = null;
                            if (SpellDetail.MeleeHand != SpellBookHands.None)
                            {
                                var MeleeBodyParts = new List<string>();
                                if (SpellDetail.MeleeHand == SpellBookHands.Left || SpellDetail.MeleeHand == SpellBookHands.Both)
                                {
                                    MeleeBodyParts.Add("LeftLowerArm");
                                }
                                if (SpellDetail.MeleeHand == SpellBookHands.Right || SpellDetail.MeleeHand == SpellBookHands.Both)
                                {
                                    MeleeBodyParts.Add("RightLowerArm");
                                }

                                MeleeAttack = CreateInstance<vMeleeAttackControl>();
                                MeleeAttack.startDamage = SpellDetail.startDamage;
                                MeleeAttack.endDamage = SpellDetail.endDamage;
                                MeleeAttack.activeRagdoll = SpellDetail.activeRagdoll;
                                MeleeAttack.attackName = SpellDetail.attackName;
                                MeleeAttack.bodyParts = MeleeBodyParts;
                                MeleeAttack.damageMultiplier = SpellDetail.damageMultiplier;
                                MeleeAttack.ignoreDefense = SpellDetail.ignoreDefense;
                                MeleeAttack.meleeAttackType = SpellDetail.meleeAttackType;
                                MeleeAttack.reactionID = SpellDetail.reactionID;
                                MeleeAttack.recoilID = SpellDetail.recoilID;
                                MeleeAttack.resetAttackTrigger = SpellDetail.resetAttackTrigger;
                            }

                            // allow movement whilst casting?
                            AnimatorStateMachine Parent = (SpellDetail.AllowMovement ? MagicContainerMove : MagicContainerFixed);
                            AnimatorStateMachine Destination = (SpellDetail.AllowMovement ? NullMove : NullFixed);

                            // validate the spell in the animator
                            if (SpellDetail.Charge)
                            {
                                // check this spell hasn't just changed between move and fixed layers
                                AnimatorStateMachine ParentInverted = (SpellDetail.AllowMovement ? MagicContainerFixed : MagicContainerMove);
                                AnimatorStateMachine removeme = FindStateMachine(vi.name + " (" + vAttribMagicID.value + ")", ParentInverted);
                                if (removeme != null)
                                {
                                    ParentInverted.RemoveStateMachine(removeme);
                                }

                                // check this spell hasn't just changed from non charged
                                AnimatorState removemeCharge = FindState(vi.name + " (" + vAttribMagicID.value + ")", Parent);
                                if (removemeCharge != null)
                                {
                                    Parent.RemoveState(removemeCharge);
                                }
                                removemeCharge = FindState(vi.name + " (" + vAttribMagicID.value + ")", ParentInverted);
                                if (removemeCharge != null)
                                {
                                    ParentInverted.RemoveState(removemeCharge);
                                }

                                // check parent state exists
                                ChangesMade += ValidateStateMachine(Parent, vi.name + " (" + vAttribMagicID.value + ")");

                                // update parent
                                Parent = FindStateMachine(vi.name + " (" + vAttribMagicID.value + ")", Parent);

                                // check charge init state
                                ChangesMade += ValidateState(
                                    Parent, anim.Controller.layers[(SpellDetail.AllowMovement ? anim.MagicLayerMoveIndex : anim.MagicLayerFixedIndex)].stateMachine,
                                    vi.name + " Charge", SpellDetail.ClipSpellChargeInit, anim.DontSetAnimationClips, 
                                    SpellDetail.MirrorCharge, SpellDetail.SpeedCharge, SpellDetail.FootIKCharge, SpellDetail.CycleOffsetCharge,
                                    true, null, new AnimatorCondition[]
                                    {
                                        new AnimatorCondition() { parameter = "MagicCharge", mode = AnimatorConditionMode.If, threshold = 0 },
                                        new AnimatorCondition() { parameter = "MagicID", mode = AnimatorConditionMode.Equals, threshold = vAttribMagicID.value }
                                    },
                                    null, null, SpellDetail.SpellOptionsCharge, null, false, false
                                );

                                // check charge hold state
                                ChangesMade += ValidateState(
                                    Parent, anim.Controller.layers[(SpellDetail.AllowMovement ? anim.MagicLayerMoveIndex : anim.MagicLayerFixedIndex)].stateMachine,
                                    vi.name + " Hold", SpellDetail.ClipSpellChargeHold, anim.DontSetAnimationClips, 
                                    SpellDetail.MirrorHold, SpellDetail.SpeedHold, SpellDetail.FootIKHold, SpellDetail.CycleOffsetHold,
                                    false, FindState(vi.name + " Charge", Parent), null,
                                    null, null, SpellDetail.SpellOptionsHold, null, true, false
                                );

                                // check charge init state
                                ChangesMade += ValidateState(
                                    Parent, anim.Controller.layers[(SpellDetail.AllowMovement ? anim.MagicLayerMoveIndex : anim.MagicLayerFixedIndex)].stateMachine,
                                    vi.name + " Release", SpellDetail.ClipSpellChargeRelease, anim.DontSetAnimationClips, 
                                    SpellDetail.MirrorRelease, SpellDetail.SpeedRelease, SpellDetail.FootIKRelease, SpellDetail.CycleOffsetRelease,
                                    true, null, new AnimatorCondition[]
                                    {
                                        new AnimatorCondition() { parameter = "MagicAttack", mode = AnimatorConditionMode.If, threshold = 0 },
                                        new AnimatorCondition() { parameter = "MagicID", mode = AnimatorConditionMode.Equals, threshold = vAttribMagicID.value }
                                    },
                                    null, Destination, SpellDetail.SpellOptionsRelease, null, false, true
                                );
                            }
                            else  // normal non charge spell
                            {
                                // check this spell hasn't just changed between move and fixed layers
                                AnimatorStateMachine ParentInverted = (SpellDetail.AllowMovement ? MagicContainerFixed : MagicContainerMove);
                                AnimatorState removeme = FindState(vi.name + " (" + vAttribMagicID.value + ")", ParentInverted);
                                if (removeme != null)
                                {
                                    ParentInverted.RemoveState(removeme);
                                }

                                // check this spell hasn't just changed from charged
                                AnimatorStateMachine removemeCharge = FindStateMachine(vi.name + " (" + vAttribMagicID.value + ")", Parent);
                                if (removemeCharge != null)
                                {
                                    Parent.RemoveStateMachine(removemeCharge);
                                }
                                removemeCharge = FindStateMachine(vi.name + " (" + vAttribMagicID.value + ")", ParentInverted);
                                if (removemeCharge != null)
                                {
                                    ParentInverted.RemoveStateMachine(removemeCharge);
                                }

                                // create if state doesn't exist
                                ChangesMade += ValidateState(
                                    Parent, anim.Controller.layers[(SpellDetail.AllowMovement ? anim.MagicLayerMoveIndex : anim.MagicLayerFixedIndex)].stateMachine,
                                    vi.name + " (" + vAttribMagicID.value + ")", SpellDetail.ClipSpellCast, anim.DontSetAnimationClips, 
                                    SpellDetail.MirrorCast, SpellDetail.SpeedCast, SpellDetail.FootIKCast, SpellDetail.CycleOffsetCast,
                                    true, null, new AnimatorCondition[] 
                                    {
                                        new AnimatorCondition() { parameter = "MagicAttack", mode = AnimatorConditionMode.If, threshold = 0 },
                                        new AnimatorCondition() { parameter = "MagicID", mode = AnimatorConditionMode.Equals, threshold = vAttribMagicID.value }
                                    },
                                    null, Destination, SpellDetail.SpellOptions, MeleeAttack, false, true
                                );                                                               
                            }
                        }
                    }
                }
            }


            //// waypoints
            //if (cc.IncludeWaypointActions)
            //{
            //    if (cc.TheAnimatorController.parameters.Count(a => a.name == "WaypointActionSet") == 0)
            //    {
            //        cc.TheAnimatorController.AddParameter("WaypointActionSet", AnimatorControllerParameterType.Int);
            //        ChangesMade += 1;
            //    }
            //    if (cc.TheAnimatorController.parameters.Count(a => a.name == "WaypointActionStart") == 0)
            //    {
            //        cc.TheAnimatorController.AddParameter("WaypointActionStart", AnimatorControllerParameterType.Trigger);
            //        ChangesMade += 1;
            //    }
            //    if (cc.TheAnimatorController.parameters.Count(a => a.name == "WaypointID") == 0)
            //    {
            //        cc.TheAnimatorController.AddParameter("WaypointID", AnimatorControllerParameterType.Int);
            //        ChangesMade += 1;
            //    }
            //}

            // skills
            if (anim.IncludeReflectSkills)
            {
                if (anim.Controller.parameters.Count(a => a.name == "Core_Level") == 0)
                {
                    anim.Controller.AddParameter("Core_Level", AnimatorControllerParameterType.Int);
                    ChangesMade += 1;
                }
                if (anim.Controller.parameters.Count(a => a.name == "Core_Life") == 0)
                {
                    anim.Controller.AddParameter("Core_Life", AnimatorControllerParameterType.Int);
                    ChangesMade += 1;
                }
                if (anim.Controller.parameters.Count(a => a.name == "Core_Mana") == 0)
                {
                    anim.Controller.AddParameter("Core_Mana", AnimatorControllerParameterType.Int);
                    ChangesMade += 1;
                }
                if (anim.Controller.parameters.Count(a => a.name == "Core_Stamina") == 0)
                {
                    anim.Controller.AddParameter("Core_Stamina", AnimatorControllerParameterType.Int);
                    ChangesMade += 1;
                }
                if (anim.Controller.parameters.Count(a => a.name == "Core_EquipLoad") == 0)
                {
                    anim.Controller.AddParameter("Core_EquipLoad", AnimatorControllerParameterType.Int);
                    ChangesMade += 1;
                }
                if (anim.Controller.parameters.Count(a => a.name == "Core_Axis") == 0)
                {
                    anim.Controller.AddParameter("Core_Axis", AnimatorControllerParameterType.Int);
                    ChangesMade += 1;
                }
                if (anim.Controller.parameters.Count(a => a.name == "Core_Alignment") == 0)
                {
                    anim.Controller.AddParameter("Core_Alignment", AnimatorControllerParameterType.Int);
                    ChangesMade += 1;
                }
                if (anim.Controller.parameters.Count(a => a.name == "Core_Race") == 0)
                {
                    anim.Controller.AddParameter("Core_Race", AnimatorControllerParameterType.Int);
                    ChangesMade += 1;
                }
                if (anim.Controller.parameters.Count(a => a.name == "Core_Rank") == 0)
                {
                    anim.Controller.AddParameter("Core_Rank", AnimatorControllerParameterType.Int);
                    ChangesMade += 1;
                }

                // dynamic skills
                string[] SkillNames = Enum.GetNames(typeof(BaseSkill));
                for (int i = 0; i < SkillNames.Length; i++)
                {
                    if (anim.Controller.parameters.Count(a => a.name == "Core_" + SkillNames[i]) == 0)
                    {
                        anim.Controller.AddParameter("Core_" + SkillNames[i], AnimatorControllerParameterType.Int);
                        ChangesMade += 1;
                    }
                }
            }
        }

        /// <summary>
        /// Check the state exists and has the correct transitions, motion and entry conditions.
        /// </summary>
        /// <param name="Parent">Parent state machine to check for the state.</param>
        /// <param name="StateName">Name of the state to check.</param>
        /// <param name="TheClip">Motion clip to apply.</param>
        /// <param name="DontApplyClip">Set don't apply when applying to a non human creature (but u have to manually set the animations).</param>
        /// <param name="SourceAnyState">Entry transition is from any state.</param>
        /// <param name="SourceState">Source state for the entry transition, null if none.</param>
        /// <param name="EntryConditions">Conditions to enter, null if none.</param>
        /// <param name="DestinationState">Destination state for the exit transition, null if none.</param>
        /// <param name="DestinationStateMachine">Destination state machine for the exit transition, null if none.</param>
        /// <returns>Count of the number of changes applied.</returns>
        protected int ValidateState(
            AnimatorStateMachine Parent, AnimatorStateMachine Root,
            string StateName, AnimationClip TheClip, 
            bool DontApplyClip, bool Mirror, float Speed, bool footIK, float cycleOffset,
            bool SourceAnyState, AnimatorState SourceState, 
            AnimatorCondition[] EntryConditions, 
            AnimatorState DestinationState, AnimatorStateMachine DestinationStateMachine, 
            SpellBookEntry SpellOptions, vMeleeAttackControl MeleeAttackBehavior,
            bool EntryHasExitTime, bool ExitHasExitTime)
        {
            // create the state if it doesn't exist
            int StateChangesMade = 0;
            AnimatorState TheState = FindState(StateName, Parent);
            if (TheState == null)
            {
                // state not found
                TheState = Parent.AddState(StateName);
                StateChangesMade += 1;
            }

                       
            // apply the clip if required
            if (TheState.motion != TheClip)
            {
                // motion clip is different
                if (!DontApplyClip)  // update the animation
                {
                    // update the clip
                    TheState.motion = TheClip;
                    TheState.mirror = Mirror;
                    TheState.speed = Speed;
                    TheState.iKOnFeet = footIK;
                    TheState.cycleOffset = cycleOffset;
                    StateChangesMade += 1;
                }
            }

            // check spell options
            if (SpellOptions != null)
            {
                AddSpellBookAttack(TheState, SpellOptions);
            }

            // remove existing melee attack behavior if any
            if (TheState.behaviours.Count(b => b.GetType() == typeof(vMeleeAttackControl)) == 0)
            {
                TheState.behaviours = TheState.behaviours.Where(b => b.GetType() != typeof(vMeleeAttackControl)).ToArray();
            }

            // add melee attack behavior if selected
            if (MeleeAttackBehavior != null)
            {
                vMeleeAttackControl NewMeleeAttack = (vMeleeAttackControl)TheState.AddStateMachineBehaviour(typeof(vMeleeAttackControl));
                //GlobalFuncs.DuckCopyShallow(NewMeleeAttack, MeleeAttackBehavior);  // this caused weirdness like adding the behavior onto the animator in the project view, replaced with manual copy
                NewMeleeAttack.activeRagdoll = MeleeAttackBehavior.activeRagdoll;
                NewMeleeAttack.allowMovementAt = MeleeAttackBehavior.allowMovementAt;
                NewMeleeAttack.attackName = MeleeAttackBehavior.attackName;
                NewMeleeAttack.bodyParts = MeleeAttackBehavior.bodyParts;
                NewMeleeAttack.damageMultiplier = MeleeAttackBehavior.damageMultiplier;
                NewMeleeAttack.endDamage = MeleeAttackBehavior.endDamage;
                NewMeleeAttack.meleeAttackType = MeleeAttackBehavior.meleeAttackType;
                NewMeleeAttack.reactionID = MeleeAttackBehavior.reactionID;
                NewMeleeAttack.recoilID = MeleeAttackBehavior.recoilID;
                NewMeleeAttack.resetAttackTrigger = MeleeAttackBehavior.resetAttackTrigger;
                NewMeleeAttack.startDamage = MeleeAttackBehavior.startDamage;
            }


            // check the source transition
            AnimatorStateTransition Entry = null;
            if (SourceAnyState)
            {
                // check the transition
                if (Root.anyStateTransitions.Count(t => t.destinationState == TheState) == 0)
                {
                    // not found add it
                    Root.AddAnyStateTransition(TheState);
                    StateChangesMade += 1;
                }

                // cache reference
                Entry = Root.anyStateTransitions.FirstOrDefault(t => t.destinationState == TheState);
            }
            else if (SourceState != null)
            {
                // check the transition
                if (SourceState.transitions.Count(t => t.destinationState == TheState) == 0)
                {
                    // not found add it
                    SourceState.AddTransition(TheState);
                    StateChangesMade += 1;
                }

                // cache reference
                Entry = SourceState.transitions.FirstOrDefault(t => t.destinationState == TheState);
            }

            // check entry transition parameters
            if (Entry != null)
            {
                // check conditions
                if (EntryConditions != null)
                {
                    // do the two arrays match
                    if (!Entry.conditions.SequenceEqual(EntryConditions))
                    {
                        // overwrite the conditions
                        Entry.conditions = EntryConditions;
                        StateChangesMade += 1;
                    }
                }

                // set exit time on the transition
                Entry.hasExitTime = EntryHasExitTime;
            }

            // check destination transition
            AnimatorStateTransition Exit = null;
            if (DestinationState != null)
            {
                // check the transition
                if (TheState.transitions.Count(t => t.destinationState == DestinationState) == 0)
                {
                    // not found add it
                    Exit = TheState.AddTransition(DestinationState);
                    StateChangesMade += 1;
                }
            }
            else if (DestinationStateMachine != null)
            {
                // check the transition
                if (TheState.transitions.Count(t => t.destinationStateMachine == DestinationStateMachine) == 0)
                {
                    // not found add it
                    Exit = TheState.AddTransition(DestinationStateMachine);
                    StateChangesMade += 1;
                }
            }

            // check the exit transitions conditions
            if (Exit != null)
            {
                Exit.hasExitTime = ExitHasExitTime;
            }

            // report changes applied
            return StateChangesMade;
        }

        /// <summary>
        /// Check the state machine exists.
        /// </summary>
        /// <param name="Parent">Parent state machine to check for the state.</param>
        /// <param name="StateName">Name of the state to check.</param>
        /// <returns>Count of the number of changes applied.</returns>
        protected int ValidateStateMachine(
            AnimatorStateMachine Parent, string StateMachineName)
        {
            // create the state if it doesn't exist
            int StateChangesMade = 0;
            AnimatorStateMachine TheStateMachine = FindStateMachine(StateMachineName, Parent);
            if (TheStateMachine == null)
            {
                // state not found
                TheStateMachine = Parent.AddStateMachine(StateMachineName);
                StateChangesMade += 1;
            }
            
            // report changes applied
            return StateChangesMade;
        }

        /// <summary>
        /// Find a state child on a state machine.
        /// </summary>
        /// <param name="StateName">Name of the state child to find.</param>
        /// <param name="Parent">Parent state machine to search for the state child.</param>
        /// <returns>State or null if not found.</returns>
        protected AnimatorState FindState(string StateName, AnimatorStateMachine Parent)
        {
            return Parent.states.FirstOrDefault(s => s.state.name == StateName).state;
        }

        /// <summary>
        /// Find a state machine child on a state machine.
        /// </summary>
        /// <param name="StateName">Name of the state machine child to find.</param>
        /// <param name="Parent">Parent state machine to search for the state machine child.</param>
        /// <returns>State machine or null if not found.</returns>
        protected AnimatorStateMachine FindStateMachine(string StateMachineName, AnimatorStateMachine Parent)
        {
            return Parent.stateMachines.FirstOrDefault(s => s.stateMachine.name == StateMachineName).stateMachine;
        }

        /// <summary>
        /// Tidy the states on a state machine.
        /// </summary>
        /// <param name="Parent">Parent state machine to tidy.</param>
        protected void TidyStateMachineChildren(AnimatorStateMachine Parent)
        {
            // define entry and exit positions
            Vector3 LeftPosition = new Vector3(-250, 0, 0);
            Vector3 RightPosition = new Vector3(1000, 0, 0);

            Parent.entryPosition = LeftPosition; LeftPosition.y += 100;
            Parent.anyStatePosition = LeftPosition;
            Parent.exitPosition = RightPosition; RightPosition.y += 100;
            Parent.parentStateMachinePosition = RightPosition;

            return;
            // setup columns
            const float SEPERATION = 100f;
            Vector3 col1 = new Vector3(0, 0, 0);
            Vector3 col2 = new Vector3(SEPERATION * 2, 0, 0);
            Vector3 col3 = new Vector3(SEPERATION * 4, 0, 0);
            Vector3 col4 = new Vector3(SEPERATION * 6, 0, 0);
            Vector3 col5 = new Vector3(SEPERATION * 6, 0, 0);
            var StatesProcessed = new List<AnimatorState>();
            //var StateMachinesProcessed = new List<AnimatorStateMachine>();

            // align the entry states in column 1
            Parent.entryPosition = col1; col1.y += SEPERATION;
            Parent.anyStatePosition = col1; col1.y += SEPERATION;
            Parent.exitPosition = col5; col5.y += SEPERATION;

            //// move all states with an entry transition to column 2
            //foreach (AnimatorTransition transition in Parent.entryTransitions)
            //{
            //    if (transition.destinationState != null)
            //    {
            //        var s = Parent.states.FirstOrDefault(state => state.state == transition.destinationState);
            //        s.position = col2; col2.y += SEPERATION;
            //        StatesProcessed.Add(transition.destinationState);
            //    } 
            //    else if (transition.destinationStateMachine != null)
            //    {
            //        var s = Parent.stateMachines.FirstOrDefault(state => state.stateMachine == transition.destinationStateMachine);
            //        s.position = col2; col2.y += SEPERATION;
            //        StateMachinesProcessed.Add(transition.destinationStateMachine);
            //    }
            //}

            //// move all states with an any transition to column 2
            //foreach (AnimatorStateTransition transition in Parent.anyStateTransitions)
            //{
            //    if (transition.destinationState != null)
            //    {
            //        var s = Parent.states.FirstOrDefault(state => state.state == transition.destinationState);
            //        s.position = col2; col2.y += SEPERATION;
            //        StatesProcessed.Add(transition.destinationState);
            //    }
            //    else if (transition.destinationStateMachine != null)
            //    {
            //        var s = Parent.stateMachines.FirstOrDefault(state => state.stateMachine == transition.destinationStateMachine);
            //        s.position = col2; col2.y += SEPERATION;
            //        StateMachinesProcessed.Add(transition.destinationStateMachine);
            //    }
            //}

            // move state machines to column2
            //foreach (ChildAnimatorStateMachine child in Parent.stateMachines)
            //{
            //    var s = child; s.position = col2; col2.y += SEPERATION;
            //}
            for (int i = 0; i < Parent.stateMachines.Length; i++)
            {
                Parent.stateMachines[i].position = col2;
                col2.y += SEPERATION;
            }

            //// move states with transitions to column3
            //foreach (ChildAnimatorState child in Parent.states)
            //{
            //    if (child.state.transitions.Length > 0)
            //    {
            //        var s = child; s.position = col3; col3.y += SEPERATION;
            //        StatesProcessed.Add(child.state);
            //    }
            //}

            //// move remaining states to column4
            //foreach (ChildAnimatorState child in Parent.states)
            //{
            //    if (!StatesProcessed.Contains(child.state))
            //    {
            //        var s = child; s.position = col4; col4.y += SEPERATION;
            //    }
            //}

            //// move remaining state machines to column3
            //foreach (ChildAnimatorStateMachine child in Parent.stateMachines)
            //{
            //    if (!StateMachinesProcessed.Contains(child.stateMachine))
            //    {
            //        var s = child; s.position = col3; col3.y += SEPERATION;
            //    }
            //}

            // move exits to col 5
            //Parent.parentStateMachinePosition = col5; col5.y += SEPERATION;
                  
        }

        /// <summary>
        /// Add the spell book attack script if not found and sync the options.
        /// </summary>
        /// <param name="TheState">State to check.</param>
        /// <param name="SpellOptions">Options to sync.</param>
        protected virtual void AddSpellBookAttack(AnimatorState TheState, SpellBookEntry SpellOptions)
        {
            // check behavior exists
            if (TheState.behaviours.Count(b => b.GetType() == typeof(SpellBookAttack)) == 0)
            {
                TheState.AddStateMachineBehaviour(typeof(SpellBookAttack));
            }

            // sync the options
            SpellBookAttack sb = (SpellBookAttack)TheState.behaviours.FirstOrDefault(b => b.GetType() == typeof(SpellBookAttack));
            if (sb.SpellOptions == null) sb.SpellOptions = new SpellBookEntry();
            GlobalFuncs.DuckCopyShallow(sb.SpellOptions, SpellOptions);
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
