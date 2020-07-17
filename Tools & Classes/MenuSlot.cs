using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
//Author William Jones
[RequireComponent(typeof(RectTransform))]
public class MenuSlot : MonoBehaviour
{
    public enum SlotType { PlayerInventory, HomeInventory, DisplaySlot, DecorationSlot };
    private MenuSlotItem item;
    public int slotIndex;
    [SerializeField] public SlotType type;
    bool isGrabbed;
    bool updateScale = false;
    bool updateNextScale = false;

    public void grabItem()
    {
        GetComponent<Image>().color = Color.green;
        isGrabbed = true;
    }

    /// <summary>
    /// Releases the item to the given slot, if it's null then nothing happens
    /// </summary>
    public void releaseItem(MenuSlot releaseTo)
    {
        Debug.Log("vvvvvvvvv Start releasing vvvvvvvvvv");
        //Note: "this" always refers to the item slot that holds the grabbed item

        HomeBaseUIController hbuic = GetComponentInParent<HomeBaseUIController>();

        GetComponent<Image>().color = Color.white;
        isGrabbed = false;

        //If the item is actually dropped onto a slot
        //And the slot is different than the one it was picked up in
        if (releaseTo != null && releaseTo != this)
        {
            //First thing to do is see if the player is "splitting" items
            //If there is a visible item with the same id in the player inventory or the home inventory then merge the two items together
            if (releaseTo.type == SlotType.HomeInventory || releaseTo.type == MenuSlot.SlotType.DisplaySlot)
            {
                //Get a matching slot
                MenuSlot matchingSlot = hbuic.findSlotWithItem(this.getItem());
                //Make sure there is a matching slot and it's not this one and it has more than 0 items
                if (matchingSlot != null && matchingSlot != this && matchingSlot.getCount() > 0)
                {
                    //If the player didn't already try to merge the items
                    if (matchingSlot != releaseTo)
                    {
                        if (releaseTo.slotIndex != 0 || releaseTo.type == SlotType.DisplaySlot)
                        {
                            //Pretend the player clicked on the matching slot
                            matchingSlot.swapWith(releaseTo);
                        }
                        else
                        {
                            releaseTo = matchingSlot;
                        }
                    }
                }
            }

            //This only happens if the player is moving something to a slot
            bool iCanMove, uCanMove, weSwap;
            iCanMove = canMoveToSlot(releaseTo);
            uCanMove = releaseTo.canMoveToSlot(this);
            Debug.Log("iCanMove: " + iCanMove + " uCanMove: " + uCanMove);
            weSwap = (iCanMove && uCanMove);
            if (weSwap)
            {
                this.swapWith(releaseTo);
            }
            else //It can't be a straight up swap
            {
                //If my item can move to the given slot
                if (iCanMove)
                {
                    //If it can't combine them then
                    if (!combineSuccess(releaseTo))
                    {
                        //Store the other one in its respective inventory
                        releaseTo.storeItem(false);
                        //Make this one empty by swapping the two items
                        this.swapWith(releaseTo);
                    }
                }
                else
                {
                    //If this item isn't a decoration slot
                    if (this.type != SlotType.DecorationSlot)
                    {
                        //Store this item into its proper home inventory (and set it to null)
                        this.storeItem(true);
                    }
                    else
                    {
                        //The the player is trying to store an item that was picked up from a decoration slot
                        returnItemToSlot();
                    }
                    //If the item in the released slot can move
                    if (uCanMove)
                    {
                        //Only swap the items if the other slot isn't a decoration slot
                        if (releaseTo.type != SlotType.DecorationSlot)
                        {
                            this.swapWith(releaseTo);
                        }
                    }
                }
            }
        }
        else //If the item was dropped somewhere that's not a slot (or dropped back onto itself)
        {
            this.returnItemToSlot();
        }

        Debug.Log("^^^^^^^^^^ Done releasing ^^^^^^^^^^");
    }

    private void LateUpdate()
    {
        if (updateScale)
        {
            //Update the model scale
            if (this.getItem() != null)
            {
                getItem().updateModelScale();
            }
            else
            {
                //If the player moves a flower to the 0th slot in the home inventory
                //Then it will move all the flowers over one
                //So it needs to update the scale of the flower in the next slot
                if (this.slotIndex == 0 && this.type == SlotType.HomeInventory)
                {
                    transform.parent.GetChild(transform.GetSiblingIndex()+1).GetComponent<MenuSlot>().getItem().updateModelScale();
                }
            }
            updateScale = false;
        }

        if (updateNextScale)
        {
            updateNextScale = false;
            updateScale = true;
        }
    }

    public void prepareUpdateScale()
    {
        updateNextScale = true;
    }

    private bool combineSuccess(MenuSlot releaseTo)
    {
        MenuSlotItem meTemp = this.getItem();
        MenuSlotItem youTemp = releaseTo.getItem();

        //If the two items should be merged
        if (this.getItem() != null && releaseTo.getItem() != null && meTemp.id == youTemp.id && meTemp.isFlower == youTemp.isFlower)
        {
            HomeBaseUIController hbuic = GetComponentInParent<HomeBaseUIController>();
            if (releaseTo.type == SlotType.PlayerInventory)//if moving to inventory
                hbuic.addHomeInventoryItem(releaseTo.getItem().id, -releaseTo.getItem().count, releaseTo.getItem().isFlower);
            else if(this.type == SlotType.PlayerInventory)//if moving out of inventory
                hbuic.addHomeInventoryItem(this.getItem().id, this.getItem().count, this.getItem().isFlower);
            //Set the other item's count
            int oldCount = releaseTo.getItem().count;
            //Destroy the item in the other slot because the grabbed item will be stored
            GameObject.Destroy(releaseTo.getItem().gameObject);
            //Set the other slot to the grabbed item
            releaseTo.setItem(this.getItem());
            releaseTo.setCount(oldCount + this.getItem().count);


            //Clear the item in the grabbed slot
            this.setItem(null);

            //It was successful so return true
            return true;
        }
        return false;
    }

    public void swapWith(MenuSlot releaseTo)
    {
        MenuSlotItem meTemp = this.getItem();
        MenuSlotItem youTemp = releaseTo.getItem();
        bool changedInventory = false;

        //If the two items can't be combined
        if (!combineSuccess(releaseTo))
        {
            //Maybe add/remove the items to the home inventories
            //tip to future William: only use one | so the evaluation doesn't short curcuit
            changedInventory = changedInventory | maybeAddToInventory(releaseTo);
            changedInventory = changedInventory | maybeRemoveFromInventory(releaseTo);
            changedInventory = changedInventory | releaseTo.maybeAddToInventory(this);
            changedInventory = changedInventory | releaseTo.maybeRemoveFromInventory(this);

            //Swap them
            this.setItem(youTemp);
            releaseTo.setItem(meTemp);
        }

        if (changedInventory && releaseTo.type != SlotType.DisplaySlot)
        {
            HomeBaseUIController hbuic = GetComponentInParent<HomeBaseUIController>();
            hbuic.updateInventoryItemCount();
        }
    }

    //Maybe adds this item to the home inventory depending on where it's from and where it moves to
    public bool maybeAddToInventory(MenuSlot moveTo)
    {
        Debug.Log("Asking to add to inventory from " + this.type + " to " + moveTo.type);
        //Add the item to the inventory if it's coming from the player slot and going to the sell display slot

        //Don't add it to the inventory if it's null
        if (this.getItem() == null) { return false; }
        //Don't add it to the home inventory if it's a home inventory slot
        if (this.type == SlotType.HomeInventory) { return false; }
        //Don't add it to the home inventory if it's a decoration slot
        if (this.type == SlotType.DecorationSlot) { return false; }
        //Don't add it to the home inventory if it's a sell display slot
        if (this.type == SlotType.DisplaySlot) { return false; }
        //Don't add it to the home inventory if it's moving to a player inventory slot
        if (moveTo.type == SlotType.PlayerInventory) { return false; }

        //If it made it here then add it
        HomeBaseUIController hbuic = GetComponentInParent<HomeBaseUIController>();
        hbuic.addHomeInventoryItem(this.getItem().id, this.getItem().count, this.getItem().isFlower);
        return true;
    }

    //Maybe removes this item from the inventory depending on where it's from and where it moves to
    public bool maybeRemoveFromInventory(MenuSlot moveTo)
    {
        Debug.Log("Asking to remove from inventory from " + this.type + " to " + moveTo.type);
        //Don't remove it from the inventory if it's null
        if (this.getItem() == null) { return false; }
        //Don't remove it from the inventory if it comes from a player slot
        if (this.type == SlotType.PlayerInventory) { return false; }
        //Don't remove it from the inventory if it goes to a home inventory slot
        if (moveTo.type == SlotType.HomeInventory) { return false; }
        //Don't remove it from the inventory if it goes to a decoration slot
        if (moveTo.type == SlotType.DecorationSlot) { return false; }
        //Don't remove it from the inventory if it goes to the sell display slot
        if (moveTo.type == SlotType.DisplaySlot) { return false; }

        //If it made it here then remove it
        HomeBaseUIController hbuic = GetComponentInParent<HomeBaseUIController>();
        hbuic.removeHomeInventoryItem(this.getItem().id, this.getItem().isFlower);
        return true;
    }

    //This is a long drawn out method to determine if the item in this slot can move to the given slot
    //The if statements aren't very well optimized but I want to be able to read it easily
    public bool canMoveToSlot(MenuSlot releaseTo)
    {
        /* h: home inventory
         * p: player inventory
         * s: sell slot
         * d: decoration home inventory
         * an * indicates that the transfer is conditional
         */

        //Home inventory -> Anywhere h>h h>p h>s h>d(allowed since the sell and display slots are never shown together)
        if (this.type == SlotType.HomeInventory)
        {
            return true;
        }

        //Sell slot -> Anywhere s>h s>p s>s s>d(allowed since the sell and display slots are never shown together)
        if (this.type == SlotType.DisplaySlot)
        {
            return true;
        }

        //An item in the player inventory can move to any other slot in the player inventory
        //Player inventory -> Player inventory p>p
        if (this.type == SlotType.PlayerInventory && releaseTo.type == SlotType.PlayerInventory)
        {
            return true;
        }

        //Decorations can move to the player inventory
        //Decoration slot -> Player inventory d>p
        if (this.type == SlotType.DecorationSlot && releaseTo.type == SlotType.PlayerInventory)
        {
            return true;
        }

        //Player slot -> Home Inventory p>h *
        if (this.type == SlotType.PlayerInventory && releaseTo.type == SlotType.HomeInventory)
        {
            //Can transfer if it's a flower or empty
            return (this.holdsFlower() || this.getItem() == null);
        }

        //Player slot -> Sell slot p>s
        if (this.type == SlotType.PlayerInventory && releaseTo.type == SlotType.DisplaySlot)
        {
            return true;
        }

        return false;
    }

    public bool holdsFlower()
    {
        if (this.getItem() != null)
        {
            return this.getItem().isFlower;
        }
        return false;
    }

    public void returnItemToSlot()
    {
        this.setItem(this.getItem());
    }

    public void setHover(bool isHovering)
    {
        if (isHovering)
        {
            GetComponent<Image>().color = Color.yellow;
        }
        else
        {
            GetComponent<Image>().color = Color.white;
        }
    }

    private GameObject newSlotItem(uint id, int count, bool isFlower)
    {
        HomeBaseUIController hbuic = GetComponentInParent<HomeBaseUIController>();
        GameObject resultObject = GameObject.Instantiate(hbuic.slotItemPrefab);
        MenuSlotItem slotItem = resultObject.GetComponent<MenuSlotItem>();
        slotItem.count = count;
        slotItem.setID(id,isFlower);
        return resultObject;
    }

    /// <summary>
    /// Stores the item in the slot into the respective inventory
    /// </summary>
    public void storeItem(bool instantUpdate)
    {
        if (this.getItem() != null)
        {
            HomeBaseUIController hbuic = GetComponentInParent<HomeBaseUIController>();
            uint key = this.getItem().id;
            int count = this.getItem().count;
            bool flower = this.getItem().isFlower;
            hbuic.addHomeInventoryItem(key, count, flower);
            Destroy(this.getItem().gameObject);
            this.setItem(null);
            if (instantUpdate)
            {
                hbuic.updateInventoryItemCount();
            }
        }
    }

    public void setItem(MenuSlotItem item)
    {
        if (item == null)
        {
            //If it's a decoration slot then set the count to 0
            if (this.type == SlotType.DecorationSlot)
            {
                //Make a new item for this slot
                uint id = this.getItem().id;
                MenuSlotItem newItem = newSlotItem(id, 0, false).GetComponent<MenuSlotItem>();
                this.setItem(newItem);
            }
            else //If it's anything other than a decoration slot then clear it
            {
                this.item = null;
            }
        }
        else
        {
            this.item = item;
            item.GetComponent<RectTransform>().SetParent(gameObject.GetComponent<RectTransform>(), false);
            item.GetComponent<RectTransform>().localPosition = Vector3.zero;
            GetComponent<RectTransform>().ForceUpdateRectTransforms();
            item.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            if (slotIndex == 0)
            {
                HomeBaseUIController hbuic = GetComponentInParent<HomeBaseUIController>();
                hbuic.reSortFlowerList();
            }
            prepareUpdateScale();
        }

        if (type == SlotType.DisplaySlot)
        {
            HomeBaseUIController hbuic = GetComponentInParent<HomeBaseUIController>();
            hbuic.updateFlowerDisplaySlot(this.getItem());
        }
    }

    public void setCount(int count)
    {
        this.getItem().setCount(count);
        if (this.type != SlotType.DecorationSlot)
        {
            if (this.getItem().count <= 0)
            {
                GameObject.Destroy(this.getItem().gameObject);
                this.item = null;
            }
        }
    }

    public int getCount()
    {
        if (this.getItem()!=null)
        {
            return this.getItem().count;
        }
        else
        {
            return 0;
        }
    }

    public void setItemNoSort(MenuSlotItem item)
    {
        this.item = item;
        if (item != null)
        {
            item.GetComponent<RectTransform>().SetParent(gameObject.GetComponent<RectTransform>(),false);
            item.GetComponent<RectTransform>().localPosition = Vector3.zero;
            GetComponent<RectTransform>().ForceUpdateRectTransforms();
            item.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
        }
    }

    public MenuSlotItem getItem()
    {
        return this.item;
    }
}
