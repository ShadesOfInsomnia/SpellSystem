using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using Invector;
using Invector.vItemManager;
using Invector.vMelee;
using Invector.vCharacterController;
using Invector.vCharacterController.vActions;
using UnityEngine.Events;
using Invector.vCharacterController.AI;
#if UNITY_EDITOR
using UnityEditor.Events;
#endif

namespace Shadex
{
    /// <summary>
    /// A very lite version of the invector item manager specifically for the AI to hold spells.
    /// </summary>
    /// <remarks>
    /// Used to provide the AI's with a inventory list, used for spell selection by the 
    /// magic AI script, enables the sharing of spells between the player and AI's.
    /// 
    /// Also allows potions to be stored/dropped on death.
    /// </remarks>
    public class MagicAIItemManager : vMonoBehaviour
    {
        /// <summary>Drop all items that have a pickup assigned in the inventory.</summary>
        [Tooltip("Drop all items that have a pickup assigned in the inventory")]
        public bool dropItemsWhenDead;

        /// <summary>Link to the same item list data as the player item manager.</summary>
        [Tooltip("Link to the same item list data as the player item manager")]
        public vItemListData itemListData;

        /// <summary>Filter items by item type.</summary>
        [Tooltip("Filter items by item type")]
        public List<vItemType> itemsFilter = new List<vItemType>() { 0 };

        /// <summary>List of the items for the AI, place spells in here and any initial weapons.</summary>
        [Tooltip("List of the items for the AI, place spells in here and any initial weapons")]
        [SerializeField] public List<ItemReference> startItems = new List<ItemReference>();

        /// <summary>List of the current items the AI holds.</summary>
        public List<vItem> items;

        /// <summary>Occurs when an item is used.</summary>
        public OnHandleItemEvent onUseItem;

        /// <summary>Occurs when an item is added to the list.</summary>
        public OnHandleItemEvent onAddItem;

        /// <summary>Occurs when an item is left.</summary>
        public OnChangeItemAmount onLeaveItem;

        /// <summary>Occurs when an item is dropped.</summary>
        public OnChangeItemAmount onDropItem;

        /// <summary>List of apply attribute events.</summary>
        [SerializeField] public List<ApplyAttributeEvent> applyAttributeEvents;

        /// <summary>Transform of the left equip point.</summary>
        [Tooltip("Transform of the left equip point")]
        public Transform defaultEquipPointL;

        /// <summary>Transform of the right equip point.</summary>
        [Tooltip("Transform of the right equip point")]
        public Transform defaultEquipPointR;
        
        /// <summary>Cache reference to the melee manager.</summary>
        protected vMeleeManager TheMeleeManager;

        /// <summary>
        /// Initialise items in the list, setup listeners.
        /// </summary>
        /// <returns>IEnumerator whilst waiting till the end of frame.</returns>
        IEnumerator Start()
        {
            TheMeleeManager = GetComponent<vMeleeManager>();

            if (dropItemsWhenDead)
            {
                var character = GetComponent<v_AIController>();
                if (character)
                    character.onDead.AddListener(DropAllItens);
            }

            var genericAction = GetComponent<vGenericAction>();
            if (genericAction != null)
                genericAction.OnDoAction.AddListener(CollectItem);

            yield return new WaitForEndOfFrame();
            items = new List<vItem>();
            if (itemListData)
            {
                for (int i = 0; i < startItems.Count; i++)
                {
                    AddItem(startItems[i], startItems[i].autoEquip);
                }
            }
        }

        /// <summary>
        /// Get the list of vItems that the AI holds.
        /// </summary>
        /// <returns>List of vItems.</returns>
        public List<vItem> GetItems()
        {
            return items;
        }

        /// <summary>
        /// Add new item to the AI inventory.
        /// </summary>
        /// <param name="itemReference">Reference object containing the vItem and related info.</param>
        /// <param name="immediate">Equip the item immediate?</param>
        public void AddItem(ItemReference itemReference, bool immediate = false)
        {
            if (itemReference != null && itemListData != null && itemListData.items.Count > 0)
            {
                var item = itemListData.items.Find(t => t.id.Equals(itemReference.id));
                if (item)
                {
                    var sameItems = items.FindAll(i => i.stackable && i.id == item.id && i.amount < i.maxStack);
                    if (sameItems.Count == 0)
                    {
                        var _item = Instantiate(item);
                        _item.name = _item.name.Replace("(Clone)", string.Empty);
                        if (itemReference.attributes != null && _item.attributes != null && item.attributes.Count == itemReference.attributes.Count)
                            _item.attributes = new List<vItemAttribute>(itemReference.attributes);
                        _item.amount = 0;
                        for (int i = 0; i < item.maxStack && _item.amount < _item.maxStack && itemReference.amount > 0; i++)
                        {
                            _item.amount++;
                            itemReference.amount--;
                        }
                        items.Add(_item);
                        onAddItem.Invoke(_item);
                        if (itemReference.autoEquip)
                        {
                            itemReference.autoEquip = false;
                            AutoEquipItem(_item, itemReference.indexArea, immediate);
                        }

                        if (itemReference.amount > 0) AddItem(itemReference);
                    }
                    else
                    {
                        var indexOffItem = items.IndexOf(sameItems[0]);

                        for (int i = 0; i < items[indexOffItem].maxStack && items[indexOffItem].amount < items[indexOffItem].maxStack && itemReference.amount > 0; i++)
                        {
                            items[indexOffItem].amount++;
                            itemReference.amount--;
                        }
                        if (itemReference.amount > 0) AddItem(itemReference);
                    }
                }
            }
        }

        /// <summary>
        /// Auto equip new item.
        /// </summary>
        /// <param name="item">Item to equip.</param>
        /// <param name="indexArea">Index of the area to equip the item.</param>
        /// <param name="immediate">Force equip immediate.</param>
        public void AutoEquipItem(vItem item, int indexArea, bool immediate = false)
        {
            if (item.type == vItemType.MeleeWeapon)
            {
                var MeleeWeapon = item.originalObject.GetComponent<vMeleeWeapon>();
                if (MeleeWeapon)
                {
                    var ActualWeapon = Instantiate(item.originalObject) as GameObject;
                    if (MeleeWeapon.meleeType == vMeleeType.OnlyDefense)  // left
                    {

                        ActualWeapon.transform.parent = defaultEquipPointL;
                        ActualWeapon.transform.localPosition = Vector3.zero;
                        ActualWeapon.transform.localEulerAngles = Vector3.zero;
                        TheMeleeManager.SetLeftWeapon(ActualWeapon);
                    } 
                    else  // right
                    {
                        ActualWeapon.transform.parent = defaultEquipPointR;
                        ActualWeapon.transform.localPosition = Vector3.zero;
                        ActualWeapon.transform.localEulerAngles = Vector3.zero;
                        TheMeleeManager.SetRightWeapon(ActualWeapon);
                    }
                }
            }
        }


        /// <summary>
        /// Occurs when the AI uses a vItem.
        /// </summary>
        /// <param name="item">vItem that has been used, drunk etc...</param>
        public void UseItem(vItem item)
        {
            if (item)
            {
                onUseItem.Invoke(item);
                if (item.attributes != null && item.attributes.Count > 0 && applyAttributeEvents.Count > 0)
                {
                    foreach (ApplyAttributeEvent attributeEvent in applyAttributeEvents)
                    {
                        var attributes = item.attributes.FindAll(a => a.name.Equals(attributeEvent.attribute));
                        foreach (vItemAttribute attribute in attributes)
                            attributeEvent.onApplyAttribute.Invoke(attribute.value);
                    }
                }
                if (item.amount <= 0 && items.Contains(item)) items.Remove(item);
            }
        }

        /// <summary>
        /// Occurs when an vItem has been dropped by the AI.
        /// </summary>
        /// <param name="item">Item that has been dropped.</param>
        /// <param name="amount">Amount that has been dropped.</param>
        public void DropItem(vItem item, int amount)
        {
            item.amount -= amount;
            if (item.dropObject != null)
            {
                var dropObject = Instantiate(item.dropObject, transform.position, transform.rotation) as GameObject;
                vItemCollection collection = dropObject.GetComponent<vItemCollection>();
                if (collection != null)
                {
                    collection.items.Clear();
                    var itemReference = new ItemReference(item.id);
                    itemReference.amount = amount;
                    itemReference.attributes = new List<vItemAttribute>(item.attributes);
                    collection.items.Add(itemReference);
                }
            }
            onDropItem.Invoke(item, amount);
            if (item.amount <= 0 && items.Contains(item))
            {
                items.Remove(item);
                Destroy(item);
            }
        }

        /// <summary>
        /// Trigger the AI dropping all items.
        /// </summary>
        /// <param name="target">Drop target.</param>
        public void DropAllItens(GameObject target = null)
        {
            if (target != null && target != gameObject) return;
            List<ItemReference> itemReferences = new List<ItemReference>();
            for (int i = 0; i < items.Count; i++)
            {
                if (itemReferences.Find(_item => _item.id == items[i].id) == null)
                {
                    var sameItens = items.FindAll(_item => _item.id == items[i].id);
                    ItemReference itemReference = new ItemReference(items[i].id);
                    for (int a = 0; a < sameItens.Count; a++)
                    {
                        itemReference.amount += sameItens[a].amount;
                        Destroy(sameItens[a]);
                    }
                    itemReferences.Add(itemReference);
                    if (items[i].dropObject)
                    {
                        var dropObject = Instantiate(items[i].dropObject, transform.position, transform.rotation) as GameObject;
                        vItemCollection collection = dropObject.GetComponent<vItemCollection>();
                        if (collection != null)
                        {
                            collection.items.Clear();
                            collection.items.Add(itemReference);
                        }
                    }
                }
            }
            items.Clear();
        }

        #region Check Items
        /// <summary>
        /// Check the vItem list for an vItem by ID.
        /// </summary>
        /// <param name="id">ID to search for.</param>
        /// <returns>Boolean of whether found.</returns>
        public bool ContainItem(int id)
        {
            return items.Exists(i => i.id == id);
        }

        /// <summary>
        /// Check the vItem list for an vItem by name.
        /// </summary>
        /// <param name="itemName">Name of the vItem.</param>
        /// <returns>Boolean of whether found.</returns>
        public bool ContainItem(string itemName)
        {
            return items.Exists(i => i.name == itemName);
        }

        /// <summary>
        /// Check the vItem list for an vItem by id and amount.
        /// </summary>
        /// <param name="id">ID of the vItem.</param>
        /// <param name="amount">Amount of the vItem.</param>
        /// <returns>Boolean of whether found.</returns>
        public bool ContainItem(int id, int amount)
        {
            var item = items.Find(i => i.id == id && i.amount >= amount);
            return item != null;
        }

        /// <summary>
        /// Check the vItem list for an vItem by name and amount.
        /// </summary>
        /// <param name="itemName">Name of the vItem.</param>
        /// <param name="amount">Amount of the vItem.</param>
        /// <returns>Boolean of whether found.</returns>
        public bool ContainItem(string itemName, int amount)
        {
            var item = items.Find(i => i.name == itemName && i.amount >= amount);
            return item != null;
        }

        /// <summary>
        /// Check the vItem list for an vItem by id and count.
        /// </summary>
        /// <param name="id">ID of the vItem.</param>
        /// <param name="count">Count of the vItem.</param>
        /// <returns>Boolean of whether found.</returns>
        public bool ContainItems(int id, int count)
        {
            var _items = items.FindAll(i => i.id == id);
            return _items != null && _items.Count >= count;
        }

        /// <summary>
        /// Check the vItem list for an vItem by name and count.
        /// </summary>
        /// <param name="itemName">Name of the vItem.</param>
        /// <param name="count">Count of the vItem.</param>
        /// <returns>Boolean of whether found.</returns>
        public bool ContainItems(string itemName, int count)
        {
            var _items = items.FindAll(i => i.name == itemName);
            return _items != null && _items.Count >= count;
        }
        #endregion

        #region Get Items 
        /// <summary>
        /// Gets a vItem by id.
        /// </summary>
        /// <param name="id">ID of the vItem.</param>
        /// <returns></returns>
        public vItem GetItem(int id)
        {
            return items.Find(i => i.id == id);
        }

        /// <summary>
        /// Gets a vItem by name.
        /// </summary>
        /// <param name="itemName">Name of the vItem.</param>
        /// <returns></returns>
        public vItem GetItem(string itemName)
        {
            return items.Find(i => i.name == itemName);
        }


        /// <summary>
        /// Gets a vItem list by id.
        /// </summary>
        /// <param name="id">ID of the vItem.</param>
        /// <returns>All matching vItems in a list.</returns>
        public List<vItem> GetItems(int id)
        {
            var _items = items.FindAll(i => i.id == id);
            return _items;
        }

        /// <summary>
        /// Gets a vItem list by name.
        /// </summary>
        /// <param name="itemName">Name of the vItem.</param>
        /// <returns>All matching vItems in a list.</returns>
        public List<vItem> GetItems(string itemName)
        {
            var _items = items.FindAll(i => i.name == itemName);
            return _items;
        }

        #endregion



        #region Item Collector    

        /// <summary>
        /// Bulk collect vItems to the inventory.
        /// </summary>
        /// <param name="collection">vItem collection to collect.</param>
        /// <param name="immediate">Add with a delay.</param>
        public virtual void CollectItems(List<ItemReference> collection, bool immediate = false)
        {
            foreach (ItemReference reference in collection)
            {
                AddItem(reference, immediate);
            }
        }

        /// <summary>
        /// Collect single vItem to the inventory.
        /// </summary>
        /// <param name="itemRef">Reference to the vItem to collect.</param>
        /// <param name="immediate">Add with a delay.</param>
        public virtual void CollectItem(ItemReference itemRef, bool immediate = false)
        {
            AddItem(itemRef, immediate);
        }

        /// <summary>
        /// Collect items with delay coroutine trigger
        /// </summary>
        /// <param name="action">Source generic action trigger.</param>
        public virtual void CollectItem(vTriggerGenericAction action)
        {
            var collection = action.GetComponentInChildren<vItemCollection>();
            if (collection != null)
            {
                if (collection.items.Count > 0)
                {
                    var itemCol = collection.items.vCopy();
                    StartCoroutine(CollectItemsWithDelay(itemCol, collection.onCollectDelay, collection.textDelay, collection.immediate));
                }
            }
        }

        /// <summary>
        /// Coroutine body that collects the vItems after a potential delay.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="onCollectDelay">Delay length.</param>
        /// <param name="textDelay">Delay on notifying the user.</param>
        /// <param name="immediate">Add with a delay.</param>
        /// <returns></returns>
        public virtual IEnumerator CollectItemsWithDelay(List<ItemReference> collection, float onCollectDelay, float textDelay, bool immediate)
        {
            yield return new WaitForSeconds(onCollectDelay);

            for (int i = 0; i < collection.Count; i++)
            {
                yield return new WaitForSeconds(textDelay);

                var item = itemListData.items.Find(_item => _item.id == collection[i].id);
                if (item != null && vItemCollectionDisplay.Instance != null)
                {
                    vItemCollectionDisplay.Instance.FadeText("Acquired:" + " " + collection[i].amount + " " + item.name, 4, 0.25f);
                }
                CollectItem(collection[i], immediate);
            }
        }

        /// <summary>
        /// Coroutine body that collects a single vItem after a potential delay.
        /// </summary>
        /// <param name="itemRef">Reference to the vItem to collect.</param>
        /// <param name="onCollectDelay">Delay length.</param>
        /// <param name="textDelay">Delay on notifying the user.</param>
        /// <param name="immediate">Add with a delay.</param>
        /// <returns></returns>
        public virtual IEnumerator CollectItemWithDelay(ItemReference itemRef, float onCollectDelay, float textDelay, bool immediate)
        {
            yield return new WaitForSeconds(onCollectDelay + textDelay);

            var item = itemListData.items.Find(_item => _item.id == itemRef.id);
            if (item != null && vItemCollectionDisplay.Instance != null)
            {
                vItemCollectionDisplay.Instance.FadeText("Acquired:" + " " + itemRef.amount + " " + item.name, 4, 0.25f);
            }
            CollectItem(itemRef, immediate);
        }

        #endregion

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
