using UnityEngine;
using UnityEditor;

namespace Shadex
{
    /// <summary>
    /// Unity menu item links to add the core classes to the active game object
    /// </summary>
    public partial class ShadexComponents
    {
        [MenuItem("Invector/Shades Spell System/Character Components/Magic AI")]
        static void MagicAIMenu()
        {
            if (Selection.activeGameObject)
                Selection.activeGameObject.AddComponent<MagicAI>();
            else
                Debug.Log("No Active Selection");
        }

        [MenuItem("Invector/Shades Spell System/Character Components/Character Equip Attributes")]
        static void CharacterEquipAttributesMenu()
        {
            if (Selection.activeGameObject)
                Selection.activeGameObject.AddComponent<CharacterEquipAttributes>();
            else
                Debug.Log("No Active Selection");
        }

        [MenuItem("Invector/Shades Spell System/Character Components/Magic AI Sprite Health")]
        static void MagicAISpriteHealthMenu()
        {
            if (Selection.activeGameObject)
                Selection.activeGameObject.AddComponent<MagicAISpriteHealth>();
            else
                Debug.Log("No Active Selection");
        }

        [MenuItem("Invector/Shades Spell System/Character Components/Generic RIG Controller")]
        static void MagicAIControllerMenu()
        {
            if (Selection.activeGameObject)
                Selection.activeGameObject.AddComponent<GenericRIGController>();
            else
                Debug.Log("No Active Selection");
        }

        [MenuItem("Invector/Shades Spell System/Character Components/Magic Settings")]
        static void MagicInputMenu()
        {
            if (Selection.activeGameObject)
                Selection.activeGameObject.AddComponent<MagicSettings>();
            else
                Debug.Log("No Active Selection");
        }

        [MenuItem("Invector/Shades Spell System/Character Components/Leveling System")]
        static void MagicLevelingSystemMenu()
        {
            if (Selection.activeGameObject)
                Selection.activeGameObject.AddComponent<CharacterInstance>();
            else
                Debug.Log("No Active Selection");
        }

        [MenuItem("Invector/Shades Spell System/Spell Components/Magic Projectile")]
        static void MagicProjectileMenu()
        {
            if (Selection.activeGameObject)
                Selection.activeGameObject.AddComponent<MagicProjectile>();
            else
                Debug.Log("No Active Selection");
        }

        [MenuItem("Invector/Shades Spell System/Spell Components/Magic Projectile Physics")]
        static void MagicProjectilePhysicsMenu()
        {
            if (Selection.activeGameObject)
                Selection.activeGameObject.AddComponent<MagicProjectilePhysics>();
            else
                Debug.Log("No Active Selection");
        }

        [MenuItem("Invector/Shades Spell System/Spell Components/Magic Teleport")]
        static void MagicTeleportMenu()
        {
            if (Selection.activeGameObject)
                Selection.activeGameObject.AddComponent<MagicTeleport>();
            else
                Debug.Log("No Active Selection");
        }

        [MenuItem("Invector/Shades Spell System/Spell Components/Destroy GameObject And Spawn")]
        static void DestroyGameObjectAndExplodeMenu()
        {
            if (Selection.activeGameObject)
                Selection.activeGameObject.AddComponent<DestroyGameObjectAndSpawn>();
            else
                Debug.Log("No Active Selection");
        }

        [MenuItem("Invector/Shades Spell System/Spell Components/Trapped Object")]
        static void MagicTrappedObjectMenu()
        {
            if (Selection.activeGameObject)
                Selection.activeGameObject.AddComponent<TrappedObject>();
            else
                Debug.Log("No Active Selection");
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
