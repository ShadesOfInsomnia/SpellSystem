using UnityEngine;
using UnityEngine.UI;
using System.Collections;
#if !VANILLA
using Invector;
#endif

namespace Shadex
{
    /// <summary>
    /// Links the leveling system to the health/mana HUD for the AI.
    /// </summary>
#if !VANILLA
    [vClassHeader("MAGIC SPRITE HEALTH", iconName = "inputIcon")]
    public class MagicAISpriteHealth : vMonoBehaviour
    {
#else
    public class MagicAISpriteHealth : MonoBehaviour {
#endif
        /// <summary>Text canvas object to display the character name.</summary>
        [Tooltip("Text canvas object to display the character name")]
        public Text NameDisplay;

        /// <summary>Slider canvas object to display the current health.</summary>
        [Tooltip("Slider canvas object to display the current health")]
        public Slider HealthSlider;

        /// <summary>Slider canvas object to display the current mana.</summary>
        [Tooltip("Slider canvas object to display the current mana")]
        public Slider ManaSlider;

        /// <summary>Damage amount display.</summary>
        [Tooltip("Damage amount display")]
        public Text DamageCounter;

        /// <summary>Delay before removing the damage value.</summary>
        [Tooltip("Delay before removing the damage value")]
        public float CounterTimer = 1.5f;

        /// <summary>
        /// accumulated damage whilst displaying.
        /// </summary>
        protected float damage;

        /// <summary>
        /// Cache the name.
        /// </summary>
        protected string NameOfAI;  

        /// <summary>
        /// Prevent endless destroy loop.
        /// </summary>
        protected bool AlreadyDestroyed;


        /// <summary>
        /// Add a listener to the leveling component if available.
        /// </summary>
        void Start()
        {
            CharacterBase levelingSystem = GetComponentInParent<CharacterBase>();
            if (levelingSystem)
            {
                NameOfAI = levelingSystem.Name;
                levelingSystem.NotifyUpdateHUD += new CharacterBase.UpdateHUDHandler(UpdateHUDListener);
                levelingSystem.ForceUpdateHUD();
            }
        }

        /// <summary>
        /// Keeps the AI HUD facing the player camera.
        /// </summary>
        void Update()
        {
            if (Camera.main != null) transform.LookAt(Camera.main.transform.position, Vector3.up);
        }

        /// <summary>
        /// Listen to the leveling system for attribute changes.
        /// </summary>
        /// <param name="Stats">Instance of the leveling system providing the stats.</param>
        /// <param name="Stats">Stats that have been updated.</param>
        private void UpdateHUDListener(CharacterBase LevelingSystem, CharacterUpdated Stats)
        {
            if (Stats.Life <= 0) // alive?
            {
                if (!AlreadyDestroyed)  // fail safe
                {
                    if (LevelingSystem) {
                        LevelingSystem.NotifyUpdateHUD -= UpdateHUDListener;
                        //Destroy(LevelingSystem.gameObject);
                    }
                    if (gameObject) Destroy(gameObject);  
                    AlreadyDestroyed = true;
                }
            }
            else // still here
            {  
                if (NameDisplay) NameDisplay.text = NameOfAI + " [" + Stats.Level.ToString() + "]";
                HealthSlider.maxValue = Stats.LifeMAX;     // keep the max health in line           
                if (HealthSlider.value > Stats.Life)
                {  // damaged?
                    Damage(HealthSlider.value - Stats.Life);  // update the HUD
                }
                HealthSlider.value = Stats.Life;  // apply the values
                ManaSlider.maxValue = Stats.ManaMAX;  // to keep all in
                ManaSlider.value = Stats.Mana;  // line with the leveling component
            }
        }   

        /// <summary>
        /// Occurs when the AI is damaged.
        /// </summary>
        /// <param name="value">Amount of damage applied.</param>
        public void Damage(float value)
        {
            try
            {
                damage += value;  // up the accumulated damage
                DamageCounter.text = damage.ToString("00");  // show it above the ai
                StartCoroutine(DamageDelay());
            }
            catch
            {
                Destroy(this);  // failsafe
            }
        }

        /// <summary>
        /// Show the damage counter for a limited time.
        /// </summary>
        /// <returns>IEnumerator until the delay has passed.</returns>
        IEnumerator DamageDelay()
        {
            yield return new WaitForSeconds(CounterTimer);
            damage = 0;
            DamageCounter.text = string.Empty;
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
