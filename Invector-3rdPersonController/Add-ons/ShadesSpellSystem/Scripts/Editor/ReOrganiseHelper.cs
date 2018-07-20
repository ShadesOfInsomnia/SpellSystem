using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using Invector.vMelee;
using Invector.vCharacterController;

namespace Shadex
{
    /// <summary>
    /// Reorganise character editor tool window.
    /// </summary>
    /// <remarks>
    /// Provides name cleanup, simplygon LOD output merging, non standard bone structure basic ragdoll creation 
    /// and equipment material/visibility handling over multiple LOD levels/genders.
    /// 
    /// TODO
    /// - allow specify of base folders
    /// </remarks>
    public class ReOrganiseHelper : EditorWindow
    {
        #region "Declarations"
        private bool bLoaded = false;
        private const int INDENT_TABS = 25;  // indents to inset labels inside toggle boxes    
        private Vector2 scrollPos;  // scroll position of the window

        // foldouts
        bool bShowOrganisation = false;
        bool bShowMergeLODBones = false;
        bool bShowGenericRagDoll = false;
        bool bShowEquipCharacter = false;

        // GUI options storage
        string sSelectByName;
        string sDeleteFromName;
        string sFind;
        string sAndReplace;
        string sCullingLevel;
        bool bLastLODLevelHalf;

        // merge LOD
        struct LODLevel
        {
            public Renderer[] renderers;
        }
        string sRootBoneName = "";
        string sLastObjectAnalysed = "";
        int iLODsFound;
        Transform tFoundMaleLOD;
        Transform tFoundFemaleLOD;

        // configure character
        //private string[] sCharacterPaths = { "Resources/Prefabs/Characters/Clean", "Resources/Prefabs/Creatures/Clean" };
        string[] sCharacterPaths = { "Invector-3rdPersonController/Add-ons.resource/SFB/Prefabs/Characters/Clean", "Invector-3rdPersonController/Add-ons.resource/SFB/Prefabs/Creatures/Clean" };
        int iCurrentCreator = 0;
        string[] sCreatorPrefix = { "All", "SFB", "Not SFB" };
        int iCurrentCharacter = -1;
        struct stCharPaths
        {
            public string sFriendlyName;
            public string sFullPath;
            public string sMaterialsSearchPath;
        }
        stCharPaths[] ccCharPaths;
        string[] sCharPathShortList;
        int iCurrentCharacterFiltered = -1;
        int iCurrentGender;
        enum eGender { Male, Female, Both };
        string[] sGenderList = { "Male", "Female" };
        struct stCharObjects
        {
            public GameObject goRef;
            public GameObject[] goLOD2Plus;
            public string sFriendlyName;
            public eGender egSex;
            public bool bEquiped;
            public int[] iMaterial;
            public int[] iMaterialVariant;
            public bool bDeleted;
        }
        stCharObjects[] ccCharComponents;
        struct stMaterialsList
        {
            public string sFriendlyName;
            public string sBaseFolder;
            public int iMasterVariant;
            public string[] sVarients;
        }
        stMaterialsList[] ccCharMaterials;

        // temp
        GameObject goActiveCharacter;
        int iNewValue;
        bool bNewValue;
        int iTotalFound;
        bool bNot1stOpenRagDoll;


        // ragdoll builder arrays & indexes
        GameObject[] goValidEnemies;  // result of analyse scene button
        string[] sValidEnemies;  // shortlist of above for the toggle
        int iCurrentEnemy = -1;  // currently selected generic ai
        float fBoneMass = 10f;  // the mass to apply to each bone

        // processing
        GameObject goBuildRoot;  // enemy currently being built
        Transform tRootBone;  // top level bone
        List<Transform> tRootBones;  // user alterable root's for the body, eg upper/lower body in a humanoid
        String[] sRootBones;  // matching list of root bone names for the GUI
        List<Transform> tLinkBones;  // user alterable root's for the body, eg upper/lower body in a humanoid
        String[] sLinkBones;  // matching list of root bone names for the GUI
        List<LimbChain> lcLimbChainList;  // start bone for each limb chain
        List<string> sAllBones;  // all known bone names for quick search
        public enum eColliderType { Capsule, Box, Sphere, DontInclude };  // types of collider
        List<BoneOptions> lAllBones;  // matching list of options
        bool bHitboxesShown;  // toggle the invector hitboxes

        // editor GUI internals
        Transform tNewValue;
        Transform tNext;
        eColliderType ctNewValue;
        bool bFound;
        SkinnedMeshRenderer targetRenderer;

        #endregion

        #region "GUI"
        /// <summary>
        /// Unity menu item link function, creates the editor window next to the inspector.
        /// </summary>
        [MenuItem("Invector/Shades Spell System/Reorganise Character")]
        public static void ShowWindow()
        {
            Type inspectorType = Type.GetType("UnityEditor.InspectorWindow,UnityEditor.dll");  // find the inspector type
            EditorWindow.GetWindow<ReOrganiseHelper>("ReOrganise", inspectorType);  // create the editor window next to the inspector
            EditorWindow.GetWindow(typeof(EditorWindow));  // fallback if no inspector window
        }  // menu item clicked

        /// <summary>
        /// Draw the inspector GUI
        /// </summary>
        void OnGUI()
        {
            try
            {
                // init GUI
                if (!bLoaded)
                {
                    bLoaded = true;  // prevent double load
                    sSelectByName = EditorPrefs.GetString("ReOrg_SelByName", "");
                    sDeleteFromName = EditorPrefs.GetString("ReOrg_DelFromName", "");
                    sFind = EditorPrefs.GetString("ReOrg_Find", "");
                    sAndReplace = EditorPrefs.GetString("ReOrg_AndReplace", "");
                    sCullingLevel = EditorPrefs.GetString("ReOrg_CullingLevel", "0.02");
                    bLastLODLevelHalf = EditorPrefs.GetBool("ReOrg_LastLODLevelHalf", true);

#if CREATEFOLDERS
                    // ensure key paths exist
                    if (!Directory.Exists(Application.dataPath + "/Prefabs"))
                    {
                        Directory.CreateDirectory(Application.dataPath + "/Prefabs");
                    }
                    if (!Directory.Exists(Application.dataPath + "/Materials"))
                    {
                        Directory.CreateDirectory(Application.dataPath + "/Materials");
                    }
                    if (!Directory.Exists(Application.dataPath + "/Textures"))
                    {
                        Directory.CreateDirectory(Application.dataPath + "/Textures");
                    }
                    if (!Directory.Exists(Application.dataPath + "/LODs"))
                    {
                        Directory.CreateDirectory(Application.dataPath + "/LODs");
                    }


                    // build available character paths           
                    string[] sPrefabSubFolders = Directory.GetDirectories(Application.dataPath + "/Prefabs");
                    if (sPrefabSubFolders.Length > 0)
                    {
                        foreach (string sDir in sPrefabSubFolders)
                        {
                            string sSlashedDir = sDir.Replace("\\", "/");
                            if (Directory.Exists(sSlashedDir + "/Clean"))
                            {
                                if (sCharacterPaths == null)
                                {
                                    sCharacterPaths = new string[1];  // create element 0                                
                                }
                                else
                                {  // not empty
                                    Array.Resize<string>(ref sCharacterPaths, sCharacterPaths.Length + 1);  // extend array by 1
                                }
                                sCharacterPaths[sCharacterPaths.Length - 1] = sSlashedDir.Replace(Application.dataPath + "/", "") + "/Clean";  // save the prefab path

                                // check the matching materials folder exists
                                if (!Directory.Exists(sSlashedDir.Replace("/Prefabs/", "/Materials/")))
                                {
                                    Directory.CreateDirectory(sSlashedDir.Replace("/Prefabs/", "/Materials/"));
                                }

                                // check the matching textures folder exists
                                if (!Directory.Exists(sSlashedDir.Replace("/Prefabs/", "/Textures/")))
                                {
                                    Directory.CreateDirectory(sSlashedDir.Replace("/Prefabs/", "/Textures/"));
                                }

                                // check the matching LODs folder exists
                                if (!Directory.Exists(sSlashedDir.Replace("/Prefabs/", "/LODs/")))
                                {
                                    Directory.CreateDirectory(sSlashedDir.Replace("/Prefabs/", "/LODs/"));
                                }
                            }
                        }
                    }
                    else  // if no sub directories defined (with a clean subfolder) create the default (characters)
                    {
                        if (!Directory.Exists(Application.dataPath + "/Prefabs/Characters"))
                        {
                            Directory.CreateDirectory(Application.dataPath + "/Prefabs/Characters");
                        }
                        Directory.CreateDirectory(Application.dataPath + "/Prefabs/Characters/Clean");  // clean sub folder is where the prefab masters live
                        if (!Directory.Exists(Application.dataPath + "/Materials/Characters"))
                        {
                            Directory.CreateDirectory(Application.dataPath + "/Materials/Characters");  // character materials
                        }
                        if (!Directory.Exists(Application.dataPath + "/Textures/Characters"))
                        {
                            Directory.CreateDirectory(Application.dataPath + "/Textures/Characters");  // character textures
                        }
                        if (!Directory.Exists(Application.dataPath + "/LODs/Characters"))
                        {
                            Directory.CreateDirectory(Application.dataPath + "/LODs/Characters");  // character LODs and FBX
                        }
                    }
#endif
                    
                    // detect characters
                    foreach (string sLocation in sCharacterPaths)
                    {
                        if (Directory.Exists(Application.dataPath + "/" + sLocation))
                        {
                            DirectoryInfo diPath = new DirectoryInfo(Application.dataPath + "/" + sLocation);
                            FileInfo[] fiPath = diPath.GetFiles("*.prefab", SearchOption.TopDirectoryOnly);
                            for (int i = 0; i < fiPath.Length; i++)
                            {
                                if (ccCharPaths == null)
                                {
                                    ccCharPaths = new stCharPaths[1];  // create element 0                                
                                }
                                else
                                {  // not empty
                                    Array.Resize<stCharPaths>(ref ccCharPaths, ccCharPaths.Length + 1);  // extend array by 1
                                }
                                ccCharPaths[ccCharPaths.Length - 1].sFullPath = "Assets/" + sLocation + "/" + fiPath[i].Name;  // store the prefab location
                                ccCharPaths[ccCharPaths.Length - 1].sMaterialsSearchPath = sLocation.Replace("Prefabs/", "Materials/").Replace("Clean", "") + "/" + fiPath[i].Name.Replace(".prefab", "");  // store the materials location
                                int iStripVersionSuffix = ccCharPaths[ccCharPaths.Length - 1].sMaterialsSearchPath.IndexOf("__");   // check for versions of the same char suffix
                                if (iStripVersionSuffix > 0)
                                {  // found
                                    ccCharPaths[ccCharPaths.Length - 1].sMaterialsSearchPath = ccCharPaths[ccCharPaths.Length - 1].sMaterialsSearchPath.Substring(0, iStripVersionSuffix); // remove from the path eg __RIG or __v2 
                                }
                                if (!Directory.Exists(Application.dataPath + "/" + ccCharPaths[ccCharPaths.Length - 1].sMaterialsSearchPath))
                                {  // check the materials path exists
                                    Debug.Log("WARNING Materials Path NOT found " + Application.dataPath + "/" + ccCharPaths[ccCharPaths.Length - 1].sMaterialsSearchPath);  // warn the user
                                }
                                ccCharPaths[ccCharPaths.Length - 1].sFriendlyName = fiPath[i].Name.Replace(".prefab", "");  // and display name                            
                                if (sCreatorPrefix[iCurrentCreator] == "All" || (sCreatorPrefix[iCurrentCreator] == "Not SFB" && !fiPath[i].Name.StartsWith("SFB")) || fiPath[i].Name.StartsWith(sCreatorPrefix[iCurrentCreator])) // name matches creator filter
                                {
                                    if (sCharPathShortList == null)
                                    {
                                        sCharPathShortList = new string[1];  // create element 0                                
                                    }
                                    else  // not empty
                                    {
                                        Array.Resize<string>(ref sCharPathShortList, sCharPathShortList.Length + 1);  // extend array by 1
                                    }
                                    sCharPathShortList[sCharPathShortList.Length - 1] = fiPath[i].Name.Replace(".prefab", "").Replace(sCreatorPrefix[iCurrentCreator], ""); // append to the filter short list
                                }
                            }
                        }
                    }

                    // check for existing base in the scene
                    GameObject goPotentialExistingBaseChar = null;  // potential name of existing base character
                    foreach (GameObject obj in UnityEngine.Object.FindObjectsOfType(typeof(GameObject)))
                    {
                        if (obj.transform.parent == null & obj.name.EndsWith("__BASE"))  // no parent (ie root object) and ends with the keyword base
                        {
                            goPotentialExistingBaseChar = obj;  // store
                            break; // work complete
                        }
                    }

                    // match the existing character (if any)
                    if (goPotentialExistingBaseChar != null)  // we have a potential base char
                    {
                        for (int i = 0; i < ccCharPaths.Length; i++)
                        {
                            if (goPotentialExistingBaseChar.name.Replace("__BASE", "") == ccCharPaths[i].sFriendlyName) // matched
                            {
                                goActiveCharacter = goPotentialExistingBaseChar;  // make the active char
                                iCurrentCharacter = i;  // assign the character ID
                                bShowEquipCharacter = true;  // expand the foldout
                                for (int j = 0; j < sCreatorPrefix.Length; j++)  // find the creator id
                                {
                                    if (ccCharPaths[i].sFriendlyName.StartsWith(sCreatorPrefix[j]))  // found the creator         
                                    {                       
                                        iCurrentCreator = j;  // assign the creator ID                                    
                                        sCharPathShortList = null;  // clear the short list
                                        foreach (stCharPaths cc in ccCharPaths)  // rebuild from the new id
                                        {
                                            if (cc.sFriendlyName.StartsWith(sCreatorPrefix[iCurrentCreator]))  // matching creator               
                                            {                         
                                                if (sCharPathShortList == null)  // empty list
                                                {
                                                    sCharPathShortList = new string[1];  // create element 0                                
                                                }
                                                else  // not empty
                                                {
                                                    Array.Resize<string>(ref sCharPathShortList, sCharPathShortList.Length + 1);  // extend array by 1
                                                }
                                                sCharPathShortList[sCharPathShortList.Length - 1] = cc.sFriendlyName.Replace(sCreatorPrefix[iCurrentCreator], "");  // append to the short list
                                                if (cc.sFriendlyName == goActiveCharacter.name.Replace("__BASE", ""))  // matching short list id
                                                {
                                                    iCurrentCharacterFiltered = sCharPathShortList.Length - 1;  // assign the short list id
                                                }
                                            }
                                        }
                                    }
                                }
                                break; // work complete
                            }
                        }
                    }
                }

                // check for changes
                EditorGUI.BeginChangeCheck();

                // start scroll area
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);

                // organisation
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.BeginHorizontal();
                bShowOrganisation = GUILayout.Toggle(bShowOrganisation, "Cleanup Children", "Foldout", GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();
                if (bShowOrganisation)
                {
                    vCollapseOtherTabs(ref bShowOrganisation);  // collapse other foldouts
                    vGUIOrganisation();  // execute
                }
                GUILayout.EndVertical();

                // Merge LODs onto 1 set of bones
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.BeginHorizontal();
                bShowMergeLODBones = GUILayout.Toggle(bShowMergeLODBones, "Merge LOD Bones", "Foldout", GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();
                if (bShowMergeLODBones)
                {
                    vCollapseOtherTabs(ref bShowMergeLODBones);  // collapse other foldouts
                    vGUIMergeLODBones();  // execute
                }
                GUILayout.EndVertical();

                //// build a generic rag doll
                //GUILayout.BeginVertical(EditorStyles.helpBox);
                //GUILayout.BeginHorizontal();
                //bShowGenericRagDoll = GUILayout.Toggle(bShowGenericRagDoll, "Generic Ragdoll", "Foldout", GUILayout.ExpandWidth(true));
                //GUILayout.EndHorizontal();
                //if (bShowGenericRagDoll)
                //{
                //    vCollapseOtherTabs(ref bShowGenericRagDoll);  // collapse other foldouts
                //    vGUIBuildGenericRagdoll();  // execute
                //}
                //GUILayout.EndVertical();

                // character equipment & material selector
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.BeginHorizontal();
                bShowEquipCharacter = GUILayout.Toggle(bShowEquipCharacter, "Equip Character", "Foldout", GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();
                if (bShowEquipCharacter)
                {
                    vCollapseOtherTabs(ref bShowEquipCharacter);  // collapse other foldouts
                    vGUIEquipCharacter();  // execute
                }
                GUILayout.EndVertical();


                // end of dynamic scrollable area
                EditorGUILayout.EndScrollView();

                // save changes, if any
                if (EditorGUI.EndChangeCheck())
                {
                    EditorPrefs.SetString("ReOrg_SelByName", sSelectByName);
                    EditorPrefs.SetString("ReOrg_DelFromName", sDeleteFromName);
                    EditorPrefs.SetString("ReOrg_Find", sFind);
                    EditorPrefs.SetString("ReOrg_AndReplace", sAndReplace);
                    EditorPrefs.SetString("ReOrg_CullingLevel", sCullingLevel);
                    EditorPrefs.SetBool("ReOrg_LastLODLevelHalf", bLastLODLevelHalf);
                }
            }
            catch (Exception ex)
            {
                vLogError(ref ex, "");
            }
        }

        /// <summary>
        /// Collapse other tabs but not self.
        /// </summary>
        /// <param name="ButNotMe">Reference to the tab that has just been opened.</param>
        void vCollapseOtherTabs(ref bool ButNotMe)
        {
            try
            {
                bShowOrganisation = false;
                bShowMergeLODBones = false;
                bShowEquipCharacter = false;
                bShowGenericRagDoll = false;
                ButNotMe = true;
            }
            catch (Exception ex)
            {
                vLogError(ref ex, "");
            }
        }
        #endregion

        #region "GUI Breakdown"
        /// <summary>
        /// Select/cleanup child nodes by partial name.
        /// </summary>
        private void vGUIOrganisation()
        {
            try
            {
                // selection & ordering group
                GUILayout.BeginHorizontal();
                GUILayout.Space(INDENT_TABS);
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Label("Selection & Order", GUILayout.ExpandWidth(true));

                // text to select child nodes by
                GUILayout.BeginHorizontal();
                GUILayout.Space(INDENT_TABS);
                sSelectByName = GUILayout.TextField(sSelectByName, GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();

                // select button
                GUILayout.BeginHorizontal();
                GUILayout.Space(INDENT_TABS);
                if (GUILayout.Button("Select By Partial Name", GUILayout.ExpandWidth(true)))
                {
                    if (sSelectByName.Length > 0)
                    {  // search term input
                        if (Selection.gameObjects.Length == 1)
                        {  // 1 editor object selected
                            GameObject[] goActiveCharacterER = new GameObject[0];
                            iTotalFound = 0;
                            goActiveCharacter = Selection.activeGameObject;
                            Selection.activeGameObject = null;
                            foreach (Transform tChild in goActiveCharacter.transform)
                            {  // loop  through children
                                if (tChild.gameObject.name.ToUpper().Contains(sSelectByName.ToUpper()))
                                {  // matching child name
                                    iTotalFound += 1;
                                    Array.Resize(ref goActiveCharacterER, iTotalFound);
                                    goActiveCharacterER[iTotalFound - 1] = tChild.gameObject;  // add to selection array
                                }
                            }
                            if (iTotalFound > 0)
                            {  // apply the selection
                                Selection.objects = goActiveCharacterER;
                            }
                            else
                            {   // or revert if no matches
                                Selection.activeGameObject = goActiveCharacter;
                            }
                            Debug.Log("Total " + (iTotalFound + 1).ToString() + " Selections!");
                        }
                        else
                        {
                            Debug.Log("Select a GameObject to Search (not 2)!");
                        }
                    }
                    else
                    {
                        Debug.Log("Search Text NOT Specified!");
                    }
                }
                GUILayout.EndHorizontal();

                // sort button
                GUILayout.BeginHorizontal();
                GUILayout.Space(INDENT_TABS);
                if (GUILayout.Button("Sort Children of Selected", GUILayout.ExpandWidth(true)))
                {
                    if (Selection.gameObjects.Length == 1)
                    {  // 1 editor object selected
                        vSortGameObjectChildren(Selection.activeGameObject);
                    }
                    else
                    {
                        Debug.Log("Select a GameObject to Sort!");
                    }
                }
                GUILayout.EndHorizontal();

                // children visibility button
                GUILayout.BeginHorizontal();
                GUILayout.Space(INDENT_TABS);
                if (GUILayout.Button("Make ALL Children Visibile", GUILayout.ExpandWidth(true)))
                {
                    if (Selection.gameObjects.Length == 1)
                    {  // 1 editor object selected
                        int iTotalFound = 0;
                        goActiveCharacter = Selection.activeGameObject;
                        foreach (Transform tChild in goActiveCharacter.GetComponentsInChildren<Transform>(true))
                        {  // loop through children recursive
                            tChild.gameObject.SetActive(true);
                            iTotalFound += 1;
                        }
                        Debug.Log("Total " + iTotalFound.ToString() + " Children Updated!");
                    }
                    else
                    {
                        Debug.Log("Select a GameObject to Update!");
                    }
                }
                GUILayout.EndHorizontal();

                // trim/find and replace 
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Space(INDENT_TABS);
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Label("Find & Replace", GUILayout.ExpandWidth(true));

                // text to trim
                GUILayout.BeginHorizontal();
                GUILayout.Space(INDENT_TABS);
                sDeleteFromName = GUILayout.TextField(sDeleteFromName, GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();

                // trim button
                GUILayout.BeginHorizontal();
                GUILayout.Space(INDENT_TABS);
                if (GUILayout.Button("Trim Text From Name", GUILayout.ExpandWidth(true)))
                {
                    if (sDeleteFromName.Length > 0)
                    {  // search term input
                        if (Selection.gameObjects.Length == 1)
                        {  // 1 editor object selected
                            int iTotalFound = 0;
                            goActiveCharacter = Selection.activeGameObject;
                            foreach (Transform tChild in goActiveCharacter.transform)
                            {  // loop  through children
                                if (tChild.gameObject.name.Contains(sDeleteFromName))
                                {  // matching child name
                                    tChild.gameObject.name = tChild.gameObject.name.Replace(sDeleteFromName, "");  // trim from the name
                                    iTotalFound += 1;
                                }
                            }
                            Debug.Log("Total " + (iTotalFound + 1).ToString() + " GameObject Names Trimmed!");
                        }
                        else
                        {
                            Debug.Log("Select a GameObject to Search (not 2)!");
                        }
                    }
                    else
                    {
                        Debug.Log("Search Text NOT Specified!");
                    }
                }
                GUILayout.EndHorizontal();

                // text to find
                GUILayout.BeginHorizontal();
                GUILayout.Space(INDENT_TABS);
                sFind = GUILayout.TextField(sFind, GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();

                // text to replace
                GUILayout.BeginHorizontal();
                GUILayout.Space(INDENT_TABS);
                sAndReplace = GUILayout.TextField(sAndReplace, GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();

                // find and replace button
                GUILayout.BeginHorizontal();
                GUILayout.Space(INDENT_TABS);
                if (GUILayout.Button("Find And Replace", GUILayout.ExpandWidth(true)))
                {
                    if ((sFind.Length + sAndReplace.Length) > 0)
                    {  // search term input
                        if (Selection.gameObjects.Length == 1)
                        {  // 1 editor object selected
                            int iTotalFound = 0;
                            goActiveCharacter = Selection.activeGameObject;
                            foreach (Transform tChild in goActiveCharacter.transform)
                            {  // loop  through children
                                if (tChild.gameObject.name.Contains(sFind))
                                {  // matching child name
                                    tChild.gameObject.name = tChild.gameObject.name.Replace(sFind, sAndReplace);  // trim from the name
                                    iTotalFound += 1;
                                }
                            }
                            Debug.Log("Total " + (iTotalFound + 1).ToString() + " GameObject Names Updated!");
                        }
                        else
                        {
                            Debug.Log("Select a GameObject to Search (not 2)!");
                        }
                    }
                    else
                    {
                        Debug.Log("Find And Replace Text NOT Specified!");
                    }
                }
                GUILayout.EndHorizontal();

                // match child name to parent
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Space(INDENT_TABS);
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Label("Name 1st Child to Match the Parent", GUILayout.ExpandWidth(true));

                // update button
                GUILayout.BeginHorizontal();
                GUILayout.Space(INDENT_TABS);
                if (GUILayout.Button("Update Immediate Child", GUILayout.ExpandWidth(true)))
                {
                    if (Selection.gameObjects.Length == 1)
                    {  // 1 editor object selected                    
                        int iTotalFound = 0;
                        goActiveCharacter = Selection.activeGameObject;
                        if (goActiveCharacter.transform.childCount > 0)
                        {  // found children
                            if (goActiveCharacter.transform.GetChild(0).gameObject.name != goActiveCharacter.transform.name)
                            {  // 1st child name does not match parent
                                goActiveCharacter.transform.GetChild(0).gameObject.name = goActiveCharacter.transform.name;  // set the name for the 1st child
                                iTotalFound += 1;  // count changes
                            }
                        }
                        Debug.Log("Total " + iTotalFound.ToString() + " child names updated!");
                    }
                    else
                    {
                        Debug.Log("Select a GameObject to Update!");
                    }
                }
                if (GUILayout.Button("Update All Children", GUILayout.ExpandWidth(true)))
                {
                    if (Selection.gameObjects.Length == 1)
                    {  // 1 editor object selected                    
                        int iTotalFound = 0;
                        goActiveCharacter = Selection.activeGameObject;
                        foreach (Transform tChild in goActiveCharacter.transform)
                        {  // loop  through children
                            if (tChild.childCount > 0)
                            {  // found children
                                if (tChild.GetChild(0).gameObject.name != tChild.name)
                                {  // 1st child name does not match parent
                                    tChild.GetChild(0).gameObject.name = tChild.name;  // set the name for the 1st child
                                    iTotalFound += 1;  // count changes
                                }
                            }
                        }
                        Debug.Log("Total " + iTotalFound.ToString() + " child names updated!");
                    }
                    else
                    {
                        Debug.Log("Select a GameObject to Update!");
                    }
                }
                GUILayout.EndHorizontal();

                // end of last group
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
            catch (Exception ex)
            {
                vLogError(ref ex, "");
            }
        }

        /// <summary>
        /// Merge LOD levels, create LOD groups.
        /// </summary>
        private void vGUIMergeLODBones()
        {
            try
            {
                // process current selection
                if (Selection.gameObjects.Length == 0)
                {  // select a gameobject to analyse                    
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(INDENT_TABS);
                    GUILayout.Label("Select a GameObject to Analyse", GUILayout.ExpandWidth(true));
                    GUILayout.EndHorizontal();
                }
                else if (Selection.gameObjects.Length > 1)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(INDENT_TABS);
                    GUILayout.Label("Select just 1 GameObject to Analyse", GUILayout.ExpandWidth(true));
                    GUILayout.EndHorizontal();
                }
                else
                { // show analyse
                    goActiveCharacter = Selection.activeGameObject;  // store the selection in temp
                    if (sLastObjectAnalysed != goActiveCharacter.name)
                    {  // analyse
                        // analyse button
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(INDENT_TABS);
                        if (GUILayout.Button("Analyse", GUILayout.ExpandWidth(true)))
                        {
                            iCurrentGender = -1;  // clear the analyse equip character
                            sRootBoneName = "";  // clear the last root bone name
                            iLODsFound = 0;  // reset found LOD count
                            tFoundMaleLOD = goActiveCharacter.transform.Find("Male");      // check for seperate Male
                            tFoundFemaleLOD = goActiveCharacter.transform.Find("Female");  // and female LOD                            
                            if ((tFoundMaleLOD && !tFoundFemaleLOD) || (!tFoundMaleLOD && tFoundFemaleLOD))
                            {   // ensure no mismatch
                                Debug.Log("When creating a gender neutral character, both genders must be present!");
                            }
                            else if (tFoundMaleLOD && tFoundFemaleLOD)
                            {   // gender netural found, both genders present
                                foreach (Transform tChild in tFoundMaleLOD)
                                {  // loop  through children of female LOD
                                    if (tChild.name.ToUpper().Contains(" LOD") || tChild.name.ToUpper().Contains("_LOD"))
                                    {
                                        if (sRootBoneName == "")
                                        {  // not found the root already
                                            foreach (Transform tChildOfLOD in tChild)
                                            {  // loop  through children of the LOD
                                                if (tChildOfLOD.GetComponent<SkinnedMeshRenderer>())
                                                { // renderer present
                                                    SkinnedMeshRenderer targetRenderer = tChildOfLOD.GetComponent<SkinnedMeshRenderer>();  // grab the render                                
                                                    Transform tRendererRoot = targetRenderer.rootBone;  // grab the rootbone of the renderer
                                                    while (tRendererRoot.parent.name != goActiveCharacter.transform.name)
                                                    {  // loop through the bone parents until the top level bone found
                                                        tRendererRoot = tRendererRoot.parent;  // move up a level
                                                    }
                                                    sRootBoneName = tRendererRoot.name;  // update the root bone name
                                                }
                                            }
                                        }
                                        iLODsFound += 1;  // up the LOD count
                                    }
                                }
                            }
                            else
                            { // flat no gender on character                           
                                foreach (Transform tChild in goActiveCharacter.transform)
                                {  // loop  through children
                                    if (tChild.GetComponent<SkinnedMeshRenderer>())
                                    { // renderer present
                                        if (sRootBoneName == "")
                                        {  // not found the root already
                                            SkinnedMeshRenderer targetRenderer = tChild.GetComponent<SkinnedMeshRenderer>();  // grab the render                                
                                            Transform tRendererRoot = targetRenderer.rootBone;  // grab the rootbone of the renderer
                                            while (tRendererRoot.parent.name != goActiveCharacter.transform.name)
                                            {  // loop through the bone parents until the top level bone found
                                                tRendererRoot = tRendererRoot.parent;  // move up a level
                                            }
                                            sRootBoneName = tRendererRoot.name;  // update the root bone name
                                        }
                                    }
                                    else if (tChild.name.ToUpper().Contains(" LOD") || tChild.name.ToUpper().Contains("_LOD"))
                                    {
                                        iLODsFound += 1;  // up the LOD count
                                    }
                                }
                            }

                            // switch mode if root bone found
                            if (sRootBoneName.Length == 0)
                            {
                                Debug.Log("Root Bone NOT found from child skinned mesh renderers!");
                            }
                            else if (iLODsFound == 0)
                            {
                                Debug.Log("LOD2+ NEED to be children of LOD1!");
                            }
                            else
                            {  // ready to merge                                
                                sLastObjectAnalysed = goActiveCharacter.name;  // set the last object anaysed name so the merge options show
                                if (!(tFoundMaleLOD && tFoundFemaleLOD))
                                {   // not a gender neutral 
                                    iLODsFound += 1; // include LOD1 in the count as LOD1 is on the base level
                                }
                            }
                        }
                        GUILayout.EndHorizontal();
                    }
                    else
                    { // already analysed, show merge and options
                      // root bone name
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(INDENT_TABS);
                        GUILayout.Label("Root Bone", GUILayout.Width(80));
                        GUILayout.Label(sRootBoneName, GUILayout.ExpandWidth(true));
                        GUILayout.EndHorizontal();

                        // root bone name
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(INDENT_TABS);
                        GUILayout.Label("LODs Found", GUILayout.Width(80));
                        GUILayout.Label(iLODsFound.ToString(), GUILayout.ExpandWidth(true));
                        GUILayout.EndHorizontal();

                        // culling level
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(INDENT_TABS);
                        GUILayout.Label("Culling Level", GUILayout.Width(80));
                        sCullingLevel = GUILayout.TextField(sCullingLevel, GUILayout.ExpandWidth(true));
                        GUILayout.EndHorizontal();

                        // last LOD level 50% of others
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(INDENT_TABS);
                        GUILayout.Label("Set LOD" + iLODsFound.ToString(), GUILayout.Width(80));
                        bLastLODLevelHalf = GUILayout.Toggle(bLastLODLevelHalf, " to half the width of LOD" + (iLODsFound - 1).ToString(), GUILayout.ExpandWidth(true));
                        GUILayout.EndHorizontal();

                        // merge button
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(INDENT_TABS);
                        if (GUILayout.Button("Attempt Merge", GUILayout.ExpandWidth(true)))
                        {
                            // init                            
                            int iTotalLOD = 1;
                            bool bFound = false;

                            // ensure culling level is within the range 0.0 - 0.5
                            try
                            {
                                if (Convert.ToDouble(sCullingLevel) < 0 || Convert.ToDouble(sCullingLevel) > 0.5)
                                {
                                    Debug.Log("Culling level must be within the range 0.0-0.5 (where 0.5 = 50%)!");
                                    return;
                                }
                            }
                            catch
                            {
                                Debug.Log("Culling level must be within the range 0.0-0.5 (where 0.5 = 50%)!");
                                return;
                            }

                            // ensure the root bone name is valid
                            tRootBone = null;
                            tRootBone = goActiveCharacter.transform.Find(sRootBoneName);
                            if (!tRootBone)
                            {
                                Debug.Log("Root Bone NOT FOUND, operation cancelled!");
                                return;
                            }

                            // ensure LOD group numbers are contiguous
                            if (tFoundMaleLOD && tFoundFemaleLOD)
                            {   // gender netural found, both genders present
                                iTotalLOD = iEnsureLODNumbersAreContiguous(tFoundMaleLOD, 0);
                                iEnsureLODNumbersAreContiguous(tFoundFemaleLOD, 0);
                            }
                            else
                            {  // single gender character
                                iTotalLOD = iEnsureLODNumbersAreContiguous(goActiveCharacter.transform, 1);
                            }

                            // ensure we dont continue if no LOD levels
                            if (iTotalLOD <= 1)
                            {
                                Debug.Log("No LOD levels found!");
                                return;
                            }

                            // create the LOD group
                            LODGroup group = goActiveCharacter.AddComponent<LODGroup>();
                            LODLevel[] levelRenders = new LODLevel[iTotalLOD];

                            // process all children, merging bones with root
                            SkinnedMeshRenderer targetRenderer;
                            Dictionary<String, Transform> boneMap = new Dictionary<String, Transform>();
                            foreach (Transform tChild in goActiveCharacter.transform)
                            {  // loop  through children
                                if (sToPureAlpha(tChild.name) == "LOD")
                                {  // LOD Level?
                                    vProcessLODLevel(tChild, ref boneMap, ref levelRenders, false);  // process LOD level objects
                                }
                                else if ((tChild.name == "Male" || tChild.name == "Female") && (tFoundMaleLOD && tFoundFemaleLOD))
                                {  // gender root
                                    foreach (Transform tGenderChild in tChild)
                                    {  // loop  through children
                                        if (sToPureAlpha(tGenderChild.name) == "LOD")
                                        {  // LOD Level?
                                            vProcessLODLevel(tGenderChild, ref boneMap, ref levelRenders, (tChild.name == "Male" && tGenderChild.name == "LOD1" ? true : false));  // process LOD level objects
                                        }
                                    }
                                }
                                else if ((tChild.name.ToUpper() != sRootBoneName.ToUpper()))
                                {  // Not the root bone, must be LOD1 objects
                                    if (tChild.GetComponent<SkinnedMeshRenderer>())
                                    { // renderer present
                                        targetRenderer = tChild.GetComponent<SkinnedMeshRenderer>();  // grab the render
                                        foreach (Transform bone in targetRenderer.bones)
                                        {  // process all bones 
                                            boneMap[bone.name] = bone;  // Add each bone to the dictionary
                                        }
                                    }
                                }
                            }


                            // create LOD1                            
                            GameObject goLOD1 = null;
                            if (!(tFoundMaleLOD && tFoundFemaleLOD))
                            {  // when not a gender neutral character
                                goLOD1 = new GameObject();  // create it
                                goLOD1.transform.name = "LOD1";  // name it
                                goLOD1.transform.parent = goActiveCharacter.transform;  // attach as child of root
                            }

                            // assign root children that are not LOD levels or the root bone to LOD1
                            MeshRenderer targetMeshRenderer;
                            bFound = true;  // loop init condition
                            while (bFound)
                            {  // keep going until all LOD1 valid children have been found
                                bFound = false;  // set the dropout condition
                                foreach (Transform tChild in goActiveCharacter.transform)
                                {  // loop  through children
                                    if (tChild.name.ToUpper() == sRootBoneName.ToUpper())
                                    {  // check for objects attached to the bones
                                        Transform[] tAllBonesAndAttachments = tChild.GetComponentsInChildren<Transform>();  // list all bone transforms
                                        foreach (Transform tMaybeBone in tAllBonesAndAttachments)
                                        {  // loop though all bones and possible attached objects
                                            if (!tMaybeBone.name.Contains("_LOD"))
                                            { // ensure not from the other LOD levels
                                                targetMeshRenderer = tMaybeBone.GetComponent<MeshRenderer>();  // attempt grab mesh render
                                                if (targetMeshRenderer)
                                                {  // found attached object                                                                   
                                                    tMaybeBone.name = tMaybeBone.name + "_LOD1";  // rename it

                                                    // add the renderer to the LOD level renderer array
                                                    if (levelRenders[0].renderers == null)
                                                    {  // 1st time
                                                        levelRenders[0].renderers = new Renderer[1];  // create 1st element
                                                    }
                                                    else
                                                    {  // already found others
                                                        Array.Resize<Renderer>(ref levelRenders[0].renderers, levelRenders[0].renderers.Length + 1);  // extend array by 1
                                                    }
                                                    levelRenders[0].renderers[levelRenders[0].renderers.Length - 1] = targetMeshRenderer;  // add the renderer to the LOD array
                                                }
                                            }
                                        }
                                    }
                                    else if (sToPureAlpha(tChild.name) != "LOD" && !(tFoundMaleLOD && tFoundFemaleLOD))
                                    {  // found LOD 1 object (ignore when gender neutral)
                                        if (tChild.GetComponent<SkinnedMeshRenderer>())
                                        { // renderer present
                                            targetRenderer = tChild.GetComponent<SkinnedMeshRenderer>();  // grab the render                                        
                                            if (levelRenders[0].renderers == null)
                                            {  // 1st time
                                                levelRenders[0].renderers = new Renderer[1];  // create 1st element
                                            }
                                            else
                                            {  // already found others
                                                Array.Resize<Renderer>(ref levelRenders[0].renderers, levelRenders[0].renderers.Length + 1);  // extend array by 1
                                            }
                                            levelRenders[0].renderers[levelRenders[0].renderers.Length - 1] = targetRenderer;  // add the renderer to the LOD array
                                        }
                                        else if (tChild.GetComponent<MeshRenderer>())
                                        { // renderer present
                                            targetMeshRenderer = tChild.GetComponent<MeshRenderer>();  // grab the render                                        
                                            if (levelRenders[0].renderers == null)
                                            {  // 1st time
                                                levelRenders[0].renderers = new Renderer[1];  // create 1st element
                                            }
                                            else
                                            {  // already found others
                                                Array.Resize<Renderer>(ref levelRenders[0].renderers, levelRenders[0].renderers.Length + 1);  // extend array by 1
                                            }
                                            levelRenders[0].renderers[levelRenders[0].renderers.Length - 1] = targetMeshRenderer;  // add the renderer to the LOD array
                                        }

                                        tChild.parent = goLOD1.transform;  // move non LOD object to the LOD1 parent                                        
                                        bFound = true;  // unset dropout loop through again
                                        break;  // dropout of the loop as the foreach list needs regenerating
                                    }
                                }
                            }

                            // fill in the LOD group
                            LOD[] lods = new LOD[iTotalLOD];  // init the LOD array
                            float fLODDivide = (1.0F / (bLastLODLevelHalf ? iTotalLOD - 1 : iTotalLOD));  // work out the division levels
                            for (int i = 0; i < iTotalLOD; i++)
                            {   // process each renderer array  
                                float fWhere = 1.0F;
                                if (i == (iTotalLOD - 1))
                                {
                                    fWhere = (float)Convert.ToDouble(sCullingLevel);
                                }
                                else if (i == (iTotalLOD - 2) && bLastLODLevelHalf)
                                {
                                    fWhere -= ((fLODDivide * (i + 1)) - (fLODDivide / 2));
                                }
                                else
                                {
                                    fWhere -= (fLODDivide * (i + 1));
                                }
                                lods[i] = new LOD(fWhere, levelRenders[i].renderers);  // add to the LOD array
                                                                                       //Debug.Log(fWhere);
                            }
                            group.SetLODs(lods);  // apply the group
                            group.RecalculateBounds();  // init the group

                            // clean up
                            foreach (Transform tChild in goActiveCharacter.transform)
                            {  // loop  through children
                                if (tChild.childCount > 0)
                                {  // transform with child
                                    vSortGameObjectChildren(tChild.gameObject);  // sort children of child
                                }
                            }
                            vSortGameObjectChildren(goActiveCharacter);  // sort overall gameobject   
                            Debug.Log("All done, " + iTotalLOD.ToString() + " levels created!");
                            sLastObjectAnalysed = "";  // reset back to analyse
                        }
                        GUILayout.EndHorizontal();
                    }
                }
            }
            catch (Exception ex)
            {
                vLogError(ref ex, "");
            }
        }

        /// <summary>
        /// Creates a ragdoll from bone chains.
        /// </summary>
        //private void vGUIBuildGenericRagdoll()
        //{
        //    try
        //    {
        //        // find valid generic rigged chars in the scene
        //        if (!bNot1stOpenRagDoll || GUILayout.Button("Refresh", GUILayout.ExpandWidth(true)))
        //        {  // analyse button
        //            bNot1stOpenRagDoll = true;  // cause refresh on first open
        //            reset();  // clear flags & arrays
        //            iCurrentEnemy = -1;  // clear selection                
        //            goValidEnemies = null;  // ensure array is empty            
        //            GameObject[] goEnemiesFound = GameObject.FindGameObjectsWithTag("Enemy");  // find all with tag enemy                
        //            Animator aniTemp;   // temp object for valid animator/non human check
        //            foreach (GameObject goPotentialGenericEnemy in goEnemiesFound)
        //            {  // process all gameobjects found
        //                aniTemp = goPotentialGenericEnemy.GetComponent<Animator>();  // grab animator
        //                if (aniTemp)
        //                {  // valid?
        //                    if (aniTemp.avatar.isValid)
        //                    {  // valid avatar?
        //                        if (goValidEnemies == null)
        //                        {  // 1st?
        //                            goValidEnemies = new GameObject[1];  // create element 0
        //                            sValidEnemies = new string[1];  // same for the shortlist
        //                        }
        //                        else
        //                        {  // array initialised
        //                            Array.Resize<GameObject>(ref goValidEnemies, goValidEnemies.Length + 1);  // extend array by 1
        //                            Array.Resize<string>(ref sValidEnemies, sValidEnemies.Length + 1);  // same for the shortlist
        //                        }
        //                        goValidEnemies[goValidEnemies.Length - 1] = goPotentialGenericEnemy;  // add the enemy to array
        //                        sValidEnemies[sValidEnemies.Length - 1] = goPotentialGenericEnemy.name + (aniTemp.isHuman ? " (Human)" : " (Generic)");  // same for the shortlist
        //                    }
        //                }
        //            }
        //        }

        //        // build screen
        //        if (goValidEnemies != null)
        //        {
        //            // available enemy list
        //            GUILayout.BeginHorizontal();
        //            GUILayout.Label("Characters", GUILayout.Width(80));
        //            iNewValue = EditorGUILayout.Popup(iCurrentEnemy, sValidEnemies, GUILayout.ExpandWidth(true));  // dropdown of available AI's
        //            if (iNewValue != iCurrentEnemy)
        //            {  // new enemy selected    
        //                reset();  // clear flags & arrays
        //                iCurrentEnemy = iNewValue;  // update the selection. 
        //                goBuildRoot = goValidEnemies[iCurrentEnemy];  // store the selection gameobject for fast access
        //                Selection.activeGameObject = goBuildRoot;  // select in hierarchy

        //                // check for previous body root bones
        //                Transform[] tAllBonesAndAttachments = goBuildRoot.GetComponentsInChildren<Transform>();  // list all transforms
        //                foreach (Transform tMaybe in tAllBonesAndAttachments)
        //                {  // check all character joints
        //                    CharacterJoint cj = tMaybe.GetComponent<CharacterJoint>();  // attempt grab joint
        //                    if (cj)
        //                    {  // found
        //                        if (cj.connectedBody)
        //                        {
        //                            CharacterJoint cjP = cj.connectedBody.GetComponent<CharacterJoint>();  // attempt grab parent joint
        //                            if (!cjP)
        //                            {  // no joint on parent so is a root
        //                                if (!tRootBones.Contains(cj.connectedBody.transform))
        //                                {  // search root bone list
        //                                    tRootBones.Add(cj.connectedBody.transform); // not found, add
        //                                }
        //                            }
        //                        }
        //                    }
        //                }

        //                // attempt find default root bone from the 1st render
        //                foreach (Transform tMaybe in tAllBonesAndAttachments)
        //                {  // loop until find a renderer
        //                    targetRenderer = tMaybe.GetComponent<SkinnedMeshRenderer>();  // grab the render
        //                    if (targetRenderer)
        //                    {  // found?                                
        //                        tNext = targetRenderer.rootBone;  // grab its root
        //                        while (tNext.parent.name != goBuildRoot.name)
        //                        {  // loop backwards to find the real root bone
        //                            tNext = tNext.parent;  // move up
        //                        }
        //                        break;  // work complete                            
        //                    }
        //                }
        //                tRootBone = tNext;
        //                if (tRootBones.Count == 0)
        //                {
        //                    tRootBones.Add(tNext);  // add, even if null
        //                }
        //                updateRootBoneNameList();  // refresh the short list
        //                if (tRootBone)
        //                {  // root bone found
        //                    Selection.activeGameObject = tRootBone.gameObject;  // select in hierarchy
        //                }

        //                // list all bones in all skinned mesh renderers
        //                foreach (Transform tMaybe in tAllBonesAndAttachments)
        //                {  // process all renderers
        //                    targetRenderer = tMaybe.GetComponent<SkinnedMeshRenderer>();  // grab the render
        //                    if (targetRenderer)
        //                    {  // found?
        //                        foreach (Transform tBone in targetRenderer.bones)
        //                        {  // process all linked bones
        //                            if (!sAllBones.Contains(tBone.name))
        //                            { // not a duplicate
        //                                sAllBones.Add(tBone.name);  // push the bone name onto the array
        //                                CharacterJoint cj = tBone.GetComponent<CharacterJoint>();  // already a joint?

        //                                // set the collider from the bone (if it exists)
        //                                eColliderType ct = (tBone.name.ToUpper() == "HEAD" ? eColliderType.Sphere : eColliderType.Capsule);  // default collider type
        //                                float fFoundRadius = 0f;
        //                                BoxCollider bc = tBone.GetComponent<BoxCollider>();
        //                                SphereCollider sc = tBone.GetComponent<SphereCollider>();
        //                                CapsuleCollider cc = tBone.GetComponent<CapsuleCollider>();
        //                                if (bc)
        //                                {
        //                                    ct = eColliderType.Box;
        //                                }
        //                                else if (sc)
        //                                {
        //                                    ct = eColliderType.Sphere;
        //                                    fFoundRadius = sc.radius;
        //                                }
        //                                else if (cc)
        //                                {
        //                                    ct = eColliderType.Capsule;
        //                                    fFoundRadius = cc.radius;
        //                                }

        //                                // set the bone options
        //                                lAllBones.Add(new BoneOptions() { Shown = cj, Type = ct, Bone = tBone, Radius = fFoundRadius });
        //                            }
        //                        }
        //                    }
        //                }

        //                // set hitboxes toggle
        //                Component[] goHitboxes = goBuildRoot.GetComponentsInChildren(typeof(vHitBox), true);  // get all hitboxes
        //                if (goHitboxes.Length > 0)
        //                {  // found some?
        //                    bHitboxesShown = goHitboxes[0].transform.gameObject.activeInHierarchy;  // toggle on if first active
        //                }
        //                else
        //                {  // thats a negative
        //                    bHitboxesShown = false;  // toggle off
        //                }

        //            }
        //            GUILayout.EndHorizontal();

        //            // character selected
        //            if (iCurrentEnemy > -1)
        //            {
        //                // list known root bones
        //                for (int i = 0; i < tRootBones.Count; i++)
        //                {
        //                    GUILayout.BeginHorizontal();
        //                    GUILayout.Label("Root Bone", GUILayout.Width(80));
        //                    tNewValue = EditorGUILayout.ObjectField(tRootBones[i], typeof(Transform), true) as Transform;
        //                    if (tRootBones[i] != tNewValue)
        //                    {  // updated?
        //                        tRootBones[i] = tNewValue;  // set
        //                        updateRootBoneNameList();  // refresh the short list                            
        //                    }
        //                    if (GUILayout.Button("X", GUILayout.Width(30)))
        //                    {  // remove root bone
        //                        tRootBones.Remove(tRootBones[i]);  // death, use with care
        //                        updateRootBoneNameList();  // refresh the short list
        //                    }
        //                    GUILayout.EndHorizontal();
        //                }
        //                if (GUILayout.Button("Add Root Bone", GUILayout.ExpandWidth(true)))
        //                {
        //                    tRootBones.Add(null);  // add an empty root bone slot
        //                    updateRootBoneNameList();  // refresh the short list                        
        //                }

        //                // mass to apply to each bone rigid body
        //                GUILayout.BeginHorizontal();
        //                GUILayout.Label("Bone Mass", GUILayout.Width(80));
        //                fBoneMass = EditorGUILayout.FloatField(fBoneMass, GUILayout.ExpandWidth(true));
        //                GUILayout.EndHorizontal();


        //                // hide/show hitboxes
        //                if (GUILayout.Button("Toggle Hitboxes " + (bHitboxesShown ? "OFF" : "ON"), GUILayout.ExpandWidth(true)))
        //                {
        //                    bHitboxesShown = !bHitboxesShown;  // toggle flag
        //                    Component[] goHitboxes = goBuildRoot.GetComponentsInChildren(typeof(vHitBox), true);  // get all hitboxes
        //                    if (goHitboxes.Length > 0)
        //                    {  // found some?
        //                        foreach (vHitBox hb in goHitboxes)
        //                        {
        //                            hb.transform.gameObject.SetActive(bHitboxesShown);  // toggle the hitbox
        //                        }
        //                    }
        //                }

        //                // list bone chains
        //                if (tRootBone)
        //                {  // found default root bone
        //                   // display limb chains
        //                    if (lcLimbChainList.Count == 0)
        //                    {  // scan the object 1st
        //                        GUILayout.BeginHorizontal();
        //                        if (GUILayout.Button("Scan for bone chains", GUILayout.ExpandWidth(true)))
        //                        {  // analyse linked bone chains list
        //                            lcLimbChainList = new List<LimbChain>();  // clear the previous limb chains
        //                            addLimbChainStarts(tRootBone);  // add limb chain starts from the root bone
        //                            bool bMore = true;  // flag to keep looping until all chains found
        //                            while (bMore)
        //                            {  // more chains to process recursivly
        //                                bMore = false;  // drop out if no more sub chains
        //                                foreach (LimbChain lc in lcLimbChainList)
        //                                {  // check all limb chains found
        //                                    if (!lc.ChainEnd)
        //                                    {  // new limb chain
        //                                       // init
        //                                        bMore = true;  // process the array again
        //                                        bool bAlreadyHas = false;  // init the processed already check
        //                                        lc.Bones = new List<int>();  // init the bone chain list
        //                                        Transform tNext = lc.ChainStart;  // init the forward loop  
        //                                        if (tNext.GetComponent<CharacterJoint>()) bAlreadyHas = true;  // joint component check
        //                                        lc.Bones.Add(sAllBones.IndexOf(tNext.name));  // add the start bone

        //                                        // loop down the chain
        //                                        while (iCountBoneChilds(tNext) == 1)
        //                                        {  // process the bone hierarchy until no more bones or new sub bone chains found                 
        //                                            tNext = tNext.GetChild(0);  // check next bone child
        //                                            lc.Links += 1;  // up the link count
        //                                            if (tNext.GetComponent<CharacterJoint>()) bAlreadyHas = true;  // joint component check
        //                                            lc.Bones.Add(sAllBones.IndexOf(tNext.name));  // add the bone                                                
        //                                        }

        //                                        // finishup
        //                                        lc.AddedAlready = bAlreadyHas;  // flag whether already added the components
        //                                                                         //lc.iBones.Add(sAllBones.IndexOf(tNext.name));  // add the bone
        //                                        lc.ChainEnd = tNext;  // store the chain end

        //                                        // enforce all members of the bone chain visible for members set to not include if any link members added already
        //                                        if (bAlreadyHas)
        //                                        {  // found link in chain with a character joint
        //                                            for (int i = 0; i < lc.Bones.Count; i++)
        //                                            {  // check all bones
        //                                               // set visibility
        //                                                if (!lAllBones[lc.Bones[i]].Shown)
        //                                                {  // not currently visible
        //                                                    lAllBones[lc.Bones[i]].Shown = true;  // flag visible
        //                                                    lAllBones[lc.Bones[i]].Type = eColliderType.DontInclude;  // flag as not included
        //                                                }

        //                                                // search for the root bone for the chain
        //                                                if (lAllBones[lc.Bones[i]].Type != eColliderType.DontInclude)
        //                                                {  // bone is included
        //                                                    CharacterJoint cj = lAllBones[lc.Bones[i]].Bone.GetComponent<CharacterJoint>();  // grab char joint
        //                                                    if (cj)
        //                                                    {  // found?
        //                                                        for (int j = 0; j < tRootBones.Count; j++)
        //                                                        {  // check all roots
        //                                                            if (cj.connectedBody.transform == tRootBones[j])
        //                                                            {  // matched root bone
        //                                                                lc.Rootbone = j;  // set the root bone for the chain
        //                                                                break;
        //                                                            }
        //                                                        }
        //                                                    }
        //                                                }
        //                                            }
        //                                        }

        //                                        // refresh the ends of the chain                                            
        //                                        checkLimbChainEnds(lc);

        //                                        // loop again?
        //                                        if (iCountBoneChilds(tNext) > 0)
        //                                        {  // sub chains found
        //                                            addLimbChainStarts(tNext);  // create more limb chain starts        
        //                                            break;  // array needs re-processing as has been modified
        //                                        }
        //                                    }
        //                                }
        //                            }

        //                            // check for limb chains linked to other limb chains (now we have them all)
        //                            foreach (LimbChain lc in lcLimbChainList)
        //                            {  // process all chains
        //                                if (lc.AddedAlready)
        //                                {  // only include chains already with components
        //                                    CharacterJoint cj = lc.ChainStart.GetComponent<CharacterJoint>();  // grab char joint
        //                                    if (cj)
        //                                    {  // success
        //                                        if (cj.connectedBody.transform != tRootBones[lc.Rootbone])
        //                                        {  // bone linked to is not the root bone
        //                                            for (int i = 0; i < tLinkBones.Count; i++)
        //                                            {  // check all link bones
        //                                                if (cj.connectedBody.transform == tLinkBones[i])
        //                                                {  // found?
        //                                                    lc.Linkbone = i + 1;  // set
        //                                                    break;  // work complete
        //                                                }
        //                                            }
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                        }
        //                        GUILayout.EndHorizontal();
        //                    }
        //                    else
        //                    {   // scanned already display
        //                        // header row
        //                        GUILayout.BeginHorizontal();
        //                        GUILayout.Label("Inc", GUILayout.Width(30));
        //                        GUILayout.Label("Root Bone", GUILayout.ExpandWidth(true));
        //                        GUILayout.Label("Link Bone", GUILayout.ExpandWidth(true));
        //                        GUILayout.Label("Chain Start", GUILayout.ExpandWidth(true));
        //                        GUILayout.Label("Chain End", GUILayout.ExpandWidth(true));
        //                        GUILayout.Label("Del", GUILayout.Width(30));
        //                        GUILayout.EndHorizontal();

        //                        // limb chain lines GUI for enable and analysis       
        //                        int iEnabledChains = 0;
        //                        int iAddedAlready = 0;
        //                        foreach (LimbChain lc in lcLimbChainList)
        //                        {  // display all limb chains found
        //                            GUILayout.BeginHorizontal();

        //                            // toggle include in ragdoll
        //                            if (lc.AddedAlready)
        //                            {  // already added components
        //                                GUILayout.Toggle(true, "", GUILayout.Width(10));  // dont allow change
        //                            }
        //                            else
        //                            {  // not added already, include
        //                                lc.Enable = GUILayout.Toggle(lc.Enable, "", GUILayout.Width(10));  // allow inclusion (or not)
        //                                if (lc.Enable) iEnabledChains++;  // count for the build button enable
        //                            }

        //                            // length of the chain      
        //                            GUILayout.Label((lc.Links < 10 ? "0" : "") + lc.Links.ToString(), GUILayout.Width(20));

        //                            // root bone to link to
        //                            iNewValue = EditorGUILayout.Popup(lc.Rootbone, sRootBones, GUILayout.ExpandWidth(true));
        //                            if (lc.Rootbone != iNewValue)
        //                            {  // new root bone                                    
        //                                lc.Rootbone = iNewValue;  // set it   

        //                                // apply the change to the character joint
        //                                if (lc.Linkbone == 0)
        //                                {  // link bone override not enabled?
        //                                    CharacterJoint cj = lc.ChainStart.GetComponent<CharacterJoint>();  // attempt grab character joint
        //                                    if (cj)
        //                                    {  // found character joint
        //                                        Rigidbody rb = tRootBones[lc.Rootbone].GetComponent<Rigidbody>();  // find root rigidbody
        //                                        if (rb)
        //                                        { // root bone has rigid body to connect to
        //                                            cj.connectedBody = rb; // connect to the new root
        //                                        }
        //                                    }
        //                                }
        //                            }

        //                            // link bone to link to instead of root
        //                            iNewValue = EditorGUILayout.Popup(lc.Linkbone, sLinkBones, GUILayout.ExpandWidth(true));
        //                            if (lc.Linkbone != iNewValue)
        //                            {  // new link bone?
        //                                lc.Linkbone = iNewValue;  // set it   

        //                                // apply the change to the character joint
        //                                CharacterJoint cj = lc.ChainStart.GetComponent<CharacterJoint>();  // attempt grab character joint
        //                                if (cj)
        //                                {  // found character joint
        //                                    if (lc.Linkbone > 0)
        //                                    {  // link bone override enabled
        //                                        Rigidbody rb = tLinkBones[lc.Linkbone - 1].GetComponent<Rigidbody>();  // find root rigidbody
        //                                        if (rb)
        //                                        { // root bone has rigid body to connect to
        //                                            cj.connectedBody = rb; // connect to the new link bone
        //                                        }
        //                                    }
        //                                    else
        //                                    {  // revert to root bone
        //                                        Rigidbody rb = tRootBones[lc.Rootbone].GetComponent<Rigidbody>();  // find root rigidbody
        //                                        if (rb)
        //                                        { // root bone has rigid body to connect to
        //                                            cj.connectedBody = rb; // connect to the new root
        //                                        }
        //                                    }
        //                                }
        //                            }

        //                            // start/end chain transforms
        //                            tNewValue = EditorGUILayout.ObjectField(lc.ChainStart, typeof(Transform), true) as Transform;  // chain start and
        //                            tNewValue = EditorGUILayout.ObjectField(lc.ChainEnd, typeof(Transform), true) as Transform;  // end transform

        //                            // remove from ragdoll
        //                            if (lc.AddedAlready)
        //                            {  // already added components
        //                                if (GUILayout.Button("x", GUILayout.Width(30)))
        //                                {  // remove components
        //                                    lc.AddedAlready = false;  // deflag already has components
        //                                    lc.Enable = false;  // deflag include in build
        //                                    tNext = lc.ChainStart;  // init the forward loop                                        
        //                                    while (iCountBoneChilds(tNext) == 1)
        //                                    {  // process the bone hierarchy until no more bones or new sub bone chains found
        //                                        RemoveBoneComponents(sAllBones.IndexOf(tNext.name), false);  // remove previous components from next link
        //                                        tNext = tNext.GetChild(0);  // iterate to the next level
        //                                    }
        //                                    RemoveBoneComponents(sAllBones.IndexOf(tNext.name), false);  // remove previous components from chain end    
        //                                }
        //                                else
        //                                {
        //                                    iAddedAlready++;  // count of how many added already
        //                                }
        //                            }
        //                            else
        //                            {  // not added already
        //                                GUILayout.Space(30);  // so no remove button
        //                            }
        //                            GUILayout.EndHorizontal();

        //                            // show bones chain links with collider/dont include options
        //                            if (lc.AddedAlready)
        //                            {  // bone chain included in the ragdoll
        //                                foreach (int i in lc.Bones)
        //                                {  // process the link chain
        //                                   // bone transform
        //                                    GUILayout.BeginHorizontal();
        //                                    tNewValue = EditorGUILayout.ObjectField(lAllBones[i].Bone, typeof(Transform), true) as Transform;

        //                                    // collider type
        //                                    ctNewValue = (eColliderType)EditorGUILayout.EnumPopup(lAllBones[i].Type, GUILayout.ExpandWidth(true));
        //                                    if (ctNewValue != lAllBones[i].Type)
        //                                    {  // changed?     
        //                                       // update the collider/inclusion
        //                                        if (lAllBones[i].Type == eColliderType.DontInclude)
        //                                        {  // bone needs including
        //                                            lAllBones[i].Type = ctNewValue;  // set new value
        //                                            AddBoneComponents(lAllBones[i].Bone, findValidBoneLinkParent(i));  // add the components
        //                                            Transform tValidChild = findValidBoneLinkChild(i);  // find next enabled child in the chain
        //                                            if (tValidChild)
        //                                            {  // found child?  if not found then id new end of the chain
        //                                                Rigidbody rb = lAllBones[i].Bone.GetComponent<Rigidbody>();  // grab the just added rigid body
        //                                                if (rb)
        //                                                {  // found rigid body?
        //                                                    CharacterJoint cj = tValidChild.GetComponent<CharacterJoint>();  // grab child char joint
        //                                                    if (cj)
        //                                                    {  // found child joint?
        //                                                        cj.connectedBody = rb;  // link to new parent
        //                                                    }
        //                                                }
        //                                            }
        //                                        }
        //                                        else if (ctNewValue == eColliderType.DontInclude)
        //                                        {  // bone needs excluding                                                                                        
        //                                            RemoveBoneComponents(i, false);  // clear off character joint, rigidbody etc 
        //                                            lAllBones[i].Shown = true;  // enforce shown
        //                                            Transform tValidChild = findValidBoneLinkChild(i);  // find the child to link to the parent
        //                                            if (tValidChild)
        //                                            {  // found child?  if not found then id new end of the chain
        //                                                Transform tValidParent = findValidBoneLinkParent(i);  // find the next included parent
        //                                                if (tValidParent)
        //                                                {  // found parent? 
        //                                                    Rigidbody rb = tValidParent.GetComponent<Rigidbody>();  // rigidbody for the child to link to
        //                                                    if (rb)
        //                                                    {  // found rigidbody?  
        //                                                        CharacterJoint cj = tValidChild.GetComponent<CharacterJoint>();  // grab child char joint
        //                                                        if (cj)
        //                                                        {  // found child joint?
        //                                                            cj.connectedBody = rb;  // link to parent
        //                                                        }
        //                                                    }
        //                                                }
        //                                            }
        //                                            lAllBones[i].Type = ctNewValue;  // set new value
        //                                        }
        //                                        else
        //                                        {  // swapping collider type                                            
        //                                            RemoveBoneComponents(i, true);  // remove previous collider     
        //                                            if (lAllBones[i].Type == eColliderType.Sphere)
        //                                            {  // handle capsule/box radius added to length
        //                                                lAllBones[i].Radius = lAllBones[i].Radius / 2;
        //                                            }
        //                                            else if (ctNewValue == eColliderType.Sphere)
        //                                            {  // dont increase if change from box to capsule
        //                                                lAllBones[i].Radius = lAllBones[i].Radius * 2;
        //                                            }
        //                                            lAllBones[i].Type = ctNewValue;  // set new value
        //                                            AddBoneCollider(i);  // add the new collider                                        
        //                                        }

        //                                        // refresh the ends of the chain                                            
        //                                        checkLimbChainEnds(lc);
        //                                    }
        //                                    GUILayout.EndHorizontal();
        //                                }

        //                            }
        //                        }

        //                        // build the ragdoll
        //                        if (iEnabledChains > 0)
        //                        {
        //                            if (GUILayout.Button("Update the Ragdoll", GUILayout.ExpandWidth(true)))
        //                            {
        //                                // add ragdoll + audio source if missing
        //                                Transform ragdollAudioSource = goBuildRoot.transform.Find("ragdollAudioSource");
        //                                if (!ragdollAudioSource)
        //                                {
        //                                    ragdollAudioSource = new GameObject("ragdollAudioSource").transform;
        //                                    ragdollAudioSource.SetParent(goBuildRoot.transform);
        //                                }
        //                                Transform collisionAudio = ragdollAudioSource.transform.Find("collisionAudio");
        //                                if (!collisionAudio)
        //                                {
        //                                    collisionAudio = new GameObject("collisionAudio", typeof(AudioSource)).transform;
        //                                    collisionAudio.SetParent(ragdollAudioSource.transform);
        //                                    collisionAudio.GetComponent<AudioSource>().playOnAwake = false;
        //                                }
        //                                if (!goBuildRoot.gameObject.GetComponent<GenericRIGRagdoll>())
        //                                {
        //                                    var rag = goBuildRoot.AddComponent<GenericRIGRagdoll>();
        //                                    rag.collisionSource = collisionAudio.GetComponent<AudioSource>();
        //                                }

        //                                // setup non joint root (aka the body)
        //                                foreach (Transform t in tRootBones)
        //                                {
        //                                    if (t)
        //                                    { // ensure root bone set
        //                                        if (!t.gameObject.GetComponent<Rigidbody>())
        //                                        {
        //                                            Rigidbody rb = t.gameObject.AddComponent<Rigidbody>();
        //                                            rb.useGravity = true;
        //                                            rb.mass = fBoneMass * 5;
        //                                        }
        //                                        if (!t.gameObject.GetComponent<BoxCollider>())
        //                                        {
        //                                            t.gameObject.AddComponent<BoxCollider>();
        //                                        }
        //                                        if (!t.gameObject.GetComponent<vCollisionMessage>())
        //                                        {
        //                                            t.gameObject.AddComponent<vCollisionMessage>();
        //                                        }
        //                                    }
        //                                }

        //                                // process the chains                                    
        //                                foreach (LimbChain lc in lcLimbChainList)
        //                                {  // process all chains
        //                                    if (lc.Enable)
        //                                    { // that are enabled
        //                                        for (int b = 0; b < lc.Bones.Count; b++)
        //                                        {  // process the bone chain
        //                                            if (lAllBones[lc.Bones[b]].Type != eColliderType.DontInclude)
        //                                            {
        //                                                Transform tParent;  // force null
        //                                                if (lAllBones[lc.Bones[b]].Bone == lc.ChainStart)
        //                                                {  // 1st enabled bone in the chain
        //                                                    if (lc.Linkbone == 0)
        //                                                    {   // no link bone override
        //                                                        tParent = tRootBones[lc.Rootbone];   // link the character joint to the root
        //                                                    }
        //                                                    else
        //                                                    {  // override found
        //                                                        tParent = tLinkBones[lc.Linkbone - 1];   // link the character joint to the link bone override
        //                                                    }
        //                                                }
        //                                                else
        //                                                {
        //                                                    tParent = findValidBoneLinkParent(lc.Bones[b]); // find the parent to link the character joint to
        //                                                }
        //                                                if (tParent)
        //                                                {  // failsafe, should always be true
        //                                                    AddBoneComponents(lAllBones[lc.Bones[b]].Bone, tParent);  // add the components
        //                                                }
        //                                            }
        //                                        }
        //                                        lc.Enable = false;  // deflag for add to build
        //                                        lc.AddedAlready = true;  // flag for component remove
        //                                    }
        //                                }
        //                            }
        //                        }


        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        vLogError(ref ex, "");
        //    }
        //}

        /// <summary>
        /// Character equipment & material selector.
        /// </summary>
        private void vGUIEquipCharacter()
        {
            try
            {
                // check current character has been manually deleted
                if (iCurrentCharacter >= 0 && goActiveCharacter == null)
                {
                    iCurrentCharacter = -1;
                    iCurrentCharacterFiltered = -1;
                }

                // creator filter
                GUILayout.BeginHorizontal();
                GUILayout.Space(INDENT_TABS);
                GUILayout.Label("Creator", GUILayout.Width(80));
                iNewValue = EditorGUILayout.Popup(iCurrentCreator, sCreatorPrefix, GUILayout.ExpandWidth(true));
                if (iNewValue != iCurrentCreator)
                {  // new creator selected
                    iCurrentCreator = iNewValue;  // store new selection
                    iCurrentCharacterFiltered = -1;  // clear the previous selection
                    sCharPathShortList = null;  // clear the filter list
                    if (goActiveCharacter != null) DestroyImmediate(goActiveCharacter);  // remove old
                    goActiveCharacter = null;
                    foreach (stCharPaths cc in ccCharPaths)
                    {  // process all detected prefabs
                        if (sCreatorPrefix[iCurrentCreator] == "All" || (sCreatorPrefix[iCurrentCreator] == "Not SFB" && !cc.sFriendlyName.StartsWith("SFB")) || cc.sFriendlyName.StartsWith(sCreatorPrefix[iCurrentCreator]))
                        { // name matches creator filter                    
                            if (sCharPathShortList == null)
                            {  // empty list
                                sCharPathShortList = new string[1];  // create element 0                                
                            }
                            else
                            {  // not empty
                                Array.Resize<string>(ref sCharPathShortList, sCharPathShortList.Length + 1);  // extend array by 1
                            }
                            sCharPathShortList[sCharPathShortList.Length - 1] = cc.sFriendlyName.Replace(sCreatorPrefix[iCurrentCreator], "");  // append to the short list
                        }
                    }
                }
                GUILayout.EndHorizontal();

                // filtered list character selection
                if (sCharPathShortList != null)
                {  // dont show if no characters found
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(INDENT_TABS);
                    GUILayout.Label("Character", GUILayout.Width(80));
                    iNewValue = EditorGUILayout.Popup(iCurrentCharacterFiltered, sCharPathShortList, GUILayout.ExpandWidth(true));
                    if (iNewValue >= 0 && iNewValue != iCurrentCharacterFiltered)
                    {  // new character selected
                        if (goActiveCharacter != null) DestroyImmediate(goActiveCharacter);  // remove old
                        iCurrentCharacterFiltered = iNewValue;  // update the selection
                        for (int i = 0; i < ccCharPaths.Length; i++)
                        {  // find the filtered name in the all list
                            if (ccCharPaths[i].sFriendlyName.Replace(sCreatorPrefix[iCurrentCreator], "") == sCharPathShortList[iCurrentCharacterFiltered])
                            {  // matching?
                                iCurrentCharacter = i;  // yes it is
                                break;  // work complete
                            }
                        }
                        UnityEngine.Object oPrefab = AssetDatabase.LoadAssetAtPath(ccCharPaths[iCurrentCharacter].sFullPath, typeof(GameObject));  // load the prefab
                        goActiveCharacter = Instantiate(oPrefab, Vector3.zero, Quaternion.identity) as GameObject;  // insert into the scene
                        goActiveCharacter.name = ccCharPaths[iCurrentCharacter].sFriendlyName + "__BASE";  // update the name                    
                        sLastObjectAnalysed = "";  // reset back to  analyse mode  
                        Transform vThirdPersonMelee = goActiveCharacter.transform.Find("vThirdPersonMelee");
                        if (vThirdPersonMelee)
                        {
                            goActiveCharacter = vThirdPersonMelee.gameObject;
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                if (goActiveCharacter != null)
                {  // ensure a character is generated
                    if (sLastObjectAnalysed != goActiveCharacter.name)
                    {  // analyse mode
                       // analyse button
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(INDENT_TABS);
                        if (GUILayout.Button("Analyse", GUILayout.ExpandWidth(true)))
                        {
                            // check object for male/female children
                            if (!(goActiveCharacter.transform.Find("Male") && goActiveCharacter.transform.Find("Female")))
                            {
                                iCurrentGender = 2;  // both
                            }
                            else
                            {
                                if (!goActiveCharacter.transform.Find("Female").gameObject.activeSelf)
                                {
                                    iCurrentGender = 0;  // male
                                }
                                else
                                {
                                    iCurrentGender = 1;  // female
                                }
                            }

                            // search for equipment/body parts
                            ccCharComponents = null;  // clear the components list
                            ccCharMaterials = null;  // clear the materials list
                            foreach (Transform tChild in goActiveCharacter.GetComponentsInChildren<Transform>(true))// loop through children recursive
                            {  
                                if (tChild.GetComponent<MeshRenderer>() || tChild.GetComponent<SkinnedMeshRenderer>())// valid mesh component        
                                {                          
                                    // check if member of LOD group parent object
                                    bool bLOD = false;  // LOD level flag
                                    bool bLOD1 = false;   // LOD1 level flag   
                                    eGender egSex = eGender.Both;
                                    if (tChild.name.Contains("_LOD")) // is it LOD in bones
                                    {  
                                        bLOD = true;  // it is
                                        if (tChild.name.EndsWith("_LOD1"))
                                        {  // is it LOD1
                                            bLOD1 = true;  // it is indeed
                                        }
                                    }
                                    else // search back to root to determine if child of LOD level
                                    { 
                                        Transform tPreviousChild = tChild;  // store the previous for when has gender
                                        Transform tFindParent = tChild;  // init the search
                                        while (tFindParent.parent.name != goActiveCharacter.name && !(
                                            tFindParent.parent.name == "Male" || 
                                            tFindParent.parent.name == "Female" || 
                                            tFindParent.parent.name.Contains("_Holder")
                                        ))
                                        {  // loop back until find child of object being analyzed                                        
                                            tFindParent = tFindParent.parent;  // move up 1 level
                                            tPreviousChild = tFindParent;  // store the current level
                                        }

                                        // dont include weapon holders
                                        if (tFindParent.parent.name.Contains("_Holder"))
                                        {
                                            continue;  // skip
                                        }

                                        // set the gender if relevent
                                        if (tFindParent.parent.name == "Male")
                                        {
                                            egSex = eGender.Male;  // its a boy
                                            tFindParent = tPreviousChild;  // move the LOD check back to the level below
                                        }
                                        else if (tFindParent.parent.name == "Female")
                                        {
                                            egSex = eGender.Female;  // its a girl
                                            tFindParent = tPreviousChild;  // move the LOD check back to the level below
                                        }

                                        // which LOD level if relevent
                                        if (sToPureAlpha(tFindParent.name) == "LOD")
                                        {  // is it a LOD level
                                            bLOD = true;  // it is
                                            if (iToPureNumeric(tFindParent.name) == 1)
                                            {  // is it LOD 1
                                                bLOD1 = true;  // it is indeed
                                            }
                                        }
                                    }

                                    // add to the array (if valid)
                                    if (bLOD1 || !bLOD)
                                    {
                                        if (ccCharComponents == null)
                                        {  // empty array
                                            ccCharComponents = new stCharObjects[1];  // create element 0
                                        }
                                        else
                                        {  // not empty
                                            Array.Resize<stCharObjects>(ref ccCharComponents, ccCharComponents.Length + 1);  // extend array by 1
                                        }
                                        ccCharComponents[ccCharComponents.Length - 1].goRef = tChild.gameObject;  // store the game object
                                        ccCharComponents[ccCharComponents.Length - 1].sFriendlyName = tChild.gameObject.name.Replace("_LOD1", "").ToUpper();  // display name
                                        ccCharComponents[ccCharComponents.Length - 1].goLOD2Plus = null;  // ensure null for search
                                        ccCharComponents[ccCharComponents.Length - 1].egSex = egSex;  // store the gender of the component
                                        ccCharComponents[ccCharComponents.Length - 1].bEquiped = tChild.gameObject.activeInHierarchy;  // is the component visible
                                        Renderer targetRenderer = tChild.GetComponent<Renderer>();  // grab the render
                                        for (int m = 0; m < targetRenderer.sharedMaterials.Length; m++)
                                        { // loop through all materials on the object
                                            if (ccCharComponents[ccCharComponents.Length - 1].iMaterial == null)
                                            {  // 1st component material
                                                ccCharComponents[ccCharComponents.Length - 1].iMaterial = new int[1];  // create empty material
                                                ccCharComponents[ccCharComponents.Length - 1].iMaterialVariant = new int[1];  // create empty material varient                                         
                                            }
                                            else
                                            {  // multi material component
                                                Array.Resize<int>(ref ccCharComponents[ccCharComponents.Length - 1].iMaterial, ccCharComponents[ccCharComponents.Length - 1].iMaterial.Length + 1);  // extend the material array
                                                Array.Resize<int>(ref ccCharComponents[ccCharComponents.Length - 1].iMaterialVariant, ccCharComponents[ccCharComponents.Length - 1].iMaterialVariant.Length + 1);  // extend the material variant array
                                            }

                                            // handle the materials
                                            ccCharComponents[ccCharComponents.Length - 1].bDeleted = false;  // init the deleted flag
                                            ccCharComponents[ccCharComponents.Length - 1].iMaterial[m] = -1;  // set to not found
                                            ccCharComponents[ccCharComponents.Length - 1].iMaterialVariant[m] = -1;  // set to not found                                        
                                            if (ccCharMaterials != null)
                                            {  // materials found already, search
                                                for (int i = 0; i < ccCharMaterials.Length; i++)
                                                {  // loop through all materials found so far
                                                    for (int j = 0; j < ccCharMaterials[i].sVarients.Length; j++)
                                                    {  // loop though all varients of the material
                                                        if (ccCharMaterials[i].sVarients[j] == targetRenderer.sharedMaterials[m].name.Replace(" (Instance)", ""))
                                                        {  // matching varient
                                                            ccCharComponents[ccCharComponents.Length - 1].iMaterial[m] = i;  // assign material
                                                            ccCharComponents[ccCharComponents.Length - 1].iMaterialVariant[m] = j;  // and variant
                                                            break;  // work complete
                                                        }
                                                    }
                                                }
                                            }
                                            if (ccCharComponents[ccCharComponents.Length - 1].iMaterial[m] == -1)
                                            {  // not found
                                                if (Directory.Exists(Application.dataPath + "/" + ccCharPaths[iCurrentCharacter].sMaterialsSearchPath))
                                                {  // ensure search path exists                                                
                                                    DirectoryInfo diPath = new DirectoryInfo(Application.dataPath + "/" + ccCharPaths[iCurrentCharacter].sMaterialsSearchPath);  // grab the search dir info
                                                    FileInfo[] fiPath = diPath.GetFiles("*.mat", SearchOption.AllDirectories);  // pull list of all materials on disk
                                                    for (int i = 0; i < fiPath.Length; i++)
                                                    {  // loop through all materials
                                                        if (fiPath[i].Name.Replace(".mat", "") == targetRenderer.sharedMaterials[m].name.Replace(" (Instance)", ""))
                                                        {  // found renderer material on disk
                                                            if (ccCharMaterials == null)
                                                            {  // 1st ever material
                                                                ccCharMaterials = new stMaterialsList[1];  // create 1st element
                                                            }
                                                            else
                                                            {  // we have some already
                                                                Array.Resize<stMaterialsList>(ref ccCharMaterials, ccCharMaterials.Length + 1);  // extend the array
                                                            }
                                                            ccCharMaterials[ccCharMaterials.Length - 1].sFriendlyName = fiPath[i].Directory.Name;  // name the group of material varients by the containing folder name
                                                            ccCharMaterials[ccCharMaterials.Length - 1].sBaseFolder = fiPath[i].DirectoryName.Replace("\\", "/").Replace(Application.dataPath + "/", "").Replace("Resources/", "");  // store the base directory
                                                            ccCharComponents[ccCharComponents.Length - 1].iMaterial[m] = ccCharMaterials.Length - 1;  // store the material id
                                                            DirectoryInfo diBasePath = new DirectoryInfo(fiPath[i].DirectoryName);  // setup the base dir info
                                                            FileInfo[] fiBasePath = diBasePath.GetFiles("*.mat", SearchOption.TopDirectoryOnly);  // pull list of all material variants on disk from the base dir
                                                            for (int ii = 0; ii < fiBasePath.Length; ii++)
                                                            {  // loop through all material variants
                                                                if (ccCharMaterials[ccCharMaterials.Length - 1].sVarients == null)
                                                                {  // 1st ever material variant
                                                                    ccCharMaterials[ccCharMaterials.Length - 1].sVarients = new string[1];  // create 1st element
                                                                }
                                                                else
                                                                {  // we have some already
                                                                    Array.Resize<string>(ref ccCharMaterials[ccCharMaterials.Length - 1].sVarients, ccCharMaterials[ccCharMaterials.Length - 1].sVarients.Length + 1);  // extend the array
                                                                }
                                                                ccCharMaterials[ccCharMaterials.Length - 1].sVarients[ccCharMaterials[ccCharMaterials.Length - 1].sVarients.Length - 1] = fiBasePath[ii].Name.Replace(".mat", "");  // store the variant name
                                                                if (fiBasePath[ii].Name.Replace(".mat", "") == targetRenderer.sharedMaterials[m].name.Replace(" (Instance)", ""))
                                                                {  // found renderer material amongst the variants
                                                                    ccCharComponents[ccCharComponents.Length - 1].iMaterialVariant[m] = ccCharMaterials[ccCharMaterials.Length - 1].sVarients.Length - 1;  // store the material variant id
                                                                    ccCharMaterials[ccCharMaterials.Length - 1].iMasterVariant = ccCharMaterials[ccCharMaterials.Length - 1].sVarients.Length - 1;  // set the master variant to match
                                                                }
                                                            }
                                                            break;  // work complete
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    { // find the LOD1 gameobject
                                        bool bFound = false;
                                        string sSearchName = tChild.name.ToUpper();  // name to search for
                                        if (sSearchName.Contains("_LOD"))
                                        { // bone LOD component
                                            sSearchName = sSearchName.Substring(0, sSearchName.IndexOf("_LOD"));  // trim of the end text _LOD2+
                                        }
                                        for (int i = 0; i < ccCharComponents.Length; i++)
                                        {  // check all components found so far
                                            if (ccCharComponents[i].sFriendlyName == sSearchName && ccCharComponents[i].egSex == egSex)
                                            {  // matching?
                                                if (ccCharComponents[i].goLOD2Plus == null)
                                                {  // 1st matching
                                                    ccCharComponents[i].goLOD2Plus = new GameObject[1];  // create the 1st element
                                                }
                                                else
                                                {  // already found some
                                                    Array.Resize<GameObject>(ref ccCharComponents[i].goLOD2Plus, ccCharComponents[i].goLOD2Plus.Length + 1);  // extend the array by 1
                                                }
                                                ccCharComponents[i].goLOD2Plus[ccCharComponents[i].goLOD2Plus.Length - 1] = tChild.gameObject;  // add to the array for LOD1                                            
                                                tChild.gameObject.SetActive(ccCharComponents[i].bEquiped);  // ensure that LOD2+ visibility matches LOD1
                                                bFound = true;  // flag so no error
                                                break;  // no need to continue
                                            }
                                        }
                                        if (!bFound)
                                        {  // unable to find LOD1, warn user
                                            Debug.Log("Unable to find LOD1 for " + sSearchName + "!");
                                        }
                                    }
                                }
                            }

                            // analyse complete move to equip
                            if (ccCharComponents != null)
                            {
                                sLastObjectAnalysed = goActiveCharacter.name;  // move to equip mode
                            }
                            else
                            {  // no equipment found
                                GUILayout.BeginHorizontal();
                                GUILayout.Space(INDENT_TABS);
                                GUILayout.Label("Mesh/Skinned Renderers NOT found as children of this object!", GUILayout.ExpandWidth(true));
                                GUILayout.EndHorizontal();
                            }
                        }
                        GUILayout.EndHorizontal();
                    }
                    else
                    { // already analysed, show equip character options
                        if (ccCharMaterials == null)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(INDENT_TABS);
                            GUILayout.Label("No Materials Found!", GUILayout.ExpandWidth(true));
                            GUILayout.EndHorizontal();
                        }
                        else
                        {
                            if (iCurrentGender != 2)
                            {  // male/female character
                               // show gender choice
                                GUILayout.BeginHorizontal();
                                GUILayout.Space(INDENT_TABS);
                                GUILayout.Label("Gender", GUILayout.Width(80));
                                iNewValue = EditorGUILayout.Popup(iCurrentGender, sGenderList, GUILayout.ExpandWidth(true));
                                GUILayout.EndHorizontal();
                                if (iNewValue != iCurrentGender)
                                {  // swap the gender visibility
                                    iCurrentGender = iNewValue;  // update the selected gender
                                    if (iCurrentGender == 0)
                                    {  // swap to male
                                        goActiveCharacter.transform.Find("Male").gameObject.SetActive(true);
                                        goActiveCharacter.transform.Find("Female").gameObject.SetActive(false);
                                    }
                                    else
                                    { // swap to female
                                        goActiveCharacter.transform.Find("Female").gameObject.SetActive(true);
                                        goActiveCharacter.transform.Find("Male").gameObject.SetActive(false);
                                    }
                                    for (int i = 0; i < ccCharComponents.Length; i++)
                                    {  // match the component visibility on the opposite sex                                
                                        for (int j = 0; j < ccCharComponents.Length; j++)
                                        {  // search for opposite gender version of the component
                                            if (i != j && (int)ccCharComponents[j].egSex == iCurrentGender &&
                                                    ccCharComponents[j].sFriendlyName == ccCharComponents[i].sFriendlyName &&
                                                    (ccCharComponents[j].egSex != ccCharComponents[i].egSex ||
                                                        (ccCharComponents[j].egSex == eGender.Both && ccCharComponents[i].egSex == eGender.Both)
                                                    )
                                            )
                                            {  // match on friendly name and opposite sex
                                                ccCharComponents[j].bEquiped = ccCharComponents[i].bEquiped;  // assign visibility
                                                ccCharComponents[j].goRef.SetActive(ccCharComponents[j].bEquiped);  // alter LOD1 visibility
                                                if (ccCharComponents[j].goLOD2Plus != null)
                                                {  // do we have LOD?
                                                    foreach (GameObject goLODChild in ccCharComponents[j].goLOD2Plus)
                                                    {  // process all
                                                        goLODChild.SetActive(ccCharComponents[j].bEquiped);  // alter visibility for LOD2+
                                                    }
                                                }
                                                break; // work complete
                                            }
                                        }
                                    }
                                }
                            }

                            // list global materials
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(INDENT_TABS);
                            GUILayout.BeginVertical(EditorStyles.helpBox);
                            for (int i = 0; i < ccCharMaterials.Length; i++)
                            {
                                if (ccCharMaterials[i].sVarients.Length > 0)
                                {  // ignore materials with only 1 variant
                                   // material name
                                    GUILayout.BeginHorizontal();
                                    GUILayout.Label(ccCharMaterials[i].sFriendlyName, GUILayout.Width(100));

                                    // master variant dropdown
                                    iNewValue = EditorGUILayout.Popup(ccCharMaterials[i].iMasterVariant, ccCharMaterials[i].sVarients, GUILayout.ExpandWidth(true));
                                    if (iNewValue != ccCharMaterials[i].iMasterVariant)
                                    { // new material selected
                                        ccCharMaterials[i].iMasterVariant = iNewValue;  // store master mat id
                                        for (int j = 0; j < ccCharComponents.Length; j++)
                                        {  // check all char components 
                                            for (int m = 0; m < ccCharComponents[j].iMaterial.Length; m++)
                                            { // check all materials on the component
                                                if (ccCharComponents[j].iMaterial[m] == i)
                                                {  // we have a match
                                                    ccCharComponents[j].iMaterialVariant[m] = iNewValue;  // update the selected variant
                                                    Material[] mats = ccCharComponents[j].goRef.GetComponent<Renderer>().sharedMaterials;  // grab the material array
                                                                                                                                           //mats[m] = Resources.Load(ccCharMaterials[ccCharComponents[j].iMaterial[m]].sBaseFolder + "/" + ccCharMaterials[ccCharComponents[j].iMaterial[m]].sVarients[ccCharComponents[j].iMaterialVariant[m]], typeof(Material)) as Material;   // load the new material variant                                                
                                                    mats[m] = AssetDatabase.LoadAssetAtPath("Assets/" + ccCharMaterials[ccCharComponents[j].iMaterial[m]].sBaseFolder + "/" + ccCharMaterials[ccCharComponents[j].iMaterial[m]].sVarients[ccCharComponents[j].iMaterialVariant[m]] + ".mat", typeof(Material)) as Material;  // load the new material variant                                                
                                                    ccCharComponents[j].goRef.GetComponent<Renderer>().sharedMaterials = mats;  // write the material array back
                                                    if (ccCharComponents[j].goLOD2Plus != null)
                                                    {  // LOD2+ found
                                                        for (int l = 0; l < ccCharComponents[j].goLOD2Plus.Length; l++)
                                                        {  // loop through LOD levels
                                                            ccCharComponents[j].goLOD2Plus[l].GetComponent<Renderer>().sharedMaterials = mats;  // write the material array back to each LOD
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    GUILayout.EndHorizontal();
                                }
                            }
                            GUILayout.EndVertical();
                            GUILayout.EndHorizontal();

                            // list equipment
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(INDENT_TABS);
                            GUILayout.BeginVertical(EditorStyles.helpBox);
                            for (int i = 0; i < ccCharComponents.Length; i++)
                            {
                                if (!ccCharComponents[i].bDeleted)
                                { // component not removed from the character
                                    if (iCurrentGender == 2 || ccCharComponents[i].egSex == eGender.Both || iCurrentGender == (int)ccCharComponents[i].egSex)
                                    {  // only list objects for the current gender
                                       // equipment name/toggle                            
                                        GUILayout.BeginHorizontal();
                                        bNewValue = GUILayout.Toggle(ccCharComponents[i].bEquiped, ccCharComponents[i].sFriendlyName);
                                        if (bNewValue != ccCharComponents[i].bEquiped)
                                        {  // checkbox ticked
                                            ccCharComponents[i].bEquiped = bNewValue;  // assign new value
                                            ccCharComponents[i].goRef.SetActive(ccCharComponents[i].bEquiped);  // alter the visibility on the model for LOD1 (or non LOD)
                                            if (ccCharComponents[i].goRef.name == ccCharComponents[i].goRef.transform.parent.gameObject.name)
                                            {  // some SFB models have the equipment inside an empty parent gameobject of the same name
                                                ccCharComponents[i].goRef.transform.parent.gameObject.SetActive(ccCharComponents[i].bEquiped);  // alter the parent visibility to match
                                            }
                                            if (ccCharComponents[i].goLOD2Plus != null)
                                            {  // do we have LOD?
                                                foreach (GameObject goLODChild in ccCharComponents[i].goLOD2Plus)
                                                {  // process all
                                                    goLODChild.SetActive(ccCharComponents[i].bEquiped);  // alter visibility for LOD2+
                                                }
                                            }
                                            if (ccCharComponents[i].egSex != eGender.Both)
                                            {  // is it a gender based component?
                                                for (int j = 0; j < ccCharComponents.Length; j++)
                                                {  // search for opposite gender version of the component
                                                    if (ccCharComponents[j].sFriendlyName == ccCharComponents[i].sFriendlyName && ccCharComponents[j].egSex != ccCharComponents[i].egSex)
                                                    {  // match on friendly name and opposite sex
                                                        ccCharComponents[j].bEquiped = ccCharComponents[i].bEquiped;  // assign visibility
                                                        ccCharComponents[j].goRef.SetActive(ccCharComponents[j].bEquiped);  // alter LOD1 visibility
                                                        if (ccCharComponents[j].goLOD2Plus != null)
                                                        {  // do we have LOD?
                                                            foreach (GameObject goLODChild in ccCharComponents[j].goLOD2Plus)
                                                            {  // process all
                                                                goLODChild.SetActive(ccCharComponents[j].bEquiped);  // alter visibility for LOD2+
                                                            }
                                                        }
                                                        break; // work complete
                                                    }
                                                }
                                            }
                                        }

                                        // materials dropdown
                                        for (int j = 0; j < ccCharComponents[i].iMaterial.Length; j++)
                                        {
                                            iNewValue = EditorGUILayout.Popup(ccCharComponents[i].iMaterialVariant[j], ccCharMaterials[ccCharComponents[i].iMaterial[j]].sVarients, GUILayout.ExpandWidth(true));  // show dropdown for the material variants
                                            if (iNewValue != ccCharComponents[i].iMaterialVariant[j])
                                            { // new material selected
                                                ccCharComponents[i].iMaterialVariant[j] = iNewValue;  // update the selected variant
                                                Material[] mats = ccCharComponents[i].goRef.GetComponent<Renderer>().sharedMaterials;  // grab the material array
                                                                                                                                       //mats[j] = Resources.Load(ccCharMaterials[ccCharComponents[i].iMaterial[j]].sBaseFolder + "/" + ccCharMaterials[ccCharComponents[i].iMaterial[j]].sVarients[ccCharComponents[i].iMaterialVariant[j]], typeof(Material)) as Material;   // load the new material variant
                                                mats[j] = AssetDatabase.LoadAssetAtPath("Assets/" + ccCharMaterials[ccCharComponents[i].iMaterial[j]].sBaseFolder + "/" + ccCharMaterials[ccCharComponents[i].iMaterial[j]].sVarients[ccCharComponents[i].iMaterialVariant[j]] + ".mat", typeof(Material)) as Material;  // load the new material variant                                                
                                                ccCharComponents[i].goRef.GetComponent<Renderer>().sharedMaterials = mats;  // write the material array back
                                                if (ccCharComponents[i].goLOD2Plus != null)
                                                {  // LOD2+ found
                                                    for (int l = 0; l < ccCharComponents[i].goLOD2Plus.Length; l++)
                                                    {  // loop through LOD levels
                                                        ccCharComponents[i].goLOD2Plus[l].GetComponent<Renderer>().sharedMaterials = mats;  // write the material array back to each LOD
                                                    }
                                                }
                                            }
                                        }
                                        GUILayout.EndHorizontal();
                                    }
                                }
                            }
                            GUILayout.EndVertical();
                            GUILayout.EndHorizontal();


                            // remove unused gender nodes 
                            GUILayout.BeginHorizontal();
                            if (iCurrentGender != 2)
                            { // if set to both then genderless character                        
                                GUILayout.Space(INDENT_TABS);
                                if (GUILayout.Button("Delete Unused Gender", GUILayout.ExpandWidth(true)))
                                {
                                    GameObject goGenderKept; // remaining gender parent
                                    int iGenderToDelete;  // remove components flag
                                    if (iCurrentGender == 0)
                                    {
                                        DestroyImmediate(goActiveCharacter.transform.Find("Female").gameObject);  // delete unused gender
                                        goGenderKept = goActiveCharacter.transform.Find("Male").gameObject;  // grab gender remaining
                                        iGenderToDelete = 1;  // remove female
                                    }
                                    else
                                    {
                                        DestroyImmediate(goActiveCharacter.transform.Find("Male").gameObject);  // delete unused gender
                                        goGenderKept = goActiveCharacter.transform.Find("Female").gameObject;  // grab gender remaining
                                        iGenderToDelete = 0;  // remove male
                                    }
                                    iTotalFound = 0;  // init the delete count
                                    while (goGenderKept.transform.childCount > 0)
                                    { // reloop through until none left
                                        foreach (Transform tChild in goGenderKept.transform)
                                        {  // process remaining child children
                                            tChild.parent = goActiveCharacter.transform;  // assign to root
                                            iTotalFound += 1;
                                        }
                                    }
                                    for (int i = 0; i < ccCharComponents.Length; i++)
                                    {  // check all components
                                        if ((int)ccCharComponents[i].egSex == iGenderToDelete)
                                        {    // found component for gender being delete
                                            ccCharComponents[i].bDeleted = true;  // flag as deleted, hiding it from the user
                                        }
                                    }
                                    DestroyImmediate(goGenderKept);  // delete empty gender object
                                    vSortGameObjectChildren(goActiveCharacter);  // ensure master game object still in order
                                    iCurrentGender = 2;  // set to gender both as now a genderless character
                                    Debug.Log("Unused gender delete, " + iTotalFound.ToString() + " children moved to root!");  // all done
                                }
                            }

                            // remove unused components
                            if (GUILayout.Button("Delete Unused Components", GUILayout.ExpandWidth(true)))
                            {
                                for (int i = 0; i < ccCharComponents.Length; i++)
                                {
                                    if (ccCharComponents[i].bEquiped == false)
                                    {  // unused equipment?
                                        if (ccCharComponents[i].goRef.name == ccCharComponents[i].goRef.transform.parent.name)
                                        {  // same named parent?
                                            DestroyImmediate(ccCharComponents[i].goRef.transform.parent.gameObject);  // delete the parent
                                        }
                                        else
                                        {  // normal
                                            DestroyImmediate(ccCharComponents[i].goRef);  // delete 
                                        }
                                        if (ccCharComponents[i].goLOD2Plus != null)
                                        {  // LOD found?
                                            for (int j = 0; j < ccCharComponents[i].goLOD2Plus.Length; j++)
                                            { // process all LOD variants
                                                if (ccCharComponents[i].goLOD2Plus[j].name == ccCharComponents[i].goLOD2Plus[j].transform.parent.name)
                                                {  // same named parent?
                                                    DestroyImmediate(ccCharComponents[i].goLOD2Plus[j].transform.parent.gameObject);  // delete the parent
                                                }
                                                else
                                                {  // normal
                                                    DestroyImmediate(ccCharComponents[i].goLOD2Plus[j]);  // delete 
                                                }
                                            }
                                        }
                                        ccCharComponents[i].bDeleted = true;  // remove from the interface
                                    }
                                }
                            }
                            GUILayout.EndHorizontal();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                vLogError(ref ex, "");
            }
        }
        #endregion

        #region "Service Functions"
        /// <summary>
        /// Sort children on a game object.
        /// </summary>
        /// <param name="SortMe">Game object to sort the children of.</param>
        private void vSortGameObjectChildren(GameObject SortMe)
        {
            try
            {
                List<Transform> children = new List<Transform>();
                for (int i = SortMe.transform.childCount - 1; i >= 0; i--)
                {
                    Transform child = SortMe.transform.GetChild(i);
                    children.Add(child);
                    child.parent = null;
                }
                children.Sort((Transform t1, Transform t2) => { return t1.name.CompareTo(t2.name); });
                foreach (Transform child in children)
                {
                    child.parent = SortMe.transform;
                }
            }
            catch (Exception ex)
            {
                vLogError(ref ex, "");
            }
        }

        /// <summary>
        /// Rename LOD levels ensuring the numbering of LOD level parent game objects are contiguous.
        /// </summary>
        /// <param name="TopLevel">First game object in the LOD, usually LOD1.</param>
        /// <param name="LODStart">Initial known levels.</param>
        /// <returns>Total LOD levels found.</returns>
        int iEnsureLODNumbersAreContiguous(Transform TopLevel, int LODStart)
        {
            int iTotalLOD = LODStart;  // init counter
            bFound = true;  // loop init condition
            while (bFound)
            {  // keep going until no LOD levels that have not been renamed are found
                bFound = false;  // set the loop breakout condition
                vSortGameObjectChildren(TopLevel.gameObject);  // sort the children                                
                foreach (Transform tChild in TopLevel)
                {  // loop  through children
                    if (tChild.name.ToUpper().Contains("_LOD") || tChild.name.ToUpper().Contains(" LOD"))
                    {  // found next LOD not renamed
                        iTotalLOD += 1;  // up the LOD count
                        tChild.name = "LOD" + iTotalLOD.ToString();  // rename the child
                        bFound = true;  // unset loop breakout
                        break;  // drop out of the loop
                    }
                }

            }
            return iTotalLOD;  // send back the LOD count
        }

        /// <summary>
        /// Reassign the sub child bones to the LOD1 bones.      
        /// </summary>
        /// <param name="Child"></param>
        /// <param name="boneMap"></param>
        /// <param name="levelRenders"></param>
        /// <param name="bRootLOD"></param>
        void vProcessLODLevel(Transform Child, ref Dictionary<String, Transform> boneMap, ref LODLevel[] levelRenders, bool bRootLOD)
        {
            SkinnedMeshRenderer targetRenderer;
            MeshRenderer targetMeshRenderer;

            int iCurrentLODLevel = iToPureNumeric(Child.name);  // store the current LOD level

            // reassign the sub child bones to the LOD1 bones                                
            Transform tLOD1Bone;
            GameObject goSubBoneRoot = null;
            String sSubRootboneName = "";
            foreach (Transform tSubChild in Child.transform)
            {  // loop  through  the sub children
                if (tSubChild.name.ToUpper() == sRootBoneName.ToUpper())
                {  // sub child bones
                    goSubBoneRoot = tSubChild.gameObject;  // store for later deletion

                    // check for objects attached directly to the bones
                    Transform[] tAllBonesAndAttachments = tSubChild.GetComponentsInChildren<Transform>();  // list all bone transforms
                    foreach (Transform tMaybeBone in tAllBonesAndAttachments)
                    {  // loop though all bones and possible attached objects
                        targetMeshRenderer = tMaybeBone.GetComponent<MeshRenderer>();  // attempt grab mesh render
                        if (targetMeshRenderer)
                        {  // found attached object
                           // rename it
                            tMaybeBone.name = tMaybeBone.name + "_LOD" + iCurrentLODLevel.ToString();

                            // build path to parent
                            Transform tParent = tMaybeBone.parent;
                            string sParentPath = tParent.name;
                            while (tParent.name != tRootBone.name)
                            {
                                tParent = tParent.parent;
                                sParentPath = tParent.name + "/" + sParentPath;
                            }

                            // attach to the LOD1 bone
                            tLOD1Bone = goActiveCharacter.transform.Find(sParentPath);  // check path
                            if (tLOD1Bone)
                            {  // valid
                                tMaybeBone.parent = tLOD1Bone;  // assign to LOD1 root bone child

                                // add the renderer to the LOD level renderer array
                                if (levelRenders[iCurrentLODLevel - 1].renderers == null)
                                {  // 1st time
                                    levelRenders[iCurrentLODLevel - 1].renderers = new Renderer[1];  // create 1st element
                                }
                                else
                                {  // already found others
                                    Array.Resize<Renderer>(ref levelRenders[iCurrentLODLevel - 1].renderers, levelRenders[iCurrentLODLevel - 1].renderers.Length + 1);  // extend array by 1
                                }
                                levelRenders[iCurrentLODLevel - 1].renderers[levelRenders[iCurrentLODLevel - 1].renderers.Length - 1] = targetMeshRenderer;  // add the renderer to the LOD array
                            }
                            else
                            {
                                Debug.Log("Bone " + tMaybeBone.parent.name + " for " + tMaybeBone.name +
                                    (tLOD1Bone ? " assigned to LOD1 (" : " NOT found on LOD1 (") + sParentPath + ")!");
                            }
                        }
                    }
                }
                else
                {
                    if (tSubChild.GetComponent<SkinnedMeshRenderer>())
                    { // skinned renderer present
                        // process the Male LOD1 bones when gender neutral (as these are the root)
                        targetRenderer = tSubChild.GetComponent<SkinnedMeshRenderer>();  // grab the render
                        if (bRootLOD)
                        {  // root scan enabled?
                            foreach (Transform bone in targetRenderer.bones)
                            {  // process all bones 
                                boneMap[bone.name] = bone;  // Add each bone to the dictionary
                            }
                        }

                        // reassign the bone transforms
                        sSubRootboneName = targetRenderer.rootBone.name;  // store the name of the root
                        Transform[] boneArray = targetRenderer.bones;  // set the boneArray to be all the bones from subChildRenderer
                        for (int i = 0; i < boneArray.Length; i++)
                        { // cycle through boneArray, assigning the bones in the array
                            if (boneMap.ContainsKey(boneArray[i].name))
                            { // If the dictionary for target bones contains this bone name
                                boneArray[i] = boneMap[boneArray[i].name];  // set the array to match the bone from the Dictionary
                            }
                        }
                        targetRenderer.bones = boneArray;  // take effect
                        foreach (Transform bone in targetRenderer.bones)
                        {  // search for the root bone
                            if (bone.name == sSubRootboneName)
                            {  // found it
                                targetRenderer.rootBone = bone;  // assign it
                            }
                        }

                        // add the renderer to the LOD level renderer array
                        if (levelRenders[iCurrentLODLevel - 1].renderers == null)
                        {  // 1st time
                            levelRenders[iCurrentLODLevel - 1].renderers = new Renderer[1];  // create 1st element
                        }
                        else
                        {  // already found others
                            Array.Resize<Renderer>(ref levelRenders[iCurrentLODLevel - 1].renderers, levelRenders[iCurrentLODLevel - 1].renderers.Length + 1);  // extend array by 1
                        }
                        levelRenders[iCurrentLODLevel - 1].renderers[levelRenders[iCurrentLODLevel - 1].renderers.Length - 1] = targetRenderer;  // add the renderer to the LOD array
                    }
                    else if (tSubChild.GetComponent<MeshRenderer>())
                    { // mesh renderer present
                        targetMeshRenderer = tSubChild.GetComponent<MeshRenderer>();  // grab the render

                        // add the renderer to the LOD level renderer array
                        if (levelRenders[iCurrentLODLevel - 1].renderers == null)
                        {  // 1st time
                            levelRenders[iCurrentLODLevel - 1].renderers = new Renderer[1];  // create 1st element
                        }
                        else
                        {  // already found others
                            Array.Resize<Renderer>(ref levelRenders[iCurrentLODLevel - 1].renderers, levelRenders[iCurrentLODLevel - 1].renderers.Length + 1);  // extend array by 1
                        }
                        levelRenders[iCurrentLODLevel - 1].renderers[levelRenders[iCurrentLODLevel - 1].renderers.Length - 1] = targetMeshRenderer;  // add the renderer to the LOD array
                    }
                }
            }
            DestroyImmediate(goSubBoneRoot, true);  // Destroy the subChildRenderers bones
        }

        /// <summary>
        /// returns pure alpha numeric string.
        /// </summary>
        /// <param name="self">Input string to strip to alpha numeric.</param>
        /// <returns>Pure numeric version of the input string.</returns>
        private string sToPureAlphaNum(string self)
        {
            return new string(Array.FindAll(self.ToCharArray(), c => char.IsLetterOrDigit(c)));
        }

        /// <summary>
        /// returns pure alpha string.
        /// </summary>
        /// <param name="self">Input string to strip to alpha only.</param>
        /// <returns>Pure alpha version of the input string.</returns>
        private string sToPureAlpha(string self)
        {
            return new string(Array.FindAll(self.ToCharArray(), c => char.IsLetter(c)));
        }

        /// <summary>
        /// returns pure alpha numeric string as an integer.
        /// </summary>
        /// <param name="self">Input string to strip to alpha numeric.</param>
        /// <returns>Pure numeric version of the input string.</returns>
        private int iToPureNumeric(string self)
        {
            return Convert.ToInt32(new string(Array.FindAll(self.ToCharArray(), c => char.IsDigit(c))));
        } 

        /// <summary>
        /// Error logging with a stack trace.
        /// </summary>
        /// <param name="ex">Exception from the try catch block.</param>
        /// <param name="Args">Optional function arguments to store with the error.</param>
        private void vLogError(ref Exception ex, string Args)
        {
            try
            {
                // trace the originating function
                System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
                System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(1);
                System.Reflection.MethodBase methodBase = stackFrame.GetMethod();

                // log the error to the unity log
                Debug.Log(methodBase.Name + "(" + Args + ") - " + ex.Message);
            }
            catch
            {  // ensure an error is output if the originating functions cannot be traced
                Debug.Log(ex.Message);
            }
        }
        #endregion

        //#region "Limb Chain Handling"
        ///// <summary>
        ///// update the GUI list with the bone names.
        ///// </summary>
        //public void updateRootBoneNameList()
        //{
        //    sRootBones = new string[tRootBones.Count];
        //    for (int i = 0; i < tRootBones.Count; i++)
        //    {
        //        if (tRootBones[i])
        //        {   // valid
        //            sRootBones[i] = tRootBones[i].name;  // append
        //        }
        //        else
        //        {  // not yet selected
        //            sRootBones[i] = "null" + i.ToString();  // show unset root bone
        //        }
        //    }
        //}

        ///// <summary>
        ///// Find all limb chain starts from the current bone.
        ///// </summary>
        ///// <param name="CheckMe">Start point of the limb chain search.</param>
        //void addLimbChainStarts(Transform CheckMe)
        //{
        //    for (int i = 0; i < CheckMe.childCount; i++)
        //    {  // process all children
        //        if (sAllBones.Contains(CheckMe.GetChild(i).name))
        //        {  // valid bone
        //            if (iCountBoneChilds(CheckMe.GetChild(i)) > 0)
        //            {  // has valid bone children                        
        //                lcLimbChainList.Add(new LimbChain() { ChainStart = CheckMe.GetChild(i), ChainEnd = null, Enable = false, Links = 0 });  // add valid limb chain with start bone
        //            }
        //        }
        //    }
        //}

        ///// <summary>
        ///// Reprocess the limb chain, assigning the start/end for the gui and link roots.
        ///// </summary>
        ///// <param name="lc">Limb chain to process.</param>
        //void checkLimbChainEnds(LimbChain lc)
        //{
        //    lc.Links = 0;
        //    lc.ChainStart = null;
        //    lc.ChainEnd = null;
        //    for (int b = 0; b < lc.Bones.Count; b++)
        //    {  // check the bone chain
        //        if (lAllBones[lc.Bones[b]].Type != eColliderType.DontInclude)
        //        {  // found included bone
        //            lc.Links += 1;  // link is included
        //            if (!lc.ChainStart)
        //            { // found the new chain start
        //                lc.ChainStart = lAllBones[lc.Bones[b]].Bone;  // store
        //            }
        //            else
        //            {  // update the chain end
        //                lc.ChainEnd = lAllBones[lc.Bones[b]].Bone;  // store
        //            }
        //        }
        //    }
        //    if (!lc.ChainEnd)
        //    { // no end link
        //        if (lc.ChainStart)
        //        {  // have a start, so only 1 link in the chain
        //            lc.ChainEnd = lc.ChainStart;
        //        }
        //        else
        //        {
        //            lc.ChainEnd = lAllBones[lc.Bones[lc.Bones.Count - 1]].Bone;  // and end
        //        }
        //    }
        //    if (!lc.ChainStart)
        //    { // all links set to dont include
        //        lc.ChainStart = lAllBones[lc.Bones[0]].Bone;  // return to default start                
        //    }
        //    rebuildLinkChainList();  // ensure all chain ends are in the link bone list after change
        //}

        ///// <summary>
        ///// Regenerates the available limb chains ends into the link chain list.
        ///// </summary>
        //void rebuildLinkChainList()
        //{
        //    tLinkBones = new List<Transform>();  // clear the link bone transform list
        //    for (int i = 0; i < lcLimbChainList.Count; i++)
        //    {  // check all limb chains
        //        if (lcLimbChainList[i].ChainEnd)
        //        { // don't check nulls failsafe 
        //            if (!tLinkBones.Contains(lcLimbChainList[i].ChainEnd))
        //            {  // found new 
        //                tLinkBones.Add(lcLimbChainList[i].ChainEnd);  // so add the transform
        //            }
        //        }
        //    }
        //    sLinkBones = null;  // clear the GUI popup list
        //    if (tLinkBones.Count > 0)
        //    {
        //        sLinkBones = new string[tLinkBones.Count + 1];  // initialise
        //        sLinkBones[0] = "Root Bone";
        //        for (int i = 0; i < tLinkBones.Count; i++)
        //        {  // process all link bones
        //            sLinkBones[i + 1] = tLinkBones[i].name;  // add the name
        //        }
        //    }
        //}

        ///// <summary>
        ///// Does this bone have child bones.
        ///// </summary>
        ///// <param name="tBone"></param>
        ///// <returns></returns>
        //int iCountBoneChilds(Transform Bone)
        //{
        //    int iBoneChildren = 0;  // init bone counter
        //    for (int j = 0; j < Bone.childCount; j++)
        //    {  // process all children of the bone
        //        if (sAllBones.Contains(Bone.GetChild(j).name))
        //        {  // valid bone child found?
        //            iBoneChildren += 1;  // found a child bone
        //        }
        //    }
        //    return iBoneChildren;  // work complete
        //}

        ///// <summary>
        ///// Clear flags & arrays for the limb chain build.
        ///// </summary>
        //void reset()
        //{
        //    tRootBone = null;  // clear the previous root
        //    tRootBones = new List<Transform>();  // clear the root bone transform list                        
        //    tLinkBones = new List<Transform>();  // clear the link bone transform list
        //    lcLimbChainList = new List<LimbChain>();  // clear the previous limb chains
        //    sAllBones = new List<string>();  // clear previous bone list
        //    lAllBones = new List<BoneOptions>();  // and the matching bone options list
        //}

        ///// <summary>
        ///// Loop backwards through the bone chain and find the parent bone that isn't skipped.
        ///// </summary>
        ///// <param name="BoneID">ID of the start bone.</param>
        ///// <returns>Transform of the parent bone that doesn't have the skip flag set.</returns>
        //public Transform findValidBoneLinkParent(int BoneID)
        //{
        //    // find which limb chain
        //    foreach (LimbChain lc in lcLimbChainList)
        //    {  // check all limb chains
        //        if (lc.AddedAlready || lc.Enable)
        //        {  // only check limb chains included in the build
        //            int iBoneChainIndexID = lc.Bones.IndexOf(BoneID);  // search the limb chain links for the boneID
        //            if (iBoneChainIndexID >= 0)
        //            { // found the limb chain
        //                for (int i = iBoneChainIndexID - 1; i >= 0; i--)
        //                {  // process limb chain links backwards
        //                    if (lAllBones[lc.Bones[i]].Type != eColliderType.DontInclude)
        //                    {   // until find an included bone chain link
        //                        return lAllBones[lc.Bones[i]].Bone;  // found valid included parent
        //                    }
        //                }

        //                // not found, return root or end of another link chain as parent
        //                if (lc.Linkbone == 0)
        //                {   // link chain parent not enabled
        //                    return tRootBones[lc.Rootbone];  // return the root bone as no valid link parent was found
        //                }
        //                else
        //                {  // linked to another chain
        //                    return tLinkBones[lc.Linkbone - 1];  // return the root bone as no valid link parent was found
        //                }
        //            }
        //        }
        //    }
        //    return null; // this should never happen
        //}

        ///// <summary>
        ///// loop backwards through the bone chain and find the parent bone that isn't skipped
        ///// </summary>
        ///// <param name="BoneID">ID of the start bone.</param>
        ///// <returns>Transform of the child bone that doesn't have the skip flag set.</returns>
        //public Transform findValidBoneLinkChild(int BoneID)
        //{
        //    // find which limb chain
        //    foreach (LimbChain lc in lcLimbChainList)
        //    {  // check all limb chains
        //        if (lc.AddedAlready)
        //        {  // only check limb chains included in the build
        //            int iBoneChainIndexID = lc.Bones.IndexOf(BoneID);  // search the limb chain links for the boneID
        //            if (iBoneChainIndexID >= 0)
        //            { // found the limb chain
        //                for (int i = iBoneChainIndexID + 1; i < lc.Bones.Count; i++)
        //                {  // process limb chain links forwards
        //                    if (lAllBones[lc.Bones[i]].Type != eColliderType.DontInclude)
        //                    {   // until find an included bone chain link
        //                        return lAllBones[lc.Bones[i]].Bone;  // found valid included parent
        //                    }
        //                }
        //                return null;  // return the nothing for end of the chain
        //            }
        //        }
        //    }
        //    return null; // this should never happen
        //}

        //#endregion

        //#region "Character Joint Handling"
        ///// <summary>
        ///// Setup the bone rigid body, collider and joint components.
        ///// </summary>
        ///// <param name="Bone">Bone transform to add the components to.</param>
        ///// <param name="Parent">Parent bone to link the rigid body connected body to.</param>
        //void AddBoneComponents(Transform Bone, Transform Parent)
        //{
        //    int iBoneID = sAllBones.IndexOf(Bone.name);  // find in the bone array
        //    lAllBones[iBoneID].Shown = true;  // enable for the GUI
        //    if (lAllBones[iBoneID].Type == eColliderType.DontInclude)  // bone was previously set to not shown
        //    {
        //        lAllBones[iBoneID].Type = eColliderType.Capsule;  // default to capsule
        //    }
        //    Rigidbody rb = Bone.GetComponent<Rigidbody>();  // attempt to grab existing rigid body
        //    if (!rb) // not found?
        //    {
        //        rb = Bone.gameObject.AddComponent<Rigidbody>();  // add the rigid body
        //        rb.mass = fBoneMass;  // apply even mass across body
        //        rb.useGravity = true;  // gravity is needed
        //    }
        //    AddBoneCollider(iBoneID); // the collider
        //    CharacterJoint cj = Bone.gameObject.AddComponent<CharacterJoint>(); // and the joint components
        //    Rigidbody rbP = Parent.GetComponent<Rigidbody>();
        //    if (!rbP)  // no rigid body on the parent
        //    {
        //        rbP = Parent.gameObject.AddComponent<Rigidbody>();  // add the rigid body
        //        rbP.mass = fBoneMass;  // apply even mass across body
        //        rbP.useGravity = true;  // gravity is needed
        //    }
        //    cj.connectedBody = Parent.GetComponent<Rigidbody>();  // connect to the parent rigid body
        //    cj.enableProjection = true;  // Brings violated constraints back into alignment even when the solver fails.
        //    Bone.gameObject.AddComponent<vCollisionMessage>();  // add the invector collision message
        //}

        ///// <summary>
        ///// Setup the bone collider.
        ///// </summary>
        ///// <param name="BoneID">ID of the bone to add the specified collider to.</param>
        //void AddBoneCollider(int BoneID)
        //{
        //    // setup the collider direction, length and radius
        //    int direction;
        //    float distance;
        //    if (iCountBoneChilds(lAllBones[BoneID].Bone) == 1)
        //    {  // middle of chain
        //        Transform childBone = lAllBones[BoneID].Bone.GetChild(0);
        //        for (int i = 0; i < lAllBones[BoneID].Bone.childCount; i++)
        //        {
        //            if (sAllBones.Contains(lAllBones[BoneID].Bone.GetChild(i).name))
        //            {
        //                childBone = lAllBones[BoneID].Bone.GetChild(i);
        //                break;
        //            }
        //        }
        //        Vector3 endPoint = childBone.position;
        //        CalculateDirection(lAllBones[BoneID].Bone.InverseTransformPoint(endPoint), out direction, out distance);
        //    }
        //    else
        //    {  // end point
        //        Vector3 endPoint = (lAllBones[BoneID].Bone.position - lAllBones[BoneID].Bone.parent.position) + lAllBones[BoneID].Bone.position;
        //        CalculateDirection(lAllBones[BoneID].Bone.InverseTransformPoint(endPoint), out direction, out distance);

        //        if (lAllBones[BoneID].Bone.GetComponentsInChildren(typeof(Transform)).Length > 1)
        //        {
        //            Bounds bounds = new Bounds();
        //            foreach (Transform child in lAllBones[BoneID].Bone.GetComponentsInChildren(typeof(Transform)))
        //            {
        //                bounds.Encapsulate(lAllBones[BoneID].Bone.InverseTransformPoint(child.position));
        //            }

        //            if (distance > 0)
        //                distance = bounds.max[direction];
        //            else
        //                distance = bounds.min[direction];
        //        }
        //    }
        //    Vector3 center = Vector3.zero;
        //    center[direction] = distance * 0.5f;
        //    if (lAllBones[BoneID].Radius == 0)
        //    {
        //        lAllBones[BoneID].Radius = Mathf.Abs((lAllBones[BoneID].Type == eColliderType.Sphere ? distance * 0.5f : distance * 0.25f));
        //    }

        //    // add the collider
        //    switch (lAllBones[BoneID].Type)
        //    {
        //        case eColliderType.Box:
        //            BoxCollider bc = lAllBones[BoneID].Bone.gameObject.AddComponent<BoxCollider>();
        //            Vector3 size = new Vector3(lAllBones[BoneID].Radius, lAllBones[BoneID].Radius, lAllBones[BoneID].Radius);
        //            size[direction] = Mathf.Abs(distance); // * 0.5f);
        //            bc.size = size;
        //            bc.center = center;
        //            break;
        //        case eColliderType.Sphere:
        //            SphereCollider sc = lAllBones[BoneID].Bone.gameObject.AddComponent<SphereCollider>();
        //            sc.center = center;
        //            sc.radius = lAllBones[BoneID].Radius;
        //            break;
        //        default:
        //            CapsuleCollider collider = (CapsuleCollider)lAllBones[BoneID].Bone.gameObject.AddComponent<CapsuleCollider>();
        //            collider.direction = direction;
        //            collider.center = center;
        //            collider.height = Mathf.Abs(distance); // *0.5f);
        //            collider.radius = lAllBones[BoneID].Radius;
        //            break;
        //    }
        //}

        ///// <summary>
        ///// Calculate longest axis of the bone.
        ///// </summary>
        ///// <param name="Point"></param>
        ///// <param name="Direction">Direction of the longest axis return parameter.</param>
        ///// <param name="Distance">Distance of the longest axis return parameter.</param>
        //static void CalculateDirection(Vector3 Point, out int Direction, out float Distance)
        //{
        //    Direction = 0;
        //    if (Mathf.Abs(Point[1]) > Mathf.Abs(Point[0]))
        //        Direction = 1;
        //    if (Mathf.Abs(Point[2]) > Mathf.Abs(Point[Direction]))
        //        Direction = 2;

        //    Distance = Point[Direction];
        //}  

        ///// <summary>
        ///// Remove previously added character joint, collider components from the bone.
        ///// </summary>
        ///// <param name="BoneID">ID of the bone.</param>
        ///// <param name="JustColliders">Only remove colliders.</param>
        //void RemoveBoneComponents(int BoneID, bool JustColliders)
        //{
        //    switch (lAllBones[BoneID].Type)  // remove which collider
        //    {
        //        case eColliderType.Box:
        //            BoxCollider bc = lAllBones[BoneID].Bone.gameObject.GetComponent<BoxCollider>();  // get 
        //            if (bc) DestroyImmediate(bc);  // remove
        //            break;
        //        case eColliderType.Sphere:
        //            SphereCollider sc = lAllBones[BoneID].Bone.gameObject.GetComponent<SphereCollider>();  // get 
        //            if (sc) DestroyImmediate(sc);  // remove
        //            break;
        //        case eColliderType.Capsule:
        //            CapsuleCollider cc = lAllBones[BoneID].Bone.gameObject.GetComponent<CapsuleCollider>();  // get 
        //            if (cc) DestroyImmediate(cc);  // remove
        //            break;
        //    }
        //    if (!JustColliders)  // remove all?
        //    {
        //        CharacterJoint cj = lAllBones[BoneID].Bone.gameObject.GetComponent<CharacterJoint>();  // get 
        //        if (cj) DestroyImmediate(cj);  // remove
        //        Rigidbody rb = lAllBones[BoneID].Bone.gameObject.GetComponent<Rigidbody>();  // get 
        //        if (rb) DestroyImmediate(rb);  // remove  
        //        vCollisionMessage cm = lAllBones[BoneID].Bone.gameObject.GetComponent<vCollisionMessage>();  // get 
        //        if (cm) DestroyImmediate(cm);  // remove  
        //        lAllBones[BoneID].Shown = false;  // also remove from the GUI
        //    }
        //}  

        //#endregion
    }

    #region "List classes"
    /// <summary>
    /// Limb chain data for use within the main limb chain list.
    /// </summary>
    public class LimbChain
    {
        /// <summary>Which root bone to attach to</summary>
        public int Rootbone;

        /// <summary>Which link bone to attach to, overrides root bone setting</summary>
        public int Linkbone;  

        /// <summary>Include in the ragdoll</summary>
        public bool Enable;  

        /// <summary>Components added</summary>
        public bool AddedAlready;  

        /// <summary>Total links in the chain (for GUI).</summary>
        public int Links;  

        /// <summary>Start of the limb chain.</summary>
        public Transform ChainStart; 

        /// <summary>End of the limb chain (for GUI).</summary>
        public Transform ChainEnd;  

        /// <summary>ID's of bones in the chain.</summary>
        public List<int> Bones;  
    }

    /// <summary>
    /// Collider settings and references for use within the bone list.
    /// </summary>
    public class BoneOptions
    {
        /// <summary>Updated depending upon limb chains selected.</summary>
        public bool Shown;  

        /// <summary>Collider override.</summary>
        public ReOrganiseHelper.eColliderType Type;  

        /// <summary>Reference to the bone.</summary>
        public Transform Bone;  

        /// <summary>Collider radius.</summary>
        public float Radius;  
    }  
#endregion
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
