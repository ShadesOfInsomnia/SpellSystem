using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Invector;
using Invector.vItemManager;
using System;

namespace Shadex
{
    /// <summary>
    /// Custom inspector GUI for the AI inventory.
    /// </summary>
    /// <remarks>
    /// Based on the invector player item manager.
    /// </remarks>
    [CustomEditor(typeof(MagicAIItemManager))]
    [System.Serializable]
    public class MagicAIItemManagerEditor : vEditorBase
    {
        #region Variables

        protected MagicAIItemManager manager;
        protected SerializedProperty itemReferenceList;
        GUISkin oldSkin;
        bool inAddItem;
        int selectedItem;
        Vector2 scroll;
        bool showManagerEvents;
        bool showItemAttributes;
        string[] ignoreProperties = new string[] { "equipPoints", "applyAttributeEvents", "items", "startItems", "onUseItem", "onAddItem", "onLeaveItem", "onDropItem", "onOpenCloseInventory", "onEquipItem", "onUnequipItem" };
        bool[] inEdition;
        string[] newPointNames;
        Transform parentBone;
        List<vItem> filteredItems;

        #endregion

        /// <summary>
        /// Initialise when enabled
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            m_Logo = (Texture2D)Resources.Load("itemManagerIcon", typeof(Texture2D));
            manager = (MagicAIItemManager)target;
            itemReferenceList = serializedObject.FindProperty("startItems");
            skin = Resources.Load("skin") as GUISkin;
            //animator = manager.GetComponent<Animator>();
        }

        /// <summary>
        /// Per frame loop whilst inspector GUI is visible
        /// </summary>
        public override void OnInspectorGUI()
        {
            oldSkin = GUI.skin;
            serializedObject.Update();
            if (skin) GUI.skin = skin;
            GUILayout.BeginVertical("MAGIC AI ITEM MANAGER", "window");
            GUILayout.Label(m_Logo, GUILayout.MaxHeight(25));

            openCloseWindow = GUILayout.Toggle(openCloseWindow, openCloseWindow ? "Close" : "Open", EditorStyles.toolbarButton);
            if (openCloseWindow)
            {
                GUI.skin = oldSkin;
                DrawPropertiesExcluding(serializedObject, ignoreProperties.Append(ignore_vMono));
                GUI.skin = skin;

                if (GUILayout.Button("Open Item List"))
                {
                    vItemListWindow.CreateWindow(manager.itemListData);
                }

                if (manager.itemListData)
                {
                    GUILayout.BeginVertical("box");
                    if (itemReferenceList.arraySize > manager.itemListData.items.Count)
                    {
                        manager.startItems.Resize(manager.itemListData.items.Count);
                    }
                    GUILayout.Box("Start Items " + manager.startItems.Count);
                    filteredItems = manager.itemsFilter.Count > 0 ? GetItemByFilter(manager.itemListData.items, manager.itemsFilter) : manager.itemListData.items;

                    if (!inAddItem && filteredItems.Count > 0 && GUILayout.Button("Add Item", EditorStyles.miniButton))
                    {
                        inAddItem = true;
                    }
                    if (inAddItem && filteredItems.Count > 0)
                    {
                        GUILayout.BeginVertical("box");
                        selectedItem = EditorGUILayout.Popup(new GUIContent("SelectItem"), selectedItem, GetItemContents(filteredItems));
                        bool isValid = true;
                        var indexSelected = manager.itemListData.items.IndexOf(filteredItems[selectedItem]);
                        if (manager.startItems.Find(i => i.id == manager.itemListData.items[indexSelected].id) != null)
                        {
                            isValid = false;
                            EditorGUILayout.HelpBox("This item already exist", MessageType.Error);
                        }
                        GUILayout.BeginHorizontal();

                        if (isValid && GUILayout.Button("Add", EditorStyles.miniButton))
                        {
                            itemReferenceList.arraySize++;

                            itemReferenceList.GetArrayElementAtIndex(itemReferenceList.arraySize - 1).FindPropertyRelative("id").intValue = manager.itemListData.items[indexSelected].id;
                            itemReferenceList.GetArrayElementAtIndex(itemReferenceList.arraySize - 1).FindPropertyRelative("amount").intValue = 1;
                            EditorUtility.SetDirty(manager);
                            serializedObject.ApplyModifiedProperties();
                            inAddItem = false;
                        }
                        if (GUILayout.Button("Cancel", EditorStyles.miniButton))
                        {
                            inAddItem = false;
                        }
                        GUILayout.EndHorizontal();


                        GUILayout.EndVertical();
                    }

                    GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
                    scroll = GUILayout.BeginScrollView(scroll, GUILayout.MinHeight(200), GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false));
                    for (int i = 0; i < manager.startItems.Count; i++)
                    {
                        var item = manager.itemListData.items.Find(t => t.id.Equals(manager.startItems[i].id));
                        if (item)
                        {
                            GUILayout.BeginVertical("box");
                            GUILayout.BeginHorizontal();
                            GUILayout.BeginHorizontal();

                            var rect = GUILayoutUtility.GetRect(50, 50);

                            if (item.icon != null)
                            {
                                DrawTextureGUI(rect, item.icon, new Vector2(50, 50));
                            }

                            var name = " ID " + item.id.ToString("00") + "\n - " + item.name + "\n - " + item.type.ToString();
                            var content = new GUIContent(name, null, "Click to Open");
                            GUILayout.Label(content, EditorStyles.miniLabel);
                            GUILayout.BeginVertical("box");
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Auto Equip", EditorStyles.miniLabel);
                            manager.startItems[i].autoEquip = EditorGUILayout.Toggle("", manager.startItems[i].autoEquip, GUILayout.Width(30));
                            if (manager.startItems[i].autoEquip)
                            {
                                GUILayout.Label("IndexArea", EditorStyles.miniLabel);
                                manager.startItems[i].indexArea = EditorGUILayout.IntField("", manager.startItems[i].indexArea, GUILayout.Width(30));
                            }
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Amount", EditorStyles.miniLabel);
                            manager.startItems[i].amount = EditorGUILayout.IntField(manager.startItems[i].amount, GUILayout.Width(30));

                            if (manager.startItems[i].amount < 1)
                            {
                                manager.startItems[i].amount = 1;
                            }

                            GUILayout.EndHorizontal();
                            if (item.attributes.Count > 0)
                                manager.startItems[i].changeAttributes = GUILayout.Toggle(manager.startItems[i].changeAttributes, new GUIContent("Change Attributes", "This is a override of the original item attributes"), EditorStyles.miniButton, GUILayout.ExpandWidth(true));
                            GUILayout.EndVertical();

                            GUILayout.EndHorizontal();

                            if (GUILayout.Button("x", GUILayout.Width(25), GUILayout.Height(25)))
                            {
                                itemReferenceList.DeleteArrayElementAtIndex(i);
                                EditorUtility.SetDirty(target);
                                serializedObject.ApplyModifiedProperties();
                                break;
                            }

                            GUILayout.EndHorizontal();

                            Color backgroundColor = GUI.backgroundColor;
                            GUI.backgroundColor = Color.clear;
                            var _rec = GUILayoutUtility.GetLastRect();
                            _rec.width -= 100;

                            EditorGUIUtility.AddCursorRect(_rec, MouseCursor.Link);

                            if (GUI.Button(_rec, ""))
                            {
                                if (manager.itemListData.inEdition)
                                {
                                    if (vItemListWindow.Instance != null)
                                        vItemListWindow.SetCurrentSelectedItem(manager.itemListData.items.IndexOf(item));
                                    else
                                        vItemListWindow.CreateWindow(manager.itemListData, manager.itemListData.items.IndexOf(item));
                                }
                                else
                                    vItemListWindow.CreateWindow(manager.itemListData, manager.itemListData.items.IndexOf(item));
                            }
                            GUILayout.Space(7);
                            GUI.backgroundColor = backgroundColor;
                            if (item.attributes != null && item.attributes.Count > 0)
                            {

                                if (manager.startItems[i].changeAttributes)
                                {
                                    if (GUILayout.Button("Reset", EditorStyles.miniButton))
                                    {
                                        manager.startItems[i].attributes = null;

                                    }
                                    if (manager.startItems[i].attributes == null)
                                    {
                                        manager.startItems[i].attributes = item.attributes.CopyAsNew();
                                    }
                                    else if (manager.startItems[i].attributes.Count != item.attributes.Count)
                                    {
                                        manager.startItems[i].attributes = item.attributes.CopyAsNew();
                                    }
                                    else
                                    {
                                        for (int a = 0; a < manager.startItems[i].attributes.Count; a++)
                                        {
                                            GUILayout.BeginHorizontal();
                                            GUILayout.Label(manager.startItems[i].attributes[a].name.ToString());
                                            manager.startItems[i].attributes[a].value = EditorGUILayout.IntField(manager.startItems[i].attributes[a].value, GUILayout.MaxWidth(60));
                                            GUILayout.EndHorizontal();
                                        }
                                    }
                                }
                            }

                            GUILayout.EndVertical();
                        }
                        else
                        {
                            itemReferenceList.DeleteArrayElementAtIndex(i);
                            EditorUtility.SetDirty(manager);
                            serializedObject.ApplyModifiedProperties();
                            break;
                        }
                    }

                    GUILayout.EndScrollView();
                    GUI.skin.box = boxStyle;

                    GUILayout.EndVertical();
                    if (GUI.changed)
                    {
                        EditorUtility.SetDirty(manager);
                        serializedObject.ApplyModifiedProperties();
                    }
                }

                var applyAttributeEvents = serializedObject.FindProperty("applyAttributeEvents");
                var onUseItem = serializedObject.FindProperty("onUseItem");
                var onAddItem = serializedObject.FindProperty("onAddItem");
                var onDropItem = serializedObject.FindProperty("onDropItem");
                var onCollectItems = serializedObject.FindProperty("onCollectItems");

                if (applyAttributeEvents != null) DrawAttributeEvents(applyAttributeEvents);
                GUILayout.BeginVertical("box");
                showManagerEvents = GUILayout.Toggle(showManagerEvents, showManagerEvents ? "Close Events" : "Open Events", EditorStyles.miniButton);
                GUI.skin = oldSkin;
                if (showManagerEvents)
                {
                    if (onAddItem != null) EditorGUILayout.PropertyField(onAddItem);
                    if (onUseItem != null) EditorGUILayout.PropertyField(onUseItem);
                    if (onDropItem != null) EditorGUILayout.PropertyField(onDropItem);
                    if (onCollectItems != null) EditorGUILayout.PropertyField(onCollectItems);
                }
                GUI.skin = skin;
                GUILayout.EndVertical();
            }
            GUILayout.EndVertical();
            if (GUI.changed)
            {
                EditorUtility.SetDirty(manager);
                serializedObject.ApplyModifiedProperties();
            }

            GUI.skin = oldSkin;
        }

        /// <summary>
        /// Draw GUI texture.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="sprite"></param>
        /// <param name="size"></param>
        void DrawTextureGUI(Rect position, Sprite sprite, Vector2 size)
        {
            Rect spriteRect = new Rect(sprite.rect.x / sprite.texture.width, sprite.rect.y / sprite.texture.height,
                                       sprite.rect.width / sprite.texture.width, sprite.rect.height / sprite.texture.height);
            Vector2 actualSize = size;
            actualSize.y *= (sprite.rect.height / sprite.rect.width);
            GUI.DrawTextureWithTexCoords(new Rect(position.x, position.y + (size.y - actualSize.y) / 2, actualSize.x, actualSize.y), sprite.texture, spriteRect);

        }

        /// <summary>
        /// vItem to GUIContent.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>Icon from the vItem</returns>
        GUIContent GetItemContent(vItem item)
        {
            var texture = item.icon != null ? item.icon.texture : null;
            return new GUIContent(item.name, texture, item.description); ;
        }

        /// <summary>
        /// Return vItem list for add.
        /// </summary>
        /// <param name="items">Items to filter.</param>
        /// <param name="filter">Filter to apply.</param>
        /// <returns>List of vItems filtered.</returns>
        List<vItem> GetItemByFilter(List<vItem> items, List<vItemType> filter)
        {
            return items.FindAll(i => filter.Contains(i.type));
        }

        /// <summary>
        /// Extract the item list into GUIContent list.
        /// </summary>
        /// <param name="items">vItem list to extract.</param>
        /// <returns>GUIContent list to return.</returns>
        GUIContent[] GetItemContents(List<vItem> items)
        {
            GUIContent[] names = new GUIContent[items.Count];
            for (int i = 0; i < items.Count; i++)
            {
                var texture = items[i].icon != null ? items[i].icon.texture : null;
                names[i] = new GUIContent(items[i].name, texture, items[i].description);
            }
            return names;
        }

        /// <summary>
        /// Convert property to an array.
        /// </summary>
        /// <typeparam name="T">Type of object array to return.</typeparam>
        /// <param name="prop">Property to convert.</param>
        /// <returns>Array of specified type.</returns>
        T[] ConvertToArray<T>(SerializedProperty prop)
        {
            T[] value = new T[prop.arraySize];
            for (int i = 0; i < prop.arraySize; i++)
            {
                object element = prop.GetArrayElementAtIndex(i).objectReferenceValue;
                value[i] = (T)element;
            }
            return value;
        }

        /// <summary>
        /// Draw attribute from property.
        /// </summary>
        /// <param name="prop">Property to draw.</param>
        void DrawAttributeEvents(SerializedProperty prop)
        {
            GUILayout.BeginVertical("box");
            prop.isExpanded = GUILayout.Toggle(prop.isExpanded, prop.isExpanded ? "Close Attribute Events" : "Open Attribute Events", EditorStyles.miniButton);
            if (prop.isExpanded)
            {
                prop.arraySize = EditorGUILayout.IntField("Attributes", prop.arraySize);
                for (int i = 0; i < prop.arraySize; i++)
                {

                    var attributeName = prop.GetArrayElementAtIndex(i).FindPropertyRelative("attribute");
                    var onApplyAttribute = prop.GetArrayElementAtIndex(i).FindPropertyRelative("onApplyAttribute");
                    try
                    {
                        GUILayout.BeginVertical("box");
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(attributeName);
                        if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(20)))
                        {
                            prop.DeleteArrayElementAtIndex(i);
                            GUILayout.EndHorizontal();
                            break;
                        }
                        GUILayout.EndHorizontal();
                        GUI.skin = oldSkin;
                        EditorGUILayout.PropertyField(onApplyAttribute);
                        GUI.skin = skin;
                        GUILayout.EndVertical();
                    }
                    catch { }

                }
            }
            GUILayout.EndVertical();
        }
    }
}

/* *****************************************************************************************************************************
 * Copyright        : 2017 Shades of Insomnia
 * Founding Members : Charles Page (Shade)
 *                  : Rob Alexander (Insomnia)
 * License          : Attribution-ShareAlike 4.0 International (CC BY-SA 4.0) https://creativecommons.org/licenses/by-sa/4.0/
 * Thanks to        : Invector team for the original source that has allowed AI to have an inventory for spells and potions
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
