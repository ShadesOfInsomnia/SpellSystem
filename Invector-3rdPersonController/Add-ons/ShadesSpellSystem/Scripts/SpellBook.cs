using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector;
using Invector.vItemManager;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif
using Invector.vMelee;

namespace Shadex
{
    /// <summary>
    /// Spell book master for the prefab, used to apply a common set of spell options across multiple animators.
    /// </summary>
    [vClassHeader("SPELL BOOK", iconName = "controllerIcon")]
    public class SpellBook : vMonoBehaviour
    {
        /// <summary>Linked item list data to sync with the animators.</summary>
        public vItemListData itemListData;

        /// <summary>List of animators/options to validate.</summary>
        public List<SpellBookApplyTo> ApplyTo = new List<SpellBookApplyTo>();

        /// <summary>Collapse the panel for the apply to list of animators.</summary>
        public bool ApplyToExpanded = true;

        /// <summary>Default animation spawn clip.</summary>
        public AnimationClip ClipSpawn;

        /// <summary>Default animation cast clip.</summary>
        public AnimationClip ClipSpellCast;

        /// <summary>Default animation charge init clip.</summary>
        public AnimationClip ClipSpellChargeInit;

        /// <summary>Default animation charge hold clip.</summary>
        public AnimationClip ClipSpellChargeHold;

        /// <summary>Default animation charge release clip.</summary>
        public AnimationClip ClipSpellChargeRelease;

        /// <summary>Default particle prefab for the left hand.</summary>
        public GameObject DefaultHandParticle;

        /// <summary>List of all spells from the inventory with prefabs, spawn options.</summary>
        public List<SpellBookListEntry> Spells = new List<SpellBookListEntry>();

        /// <summary>Filter the spells by damage type.</summary>
        public bool DamageFilterEnabled;

        /// <summary>Index of the filter.</summary>
        public BaseDamage DamageFilterIndex;
    }

    /// <summary>
    /// The actual spell book entry also used on the animator states.
    /// </summary>
    [Serializable]
    public class SpellBookEntry 
    {                
        /// <summary>Prefab of the particle system to attach to the limb.</summary>
        public GameObject LimbParticleEffect;

        /// <summary>The limb that the LimbParticleEffect should follow.</summary>
        public AvatarIKGoal attackLimb;

        /// <summary>(Optional) The 2nd Prefab of the particle system to attach to second limb.</summary>
        public GameObject LimbParticleEffect2;

        /// <summary>(Optional) The 2nd limb that the LimbParticleEffect should follow, if a one handed spell, set to the same limb as above.</summary>
        public AvatarIKGoal attackLimb2;

        /// <summary>Force no pooling of the limb effect, useful if the limb particles wont be reused in the scene.</summary>
        public bool DoNotPoolLimbParticles;

        /// <summary>For the charge animator state, destroys all spawned on state exit.</summary>
        public bool ChargeState;

        /// <summary>List of all prefabs to spawn within the animator time frame.</summary>
        public List<SpawnerOptionsOverTime> SpawnOverTime = new List<SpawnerOptionsOverTime>();

        /// <summary>Sub type, for display alterations, know which type of cast thyself is.</summary>
        public SpellBookEntrySubType SubType;

        /// <summary>Icon for the spell from the inventory.</summary>
        public Sprite Icon;

        /// <summary>Name of the spell from the inventory.</summary>
        public string SpellName;

    }

    /// <summary>
    /// Used to determine which of the spell options to create a state from.
    /// </summary>
    public enum SpellBookEntrySubType
    {
        Casting, Charge, Hold, Release
    }

    /// <summary>
    /// Which hands (or none) to attach hand particles to.
    /// </summary>
    public enum SpellBookHands
    {
        None, Left, Right, Both
    }


    /// <summary>
    /// List entry with settings regarding the animators to update.
    /// </summary>
    [Serializable]
    public class SpellBookApplyTo
    {
#if UNITY_EDITOR
        /// <summary>Animation controller to update. </summary>       
        public AnimatorController Controller;
#endif
        /// <summary>Default animation spawn clip.</summary>
        public AnimationClip ClipSpawn;

        /// <summary>Apply the spell book animations to the controller.</summary>
        public bool DontSetAnimationClips;

        /// <summary>Index of the layer to add the magic to aka full body.</summary>
        public int MagicLayerFixedIndex;

        /// <summary>Index of the layer to add the magic to aka full body.</summary>
        public int MagicLayerMoveIndex;

        /// <summary>List of the current animator layer names.</summary>
        public List<string> AllLayerNames;

        /// <summary>Include reflect leveling system skills to the animator.</summary>
        public bool IncludeReflectSkills = true;

        /// <summary>Include reflect leveling system skills to the animator.</summary>
        public bool DedicatedFullBodyLayer;
    }

    /// <summary>
    /// Information about the spell book entry used in the custom editor class.
    /// </summary>
    [Serializable]
    public class SpellBookListEntry 
    {
        /// <summary>Magic ID in the inventory that this entry links to.</summary>
        public int MagicID;

        /// <summary>Is this spell entry expanded.</summary>
        public bool Expanded;
        
        /// <summary>Is this spell chargeable.</summary>
        public bool Charge;

        /// <summary>Allow character movement whilst casting, affects which layer the spell state gets created in.</summary>
        public bool AllowMovement;
        

        /// <summary>Animation cast clip.</summary>
        public AnimationClip ClipSpellCast;

        /// <summary>Speed of the cast animation.</summary>
        public float SpeedCast;

        /// <summary>Mirror the cast animation.</summary>
        public bool MirrorCast;

        /// <summaryAdd foot ik to the cast animation.</summary>
        public bool FootIKCast;

        /// <summary>Cycle offset for the cast animation.</summary>
        public float CycleOffsetCast;


        /// <summary>Animation charge init clip.</summary>
        public AnimationClip ClipSpellChargeInit;

        /// <summary>Speed of the cast animation.</summary>
        public float SpeedCharge;

        /// <summary>Mirror the cast animation.</summary>
        public bool MirrorCharge;

        /// <summary>Apply foot ik to the charge animation.</summary>
        public bool FootIKCharge;

        /// <summary>Cycle offset for the charge animation.</summary>
        public float CycleOffsetCharge;


        /// <summary>Animation charge hold clip.</summary>
        public AnimationClip ClipSpellChargeHold;

        /// <summary>Speed of the cast animation.</summary>
        public float SpeedHold;

        /// <summary>Mirror the cast animation.</summary>
        public bool MirrorHold;

        /// <summaryApply foot ik to the hold animation.</summary>
        public bool FootIKHold;

        /// <summary>Cycle offset for the hold animation.</summary>
        public float CycleOffsetHold;


        /// <summary>Animation charge release clip.</summary>
        public AnimationClip ClipSpellChargeRelease;

        /// <summary>Speed of the cast animation.</summary>
        public float SpeedRelease;

        /// <summary>Mirror the cast animation.</summary>
        public bool MirrorRelease;

        /// <summaryApply foot ik to the hold animation.</summary>
        public bool FootIKRelease;

        /// <summaryCycle offset for the release animation.</summary>
        public float CycleOffsetRelease;


        /// <summary>Spell options to be copied to the animator states for normal casting.</summary>
        public SpellBookEntry SpellOptions;

        /// <summary>Spell options to be copied to the animator states for charge.</summary>
        public SpellBookEntry SpellOptionsCharge;

        /// <summary>Spell options to be copied to the animator states for hold.</summary>
        public SpellBookEntry SpellOptionsHold;

        /// <summary>Spell options to be copied to the animator states for release.</summary>
        public SpellBookEntry SpellOptionsRelease;


        /// <summary>Which hand is the damage caused by.</summary>
        public SpellBookHands MeleeHand;

        /// <summary>Type of melee attack to enable.</summary>
        public vAttackType meleeAttackType;

        /// <summary>Time within the animation to enable damage.</summary>
        public float startDamage;

        /// <summary>Time within the animation to disable damage.</summary>
        public float endDamage;

        /// <summary>Time within the animation to allow movement again.</summary>
        public float allowMovementAt;

        /// <summary>Multiply the amount of damage caused.</summary>
        public int damageMultiplier;

        /// <summary>Recoil ID to pass to the damage enabler.</summary>
        public int recoilID;

        /// <summary>Animation reaction ID.</summary>
        public int reactionID;

        /// <summary>You can use a name as reference to trigger a custom HitDamageParticle.</summary>
        public string attackName;

        /// <summary>Ignore any defense on the target.</summary>
        public bool ignoreDefense;

        /// <summary>Activate the target ragdoll.</summary>
        public bool activeRagdoll;

        /// <summary>Reset the attack trigger.</summary>
        public bool resetAttackTrigger;

        /// <summary>Filter the list based upon damage.</summary>
        public BaseDamage DamageType;
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
