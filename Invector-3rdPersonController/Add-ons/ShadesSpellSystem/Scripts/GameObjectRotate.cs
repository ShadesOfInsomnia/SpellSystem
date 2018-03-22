using UnityEngine;
#if !VANILLA
using Invector;
#endif

namespace Shadex
{
    /// <summary>
    /// Rotate parent game object with a coordinate space.
    /// </summary>
#if !VANILLA
    [vClassHeader("GAMEOBJECT ROTATE", iconName = "triggerIcon")]
    public class GameObjectRotate : vMonoBehaviour
    {
#else
    public class GameObjectRotate : MonoBehaviour {
#endif
        /// <summary>Direction of rotation.</summary>
        public Vector3 rotationDirection;

        /// <summary>Affects smoothing speed.</summary>
        public float durationTime;

        /// <summary>Coordinate space.</summary>
        public Space space;

        // internal
        private float smooth;


        /// <summary>
        /// Apply the rotation once per frame.
        /// </summary>
        void Update()
        {
            smooth = Time.deltaTime * durationTime;
            transform.Rotate(rotationDirection * smooth, space);
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
