using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;
using System;
using System.Linq;

namespace Shadex
{
    /// <summary>
    /// SQLLite data layer instance
    /// </summary>
    public class CharacterDataSQLLite : CharacterDataBase
    {
        #region "Abstract method overrides"

        /// <summary>
        /// Check for database upgrades. 
        /// </summary>
        /// <param name="Clear">Empty the database of data.</param>
        public override void ValidateDatabase(bool Clear)
        {
            try
            {
                dbconn = openDB();  // create a SQL connection

                // ensure control table exists              
                if (iExecSQLReader(ref dbconn, "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Control'") == 0)
                {
                    bExecSQLCommand(ref dbconn, "CREATE TABLE Control (Version INTEGER)");
                    Debug.Log("Control table created");
                }

                // grab the current database version
                int DatabaseVersion = iExecSQLReader(ref dbconn, "SELECT Version FROM Control LIMIT 1");

                // update the tables to version 1
                if (DatabaseVersion < 1)
                {
                    // create the Scenes lookup table           
                    if (iExecSQLReader(ref dbconn, "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Scenes'") == 0)
                    {
                        bExecSQLCommand(ref dbconn, "CREATE TABLE Scenes (ID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, SceneName TEXT)");
                        Debug.Log("Scenes table created");
                    }

                    // create the SaveSlots control table           
                    if (iExecSQLReader(ref dbconn, "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='SaveSlots'") == 0)
                    {
                        bExecSQLCommand(ref dbconn, "CREATE TABLE SaveSlots (" +
                            "ID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE," +
                            "CharacterName TEXT,CreatedOn TEXT," +
                            "Axis INTEGER,Alignment INTEGER,Race INTEGER,Class INTEGER)");
                        Debug.Log("SaveSlots table created");
                    }

                    // create the SaveGames control table           
                    if (iExecSQLReader(ref dbconn, "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='SaveGames'") == 0)
                    {
                        bExecSQLCommand(ref dbconn, "CREATE TABLE SaveGames (" +
                            "ID	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                            "SlotID	INTEGER,CreatedOn TEXT," +
                            "SceneID INTEGER,Level INTEGER," +
                            "XP	INTEGER,UnspentSkillPoints INTEGER)");
                        Debug.Log("SaveGames table created");
                    }

                    // create the SaveGameAttributes lookup table           
                    if (iExecSQLReader(ref dbconn, "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='SaveGameAttributes'") == 0)
                    {
                        bExecSQLCommand(ref dbconn, "CREATE TABLE SaveGameAttributes (" +
                            "ID	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                            "SaveGameID	INTEGER,AttributeID INTEGER,AttributeValue REAL)");
                        Debug.Log("SaveGameAttributes table created");
                    }

                    // update the control record
                    DatabaseVersion = 1;
                    bExecSQLCommand(ref dbconn, "INSERT INTO Control (Version) VALUES (1)");  // insert version 1 upgrade success
                    Debug.Log("Upgrade to Database Version 1 Completed");
                }

                // empty database?
                if (Clear)
                {
                    bExecSQLCommand(ref dbconn, "DELETE FROM Scenes");
                    bExecSQLCommand(ref dbconn, "DELETE FROM SaveSlots");
                    bExecSQLCommand(ref dbconn, "DELETE FROM SaveGames");
                    bExecSQLCommand(ref dbconn, "DELETE FROM SaveGameAttributes");
                    Debug.Log("All Data cleared...");
                }

                // all done
                Debug.Log(Application.dataPath + LOCAL_DATABASE_PATH + "/" + DATABASE_NAME + ".db Validate Completed!");
            }
            catch (Exception ex)
            {
                if (GlobalFuncs.DEBUGGING_MESSAGES)
                {
                    Debug.Log(ex.Message);
                }
            }
            finally
            {
                closeDB(ref dbconn);
            }
        }

        /// <summary>
        /// Load the header data array
        /// </summary>
        /// <param name="SaveGameID">ID of the save game being loaded.</param>
        /// <param name="SceneName">Return the name of the scene to load.</param>
        /// <param name="Data">Return the character data in list form.</param>
        protected override void LoadHeader(int SaveGameID, ref string SceneName, ref List<SimpleDataHeader> Data)
        {
            try
            {
                dbconn = openDB();  // create a SQL connection

                // create the select statement
                string SQL = "SELECT SceneName";
                foreach (SimpleDataHeader sdh in Data)
                {
                    SQL += ", " + sdh.Name;
                }
                SQL += " FROM SaveGames INNER JOIN SaveSlots ON SaveGames.SlotID = SaveSlots.ID " +
                    "INNER JOIN Scenes ON SaveGames.SceneID = Scenes.ID " +
                    "WHERE SaveGames.ID = " + SaveGameID.ToString();

                // execute 
                DataTable dt = dtExecSQLReader(ref dbconn, SQL);

                // write back to the data block
                if (dt.Rows.Count > 0)
                {
                    SceneName = dt.Rows[0]["SceneName"].ToString();
                    for (int i = 0; i < Data.Count; i++)
                    {
                        Data[i].Value = dt.Rows[0][Data[i].Name].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                if (GlobalFuncs.DEBUGGING_MESSAGES)
                {
                    Debug.Log("LoadHeader(" + SaveGameID.ToString() + ") " + ex.Message);
                }
            }
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
                return (float)dExecSQLReader(
                    ref dbconn, "SELECT AttributeValue FROM SaveGameAttributes WHERE " +
                    "SaveGameID = " + SaveGameID.ToString() + " AND AttributeID = " + ID.ToString()
                );
            }
            catch (Exception ex)
            {
                if (GlobalFuncs.DEBUGGING_MESSAGES)
                {
                    Debug.Log("LoadAttribute(" + SaveGameID + "," + ID.ToString() + ") " + ex.Message);
                }
                return 0;
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
            try
            {
                dbconn = openDB();  // create a SQL connection

                // new save slot?
                if (SlotID == 0)
                {
                    // create the insert statement
                    string SQLfields = "INSERT INTO SaveSlots (CreatedOn";
                    string SQLvalues = ") VALUES ('" + DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss") + "'";
                    foreach (SimpleDataHeader sdh in Data.Where(d => d.Level == SimpleDataLevel.Slot))
                    {
                        SQLfields += ", " + sdh.Name;
                        SQLvalues += ", " + (sdh.Type == SimpleDataType.String ? "'" : "") + sdh.Value + (sdh.Type == SimpleDataType.String ? "'" : "");
                    }

                    // execute and store the slotid
                    bExecSQLCommand(ref dbconn, SQLfields + SQLvalues + ")");
                    SlotID = iExecSQLReader(ref dbconn, "SELECT last_insert_rowid()");
                }

                // find scene id
                int SceneID = iExecSQLReader(ref dbconn, "SELECT ID FROM Scenes WHERE SceneName = '" + SceneName + "'");
                if (SceneID == -1)
                {  // not found, insert it
                    bExecSQLCommand(ref dbconn, "INSERT INTO Scenes (SceneName) VALUES ('" + SceneName + "')");
                    SceneID = iExecSQLReader(ref dbconn, "SELECT last_insert_rowid()");
                }

                // create the save game header 
                if (SaveGameID == -1)
                {  // new save game?
                    // create the insert statement
                    string SQLfields = "INSERT INTO SaveGames (CreatedOn, SlotID, SceneID";
                    string SQLvalues = ") VALUES ('" + DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss") + "', " + SlotID + ", " + SceneID;
                    foreach (SimpleDataHeader sdh in Data.Where(d => d.Level == SimpleDataLevel.SaveGame))
                    {
                        SQLfields += ", " + sdh.Name;
                        SQLvalues += ", " + (sdh.Type == SimpleDataType.String ? "'" : "") + sdh.Value + (sdh.Type == SimpleDataType.String ? "'" : "");
                    }

                    // execute and store the slotid
                    bExecSQLCommand(ref dbconn, SQLfields + SQLvalues + ")");
                    SaveGameID = iExecSQLReader(ref dbconn, "SELECT last_insert_rowid()");
                }
                else
                {  // overwrite, update the header and remove previous attributes
                    // create the update statement
                    string SQLupdate = "UPDATE SaveGames SET " +
                        "CreatedOn = '" + DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss") + "', " +
                        "SceneID = " + SceneID.ToString();
                    foreach (SimpleDataHeader sdh in Data.Where(d => d.Level == SimpleDataLevel.SaveGame))
                    {
                        SQLupdate += ", " + sdh.Name + " = " + (sdh.Type == SimpleDataType.String ? "'" : "") + sdh.Value + (sdh.Type == SimpleDataType.String ? "'" : "");
                    }

                    // execute and clear previous attributes
                    bExecSQLCommand(ref dbconn, SQLupdate + " WHERE ID = " + SaveGameID.ToString());
                    bExecSQLCommand(ref dbconn, "DELETE FROM SaveGameAttributes WHERE SaveGameID = " + SaveGameID.ToString());
                }
            }
            catch (Exception ex)
            {
                if (GlobalFuncs.DEBUGGING_MESSAGES)
                {
                    Debug.Log("SaveHeader(" + SceneName + "," + SaveGameID.ToString() + ") " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Save attribute ID/Value pair.
        /// </summary>
        /// <param name="SaveGameID">ID of the save game.</param>
        /// <param name="ID">Attribute ID being saved.</param>
        /// <param name="Value">Value of the attribute</param>
        protected override void SaveAttribute(int SaveGameID, int ID, float Value)
        {
            try
            {
                bExecSQLCommand(
                    ref dbconn, "INSERT INTO SaveGameAttributes (SaveGameID, AttributeID, AttributeValue) VALUES (" +
                        SaveGameID.ToString() + "," + ID.ToString() + "," + Value.ToString() + ")"
                );
            }
            catch (Exception ex)
            {
                if (GlobalFuncs.DEBUGGING_MESSAGES)
                {
                    Debug.Log("SaveAttribute(" + SaveGameID + "," + ID.ToString() + "," + Value.ToString() + ") " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Clear locks on the SQLLite d/b.
        /// </summary>
        protected override void CleanUp()
        {
            closeDB(ref dbconn);
        }

        /// <summary>
        /// Returns ID of latest save for continue menu option, 0 if no save games found.
        /// </summary>
        /// <param name="SlotID">Returned SlotID</param>
        /// <param name="SaveGameID">Returned SaveGameID</param>
        public override void FindMostRecentSaveGameID(ref int SlotID, ref int SaveGameID)
        {
            try
            {
                dbconn = openDB();  // create a SQL connection

                // find most recent save game/slot ID's
                SaveGameID = iExecSQLReader(ref dbconn, "SELECT ID FROM SaveGames ORDER BY ID DESC LIMIT 1");
                SlotID = iExecSQLReader(ref dbconn, "SELECT SlotID FROM SaveGames WHERE ID = " + SaveGameID);
            }
            catch (Exception ex)
            {
                if (GlobalFuncs.DEBUGGING_MESSAGES)
                {
                    Debug.Log("FindMostRecentSaveGameID() " + ex.Message);
                }
            }
            finally
            {
                closeDB(ref dbconn);
            }
        }

        /// <summary>
        /// Returns list of available character slots.
        /// </summary>
        /// <returns>List of SlotID/CharacterName + Date pairs.</returns>
        public override List<SimpleDataPair> GetCharacterSlotList()
        {
            return GetShortList("SaveSlots", "ID", new string[] { "CharacterName", "CreatedOn" }, "CreatedOn", true, "", "", false, "~");
        }

        /// <summary>
        /// Returns list of available save games for the selected slot.
        /// </summary>
        /// <param name="SelectedSlotID">Slot ID to filter the save game list by.</param>
        /// <returns>List of SaveGameID/SceneName + XP + Date pairs</returns>
        public override List<SimpleDataPair> GetSaveGameListForSlot(int SelectedSlotID)
        {
            return GetShortList("vSaveGames", "ID", new string[] { "CreatedOn", "Level", "XP", "SceneName" }, "CreatedOn", true, "SlotID", SelectedSlotID.ToString(), false, "~");
        }  

        #endregion

        #region "SQL Lite helpers"

        /// <summary>Internal database connection.</summary>
        internal IDbConnection dbconn;

        /// <summary>
        /// return an open connection to the SQLLite data source.
        /// </summary>
        /// <returns>Database connection.</returns>
        public IDbConnection openDB()
        {
            IDbConnection dbconn = new SqliteConnection("URI=file:" + Application.dataPath + LOCAL_DATABASE_PATH + "/" + DATABASE_NAME + "SQL.db");
            dbconn.Open();
            return dbconn;
        }

        /// <summary>
        /// Close previously opened SQLLite connection.
        /// </summary>
        /// <param name="dbconn">Database connection to be closed.</param>
        public void closeDB(ref IDbConnection dbconn)
        {
            if (dbconn != null)
            {
                dbconn.Close();
                dbconn = null;
                GC.Collect();
            }
        }

        /// <summary>
        /// Execute a SQL command on the specified database.
        /// </summary>
        /// <param name="dbconn">Database connection.</param>
        /// <param name="Command">SQL command to execute.</param>
        /// <returns>Boolean success.</returns>
        public bool bExecSQLCommand(ref IDbConnection dbconn, string Command)
        {
            IDbCommand dbcmd = null;
            bool bSuccess = false;
            try
            {
                // execute the SQL command
                dbcmd = dbconn.CreateCommand();
                dbcmd.CommandText = Command;
                dbcmd.ExecuteNonQuery();
                bSuccess = true;  // all is well
            }
            catch (Exception ex)
            {
                if (GlobalFuncs.DEBUGGING_MESSAGES)
                {
                    Debug.Log("bExecSQLReader(" + Command + ") " + ex.Message);
                }
            }
            finally
            {  // clean up
                if (dbcmd != null)
                {
                    dbcmd.Dispose();
                    dbcmd = null;
                }
                GC.Collect();  // ensure no leftover garbage
            }
            return bSuccess;  // return false on error executing the command
        }

        /// <summary>
        /// Execute SQL data reader.
        /// </summary>
        /// <param name="dbconn">Database connection.</param>
        /// <param name="Command">SQL command to execute.</param>
        /// <returns>Data table of the result set.</returns>
        public DataTable dtExecSQLReader(ref IDbConnection dbconn, string Command)
        {
            DataTable dtReturn = new DataTable();
            IDbCommand dbcmd = null;
            IDataReader reader = null;
            try
            {
                // execute the reader
                dbcmd = dbconn.CreateCommand();
                dbcmd.CommandText = Command;
                reader = dbcmd.ExecuteReader();

                // load reader table output
                dtReturn.Load(reader);
            }
            catch (Exception ex)
            {
                if (GlobalFuncs.DEBUGGING_MESSAGES)
                {
                    Debug.Log("dtExecSQLReader(" + Command + ") " + ex.Message);
                }
            }
            finally
            {  // clean up
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                    reader = null;
                }
                if (dbcmd != null)
                {
                    dbcmd.Dispose();
                    dbcmd = null;
                }
                GC.Collect();  // ensure no leftover garbage
            }
            return dtReturn;  // return output, zero rows data table on error 
        }

        /// <summary>
        /// Execute SQL data reader returning 1st column/row as a string.
        /// </summary>
        /// <param name="dbconn">Database connection.</param>
        /// <param name="Command">SQL command to execute.</param>
        /// <returns>First column as a string.</returns>
        public string sExecSQLReader(ref IDbConnection dbconn, string Command)
        {
            string sReturn = "";
            DataTable dtTemp = new DataTable();
            try
            {
                // load reader table output            
                dtTemp = dtExecSQLReader(ref dbconn, Command);

                // return first row/column as a string
                if (dtTemp.Rows.Count > 0)
                {
                    sReturn = dtTemp.Rows[0][0].ToString();
                }
            }
            catch (Exception ex)
            {
                if (GlobalFuncs.DEBUGGING_MESSAGES)
                {
                    Debug.Log("sExecSQLReader(" + Command + ") " + ex.Message);
                }
            }
            finally
            {  // clean up
                dtTemp.Dispose();
                dtTemp = null;
                GC.Collect();  // ensure no leftover garbage
            }
            return sReturn;  // return output, empty string on error
        }

        /// <summary>
        /// Execute SQL data reader returning 1st column/row as a double.
        /// </summary>
        /// <param name="dbconn">Database connection.</param>
        /// <param name="Command">SQL command to execute.</param>
        /// <returns>First column as a double.</returns>
        public double dExecSQLReader(ref IDbConnection dbconn, string Command)
        {
            double dReturn = 0;
            DataTable dtTemp = new DataTable();
            try
            {
                // load reader table output            
                dtTemp = dtExecSQLReader(ref dbconn, Command);

                // return first row/column as a string
                if (dtTemp.Rows.Count > 0)
                {
                    dReturn = Convert.ToDouble(dtTemp.Rows[0][0].ToString());
                }
            }
            catch (Exception ex)
            {
                if (GlobalFuncs.DEBUGGING_MESSAGES)
                {
                    Debug.Log("dExecSQLReader(" + Command + ") " + ex.Message);
                }
            }
            finally
            {  // clean up
                dtTemp.Dispose();
                dtTemp = null;
                GC.Collect();  // ensure no leftover garbage
            }
            return dReturn;  // return output, zero on error
        }

        /// <summary>
        /// Execute SQL data reader returning 1st column/row as a integer.
        /// </summary>
        /// <param name="dbconn">Database connection.</param>
        /// <param name="Command">SQL command to execute.</param>
        /// <returns>First column as a integer.</returns>
        public int iExecSQLReader(ref IDbConnection dbconn, string Command)
        {
            int iReturn = -1;
            DataTable dtTemp = new DataTable();
            try
            {
                // load reader table output            
                dtTemp = dtExecSQLReader(ref dbconn, Command);

                // return first row/column as a string
                if (dtTemp.Rows.Count > 0)
                {
                    iReturn = Convert.ToInt16(dtTemp.Rows[0][0].ToString());
                }
            }
            catch (Exception ex)
            {
                if (GlobalFuncs.DEBUGGING_MESSAGES)
                {
                    Debug.Log("iExecSQLReader(" + Command + ") " + ex.Message);
                }
            }
            finally
            {  // clean up
                dtTemp.Dispose();
                dtTemp = null;
                GC.Collect();  // ensure no leftover garbage
            }
            return iReturn;  // return output, zero on error
        }  // execute SQL data reader returning 1st column/row as an int

        /// <summary>
        /// Return id/name pairs for the selected table query.
        /// </summary>
        /// <param name="TableName">Name of table/view to select from.</param>
        /// <param name="ValueMember">Pair value member field.</param>
        /// <param name="DisplayMember">Pair display member field.</param>
        /// <param name="SortBy">Sort field.</param>
        /// <param name="SortDescending">Sort direction.</param>
        /// <param name="WhereField">Conditions on which field.</param>
        /// <param name="WhereValue">Conditions meeting which value.</param>
        /// <param name="IncludeNew">Include a new row at the top of the table.</param>
        /// <param name="DisplaySeparator">Separator field for return.</param>
        /// <returns>List in pair format from the table arguments.</returns>
        public List<SimpleDataPair> GetShortList(string TableName, string ValueMember, string[] DisplayMember, string SortBy, bool SortDescending, string WhereField, string WhereValue, bool IncludeNew, string DisplaySeparator)
        {
            try
            {
                dbconn = openDB();  // create a SQL connection

                // build the display selector
                string DisplayMemberConcat = "";
                foreach (string s in DisplayMember)
                {
                    if (DisplayMemberConcat.Length > 0)
                    {
                        DisplayMemberConcat += "|| '" + DisplaySeparator + "' || ";
                    }
                    DisplayMemberConcat += s;
                }

                // grab the list to data table
                DataTable dt = dtExecSQLReader(ref dbconn,
                    "SELECT " + ValueMember + " AS Value," +
                    DisplayMemberConcat + " AS Display FROM " + TableName +
                    (WhereField.Length > 0 ? " WHERE " + WhereField + " = " + WhereValue : "") +
                    " ORDER BY " + SortBy + (SortDescending ? " DESC" : " ASC"));

                // all done
                List<SimpleDataPair> Pairs = new List<SimpleDataPair>();
                if (IncludeNew)
                {
                    Pairs.Add(new SimpleDataPair() { Value = 0, Display = "Add New" });
                }
                foreach (DataRow dr in dt.Rows)
                {
                    Pairs.Add(new SimpleDataPair() { Value = Convert.ToInt32(dr["Value"]), Display = dr["Display"].ToString() });
                }
                return Pairs;
            }
            catch (Exception ex)
            {
                if (GlobalFuncs.DEBUGGING_MESSAGES)
                {
                    Debug.Log(ex.Message);
                }
                return null;
            }
            finally
            {
                closeDB(ref dbconn);
            }
        } 

        /// <summary>
        /// Updates string single quotes to double quotes.
        /// </summary>
        /// <param name="sIn"></param>
        /// <returns></returns>
        public string sSQLSafe(string sIn)
        {
            return sIn.Replace("'", "''");
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
