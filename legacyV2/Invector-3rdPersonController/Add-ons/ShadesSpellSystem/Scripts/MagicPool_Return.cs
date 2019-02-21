using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if !VANILLA
using Invector;
#endif

namespace Shadex
{
    /// <summary>
    /// Return the parent to the magic pool after time length.
    /// </summary>
#if !VANILLA
    [vClassHeader("RETURN TO MAGICAL POOL", iconName = "triggerIcon")]
    public class MagicPool_Return : vMonoBehaviour
    {
#else
    public class MagicPool_Return : MonoBehaviour {
#endif
        /// <summary>ID of the magic pool slot to return to.</summary>
        [Tooltip("ID of the magic pool slot to return to")]
        public int PoolSlotID;

        /// <summary>Delay before returning to the pool.</summary>
        [Tooltip("Delay before returning to the pool")]
        public float Delay;

        // internal
        private Coroutine Delaying;

        /// <summary>
        /// Start the delay then return on enable.
        /// </summary>
        void OnEnable()
        {
            if (PoolSlotID > 0 && Delay > 0)
            {  // failsafe
                Delaying = StartCoroutine(DelayedReturnToPool());  // start the delayed return
            }
        }

        /// <summary>
        /// Delay then return to the magic pool (or destroy if pool not enabled).
        /// </summary>
        /// <returns>IEnujmerator until the delay has passed.</returns>
        IEnumerator DelayedReturnToPool()
        {
            yield return new WaitForSeconds(Delay);  // wait
            GlobalFuncs.ReturnToThePoolOrDestroy(PoolSlotID, gameObject);  // send back to the pool
        }

        /// <summary>
        /// Ensure coroutine is stopped on deactivation.
        /// </summary>
        void OnDisable()
        {
            if (Delaying != null)
            {
                StopCoroutine(Delaying);
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
