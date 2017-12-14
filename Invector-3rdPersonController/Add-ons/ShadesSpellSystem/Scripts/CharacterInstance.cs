using System.Collections.Generic;
using System;

namespace Shadex
{
    /// <summary>
    /// Instance of the base abstract leveling system, add to both the player and AI's.
    /// </summary>
    /// <remarks>
    /// Clone this class (and its editor) to make alterations, all functions are looking for the CharacterBase class
    /// and will work with clones of this class, it has been done this way to allow future updates not overwriting 
    /// any calculation changes that are made.  
    /// 
    /// Even if not altering the damage mitigation calculations, see the RebuildModifiers function as this determines
    /// what base skill points/resistances each class/race has.
    /// </remarks>
    public class CharacterInstance : CharacterBase
    {
        // formula constants for readability, not mandatory, alter the math of the core calculations in this class
        /// <summary>base life value.</summary>
        const int HP_BASE = 36;                     

        /// <summary>each level up gains x life.</summary>
        const int HP_PERLEVEL = 4;                  

        /// <summary>base life value.</summary>
        const int MANA_BASE = 100;                  

        /// <summary>each int point up gains x mana.</summary>
        const int MANA_PERINT = 6;                  

        /// <summary>base stamina + endurance / weight.</summary>
        const int STAMINA_BASE = 150;               

        /// <summary>resistance gained per level .</summary>
        const float RESIST_PERLEVEL = 0.05f;             

        /// <summary>baseline level up.</summary>
        const int LEVELUP_XPBASE = 1000;            

        /// <summary>banding bonus, increases as move up through the level bands.</summary>
        const int LEVELUP_XPBAND_BONUS = 250;       

        /// <summary>number of level bands.</summary>
        const int LEVELUP_XPBAND_NUMBER = 50;       

        /// <summary>damage reduction per point of armour.</summary>
        const float ARMOUR_REDUCTION = 0.05f;       

        /// <summary>spell scale = 1f + intelligence * 0.01.</summary>
        const float SPELLPOWERSCALE = 0.01f;        

        /// <summary>limit the size.</summary>
        const float SPELLPOWERMAX = 3f;

        /// <summary>
        /// rebuild the modifier list/cache after class/race changes.
        /// </summary>
        public override void RebuildModifiers()
        {
            // clear the list
            Modifiers.RemoveAll(s => s.src == ModifierSource.Character);

            // push race modifiers
            switch (CurrentRace)
            {
                case BaseRace.Human:
                    AddModifier(BaseSkill.Constitution, 5);
                    AddModifier(BaseSkill.Charisma, -2);
                    AddModifier(BaseDamage.Physical, 5);
                    CurrentArmour = 8;
                    break;
                case BaseRace.Dwarf:
                    AddModifier(BaseSkill.Strength, 5);
                    AddModifier(BaseSkill.Dexterity, -2);
                    AddModifier(BaseDamage.Fire, 5);
                    CurrentArmour = 10;
                    break;
                case BaseRace.Elf:
                    AddModifier(BaseSkill.Intelligence, 5);
                    AddModifier(BaseSkill.Wisdom, -2);
                    AddModifier(BaseDamage.Poison, 5);
                    CurrentArmour = 5;
                    break;
            }

            // push class modifiers
            switch (CurrentClass)
            {
                case BaseClass.Knight:
                    AddModifier(BaseSkill.Strength, 7);
                    AddModifier(BaseDamage.Physical, 6);
                    CurrentArmour += 10;
                    break;
                case BaseClass.Mage:
                    AddModifier(BaseSkill.Strength, -2);
                    AddModifier(BaseSkill.Intelligence, 7);
                    AddModifier(BaseDamage.Fire, 2);
                    AddModifier(BaseDamage.Lightning, 2);
                    AddModifier(BaseDamage.Cold, 2);
                    CurrentArmour -= 5;
                    break;
                case BaseClass.Hunter:
                    AddModifier(BaseSkill.Dexterity, 3);
                    AddModifier(BaseSkill.Strength, 2);
                    AddModifier(BaseDamage.Physical, 5);
                    AddModifier(BaseDamage.Poison, 6);
                    CurrentArmour += 2;
                    break;
            }

            // push alignment modifiers
            switch (CurrentAlignment)
            {
                case BaseAlignment.Good:
                    AddModifier(BaseDamage.Occult, 10);
                    break;
                case BaseAlignment.Evil:
                    AddModifier(BaseDamage.Holy, 10);
                    break;
            }

            // push axis modifiers
            switch (CurrentAxis)
            {
                case BaseAxis.Lawful:
                    AddModifier(BaseSkill.Wisdom, 10);
                    break;
                case BaseAxis.Chaotic:
                    AddModifier(BaseSkill.Charisma, 10);
                    break;
            }

            // rebuild modifier cache
            rebuildModifierTotals(true);

        }

        /// <summary>
        /// Formula to determine next level up.
        /// </summary>
        /// <param name="CurrentLevel">Current character level.</param>
        /// <returns>XP required to level up.</returns>
        public override int CalcXPToNextLevel(int CurrentLevel)
        {
            return CurrentLevel * (                            // return the current level * the level up total
                LEVELUP_XPBASE + (                              // add band total to the flat level up amount
                    LEVELUP_XPBAND_BONUS * (int)Math.Ceiling(   // multiply band increase by the current band rounded up
                        (double)CurrentLevel / (               // divide current level by band size 
                            CharacterDefaults.MAXLEVEL / LEVELUP_XPBAND_NUMBER    // calc band size
                        )
                    )
                )
            );
        }

        /// <summary>
        /// Formula to determine max life from stats.
        /// </summary>
        /// <param name="UpdateToMAX">Option to MAX out the life attribute on upgrade.</param>
        public override void CalcMaxLife(bool UpdateToMAX)
        {
            // calc value of each HP
            int iSkillToLife;
            if (CurrentLevel > 70) iSkillToLife = 35;
            else if (CurrentLevel > 25) iSkillToLife = 25;
            else iSkillToLife = 10;

            // set max life 
            MAXLife = HP_BASE + (HP_PERLEVEL * CurrentLevel) +
                ((int)(Skills[(int)BaseSkill.Constitution].Value + SkillModTotals[(int)BaseSkill.Constitution].Value) * iSkillToLife);
            if (UpdateToMAX)
            {
                CurrentLife = MAXLife;
            }
        }

        /// <summary>
        /// Formula to determine bonus resistance per level.
        /// </summary>
        public override void CalcResistLevelBonus()
        {
            if (CurrentClass == BaseClass.Mage)
            {
                reCalcBaseResist(RESIST_PERLEVEL * 2);
            }
            else
            {
                reCalcBaseResist(RESIST_PERLEVEL);
            }
        }

        /// <summary>
        /// Formula to determine max stamina from stats.
        /// </summary>
        public override void CalcMaxStamina()
        { 
            MAXStamina = STAMINA_BASE +
                (int)((Skills[(int)BaseSkill.Endurance].Value + SkillModTotals[(int)BaseSkill.Endurance].Value) - CurrentEquipLoad);
        }

        /// <summary>
        /// Formula to determine max mana from stats
        /// </summary>
        /// <param name="UpdateToMAX">Option to MAX out the mana attribute on upgrade.</param>
        public override void CalcMaxMana(bool UpdateToMAX)
        {
            MAXMana = MANA_BASE + (
                (int)(Skills[(int)BaseSkill.Intelligence].Value + SkillModTotals[(int)BaseSkill.Intelligence].Value) * MANA_PERINT);
            if (UpdateToMAX)
            {
                CurrentMana = MAXMana;
            }
        }

        /// <summary>
        /// Formula to determine max equip weight from stats.
        /// </summary>
        public override void CalcMaxEquipLoad()
        {
            MAXEquipLoad = (int)((
                Skills[(int)BaseSkill.Endurance].Value + SkillModTotals[(int)BaseSkill.Endurance].Value +
                ((Skills[(int)BaseSkill.Strength].Value + SkillModTotals[(int)BaseSkill.Strength].Value) / 10)
                ) * 2);
        }

        /// <summary>
        /// Formula to link to the spell size by level script.
        /// </summary>
        /// <returns>Spell scale modifier.</returns>
        public override float CalcSpellScale()
        {
            float SpellScale = 1f + Skills[(int)BaseSkill.Intelligence].Value / SPELLPOWERSCALE;
            if (SpellScale > SPELLPOWERMAX)
            {
                return SPELLPOWERMAX;
            }
            else
            {
                return SpellScale;
            }
        }

        /// <summary>
        /// Formula for damage mitigation based upon damage type.
        /// </summary>
        /// <param name="Type">Type of damage incoming.</param>
        /// <param name="Amount">Amount of damage received.</param>
        /// <returns>Reduced damage based upon resistances/armour.</returns>
        public override float MitigateDamge(BaseDamage Type, float Amount)
        {
            if (Type == BaseDamage.Physical)
            {  // physical damage?
                if (CurrentArmour + BonusArmour > 0)
                {  // has armour?
                    Amount -= (ARMOUR_REDUCTION * (CurrentArmour + BonusArmour));  // apply armour mitigation
                }
            }
            if ((Resist[(int)Type].Value + ResistModTotals[(int)Type].Value) > 0)
            {  // has resistance to this damage type?
                return Amount - ((Amount / 100) * (Resist[(int)Type].Value + ResistModTotals[(int)Type].Value));  // apply resistance mitigation
            }
            else  // no resistance mitigation
            {
                return Amount;  // unmodified damage value
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
