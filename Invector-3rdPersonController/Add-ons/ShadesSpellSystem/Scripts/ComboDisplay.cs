using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shadex
{
    /// <summary>
    /// Updated the player HUD when a combo is completed.
    /// </summary>
    public class ComboDisplay : StateMachineBehaviour
    {        
        /// <summary>Name of the combo completed for display in the player HUD.</summary>
        [Tooltip("Name of the combo completed for display in the player HUD")]
        public string ComboName;

        /// <summary>Combo branch depth aka 1st, 2nd, 3rd etc in the combo branch.</summary>
        [Tooltip("Combo branch depth aka 1st, 2nd, 3rd etc in the combo branch")]
        [Range(1, 15)]
        public int ComboDepth = 1;
              

        /// <summary>
        /// Occurs when the animator enters the parent state, creates hand particles and targeting.
        /// </summary>
        /// <param name="animator">Reference to the parent animator.</param>
        /// <param name="stateInfo">Information about the state.</param>
        /// <param name="layerIndex">Index of the current animator layer.</param>
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (animator.gameObject.tag == "Player")  // ignore for the AI
            {
                GlobalFuncs.TheMagicalSettings().UpdateComboDisplay(ComboDepth, ComboName);
            }
        }
    }
}