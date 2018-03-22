using UnityEngine;
#if !VANILLA
using Invector;
#endif

namespace Shadex
{
    /// <summary>
    /// Scale the particles based upon character level
    /// </summary>
    /// <remarks>
    /// Provides scale to the child particles calculated by the level of the character that fired them.  
    /// Dependent upon the leveling system being in play and standard unity particle systems being present.
    /// </remarks>
#if !VANILLA
    [vClassHeader("SPELL SIZE BY LEVEL", iconName = "ammoIcon")]
    public class SpellSizeByLevel : vMonoBehaviour
    {
#else
    public class SpellSizeByLevel : MonoBehaviour {
#endif
        /// <summary>Minimum scale of the spell, corresponds with character level 1.</summary>
        [Tooltip("Minimum scale of the spell, corresponds with character level 1")]
        public float MinSize = 1f;

        /// <summary>Maximum scale of the spell, corresponds with character level cap.</summary>
        [Tooltip("Maximum scale of the spell, corresponds with character level cap")]
        public float MaxSize = 3f;

        /// <summary>Character level where the spell no longer increases in siz.</summary>
        [Tooltip("Character level where the spell no longer increases in size")]
        public int LevelCap = 20;

        /// <summary>Include the X axis in the scaling.</summary>
        [Tooltip("Include the X axis in the scaling")]
        public bool IncludeX = true;

        /// <summary>Include the Y axis in the scaling.</summary>
        [Tooltip("Include the Y axis in the scaling")]
        public bool IncludeY = true;

        /// <summary>Include the Z axis in the scaling.</summary>
        [Tooltip("Include the Z axis in the scaling")]
        public bool IncludeZ = true;

        /// <summary>
        /// Occurs when the script is enabled, updating the spell size based upon character level
        /// </summary>
        void OnEnable()
        {
            CharacterBase LevelingSystem = GetComponentInParent<CharacterBase>();
            if (LevelingSystem)
            {
                // determine scale amount based upon character level
                int CappedLevel = (LevelingSystem.CurrentLevel > LevelCap ? LevelCap : LevelingSystem.CurrentLevel);
                float SpellLevel = MinSize + (((MaxSize - MinSize) / LevelCap) * CappedLevel);

                // build the scale
                Vector3 LevelledScale = new Vector3(
                    (IncludeX ? SpellLevel : transform.localScale.x),
                    (IncludeY ? SpellLevel : transform.localScale.y),
                    (IncludeZ ? SpellLevel : transform.localScale.z)
                );

                // apply to all child particle systems
                foreach (ParticleSystem p in GetComponentsInChildren<ParticleSystem>())
                {
                    ParticleSystem.MainModule newMain = p.main;
                    newMain.scalingMode = ParticleSystemScalingMode.Local;
                    p.transform.localScale = LevelledScale;
                }

                // unset parent if projectile
                if (GetComponent<MagicProjectile>())
                {
                    transform.parent = null;
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
