using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if !VANILLA
using Invector;
#endif

namespace Shadex
{
    /// <summary>
    /// Equipment attributes data class, attach to equipment to apply skill/resistance values to the leveling system.
    /// </summary>
#if !VANILLA
    [vClassHeader("EQUIPMENT ATTRIBUTES", iconName = "ladderIcon")]
    public class CharacterEquipAttributes : vMonoBehaviour
    {
#else
    public class CharacterEquipAttributes : MonoBehaviour {
#endif
        /// <summary>Core attribute modifiers of the equipment.</summary>
        [Tooltip("Core attribute modifiers of the equipment")]
        public List<CoreBonus> Core = new List<CoreBonus>();

        /// <summary>Skill point modifiers of the equipment.</summary>
        [Tooltip("Skill point modifiers of the equipment")]
        public List<SkillBonus> SkillPoints = new List<SkillBonus>();

        /// <summary>Resistance modifiers of the equipment.</summary>
        [Tooltip("Resistance modifiers of the equipment")]
        public List<MagicDamage> Resistance = new List<MagicDamage>();

        // internal
        private CharacterBase LevelingSystem;
        private bool Updated;

        /// <summary>
        /// Wait till parented to find the leveling system, then update the modifier stack
        /// </summary>
        void Update()
        {
            if (!Updated)
            {  // not yet applied the update
                if (!LevelingSystem)
                {  // still not found?
                    LevelingSystem = GetComponentInParent<CharacterBase>();   // attempt grab
                    if (LevelingSystem)
                    {  // success?
                        LevelingSystem.reCalcEquipmentBonuses(null, true);  // force equip magic attribute update on the parent
                        Updated = true;  // don't continuously update
                    }
                }
            }
        }  

        /// <summary>
        /// Occurs when the parent item is disabled or unequiped, removes the equipment bonuses
        /// </summary>
        void OnDisable()
        {
            if (LevelingSystem)
            {  // previously had updated a leveling system?
                LevelingSystem.reCalcEquipmentBonuses(this, false);   // force equip magic attribute update
                LevelingSystem = null;   // clear link to character
                Updated = false;  // clear the updated flag for another pickup
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
