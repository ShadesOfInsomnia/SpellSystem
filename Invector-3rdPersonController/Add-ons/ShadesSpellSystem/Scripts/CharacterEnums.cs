using System.Collections.Generic;
using UnityEngine;
using System;

namespace Shadex
{
    /// <summary>
    /// System wide character defaults, will be moved to magic settings later.
    /// </summary>
    public static class CharacterDefaults
    {
        /// <summary>Level cap.</summary>
        public const int MAXLEVEL = 999;

        /// <summary>Max value for all skill points    .</summary>
        public const int SKILLS_MAX_VALUE = 99;

        /// <summary>Initial value (before class/race modifiers) of all attributes.</summary>
        public const int SKILLS_INITIAL_VALUE = 8;

        /// <summary>How many extra points per level up.</summary>
        public const int LEVELUP_SKILL_POINTS = 4;

        /// <summary>Provide some unspent points for the player to spend at level 1.</summary>
        public const int LEVELUP_INITIAL_UNSPENT_POINTS = 0;    
    }

    /// <summary>
    /// Skill points enum, controls what skill points are available for the calculations.
    /// </summary>
    public enum BaseSkill
    {
        Strength, Constitution, Endurance, Dexterity, Intelligence, Wisdom, Charisma
    }

    /// <summary>
    /// Elemental damage types, note physical is required as element 0.
    /// </summary>
    public enum BaseDamage
    {
        Physical, Holy, Occult, Fire, Cold, Lightning, Poison, Bleed
    }  

    /// <summary>
    /// Available character axis, aka lawfulness.
    /// </summary>
    public enum BaseAxis
    {
        Neutral, Lawful, Chaotic
    }

    /// <summary>
    /// Available character alignments.
    /// </summary>
    public enum BaseAlignment
    {
        Good, Evil
    }

    /// <summary>
    /// Available character races.
    /// </summary>
    public enum BaseRace
    {
        Human, Dwarf, Elf
    }

    /// <summary>
    /// Available character classes.
    /// </summary>
    public enum BaseClass
    {
        Knight, Mage, Hunter
    }

    /// <summary>
    /// Available collectable item drops.
    /// </summary>
    public enum BaseCollectable
    {
        Gold, Shrooms
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
