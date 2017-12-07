using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace Shadex
{
    /// <summary>
    /// Custom inspector for the character base class
    /// </summary>
    public class CharacterBaseEditor : Editor
    {
        internal bool bOpenCloseWindow = false;
        internal bool bOpenDataHelpers = false;
        internal bool bShowCore = true;
        internal bool bShowSkillPoints = true;
        internal bool bShowResistances = true;
        internal bool bShowCollectables = true;
        internal bool bShowConditions = true;
        internal BaseIncrease CurrentIncrease = BaseIncrease.Fives;
        internal List<SimpleDataPair> SaveSlots;
        internal List<SimpleDataPair> SaveGames;
        internal int iNewValue;

#if !VANILLA
        public GUISkin skin;
        public GUISkin oldSkin;
        public Texture2D m_Logo;

        protected void OnEnable()
        {
            skin = Resources.Load("skin") as GUISkin;
            skin.label.alignment = TextAnchor.UpperLeft;
            m_Logo = (Texture2D)Resources.Load("ladderIcon", typeof(Texture2D));
        }
#endif
        /// <summary>
        /// Inspector custom GUI override
        /// </summary>
        public override void OnInspectorGUI()
        {
#if !VANILLA
            oldSkin = GUI.skin;
            if (skin) GUI.skin = skin;
#endif
            GUILayout.BeginVertical("CHARACTER ATTRIBUTES", "window");
#if !VANILLA
            GUILayout.Label(m_Logo, GUILayout.MaxHeight(25));
#endif
            // grab the underlying class instance
            CharacterBase cc = (CharacterBase)target;

            // show values + modifier
            bOpenCloseWindow = GUILayout.Toggle(bOpenCloseWindow, bOpenCloseWindow ? "Close" : "Open", EditorStyles.toolbarButton);
            if (bOpenCloseWindow)
            {
                // reset to defaults
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Reset to Defaults", GUILayout.ExpandWidth(true)))
                {
                    cc.ConditionsRoot = cc.transform.Find("Conditions");
                    cc.ResetToDefaults();
                }
                GUILayout.EndHorizontal();

                // increase by
                GUILayout.BeginHorizontal();
#if !VANILLA
                GUI.skin = oldSkin;
#endif
                GUILayout.Label("Increase By", GUILayout.Width(75));
                CurrentIncrease = (BaseIncrease)EditorGUILayout.EnumPopup(CurrentIncrease, GUILayout.ExpandWidth(true));
#if !VANILLA
                GUI.skin = skin;
#endif
                GUILayout.EndHorizontal();

                // core foldout
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.BeginHorizontal();
#if !VANILLA
                GUI.skin = oldSkin;
#endif
                bShowCore = GUILayout.Toggle(bShowCore, "Core Attributes", "Foldout", GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();
                if (bShowCore)
                {
                    GUICoreDisplay(ref cc);
                }
#if !VANILLA
                GUI.skin = skin;
#endif
                GUILayout.EndVertical();

                // collectables foldout
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.BeginHorizontal();
#if !VANILLA
                GUI.skin = oldSkin;
#endif
                bShowCollectables = GUILayout.Toggle(bShowCollectables, "Collectables", "Foldout", GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();
                if (bShowCollectables)
                {
                    for (int i = 0; i < cc.Collectables.Count; i++)
                    {
                        // name
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(cc.Collectables[i].Type.ToString(), GUILayout.Width(75));

                        // allow value modification
                        GUIGenericValueDisplay(ref cc, ref cc.Collectables[i].Value, 0, -1);

                        // current value
                        GUI.skin.label.alignment = TextAnchor.UpperRight;
                        GUILayout.Label(cc.Collectables[i].Value.ToString("#,##0"), GUILayout.ExpandWidth(true));
                        GUI.skin.label.alignment = TextAnchor.UpperLeft;
                        GUILayout.EndHorizontal();

                        // only the player sees the collectable prefabs config as are global
                        if (cc.gameObject.tag == "Player")
                        {
                            // temp initialise for our existing scenes
                            if (cc.Collectables[i].Spawns == null)
                            {
                                cc.Collectables[i].Spawns = new List<CollectablePrefab>();
                            }  // remove b4 release

                            // list all prefabs
                            for (int s = 0; s < cc.Collectables[i].Spawns.Count; s++)
                            {
                                GUILayout.BeginHorizontal();
                                GUILayout.Space(10);

                                // prefab
                                GUILayout.Label("Prefab", GUILayout.Width(65));
                                cc.Collectables[i].Spawns[s].Prefab = EditorGUILayout.ObjectField(cc.Collectables[i].Spawns[s].Prefab, typeof(GameObject), false) as GameObject;

                                // amount
                                GUILayout.Label(" = ", GUILayout.Width(20));
                                cc.Collectables[i].Spawns[s].Amount = (int)EditorGUILayout.FloatField(cc.Collectables[i].Spawns[s].Amount, GUILayout.Width(35));
                                if (cc.Collectables[i].Spawns[s].Amount < 1) cc.Collectables[i].Spawns[s].Amount = 1;

                                // remove
                                if (GUILayout.Button("X", GUILayout.Width(30)))
                                {
                                    cc.Collectables[i].Spawns.RemoveAt(s);
                                    break;  // drop out till next editor frame
                                }
                                GUILayout.EndHorizontal();
                            }

                            // add more
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(75);
                            if (GUILayout.Button("Add Prefab/Value", GUILayout.ExpandWidth(true)))
                            {
                                cc.Collectables[i].Spawns.Add(new CollectablePrefab());
                            }
                            GUILayout.EndHorizontal();
                        }
                    }
                }
#if !VANILLA
                GUI.skin = skin;
#endif
                GUILayout.EndVertical();


                // skill points foldout
                if (cc.Skills != null)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.BeginHorizontal();
#if !VANILLA
                    GUI.skin = oldSkin;
#endif
                    bShowSkillPoints = GUILayout.Toggle(bShowSkillPoints, "Skill Points", "Foldout", GUILayout.ExpandWidth(true));
                    GUILayout.EndHorizontal();
                    if (bShowSkillPoints)
                    {
                        // available skill points
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Unspent", GUILayout.Width(75));
                        if (cc.UnspentSkillPoints > 0)
                        {
                            if (GUILayout.Button("Seq", GUILayout.Width(64)))
                            {
                                cc.DistributePoints(false);
                            }
                            if (GUILayout.Button("Rnd", GUILayout.Width(64)))
                            {
                                cc.DistributePoints(true);
                            }
                        }
                        GUILayout.Label(cc.UnspentSkillPoints.ToString(), GUILayout.ExpandWidth(true));
                        GUILayout.EndHorizontal();

                        // skill point list                
                        for (int i = 0; i < cc.Skills.Count; i++)
                        {
                            GUILayout.BeginHorizontal();
                            GUISkillPointDisplay(ref cc, ref i);
                            GUILayout.EndHorizontal();
                        }
                    }
#if !VANILLA
                    GUI.skin = skin;
#endif
                    GUILayout.EndVertical();
                }

                // resistances foldout
                if (cc.Resist != null)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.BeginHorizontal();
#if !VANILLA
                    GUI.skin = oldSkin;
#endif
                    bShowResistances = GUILayout.Toggle(bShowResistances, "Resistances", "Foldout", GUILayout.ExpandWidth(true));
                    GUILayout.EndHorizontal();
                    if (bShowResistances)
                    {
                        for (int i = 0; i < cc.Resist.Count; i++)
                        {
                            // attribute name
                            GUILayout.BeginHorizontal();
                            GUILayout.Label(cc.Resist[i].Resist.ToString(), GUILayout.Width(75));

                            // allow value modification
                            GUIGenericValueDisplay(ref cc, ref cc.Resist[i].Value, 0, 100);

                            // current value
                            GUI.skin.label.alignment = TextAnchor.UpperRight;
                            GUILayout.Label("(" + (cc.ResistModTotals[i].Value > 0 ? "+" : "") + cc.ResistModTotals[i].Value.ToString() + ") " + cc.Resist[i].Value.ToString() + '%', GUILayout.ExpandWidth(true));
                            GUI.skin.label.alignment = TextAnchor.UpperLeft;
                            GUILayout.EndHorizontal();
                        }
                    }
#if !VANILLA
                    GUI.skin = skin;
#endif
                    GUILayout.EndVertical();


                }

                // conditions foldout
                if (cc.Conditions != null)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.BeginHorizontal();
#if !VANILLA
                    GUI.skin = oldSkin;
#endif
                    bShowConditions = GUILayout.Toggle(bShowConditions, "Elemental Conditions", "Foldout", GUILayout.ExpandWidth(true));
                    GUILayout.EndHorizontal();
                    if (bShowConditions)
                    {
                        for (int i = 0; i < cc.Conditions.Count; i++)
                        {
                            if (cc.Conditions[i].Type != BaseDamage.Physical)  // ignore physical
                            {
                                // attribute name
                                GUILayout.BeginHorizontal();
                                GUILayout.Label(cc.Conditions[i].Type.ToString(), GUILayout.Width(75));

                                // display 
                                //GUILayout.Label("Display", GUILayout.Width(65));
                                cc.Conditions[i].Display = EditorGUILayout.ObjectField(cc.Conditions[i].Display, typeof(GameObject), false) as GameObject;
                                GUILayout.EndHorizontal();
                            }
                        }
                    }
#if !VANILLA
                    GUI.skin = skin;
#endif
                    GUILayout.EndVertical();


                }

                // data layer helper functions
                if (cc.gameObject.tag == "Player")
                {
                    bOpenDataHelpers = GUILayout.Toggle(bOpenDataHelpers, bOpenDataHelpers ? "Close Data Layer Helpers" : "Open Data Layer Helpers", EditorStyles.toolbarButton);
                    if (bOpenDataHelpers)
                    {
                        // attempt find data layer
                        if (!GlobalFuncs.TheDatabase())
                        {
                            GUILayout.Label("Data layer NOT found..", GUILayout.ExpandWidth(true));
                            GUILayout.Label("Add a data layer connector..", GUILayout.ExpandWidth(true));
                            GUILayout.Label("E.g. CharacterDataSQLLite!", GUILayout.ExpandWidth(true));
                        }
                        else
                        {
                            // reset to defaults
                            GUILayout.BeginHorizontal();
                            if (GUILayout.Button("Validate DB", GUILayout.ExpandWidth(true)))
                            {
                                GlobalFuncs.TheDatabase().ValidateDatabase(false);
                            }
                            if (GUILayout.Button("Wipe DB", GUILayout.ExpandWidth(true)))
                            {
                                GlobalFuncs.TheDatabase().ValidateDatabase(true);
                                SaveSlots = null;  // force reload
                                SaveGames = null;  // of slot/save shortlists
                                cc.SaveSlotID = 0;
                                cc.LastSaveGameID = 0;
                            }
                            GUILayout.EndHorizontal();

                            //// slot choice
                            //GUILayout.BeginHorizontal();
                            //GUILayout.Label("Save Slot", GUILayout.Width(60));
                            //if (SaveSlots == null) {
                            //    SaveSlots = GlobalFuncs.TheDatabase().GetShortList("SaveSlots", "ID", new string[] { "CharacterName", "CreatedOn" }, "CreatedOn", true, "", "", true, " :: ");
                            //}
                            //iNewValue = GlobalFuncs.TheDatabase().SimpleDataGUIPopup(ref SaveSlots, cc.SaveSlotID, GUILayout.ExpandWidth(true));
                            //if (cc.SaveSlotID != iNewValue) {
                            //    cc.SaveSlotID = iNewValue;
                            //    SaveGames = null;  // force reload of dependant save game list
                            //}

                            //// save choice dependant upon slot
                            //GUILayout.Label("Save Game", GUILayout.Width(75));
                            //if (SaveGames == null) {
                            //    SaveGames = GlobalFuncs.TheDatabase().GetShortList("SaveGames", "ID", new string[] { "CreatedOn" }, "CreatedOn", true, "SlotID", cc.SaveSlotID.ToString(), true, " :: ");
                            //}
                            //cc.LastSaveGameID = GlobalFuncs.TheDatabase().SimpleDataGUIPopup(ref SaveGames, cc.LastSaveGameID, GUILayout.ExpandWidth(true));
                            //GUILayout.EndHorizontal();

                            //GUILayout.BeginHorizontal();
                            //if (cc.LastSaveGameID > 0) {
                            //    if (GUILayout.Button("Load", GUILayout.ExpandWidth(true))) {
                            //        GlobalFuncs.TheDatabase().LoadPlayerState(ref cc, false, cc.LastSaveGameID);
                            //    }
                            //}
                            //if (GUILayout.Button("Save", GUILayout.ExpandWidth(true))) {
                            //    cc.LastSaveGameID = GlobalFuncs.TheDatabase().SavePlayerState(ref cc, SceneManager.GetActiveScene().name, (cc.LastSaveGameID > 0 ? cc.LastSaveGameID : - 1));
                            //    SaveSlots = null;  // force reload
                            //    SaveGames = null;  // of slot/save shortlists
                            //}
                            //GUILayout.EndHorizontal();
                        }
                    }
                }
            }
            GUILayout.EndVertical();
#if !VANILLA
            GUI.skin = oldSkin;
#endif
        }  

        protected void GUICoreDisplay(ref CharacterBase cc)
        {
            // name of the character
            GUILayout.BeginHorizontal();
            GUILayout.Label("Name", GUILayout.Width(75));
            cc.Name = GUILayout.TextField(cc.Name, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            // axis
            GUILayout.BeginHorizontal();
            GUILayout.Label("Axis", GUILayout.Width(75));
            BaseAxis NewAxis = (BaseAxis)EditorGUILayout.EnumPopup(cc.CurrentAxis, GUILayout.ExpandWidth(true));
            if (cc.CurrentAxis != NewAxis)
            {
                cc.CurrentAxis = NewAxis;
                cc.RebuildModifiers();
            }
            GUILayout.EndHorizontal();

            // alignment
            if (cc.CurrentAxis != BaseAxis.Neutral)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Alignment", GUILayout.Width(75));
                BaseAlignment NewAlignment = (BaseAlignment)EditorGUILayout.EnumPopup(cc.CurrentAlignment, GUILayout.ExpandWidth(true));
                if (cc.CurrentAlignment != NewAlignment)
                {
                    cc.CurrentAlignment = NewAlignment;
                    cc.RebuildModifiers();
                }
                GUILayout.EndHorizontal();
            }

            // race
            GUILayout.BeginHorizontal();
            GUILayout.Label("Race", GUILayout.Width(75));
            BaseRace NewRace = (BaseRace)EditorGUILayout.EnumPopup(cc.CurrentRace, GUILayout.ExpandWidth(true));
            if (cc.CurrentRace != NewRace)
            {
                cc.CurrentRace = NewRace;
                cc.RebuildModifiers();
            }
            GUILayout.EndHorizontal();

            // class
            GUILayout.BeginHorizontal();
            GUILayout.Label("Class", GUILayout.Width(75));
            BaseClass NewClass = (BaseClass)EditorGUILayout.EnumPopup(cc.CurrentClass, GUILayout.ExpandWidth(true));
            if (cc.CurrentClass != NewClass)
            {
                cc.CurrentClass = NewClass;
                cc.RebuildModifiers();
            }
            GUILayout.EndHorizontal();

            // life
            GUILayout.BeginHorizontal();
            GUILayout.Label("Life", GUILayout.Width(75));
            GUI.skin.label.alignment = TextAnchor.UpperRight;
            GUILayout.Label(cc.CurrentLife.ToString("#,##0") + "/" + (cc.MAXLife + cc.BonusMAXLife).ToString("#,##0"), GUILayout.ExpandWidth(true));
            GUI.skin.label.alignment = TextAnchor.UpperLeft;
            GUILayout.EndHorizontal();

            // life regen
            GUILayout.BeginHorizontal();
            GUILayout.Label("Life Regen", GUILayout.Width(75));
            GUIGenericValueDisplay(ref cc, ref cc.RegenLife, 0, 999);
            GUI.skin.label.alignment = TextAnchor.UpperRight;
            GUILayout.Label((cc.RegenLife + cc.BonusRegenLife).ToString() + " per sec", GUILayout.ExpandWidth(true));
            GUI.skin.label.alignment = TextAnchor.UpperLeft;
            GUILayout.EndHorizontal();

            // mana
            GUILayout.BeginHorizontal();
            GUILayout.Label("Mana", GUILayout.Width(75));
            GUI.skin.label.alignment = TextAnchor.UpperRight;
            GUILayout.Label(cc.CurrentMana.ToString("#,##0") + "/" + (cc.MAXMana + cc.BonusMAXMana).ToString("#,##0"), GUILayout.ExpandWidth(true));
            GUI.skin.label.alignment = TextAnchor.UpperLeft;
            GUILayout.EndHorizontal();

            // mana regen
            GUILayout.BeginHorizontal();
            GUILayout.Label("Mana Regen", GUILayout.Width(75));
            GUIGenericValueDisplay(ref cc, ref cc.RegenMana, 0, 999);
            GUI.skin.label.alignment = TextAnchor.UpperRight;
            GUILayout.Label((cc.RegenMana + cc.BonusRegenMana).ToString() + " per sec", GUILayout.ExpandWidth(true));
            GUI.skin.label.alignment = TextAnchor.UpperLeft;
            GUILayout.EndHorizontal();


            // stamina
            GUILayout.BeginHorizontal();
            GUILayout.Label("Stamina", GUILayout.Width(75));
            GUI.skin.label.alignment = TextAnchor.UpperRight;
            GUILayout.Label((cc.MAXStamina + cc.BonusMAXStamina).ToString("#,##0"), GUILayout.ExpandWidth(true));  //cc.CurrentStamina.ToString("#,##0") + "/" +
            GUI.skin.label.alignment = TextAnchor.UpperLeft;
            GUILayout.EndHorizontal();

            // armour
            GUILayout.BeginHorizontal();
            GUILayout.Label("Armour", GUILayout.Width(75));
            GUI.skin.label.alignment = TextAnchor.UpperRight;
            GUILayout.Label("Base " + cc.CurrentArmour.ToString("#,##0") + " Equipped " + cc.BonusArmour.ToString("#,##0"), GUILayout.ExpandWidth(true));
            GUI.skin.label.alignment = TextAnchor.UpperLeft;
            GUILayout.EndHorizontal();

            // weight
            GUILayout.BeginHorizontal();
            GUILayout.Label("EquipLoad", GUILayout.Width(75));
            GUI.skin.label.alignment = TextAnchor.UpperRight;
            GUILayout.Label(cc.CurrentEquipLoad.ToString("#,##0") + "/" + cc.MAXEquipLoad.ToString("#,##0"), GUILayout.ExpandWidth(true));
            GUI.skin.label.alignment = TextAnchor.UpperLeft;
            GUILayout.EndHorizontal();



            // XP & next level
            GUILayout.BeginHorizontal();
            GUILayout.Label("XP", GUILayout.Width(75));
            GUI.skin.label.alignment = TextAnchor.UpperRight;
            GUILayout.Label(cc.CurrentXP.ToString("#,##0") + "/" + cc.XPToNextLevel.ToString("#,##0"), GUILayout.ExpandWidth(true));
            GUI.skin.label.alignment = TextAnchor.UpperLeft;
            GUILayout.EndHorizontal();

            // current level
            GUILayout.BeginHorizontal();
            GUILevelDisplay(ref cc);
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Current level, XP, XP to next level and modifier buttons.
        /// </summary>
        /// <param name="cc">Instance of the leveling system.</param>
        protected void GUILevelDisplay(ref CharacterBase cc)
        {
            GUILayout.Label("Level", GUILayout.Width(75));

            // decrease level by five
            if (cc.CurrentLevel > (int)CurrentIncrease)
            {
                if (GUILayout.Button("-" + (int)CurrentIncrease, GUILayout.Width(40)))
                {
                    cc.CurrentLevel -= (int)CurrentIncrease;  // reduce the level
                    cc.CurrentXP = cc.CalcXPToNextLevel(cc.CurrentLevel - 1);  // update the current XP amount
                    cc.XPToNextLevel = cc.CalcXPToNextLevel(cc.CurrentLevel);  // update the level up XP goal

                    cc.UnspentSkillPoints -= (CharacterDefaults.LEVELUP_SKILL_POINTS * (int)CurrentIncrease);  // decrease available points

                    if (cc.UnspentSkillPoints < 0)
                    {  // level reduction skill points applied already?
                        for (int j = 0; j < (CharacterDefaults.LEVELUP_SKILL_POINTS * (int)CurrentIncrease); j++)
                        {  // reloop for when all points applied to one attribute
                            if (cc.UnspentSkillPoints == 0) break;  // work complete when unspent no longer negative

                            for (int i = 0; i < cc.Skills.Count; i++)
                            {  // process all attributes
                                if (cc.Skills[i].Value > 0)
                                {  // above the min?
                                    cc.Skills[i].Value -= 1;  // remove a skill point
                                    cc.UnspentSkillPoints += 1;  // increase the unspent
                                }
                                if (cc.UnspentSkillPoints == 0) break;  // work complete when unspent no longer negative
                            }
                        }
                    }

                    cc.reCalcCore(true);  // update the core (life, mana, etc) stats
                }
            }
            else
            {
                GUILayout.Space(44);
            }

            // decrease level by one
            if (cc.CurrentLevel > 1)
            {
                if (GUILayout.Button("-", GUILayout.Width(40)))
                {
                    cc.CurrentLevel -= 1;  // reduce the level
                    cc.CurrentXP = cc.CalcXPToNextLevel(cc.CurrentLevel - 1);  // update the current XP amount
                    cc.XPToNextLevel = cc.CalcXPToNextLevel(cc.CurrentLevel);  // update the level up XP goal

                    cc.UnspentSkillPoints -= CharacterDefaults.LEVELUP_SKILL_POINTS;  // decrease available points

                    if (cc.UnspentSkillPoints < 0)
                    {  // level reduction skill points applied already?
                        for (int j = 0; j < CharacterDefaults.LEVELUP_SKILL_POINTS; j++)
                        {  // reloop for when all points applied to one attribute
                            if (cc.UnspentSkillPoints == 0) break;  // work complete when unspent no longer negative

                            for (int i = 0; i < cc.Skills.Count; i++)
                            {  // process all attributes
                                if (cc.Skills[i].Value > 0)
                                {  // above the min?
                                    cc.Skills[i].Value -= 1;  // remove a skill point
                                    cc.UnspentSkillPoints += 1;  // increase the unspent
                                }
                                if (cc.UnspentSkillPoints == 0) break;  // work complete when unspent no longer negative
                            }
                        }
                    }

                    cc.reCalcCore(true);  // update the core (life, mana, etc) stats
                }
            }
            else
            {
                GUILayout.Space(44);
            }

            // add one level
            if (cc.CurrentLevel < CharacterDefaults.MAXLEVEL)
            {
                if (GUILayout.Button("+", GUILayout.Width(40)))
                {
                    cc.CurrentLevel += 1;  // increase the level
                    cc.CurrentXP = cc.CalcXPToNextLevel(cc.CurrentLevel - 1);  // update the current XP amount
                    cc.XPToNextLevel = cc.CalcXPToNextLevel(cc.CurrentLevel);  // update the level up XP goal
                    cc.UnspentSkillPoints += CharacterDefaults.LEVELUP_SKILL_POINTS;  // increase available points
                    cc.reCalcCore(true);  // update the core (life, mana, etc) stats
                }
            }
            else
            {
                GUILayout.Space(44);
            }

            // add 5 levels
            if (cc.CurrentLevel + (int)CurrentIncrease < CharacterDefaults.MAXLEVEL)
            {
                if (GUILayout.Button("+" + (int)CurrentIncrease, GUILayout.Width(40)))
                {
                    cc.CurrentLevel += (int)CurrentIncrease;  // increase the level
                    cc.CurrentXP = cc.CalcXPToNextLevel(cc.CurrentLevel - 1);  // update the current XP amount
                    cc.XPToNextLevel = cc.CalcXPToNextLevel(cc.CurrentLevel);  // update the level up XP goal
                    cc.UnspentSkillPoints += (CharacterDefaults.LEVELUP_SKILL_POINTS * (int)CurrentIncrease);  // increase available points
                    cc.reCalcCore(true);  // update the core (life, mana, etc) stats
                }
            }
            else
            {
                GUILayout.Space(44);
            }

            // actual character level
            GUI.skin.label.alignment = TextAnchor.UpperRight;
            GUILayout.Label(cc.CurrentLevel.ToString(), GUILayout.ExpandWidth(true));
            GUI.skin.label.alignment = TextAnchor.UpperLeft;

        }

        /// <summary>
        /// Skill point name, value & point distribution.
        /// </summary>
        /// <param name="cc">Instance of the leveling system.</param>
        /// <param name="i">Loop iterator.</param>
        protected void GUISkillPointDisplay(ref CharacterBase cc, ref int i)
        {
            // attribute name
            GUILayout.Label(cc.Skills[i].Skill.ToString(), GUILayout.Width(75));

            // remove 5 points from this attribute
            if (cc.Skills[i].Value - (int)CurrentIncrease > 0)
            {
                if (GUILayout.Button("-" + (int)CurrentIncrease, GUILayout.Width(40)))
                {
                    cc.Skills[i].Value -= (int)CurrentIncrease;
                    cc.UnspentSkillPoints += (int)CurrentIncrease;
                    cc.reCalcCore(true);
                }
            }
            else
            {
                GUILayout.Space(44);
            }

            // remove single skill point from this attribute
            if (cc.Skills[i].Value > 0)
            {
                if (GUILayout.Button("-", GUILayout.Width(40)))
                {
                    cc.Skills[i].Value -= 1;
                    cc.UnspentSkillPoints += 1;
                    cc.reCalcCore(true);
                }
            }
            else
            {
                GUILayout.Space(44);
            }

            // add single unspent point to this attribute                 
            if (cc.UnspentSkillPoints > 0 && cc.Skills[i].Value < CharacterDefaults.SKILLS_MAX_VALUE)
            {
                if (GUILayout.Button("+", GUILayout.Width(40)))
                {
                    cc.Skills[i].Value += 1;
                    cc.UnspentSkillPoints -= 1;
                    cc.reCalcCore(true);
                }
            }
            else
            {
                GUILayout.Space(44);
            }

            // add 5 unspent points to this attribute
            if (cc.UnspentSkillPoints >= (int)CurrentIncrease && cc.Skills[i].Value + (int)CurrentIncrease < CharacterDefaults.SKILLS_MAX_VALUE)
            {
                if (GUILayout.Button("+" + (int)CurrentIncrease, GUILayout.Width(40)))
                {
                    cc.Skills[i].Value += (int)CurrentIncrease;
                    cc.UnspentSkillPoints -= (int)CurrentIncrease;
                    cc.reCalcCore(true);
                }
            }
            else
            {
                GUILayout.Space(44);
            }


            // current value
            GUI.skin.label.alignment = TextAnchor.UpperRight;
            GUILayout.Label("(" + (cc.SkillModTotals[i].Value > 0 ? "+" : "") + cc.SkillModTotals[i].Value.ToString() + ") " + cc.Skills[i].Value.ToString(), GUILayout.ExpandWidth(true));
            GUI.skin.label.alignment = TextAnchor.UpperLeft;
        }

        /// <summary>
        /// Generic value modifier with buttons to change the value.
        /// </summary>
        /// <param name="cc">Instance of the leveling system.</param>
        /// <param name="CurrentValue">Current value of the modifier.</param>
        /// <param name="Min">Minimum range.</param>
        /// <param name="Max">Maximum range.</param>
        /// <returns></returns>
        protected bool GUIGenericValueDisplay(ref CharacterBase cc, ref float CurrentValue, float Min, float Max)
        {
            // remove 5 from this 
            if (CurrentValue - (int)CurrentIncrease > Min)
            {
                if (GUILayout.Button("-" + (int)CurrentIncrease, GUILayout.Width(40)))
                {
                    CurrentValue -= (int)CurrentIncrease;
                    return true;
                }
            }
            else
            {
                GUILayout.Space(44);
            }

            // remove 1 from this 
            if (CurrentValue > Min)
            {
                if (GUILayout.Button("-", GUILayout.Width(40)))
                {
                    CurrentValue -= 1;
                    return true;
                }
            }
            else
            {
                GUILayout.Space(44);
            }

            // add 1 to this 
            if (Max == -1 || CurrentValue < Max)
            {
                if (GUILayout.Button("+", GUILayout.Width(40)))
                {
                    CurrentValue += 1;
                    return true;
                }
            }
            else
            {
                GUILayout.Space(44);
            }

            // add 5 to this 
            if (Max == -1 || CurrentValue + (int)CurrentIncrease < Max)
            {
                if (GUILayout.Button("+" + (int)CurrentIncrease, GUILayout.Width(40)))
                {
                    CurrentValue += (int)CurrentIncrease;
                    return true;
                }
            }
            else
            {
                GUILayout.Space(44);
            }
            return false;
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
