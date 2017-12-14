using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
//using UnityEditor.Events;
#if !VANILLA
using Invector;
using Invector.ItemManager;
#endif

namespace Shadex
{
    /// <summary>
    /// Character display in game menu window.
    /// </summary>
#if !VANILLA
    [vClassHeader("CHARACTER DISPLAY", iconName = "inputIcon")]
    public class CharacterDisplay : vMonoBehaviour
    {
#else
    public class CharacterDisplay : MonoBehaviour {
#endif        
        [Header("Default Positions")]
        /// <summary>Spacing of the rows.</summary>
        [Tooltip("Spacing of the rows")]
        public float Spacing = 2f;

        /// <summary>Default position/size of the header, row height comes from this element.</summary>
        [Tooltip("Default position/size of the header, row height comes from this element")]
        public RectTransform Header;

        /// <summary>Default position/size of the attribute icon, height is ignored.</summary>
        [Tooltip("Default position/size of the attribute icon, height is ignored")]
        public RectTransform AttributeIcon;

        /// <summary>Default position/size of the attribute name, height is ignored.</summary>
        [Tooltip("Default position/size of the attribute name, height is ignored")]
        public RectTransform AttributeName;

        /// <summary>Default position/size of the attribute value, height is ignored.</summary>
        [Tooltip("Default position/size of the attribute value, height is ignored")]
        public RectTransform AttributeValue;

        /// <summary>Default position/size of the spend skill point button, height is ignored.</summary>
        [Tooltip("Default position/size of the spend skill point button, height is ignored")]
        public RectTransform AttributeSpendPlus;

        /// <summary>Invert the insert positions, aka Y=0 then -30 then -60 etc.</summary>
        [Tooltip("Invert the insert positions, aka Y=0 then -30 then -60 etc")]
        public bool InvertPositions = true;

        [Header("Display Pane Options")]
        /// <summary>List of the available panes to dynamically insert attributes into (note all panes must be of equal width but not height.</summary>
        [Tooltip("List of the available panes to dynamically insert attributes into (note all panes must be of equal width but not height")]
        public List<GameObject> Panes = new List<GameObject>();

        /// <summary>Force each section into a different pane.</summary>
        [Tooltip("Force each section into a different pane")]
        public bool OneHeaderPerPane;

        /// <summary>Game object to show/hide.</summary>
        [Tooltip("Game object to show/hide")]
        public GameObject ParentMenu;

#if !VANILLA
        private vInventory control;
#endif
        private CharacterBase levelingSystem;

        private Text DefHeaderTXT;
        private Text DefAttribTXT;
        private Text DefValueTXT;
        private Text DefPlusTXT;

        private string LastSection = "";
        private int RowsInserted;
        private int CurrentPane;
        private int CurrentPaneRows;

        private List<Text> DisplayValues = new List<Text>();
        private List<GameObject> SkillIncreaseButtons = new List<GameObject>();
        private bool ScreenDrawnAlready;
        private int iNextAttribute;

        /// <summary>
        /// Disable the main menu and redraw the character display.
        /// </summary>
        void OnEnable()
        {
            // deactivate main menu
            if (ParentMenu)
            {
                ParentMenu.SetActive(false);
            }

            if (!ScreenDrawnAlready)
            {  // draw the full display on first open
#if !VANILLA
                control = GetComponentInParent<vInventory>();
#endif
                if (Header && AttributeIcon && AttributeName && AttributeValue)
                {
                    DefHeaderTXT = Header.gameObject.GetComponent<Text>();
                    DefAttribTXT = AttributeName.gameObject.GetComponent<Text>();
                    DefValueTXT = AttributeValue.gameObject.GetComponent<Text>();
                    DefPlusTXT = AttributeSpendPlus.gameObject.GetComponent<Text>();
                    if (DefHeaderTXT && DefAttribTXT && DefValueTXT && DefPlusTXT)
                    {
                        GameObject player = GlobalFuncs.FindPlayerInstance();
                        if (player)
                        {
                            levelingSystem = player.GetComponent<CharacterBase>();
                            if (levelingSystem)
                            {  // all required components found, draw the stats with labels
                                CurrentPane = -1;
                                reDrawAttributes();
                                ScreenDrawnAlready = true;
                            }
                            else
                            {
                                if (GlobalFuncs.DEBUGGING_MESSAGES)
                                {
                                    Debug.Log("Player Leveling System NOT found");
                                }
                            }
                        }
                        else
                        {
                            if (GlobalFuncs.DEBUGGING_MESSAGES)
                            {
                                Debug.Log("Player NOT found");
                            }
                        }
                    }
                    else
                    {
                        if (GlobalFuncs.DEBUGGING_MESSAGES)
                        {
                            Debug.Log("GUI Text component is required on the default header/attribute name objects");
                        }
                    }
                }
                else
                {
                    if (GlobalFuncs.DEBUGGING_MESSAGES)
                    {
                        Debug.Log("Default UI rect transforms NOT set");
                    }
                }
            }
            else
            {  // already have the components
                reDrawAttributes();
            }
        }

        /// <summary>
        /// Enable the main menu when this window is closed.
        /// </summary>
        void OnDisable()
        {
            if (ParentMenu)
            {
                ParentMenu.SetActive(true);
            }
        }

        /// <summary>
        /// Detect back key using the inventory input.
        /// </summary>
        void Update()
        {
#if !VANILLA
            if (control)
            {
                if (control.cancel.GetButtonUp())
                {
                    gameObject.SetActive(false);
                }
            }
#else
            if (Input.GetKey("Esc")) {
                gameObject.SetActive(false);
            }
#endif
        }

        /// <summary>
        /// Spend skill point and refresh.
        /// </summary>
        /// <param name="Skill">Skill point to update.</param>
        public void SpendSkillPoint(BaseSkill Skill)
        {
            if (levelingSystem)
            {
                levelingSystem.UnspentSkillPoints -= 1;
                levelingSystem.Skills[levelingSystem.Skills.FindIndex(s => s.Skill == Skill)].Value += 1;
                levelingSystem.reCalcCore(false);
                reDrawAttributes();
            }
        }

        /// <summary>
        /// ReDraw all attributes.
        /// </summary>
        void reDrawAttributes()
        {
            iNextAttribute = 0;
            drawAttribute(levelingSystem.Name, "Alignment", levelingSystem.CurrentAxis.ToString() + " " + levelingSystem.CurrentAlignment.ToString(), false, false);
            drawAttribute(levelingSystem.Name, "Race", levelingSystem.CurrentRace.ToString(), false, false);
            drawAttribute(levelingSystem.Name, "Class", levelingSystem.CurrentClass.ToString(), false, false);
            drawAttribute(levelingSystem.Name, "Level", levelingSystem.CurrentLevel.ToString("#,##0"), false, false);
            drawAttribute(levelingSystem.Name, "XP", levelingSystem.CurrentXP.ToString("#,##0"), false, false);

            drawAttribute("Skills", "Unspent Points", levelingSystem.UnspentSkillPoints.ToString("#,##0"), false, false);
            for (int i = 0; i < levelingSystem.Skills.Count; i++)
            {
                drawAttribute("Skills", levelingSystem.Skills[i].Skill.ToString(), (levelingSystem.Skills[i].Value + levelingSystem.SkillModTotals[i].Value).ToString("#,##0"), false, true);
            }

            drawAttribute("Core", "Life", (levelingSystem.MAXLife + levelingSystem.BonusMAXLife).ToString("#,##0"), true, false);
            drawAttribute("Core", "Life Regen", (levelingSystem.RegenLife + levelingSystem.BonusRegenLife).ToString("#,##0"), false, false);
            drawAttribute("Core", "Mana", (levelingSystem.MAXMana + levelingSystem.BonusMAXMana).ToString("#,##0"), false, false);
            drawAttribute("Core", "Mana Regen", (levelingSystem.RegenMana + levelingSystem.BonusRegenMana).ToString("#,##0"), false, false);
            drawAttribute("Core", "Armour", (levelingSystem.CurrentArmour + levelingSystem.BonusArmour).ToString("#,##0"), false, false);
            drawAttribute("Core", "Stamina", (levelingSystem.MAXStamina + levelingSystem.BonusMAXStamina).ToString("#,##0"), false, false);

            for (int i = 0; i < levelingSystem.Resist.Count; i++)
            {
                drawAttribute("Resistances", levelingSystem.Resist[i].Resist.ToString(), (levelingSystem.Resist[i].Value + levelingSystem.ResistModTotals[i].Value).ToString("#,##0"), false, false);
            }

            for (int i = 0; i < levelingSystem.Collectables.Count; i++)
            {
                drawAttribute("Collectables", levelingSystem.Collectables[i].Type.ToString(), levelingSystem.Collectables[i].Value.ToString("#,##0"), false, false);
            }
        }

        /// <summary>
        /// Draw a specific attribute.
        /// </summary>
        /// <param name="Section">Section group of the attribute.</param>
        /// <param name="Name">Attribute name.</param>
        /// <param name="Value">Current attribute value.</param>
        /// <param name="ForceNewPane">Force new pane to draw in.</param>
        /// <param name="isSkill">Is this an upgradeable skill.</param>
        void drawAttribute(string Section, string Name, string Value, bool ForceNewPane, bool isSkill)
        {
            if (!ScreenDrawnAlready)
            {
                // determine whether new pane needed
                if ((CurrentPane != 0 && OneHeaderPerPane && LastSection != Section) || ((CurrentPaneRows - RowsInserted) < 2) || ForceNewPane)
                {  // one pane per header or out of rows
                    CurrentPane += 1;
                    RowsInserted = 0;

                    // store the number of rows in the new pane
                    CurrentPaneRows = (int)(Panes[CurrentPane].GetComponent<RectTransform>().sizeDelta.y / (Spacing + Header.sizeDelta.y));
                }

                // output header on change
                if (LastSection != Section)
                {  // changed?
                    LastSection = Section;  // keep last for next check    

                    // insert the header
                    GameObject NewHeader = new GameObject();
                    NewHeader.name = "Header_" + Section;
                    NewHeader.transform.SetParent(Panes[CurrentPane].transform);
                    AddRectTransform(NewHeader, Header);
                    AddText(NewHeader, DefHeaderTXT, Section);

                    // increase offset
                    RowsInserted += 1;
                }

                // output the attribute icon
                GameObject NewIcon = new GameObject();
                NewIcon.name = "Icon_" + Name;
                NewIcon.transform.SetParent(Panes[CurrentPane].transform);
                AddRectTransform(NewIcon, AttributeIcon);
                AddIcon(NewIcon, Name);

                // output the attribute name
                GameObject NewTitle = new GameObject();
                NewTitle.name = "Title_" + Name;
                NewTitle.transform.SetParent(Panes[CurrentPane].transform);
                AddRectTransform(NewTitle, AttributeName);
                AddText(NewTitle, DefAttribTXT, Name);

                // output the attribute value
                GameObject NewValue = new GameObject();
                NewValue.name = "Value_" + Name;
                NewValue.transform.SetParent(Panes[CurrentPane].transform);
                AddRectTransform(NewValue, AttributeValue);
                DisplayValues.Add(AddText(NewValue, DefValueTXT, Value.ToString()));

                // output increase skill button
                if (isSkill)
                {
                    // button base
                    GameObject NewSkillIncreaseButton = new GameObject();
                    NewSkillIncreaseButton.name = "SkillIncrease_" + Name;
                    NewSkillIncreaseButton.transform.parent = Panes[CurrentPane].transform;
                    AddRectTransform(NewSkillIncreaseButton, AttributeSpendPlus);
                    AddText(NewSkillIncreaseButton, DefPlusTXT, DefPlusTXT.text);

                    // sub selector graphic (clone)
                    GameObject NewSkillSelector = Instantiate(AttributeSpendPlus.transform.Find("selector").gameObject);
                    NewSkillSelector.transform.SetParent(NewSkillIncreaseButton.transform, false);
                    NewSkillSelector.name = "selector";

                    // link selectable image script
                    Selectable NewSkillSelectScript = NewSkillIncreaseButton.AddComponent<Selectable>();
                    NewSkillSelectScript.image = NewSkillSelector.GetComponent<Image>();

                    // add the spend type script
                    CharacterSpendSkillPoint NewSpendSkillPoint = NewSkillIncreaseButton.AddComponent<CharacterSpendSkillPoint>();
                    NewSpendSkillPoint.Type = (BaseSkill)System.Enum.Parse(typeof(BaseSkill), Name);

                    // add the event trigger                    
                    EventTrigger NewEventTrigger = NewSkillIncreaseButton.AddComponent<EventTrigger>();
                    List<EventTrigger.Entry> triggers = new List<EventTrigger.Entry>();

                    // create pointer click
                    EventTrigger.Entry clickEventHandler = new EventTrigger.Entry();
                    clickEventHandler.eventID = EventTriggerType.PointerClick;
                    triggers.Add(clickEventHandler);

                    // create submit press
                    EventTrigger.Entry submitEventHandler = new EventTrigger.Entry();
                    submitEventHandler.eventID = EventTriggerType.Submit;
                    triggers.Add(submitEventHandler);

                    // apply links to the event trigger 
                    NewEventTrigger.triggers = triggers;
//#if UNITY_EDITOR
//                    //UnityEventTools.AddPersistentListener(clickEventHandler.callback, NewSpendSkillPoint.OnClick);
//                    //UnityEventTools.AddPersistentListener(submitEventHandler.callback, NewSpendSkillPoint.OnClick);
//#else
//#endif
                    clickEventHandler.callback.AddListener(NewSpendSkillPoint.OnClick);
                    submitEventHandler.callback.AddListener(NewSpendSkillPoint.OnClick);

                    // add to the refresh array
                    SkillIncreaseButtons.Add(NewSkillIncreaseButton);

                    // set visibility
                    NewSkillIncreaseButton.SetActive(levelingSystem.UnspentSkillPoints > 0);
                }
                else
                {  // not a skill, insert null for rapid access via same length arrays
                    SkillIncreaseButtons.Add(null);
                }

                // increase offset
                RowsInserted += 1;
            }
            else
            {  // drawn already, just update the value
                DisplayValues[iNextAttribute].text = Value;
                if (isSkill)
                {  // show the skill increase button, if skill points to spend
                    SkillIncreaseButtons[iNextAttribute].SetActive(levelingSystem.UnspentSkillPoints > 0);
                }
                iNextAttribute += 1;
            }
        }

        /// <summary>
        /// Add cloned rect transform element.
        /// </summary>
        /// <param name="AddRectToMe">Where to add the rect transform.</param>
        /// <param name="CloneRect">Rect transform to clone.</param>
        void AddRectTransform(GameObject AddRectToMe, RectTransform CloneRect)
        {
            RectTransform RectNew = AddRectToMe.AddComponent<RectTransform>();
            RectNew.anchoredPosition3D = new Vector3(CloneRect.anchoredPosition3D.x, Header.anchoredPosition3D.y - (RowsInserted * (Spacing + Header.sizeDelta.y)), CloneRect.anchoredPosition3D.z);
            RectNew.sizeDelta = new Vector2(CloneRect.sizeDelta.x, CloneRect.sizeDelta.y);
            RectNew.localScale = Vector3.one;
            RectNew.pivot = CloneRect.pivot;
            RectNew.anchorMin = CloneRect.anchorMin;
            RectNew.anchorMax = CloneRect.anchorMax;
            AddRectToMe.AddComponent<CanvasRenderer>();
        }

        /// <summary>
        /// Add cloned text element.
        /// </summary>
        /// <param name="AddTextToMe">Where to add the clone to.</param>
        /// <param name="CloneFrom">Text element to clone from.</param>
        /// <param name="DisplayText">Text to write into the element.</param>
        /// <returns></returns>
        Text AddText(GameObject AddTextToMe, Text CloneFrom, string DisplayText)
        {
            Text TextNew = AddTextToMe.AddComponent<Text>();
            TextNew.text = DisplayText;
            TextNew.fontSize = CloneFrom.fontSize;
            TextNew.fontStyle = CloneFrom.fontStyle;
            TextNew.font = CloneFrom.font;
            TextNew.alignment = CloneFrom.alignment;
            TextNew.resizeTextForBestFit = true;
            return TextNew;
        }

        /// <summary>
        /// Add cloned icon from resources.
        /// </summary>
        /// <param name="AddIconToMe">Where to add the icon.</param>
        /// <param name="IconName">Name of the icon to load.</param>
        void AddIcon(GameObject AddIconToMe, string IconName)
        {
            Image ImageNew = AddIconToMe.AddComponent<Image>();
            Sprite SpriteNew = Resources.Load<Sprite>("MagicAttributes/" + IconName);
            if (SpriteNew)
            {
                ImageNew.sprite = SpriteNew;
            }
            else
            {
                if (GlobalFuncs.DEBUGGING_MESSAGES)
                {
                    Debug.Log("Failed to load " + IconName + ".png");
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
