using UnityEngine;

#if !VANILLA
using Invector;
#endif

namespace Shadex
{
    /// <summary>
    /// Set the parent game object to not be destroyed when a scene is loaded.
    /// </summary>
    /// <remarks>
    /// Only works on root level game objects.
    /// </remarks>
#if !VANILLA
    [vClassHeader("DONT DESTROY ONLOAD", iconName = "triggerIcon")]
    public class DontDestroyOnLoad : vMonoBehaviour
    {
#else
    public class DontDestroyOnLoad : MonoBehaviour {
#endif
        /// <summary>
        /// Apply don't destroy to parent
        /// </summary>
        private void Awake()
        {
            DontDestroyOnLoad(transform.gameObject);
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
