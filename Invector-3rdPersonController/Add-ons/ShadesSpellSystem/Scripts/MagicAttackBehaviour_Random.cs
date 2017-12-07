using UnityEngine;
using System.Collections.Generic;
using System;

namespace Shadex
{
    /// <summary>
    /// Animator state machine random state selection state for AI's.
    /// </summary>
    /// <remarks>
    /// Originally used when the AI's did not have the invector AI inventory component attached, still useful if the 
    /// invector inventory is not in use and a way of selecting a random spell is desired from the animator purely.
    /// Note the fields are still present in the magic settings to enable this possibility.
    /// </remarks>
    public class MagicAttackBehaviour_Random : StateMachineBehaviour
    {
        /// <summary>Random attack animator parameter name (must be type int in the animator).</summary>
        [Tooltip("Random Attack animator parameter name (must be type int)")]
        public string RandomAttackName = "MagicID";

        /// <summary>Default Magic ID, for when none in range found, a none attacking taunt would be a good idea.</summary>
        [Tooltip("Default Magic ID, for when none in range found, a none attacking taunt would be a good idea")]
        public int DefaultAttack = 1;

        /// <summary>Magic Attack ID's with min/max ranges to be randomly selected.</summary>
        [Tooltip("Magic Attack ID's with min/max ranges")]
        public List<MagicAttackBehaviour_RandomProperties> Attacks = new List<MagicAttackBehaviour_RandomProperties>();

        /// <summary>
        /// Randomly moves to another animator state upon state enter.
        /// </summary>
        /// <param name="animator">Reference to the parent animator.</param>
        /// <param name="stateInfo">Information about the state.</param>
        /// <param name="layerIndex">Index of the animator layer.</param>
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            GameObject player = GlobalFuncs.FindPlayerInstance();  // grab the player
            if (player)
            {
                // work out the distance to the player
                float dist = Vector3.Distance(animator.transform.position, player.transform.position);

                // find available attacks
                int[] iAvailableAttacks = null;
                for (int i = 0; i < Attacks.Count; i++)
                {
                    // add to the available list if has no range or is in range
                    if (!Attacks[i].CheckRange || (dist >= Attacks[i].MinAttackRange && dist <= Attacks[i].MaxAttackRange))
                    {
                        for (int w = 0; w < Attacks[i].Weight + 1; w++)
                        {
                            if (iAvailableAttacks == null)
                            {
                                iAvailableAttacks = new int[1];  // create element 0                                
                            }
                            else
                            {  // not empty
                                Array.Resize<int>(ref iAvailableAttacks, iAvailableAttacks.Length + 1);  // extend array by 1
                            }
                            iAvailableAttacks[iAvailableAttacks.Length - 1] = i;  // add to the slot bag
                        }
                    }
                }

                // now lets attack
                if (iAvailableAttacks != null)
                { // cause the attack, which should be linked to this state matching conditions on the transition (named in RandomAttackName)
                    // random attack selection
                    int iRandomAttack = UnityEngine.Random.Range(1, iAvailableAttacks.Length + 1) - 1;

                    // face the player?
                    if (Attacks[iAvailableAttacks[iRandomAttack]].FacePlayer)
                    {
                        animator.transform.LookAt(player.transform.localPosition);
                    }

                    // attack
                    animator.SetInteger(RandomAttackName, Attacks[iAvailableAttacks[iRandomAttack]].MagicId);
                }
                else
                {  // all attacks fail range check, show default
                    animator.SetInteger(RandomAttackName, DefaultAttack);
                }
            }
            else
            {  // player not found, show default
                animator.SetInteger(RandomAttackName, DefaultAttack);
            }
        }
    }

    /// <summary>
    /// Data list class for the random selection of spell, includes ranges
    /// </summary>
    [Serializable]
    public class MagicAttackBehaviour_RandomProperties
    {
        /// <summary>Magic Attack Id to fire.</summary>
        [Tooltip("Magic Attack Id to fire")]
        public int MagicId;

        /// <summary>Include a range check.</summary>
        [Tooltip("Include a range check")]
        public bool CheckRange;

        /// <summary>Minimum range from the player to include spell in the attack.</summary>
        [Tooltip("Minimum range from the player to include spell in the attack")]
        public float MinAttackRange;

        /// <summary>Maximum range from the player to include spell in the attack.</summary>
        [Tooltip("Maximum range from the player to include spell in the attack")]
        public float MaxAttackRange;

        /// <summary>Rotate to face the player before launching the attack.</summary>
        [Tooltip("Rotate to face the player before launching the attack")]
        public bool FacePlayer;

        /// <summary>Increase above zero to increase the chance of this attack being chosen.</summary>
        [Tooltip("Increase above zero to increase the chance of this attack being chosen")]
        public int Weight;
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
