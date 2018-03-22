using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Invector;
using Invector.vCharacterController;
using Invector.vItemManager;
using Invector.vMelee;
using UnityEditor;

namespace Shadex
{
    /// <summary>
    /// Original leveling system for the player.
    /// </summary>
    /// <remarks>
    /// This has been replaced with the CharacterBase/Instance classes.  
    /// Useful if your after a clean and simple character leveling system to extend.
    /// </remarks>
    [vClassHeader("MAGIC LEVELING SYSTEM", iconName = "ladderIcon")]
    public class OriginalLevelingSystem : vMonoBehaviour
    {
        /// <summary>Link to the invector player script.</summary>
        [Header("Bridge")]
        public vThirdPersonController MyChar;

        /// <summary>Link to the invector player melee manager.</summary>
        public vMeleeManager MyMelee;

        /// <summary>Total XP acquired</summary>
        [Header("Level Variables")]
        public static int XPCurrent;

        /// <summary>XP till level up.</summary>
        public int XPToNextLevel;

        /// <summary>Current level of the character.</summary>
        public int CurrentLevel = 1;

        /// <summary>Multiplier used in the level up XP calc.</summary>
        public int LevelMultiplier;

        /// <summary>Total strength.</summary>
        [Header("Attributes")]
        public int Strength = 18;

        /// <summary>Total dexterity.</summary>
        public int Dexterity = 18;

        /// <summary>Total intelligence.</summary>
        public int Intelligence = 18;

        /// <summary>Total wisdom.</summary>
        public int Wisdom = 18;

        /// <summary>Total constitution.</summary>
        public int Constitution = 18;

        /// <summary>Current mana level.</summary>
        [Header("Sub Attributes")]
        public int Mana = 100;

        /// <summary>Max mana allowed with present stats.</summary>
        public int ManaMax = 100;

        /// <summary>Min HP dice roll</summary>
        [Header("Attribute Dice Rolls")]
        public int HPDiceMin = 1;

        /// <summary>Max HP dice roll</summary>
        public int HPDiceMax = 4;

        /// <summary>Min stamina dice roll</summary>
        public int StamDiceMin = 1;

        /// <summary>Max stamina dice roll</summary>
        public int StamDiceMax = 4;

        /// <summary>Min mana dice roll</summary>
        public int ManaDiceMin = 1;

        /// <summary>Max mana dice roll</summary>
        public int ManaDiceMax = 6;

        /// <summary>Link to the uGUI text element to show amount of XP.</summary>
        [Header("GUI Display")]
        public Text XPText;

        /// <summary>Link to the uGUI text element to show character level.</summary>
        public Text LevelText;

        /// <summary>Link to the uGUI text element to show when next level achieved.</summary>
        public Text LevelUpText;

        /// <summary>Length to cross fade the level up text.</summary>
        public float FadeDuration = 3;

        /// <summary>Link to the uGUI slider element to show amount of mana.</summary>
        public Slider ManaSlider;


        /// <summary>
        /// initial settings.
        /// </summary>
        void Start()
        {
            Mana = ManaMax;  // initialise the internal mana to the max allowed
            if (ManaSlider)
            {  // UI specified
                ManaSlider.maxValue = ManaMax;  // update the mana UI value
                ManaSlider.value = ManaMax;  // and initial value
            }
        }

        /// <summary>
        /// GUI stats update and next level checking.
        /// </summary>
        void Update()
        {
            UpdateScore();
            if (XPCurrent >= XPToNextLevel)
            {
                LevelUp();
                LevelProgression();
            }
        }


        /// <summary>
        /// Update the score to GUI.
        /// </summary>
        void UpdateScore()
        {
            XPText.text = "XP " + XPCurrent;
            LevelText.text = " " + CurrentLevel;
            //LevelText.text = "Lvl: " + CurrentLevel;
        }


        /// <summary>
        /// Calc whether next level reached.
        /// </summary>
        void LevelProgression()
        {
            //XPToNextLevel = (XPToNextLevel * 2);
            XPToNextLevel += (LevelMultiplier + (XPToNextLevel / 5));   //IE 20% of level multiplier + XPToNextLevel 
        }

        /// <summary>
        /// Level up and cross fade notification to GUI.
        /// </summary>
        void LevelUp()
        {
            Debug.Log("level up");
            CurrentLevel = CurrentLevel + 1;
            XPCurrent = 0;
            AttributeLevelUp();

            //MyMelee.defaultDamage.damageValue = MyMelee.defaultDamage.damageValue + 200 ;		// trying to set unarmed damage
            LevelUpText.CrossFadeAlpha(1f, 0.01f, false);
            LevelUpText.text = "Congratulations, you reached level " + CurrentLevel;
            LevelUpText.CrossFadeAlpha(0f, FadeDuration, false);
        }

        /// <summary>
        /// Upgrade attributes based on dice rolls.
        /// </summary>
        void AttributeLevelUp()
        {
            MyChar.maxHealth = MyChar.maxHealth + Random.Range(HPDiceMin, HPDiceMax) + (Constitution / 4);
            MyChar.maxHealth = MyChar.maxHealth + Random.Range(StamDiceMin, StamDiceMax) + (Dexterity / 4);
            ManaMax = ManaMax + Random.Range(ManaDiceMin, ManaDiceMax) + (Intelligence * 2);
        }

        /// <summary>
        /// Link to MagicInput event onUseMana to apply the spell mana cost from the vItem attribute.
        /// </summary>
        /// <param name="ManaCost"></param>
        public void UseMana(int ManaCost)
        {
            Mana -= ManaCost;  // subtract the used mana
            if (ManaSlider)
            {  // UI specified
                ManaSlider.value = Mana;  // update the UI
            }
        }


        /// <summary>
        /// Link to vItemManager event onUseItem to apply potion values to the leveling system mana fields.
        /// </summary>
        /// <param name="DrinkMe"></param>
        public void UsePotion(vItem DrinkMe)
        {
            if (DrinkMe.type == vItemType.Consumable)
            {  // ensure is a potion
                foreach (vItemAttribute viaAttrib in DrinkMe.attributes)
                {  // check for valid attributes
                    switch (viaAttrib.name.ToString())
                    {  // naming is important
                        case "Mana":
                            Mana += viaAttrib.value;  // apply the mana increase
                            ManaSlider.value = Mana;  // update the UI
                            break;
                        case "MaxMana":
                            Mana = ManaMax;  // max out the mana 
                            ManaSlider.value = Mana;  // update the UI
                            break;
                    }
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
