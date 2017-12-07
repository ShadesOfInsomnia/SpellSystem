using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using System;
using System.Linq;

namespace Shadex
{
    /// <summary>
    /// SQLLite data layer instance
    /// </summary>
    public class CharacterDataEasySave2 : CharacterDataBase
    {
        #region "Abstract method overrides"

        /// <summary>Path to the local easy save 2 database.</summary>
        string localDB;

        /// <summary>
        /// Initialise the base class and create the easy save to path.
        /// </summary>
        public override void Start()
        {
            base.Start();
            localDB = Application.dataPath + LOCAL_DATABASE_PATH + "/" + DATABASE_NAME + "ES2.txt";
        }

        /// <summary>
        /// Check for database upgrades. 
        /// </summary>
        /// <param name="Clear">Empty the database of data.</param>
        public override void ValidateDatabase(bool Clear)
        {
            Start();
            if (Clear)
            {
                ES2.Delete(localDB);
            }
            if (!ES2.Exists(localDB))
            {
                using (ES2Writer writer = ES2Writer.Create(localDB))
                {
                    writer.Write("1.0", "version");
                    writer.Save();
                }
            }
        } // check for database upgrades 

        /// <summary>
        /// Load the header data array
        /// </summary>
        /// <param name="SaveGameID">ID of the save game being loaded.</param>
        /// <param name="SceneName">Return the name of the scene to load.</param>
        /// <param name="Data">Return the character data in list form.</param>
        protected override void LoadHeader(int SaveGameID, ref string SceneName, ref List<SimpleDataHeader> Data)
        {
            // load slot data list
            var AllSlots = new Dictionary<int, string>();
            if (ES2.Exists(localDB + "?tag=SaveSlots"))
            {
                AllSlots = ES2.LoadDictionary<int, string>(localDB + "?tag=SaveSlots");
            }

            // load save game data list
            var AllSaveGames = new Dictionary<int, string>();
            if (ES2.Exists(localDB + "?tag=SaveGames"))
            {
                AllSaveGames = ES2.LoadDictionary<int, string>(localDB + "?tag=SaveGames");
            }

            // extract the save game JSON to simpleData
            List<SimpleDataHeader> SaveGameData = JSONToHeader(AllSaveGames[SaveGameID], SimpleDataLevel.SaveGame);

            // find the slot/scene ids from this save game
            int SlotID = Convert.ToInt32(SaveGameData.Find(s => s.Name == "SlotID").Value);
            int SceneID = Convert.ToInt32(SaveGameData.Find(s => s.Name == "SceneID").Value);

            // find the scene name to load
            var AllScenes = new Dictionary<int, string>();
            if (ES2.Exists(localDB + "?tag=Scenes"))
            {
                AllScenes = ES2.LoadDictionary<int, string>(localDB + "?tag=Scenes");
            }
            SceneName = AllScenes[SceneID];

            // extract the slot game JSON to simpleData
            List<SimpleDataHeader> SlotGameData = JSONToHeader(AllSlots[SlotID], SimpleDataLevel.Slot);

            // merge both lists for readback
            Data = SlotGameData.Concat(SaveGameData).ToList();
        }

        /// <summary>
        /// Load attribute value.
        /// </summary>
        /// <param name="SaveGameID">Save game ID containing the attribute.</param>
        /// <param name="ID">Attribute ID</param>
        /// <returns>Value of specified attribute.</returns>
        protected override float LoadAttribute(int SaveGameID, int ID)
        {
            try
            {
                var AllAttributes = new Dictionary<int, float>();
                if (ES2.Exists(localDB + "?tag=SaveGameAttributes_" + SaveGameID))
                {
                    AllAttributes = ES2.LoadDictionary<int, float>(localDB + "?tag=SaveGameAttributes_" + SaveGameID);
                    return AllAttributes[ID];
                }
                else
                {
                    return 0;  // not found
                }
            }
            catch
            {
                return 0;  // not found
            }
        }

        /// <summary>
        /// Save the header, creating slot if needed.
        /// </summary>
        /// <param name="SlotID">Returns the slotID if one was created.</param>
        /// <param name="SaveGameID">Returns the save game ID if one was created.</param>
        /// <param name="SceneName">Name of the scene saved.</param>
        /// <param name="Data">List of header attributes to save.</param>
        protected override void SaveHeader(ref int SlotID, ref int SaveGameID, string SceneName, List<SimpleDataHeader> Data)
        {
            // new save slot?
            if (SlotID == 0)
            {
                // load slot data list
                var AllSlots = new Dictionary<int, string>();
                if (ES2.Exists(localDB + "?tag=SaveSlots"))
                {
                    AllSlots = ES2.LoadDictionary<int, string>(localDB + "?tag=SaveSlots");
                }

                // build new slot header
                if (AllSlots.Count == 0)
                {  // 1st ever slot
                    SlotID = 1;
                }
                else
                {  // find the highest key+1
                    SlotID = AllSlots.Max(s => s.Key) + 1;
                }
                AllSlots[SlotID] =
                    @"{""CreatedOn"":""" + DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss") + @"""," +
                    HeaderToJSON(Data.Where(d => d.Level == SimpleDataLevel.Slot).ToList()) + "}";

                // save the slot data list
                ES2.Save(AllSlots, localDB + "?tag=SaveSlots");
            }

            // find scene id
            var AllScenes = new Dictionary<int, string>();
            if (ES2.Exists(localDB + "?tag=Scenes"))
            {
                AllScenes = ES2.LoadDictionary<int, string>(localDB + "?tag=Scenes");
            }
            int SceneID = 1;
            bool bFound = false;
            if (AllScenes.Count > 0)
            {
                try
                {
                    SceneID = AllScenes.FirstOrDefault(x => x.Value == SceneName).Key;  // find scene id
                    bFound = true;
                }
                catch
                {  // fail
                    SceneID = AllScenes.Max(s => s.Key) + 1;
                }
            }

            // write the scene name (if new)
            if (!bFound)
            {
                AllScenes[SceneID] = SceneName;
                ES2.Save(AllScenes, localDB + "?tag=Scenes");
            }

            // load the save games for this slot 
            var AllSaveGames = new Dictionary<int, string>();
            if (ES2.Exists(localDB + "?tag=SaveGames"))
            {
                AllSaveGames = ES2.LoadDictionary<int, string>(localDB + "?tag=SaveGames");
            }

            // create new save game id
            if (SaveGameID == -1)
            {  // ID not assigned
                if (AllSaveGames.Count == 0)
                {
                    SaveGameID = 1;
                }
                else
                {
                    SaveGameID = AllSaveGames.Max(s => s.Key) + 1;
                }
            }

            // write save game data
            AllSaveGames[SaveGameID] =
                @"{""CreatedOn"":""" + DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss") + @"""," +
                @"""SceneID"":" + SceneID + "," +
                @"""SlotID"":" + SlotID + "," +
                HeaderToJSON(Data.Where(d => d.Level == SimpleDataLevel.SaveGame).ToList()) + "}";
            ES2.Save(AllSaveGames, localDB + "?tag=SaveGames");

            // update last save game/slot id for continue menu option
            ES2.Save(SlotID.ToString() + "~" + SaveGameID.ToString(), localDB + "?tag=Continue");
        }

        /// <summary>
        /// Save attribute ID/Value pair.
        /// </summary>
        /// <param name="SaveGameID">ID of the save game.</param>
        /// <param name="ID">Attribute ID being saved.</param>
        /// <param name="Value">Value of the attribute</param>
        protected override void SaveAttribute(int SaveGameID, int ID, float Value)
        {
            var AllAttributes = new Dictionary<int, float>();
            if (ES2.Exists(localDB + "?tag=SaveGameAttributes_" + SaveGameID))
            {
                AllAttributes = ES2.LoadDictionary<int, float>(localDB + "?tag=SaveGameAttributes_" + SaveGameID);
            }
            AllAttributes[ID] = Value;
            ES2.Save(AllAttributes, localDB + "?tag=SaveGameAttributes_" + SaveGameID);
        }  

        /// <summary>
        /// Not needed so far for easy save 2
        /// </summary>
        protected override void CleanUp()
        {
            // nothing to do here?
        }

        /// <summary>
        /// Returns ID of latest save for continue menu option, 0 if no save games found.
        /// </summary>
        /// <param name="SlotID">Returned SlotID</param>
        /// <param name="SaveGameID">Returned SaveGameID</param>
        public override void FindMostRecentSaveGameID(ref int SlotID, ref int SaveGameID)
        {
            SlotID = 0;
            SaveGameID = 0;
            if (ES2.Exists(localDB + "?tag=Continue"))
            {
                string MostRecent = ES2.Load<string>(localDB + "?tag=Continue");
                if (MostRecent.Contains("~"))
                {
                    string[] MostRecentSplit = MostRecent.Split('~');
                    SlotID = Convert.ToInt32(MostRecentSplit[0]);
                    SaveGameID = Convert.ToInt32(MostRecentSplit[1]);
                }
            }
        }

        /// <summary>
        /// Returns list of available character slots.
        /// </summary>
        /// <returns>List of SlotID/CharacterName + Date pairs.</returns>
        public override List<SimpleDataPair> GetCharacterSlotList()
        {
            var SimpleReturn = new List<SimpleDataPair>();
            var AllSlots = new Dictionary<int, string>();
            if (ES2.Exists(localDB + "?tag=SaveSlots"))
            {
                AllSlots = ES2.LoadDictionary<int, string>(localDB + "?tag=SaveSlots");
                foreach (int Key in AllSlots.Keys.OrderByDescending(x => x))
                {
                    SimpleReturn.Add(new SimpleDataPair() { Value = Key, Display = GetDelimitedDataFromJSON("~", AllSlots[Key], new string[] { "CharacterName", "CreatedOn" }) });
                }
            }
            return SimpleReturn;
        }

        /// <summary>
        /// Returns list of available save games for the selected slot.
        /// </summary>
        /// <param name="SelectedSlotID">Slot ID to filter the save game list by.</param>
        /// <returns>List of SaveGameID/SceneName + XP + Date pairs</returns>
        public override List<SimpleDataPair> GetSaveGameListForSlot(int SelectedSlotID)
        {
            var SimpleReturn = new List<SimpleDataPair>();
            var AllSaves = new Dictionary<int, string>();
            if (ES2.Exists(localDB + "?tag=SaveGames"))
            {
                AllSaves = ES2.LoadDictionary<int, string>(localDB + "?tag=SaveGames");
                foreach (int Key in AllSaves.Keys.OrderByDescending(x => x))
                {
                    if (AllSaves[Key].Contains("\"SlotID\":" + SelectedSlotID))
                    {
                        SimpleReturn.Add(new SimpleDataPair() { Value = Key, Display = GetDelimitedDataFromJSON("~", AllSaves[Key], new string[] { "CreatedOn", "Level", "XP", "SceneID" }) });
                    }
                }
            }
            return SimpleReturn;
        }  

        #endregion

        #region "EasySave2 Helpers"

        /// <summary>
        /// Convert header list into JSON.
        /// </summary>
        /// <param name="Header">List of header data.</param>
        /// <returns>Singular JSON string.</returns>
        public string HeaderToJSON(List<SimpleDataHeader> Header)
        {
            string JSON = "";
            foreach (SimpleDataHeader HeaderItem in Header)
            {
                if (JSON.Length > 0) JSON += ",";
                JSON +=
                    @"""" + HeaderItem.Name + @""":" +
                    (HeaderItem.Type == SimpleDataType.String ? @"""" : "") +
                    HeaderItem.Value +
                    (HeaderItem.Type == SimpleDataType.String ? @"""" : "");
            }
            return JSON;
        }

        /// <summary>
        /// Turns stored JSON back into the header list.
        /// </summary>
        /// <param name="JSON">JSON string to convert.</param>
        /// <param name="DataLevel">Data level (Slot/SaveGame) to attach to the data.</param>
        /// <returns>List of header attributes.</returns>
        public List<SimpleDataHeader> JSONToHeader(string JSON, SimpleDataLevel DataLevel)
        {
            var Header = new List<SimpleDataHeader>();
            string[] JSONSplit = JSON.Replace("{", string.Empty).Replace("}", string.Empty).Split(new[] { ",\"" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string Prop in JSONSplit)
            {
                string[] PropSplit = Prop.Split(new[] { "\":" }, StringSplitOptions.RemoveEmptyEntries);
                Header.Add(new SimpleDataHeader()
                {
                    Level = DataLevel,
                    Name = PropSplit[0].Replace("\"", string.Empty),
                    Value = PropSplit[1].Replace("\"", string.Empty),
                    Type = (PropSplit[1].Contains("\"") ? SimpleDataType.String : SimpleDataType.Number)
                });
            }
            return Header;
        }

        /// <summary>
        /// Extract delimited data from stored JSON string.
        /// </summary>
        /// <param name="Delimiter">Character data to separate the extracted fields by.</param>
        /// <param name="JSON">JSON string to extract from.</param>
        /// <param name="Fields">Field list to extract.</param>
        /// <returns>Data string of the desired fields.</returns>
        public string GetDelimitedDataFromJSON(string Delimiter, string JSON, string[] Fields)
        {
            // build lookup dictionary of the fields desired from the JSON
            string[] JSONSplit = JSON.Replace("{", string.Empty).Replace("}", string.Empty).Split(new[] { ",\"" }, StringSplitOptions.RemoveEmptyEntries);
            var FieldLookup = new Dictionary<string, string>();
            foreach (string Prop in JSONSplit)
            {
                string[] PropSplit = Prop.Split(new[] { "\":" }, StringSplitOptions.RemoveEmptyEntries);
                if (Fields.Contains(PropSplit[0].Replace("\"", string.Empty)))
                {
                    FieldLookup[PropSplit[0].Replace("\"", string.Empty)] = PropSplit[1].Replace("\"", string.Empty);
                }
            }

            // build return string in the correct field order 
            string ReturnAllDelimitered = "";
            for (int i = 0; i < Fields.Length; i++)
            {
                if (ReturnAllDelimitered.Length > 0) ReturnAllDelimitered += Delimiter;
                if (Fields[i] == "SceneID")
                {  // join to scenes lookup
                    var AllScenes = new Dictionary<int, string>();
                    if (ES2.Exists(localDB + "?tag=Scenes"))
                    {
                        AllScenes = ES2.LoadDictionary<int, string>(localDB + "?tag=Scenes");
                    }
                    ReturnAllDelimitered += AllScenes[Convert.ToInt32(FieldLookup[Fields[i]])];
                }
                else
                {  // normal pure value
                    ReturnAllDelimitered += FieldLookup[Fields[i]];
                }
            }
            return ReturnAllDelimitered;
        }

        #endregion
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
