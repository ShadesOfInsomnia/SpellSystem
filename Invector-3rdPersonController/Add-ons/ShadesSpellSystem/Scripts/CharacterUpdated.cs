using System;

namespace Shadex
{
    /// <summary>
    /// Leveling system attributes updated for the update HUD event.
    /// </summary>
    public class CharacterUpdated : EventArgs
    {
        /// <summary>Current XP.</summary>
        public int XP { get; set; }

        /// <summary>Current character level.</summary>
        public int Level { get; set; }

        /// <summary>Current HP.</summary>
        public float Life { get; set; }

        /// <summary>Maximum HP.</summary>
        public int LifeMAX { get; set; }

        /// <summary>Current mana.</summary>
        public float Mana { get; set; }

        /// <summary>Maximum mana.</summary>
        public int ManaMAX { get; set; }

        /// <summary>Maximum stamina.</summary>
        public int StaminaMAX { get; set; }
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
