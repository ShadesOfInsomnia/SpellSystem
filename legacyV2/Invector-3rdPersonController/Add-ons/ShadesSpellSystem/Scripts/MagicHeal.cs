using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if !VANILLA
using Invector;
#endif

namespace Shadex
{
    /// <summary>
    /// Magic heal for projectiles or radius spells.
    /// </summary>
#if !VANILLA
    [vClassHeader("MAGICAL HEAL", iconName = "ammoIcon")]
    public class MagicHeal : vMonoBehaviour
    {
#else
    public class MagicHeal : MonoBehaviour {
#endif
        /// <summary>Heal radius from collision point, leave as zero to only heal object is collided with.</summary>
        [Tooltip("Heal radius from collision point, leave as zero to only heal object is collided with")]
        public float Radius;

        /// <summary>Heal amount to apply.</summary>
        [Tooltip("Heal amount to apply")]
        public float Amount = 50f;

        // hidden from GUI
        /// <summary>Targeting layers, set by the pool when the master instance is created.</summary>
        [HideInInspector] public LayerMask HealTargetLayers;

        /// <summary>Targeting tags, set by the pool when the master instance is created.</summary>
        [HideInInspector] public string[] HealTargetTags;

        // internal
        private bool HealedAlready;

        /// <summary>
        /// reset the applied flag when enabled from the pool
        /// </summary>
        void OnEnable()
        {
            HealedAlready = false;
        }

        /// <summary>
        /// Occurs when the parent game object collider is triggered by another collider.
        /// </summary>
        /// <param name="c">Collider to potentially apply the healing to.</param>
        private void OnTriggerEnter(Collider c)
        {
            if (HealedAlready) return;  // prevent double heal due to multiple colliders hit on the same character

            if ((HealTargetLayers.value & 1 << c.gameObject.layer) == 1 << c.gameObject.layer)
            {  // valid heal target  
                CharacterBase LevellingSystem = c.gameObject.GetComponentInParent<CharacterBase>();  // levelling system on root?
                if (LevellingSystem)
                {  // sure
                    LevellingSystem.CurrentLife += Amount;  // increase HP
                    LevellingSystem.ForceUpdateHUD();  // update HUD

                    // handle heal radius ability
                    if (Radius > 0)
                    {  // enabled
                        List<Transform> listTargetsInRange = GlobalFuncs.FindAllTargetsWithinRange(transform.position, Radius, HealTargetLayers, HealTargetTags, false, 1f, true);  // find all within range
                        foreach (Transform Target in listTargetsInRange)
                        {
                            CharacterBase TargetLevellingSystem = Target.GetComponentInParent<CharacterBase>();  // levelling system on root?
                            if (TargetLevellingSystem && LevellingSystem.gameObject.name != TargetLevellingSystem.gameObject.name)
                            {  // found and not self
                                TargetLevellingSystem.CurrentLife += Amount;  // increase HP
                                TargetLevellingSystem.ForceUpdateHUD();  // update HUD
                            }
                        }
                    }

                    // work complete
                    HealedAlready = true;
                }
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
