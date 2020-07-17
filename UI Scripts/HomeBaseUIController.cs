using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;
//Author William Jones
public class HomeBaseUIController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] RectTransform actionContainer;
    [SerializeField] RectTransform pageContainer;
    [SerializeField] GameObject flowerActionPrefab;
    [SerializeField] GameObject decorationActionPrefab;
    [SerializeField] GameObject upgradeActionPrefab;
    [SerializeField] GameObject flowerPagePrefab;
    [SerializeField] GameObject decorationPagePrefab;
    [SerializeField] GameObject upgradePagePrefab;
    [SerializeField] GameObject decorationPurchasePrefab;
    [SerializeField] RectTransform playerInventoryContainer;
    [SerializeField] GameObject playerInventorySlotPrefab;
    [SerializeField] GameObject homeInventorySlotPrefab;
    [SerializeField] GameObject flowerValueCanvas;
    [SerializeField] public GameObject slotItemPrefab;
    [SerializeField] Dictionary<uint,int> decorationValue;

    GameObject currentActionObject;
    GameObject currentPageObject;
    GameObject otherCanvas;

    FlowerPlayerControler fpc;

    public List<GameObject> homeFlowerSlots;
    Dictionary<uint, int> homeFlowerInventory; //All the flowers the player has
    Dictionary<uint, int> displayInventory; //All the flowers that should appear in the search
    Dictionary<uint, int> homeDecorationInventory;
    Dictionary<uint, int> homeDecorationsBought;

    [SerializeField] LayerMask slotLayerMask;

    public MenuSlot grabbedSlot;
    public MenuSlot initialClickSlot;
    bool canGrabItem = true;
    float clickHoldTimer = 0f;
    float clickHoldThreshold = .35f;
    GameObject clickedOn;
    bool slotIsClicked;

    private int[,] decorationTabIDs;
    private int decorationTab = 0;

    int flowersPerPage;
    int currentFlowerPage;

    string sellPrefix = "SELL";
    string sellAllPrefix = "SELL ALL";

    bool updateNextFrame = false;
    bool updateThisFrame = false;

    GameObject playerStuffContainer;
    GameObject topContainer;

    float slotSize = 150;

    public enum TabType { Flowers, Decorations, Upgrades}
    public TabType currentTab;
    public enum UpgradeType { Jetpack, InventorySize}

    uint searchFlower = 0;
    uint searchMask = 0;

    int jetpackPurchasePrice = 1000;
    int warpDrivePurchasePrice = 1000;
    int jetpackUpgradePerLevel = 500;

    int currentQuestID = 0;

    [SerializeField] Sprite upgradeFilledSprite;
    private void Start()
    {
        setupDecorationTabThings();

        homeFlowerSlots = new List<GameObject>();
        otherCanvas = GameObject.Find("UICanvas");
        fpc = GameObject.Find("FPSController").GetComponent<FlowerPlayerControler>();
        homeFlowerInventory = HomeInventoryData.instance.homeFlowerInventory;
        homeDecorationInventory = HomeInventoryData.instance.homeDecorationInventory;
        homeDecorationsBought = HomeInventoryData.instance.homeDecorationsBought;

        playerStuffContainer = GameObject.Find("playerInventoryContainer");
        topContainer = GameObject.Find("topContainer");

        //Set up the player inventory
        FillPlayerInventoryPanel();

        //Figure out how many flowers to put on each page
        int gridWidth = (int)Mathf.Floor(Screen.width / (slotSize * 2));
        int gridHeight = (int)Mathf.Floor(Screen.height / (slotSize*2));
        //Subtract one because one slot is always needed as an empty one
        flowersPerPage = (gridHeight * gridWidth) - 1;

        GetComponent<Canvas>().worldCamera = GameObject.Find("UICamera").GetComponent<Camera>();
        GetComponent<Canvas>().planeDistance = 19f;
        SetFlowersTab();
        setPageCountText();
        updateMoneyText();

        currentQuestID = otherCanvas.GetComponent<UIControllerScript>().currentQuestId;

        //Only de-activate the other canvas at the end of the start
        otherCanvas.SetActive(false);
    }

    private void setupDecorationTabThings()
    {
        //Set up the IDs for the decoration tabs
        //[decoration tab id, slot id]
        decorationTabIDs = new int[4, 4];
        decorationValue = new Dictionary<uint, int>();

        //Japanese decorations
        decorationTabIDs[0, 0] = 0;
        decorationValue.Add(0, 25);
        decorationTabIDs[0, 1] = 1;
        decorationValue.Add(1, 35);
        decorationTabIDs[0, 2] = 2;
        decorationValue.Add(2, 50);
        decorationTabIDs[0, 3] = 3;
        decorationValue.Add(3, 100);

        //Urban decorations
        decorationTabIDs[1, 0] = 10;
        decorationValue.Add(10, 25);
        decorationTabIDs[1, 1] = 11;
        decorationValue.Add(11, 35);
        decorationTabIDs[1, 2] = 12;
        decorationValue.Add(12, 50);
        decorationTabIDs[1, 3] = 13;
        decorationValue.Add(13, 100);

        //Suburban decorations
        decorationTabIDs[2, 0] = 20;
        decorationValue.Add(20, 25);
        decorationTabIDs[2, 1] = 21;
        decorationValue.Add(21, 35);
        decorationTabIDs[2, 2] = 22;
        decorationValue.Add(22, 50);
        decorationTabIDs[2, 3] = 23;
        decorationValue.Add(23, 100);

        //Misc decorations
        decorationTabIDs[3, 0] = 100; //grass
        decorationValue.Add(100, 200); //grass price

        decorationTabIDs[3, 1] = 101; //super mega ultra pillar
        decorationValue.Add(101, 10000); //smup price

        decorationTabIDs[3, 2] = 102; //flag
        decorationValue.Add(102, 3000); //flag price

        decorationTabIDs[3, 3] = 103; //windmill
        decorationValue.Add(103, 1000); //windmill price
    }

    private void Update()
    {
        //Need to wait one frame for the layout to get set up
        //before accessing the correct sizes
        if (updateThisFrame)
        {
            performLayoutUpdate();
        }

        //Check for escape key press
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E))
        {
            CloseMenu();
        }

        updateMouseControls();
    }

    public void setPanelsActive(bool isActive)
    {
        playerStuffContainer.SetActive(isActive);
        topContainer.SetActive(isActive);

        if (currentTab == TabType.Flowers)
        {
            SetFlowersTab();
            currentFlowerPage = 0;
        }
    }

    public void hidePanelsForSearch()
    {
        playerStuffContainer.SetActive(false);
        topContainer.SetActive(false);
        currentFlowerPage = 0;
    }

    private void FillPlayerInventoryPanel()
    {
        //Get the player's inventory
        UIControllerScript uic = otherCanvas.GetComponent<UIControllerScript>();
        List<Placeable> inventoryData = uic.getInventoryData();
        List<int> inventoryCount = uic.getInventoryCount();

        //Get the panel to add to
        RectTransform pip = GameObject.Find("playerInventoryPanel").GetComponent<RectTransform>();

        for(int ii=0; ii < fpc.inventorySize; ii++)
        {
            //Add item to player inventory rect
            //Build the slot regardless of whether or not it will be empty
            GameObject slotObject = Instantiate(playerInventorySlotPrefab, pip);
            MenuSlot slot = slotObject.GetComponent<MenuSlot>();
            slot.slotIndex = ii;

            //Add an item to the slot if it shouldn't be empty
            if (inventoryData[ii] != null && inventoryCount[ii] > 0)
            {
                //Add the slot item to the slot
                GameObject itemObject = Instantiate(slotItemPrefab, slot.GetComponent<RectTransform>());
                MenuSlotItem item = itemObject.GetComponent<MenuSlotItem>();
                item.setID(inventoryData[ii].id, inventoryData[ii].isFlower);
                item.setCount(inventoryCount[ii]);

                slot.GetComponent<MenuSlot>().setItem(item);
            }
        }
    }

    /// <summary>
    /// Tell the world manager to close the menu and go back to the home screen
    /// </summary>
    public void CloseMenu()
    {
        otherCanvas.SetActive(true);

        //ALter the player's home inventory
        PlayerInventorySync();

        //Get rid of the home base menu
        WorldManager.instance.DeactivateHomeBaseMenu();
    }

    /// <summary>
    /// Syncs the player inventory with the flowers in the player inventory home base panel
    /// </summary>
    private void PlayerInventorySync()
    {
        FlowerProperties fp = GameObject.Find("World").GetComponent<FlowerProperties>();
        UIControllerScript uic = otherCanvas.GetComponent<UIControllerScript>();

        //Now fill the other canvas and the UI inventory with the right items
        //Loop through the entire inventory
        for (int ii = 0; ii < fpc.inventorySize; ii++)
        {
            MenuSlot hInventoryItem = playerInventoryContainer.GetChild(ii).GetComponent<MenuSlot>();
            Placeable slotPlaceable = uic.getInventoryDataAtSlot(ii);
            uint currentSlotID = 0;
            bool currentSlotIsFlower = false;
            if (slotPlaceable != null)
            {
                currentSlotID = slotPlaceable.id;
                currentSlotIsFlower = slotPlaceable.isFlower;
            }

            //If there's an item in the home base player inventory
            if (hInventoryItem.getItem() != null)
            {
                uic.inventoryClearSlot(ii);

                GameObject newObj;
                //Create a placeable based on the slot's id
                if (hInventoryItem.getItem().isFlower)
                {
                    //Make a flower
                    newObj = Instantiate(Resources.Load("Prefabs/FlowerPrefab", typeof(GameObject)) as GameObject);
                    FlowerObj p = newObj.GetComponent<FlowerObj>();
                    p.id = hInventoryItem.getItem().id;

                    int[] petal = new int[2] { fp.getPetalIndexFromIndex(p.id), fp.getPetalColorIndexFromIndex(p.id)};
                    int[] pistil = new int[2] { fp.getPistilIndexFromIndex(p.id), fp.getPistilColorIndexFromIndex(p.id)};
                    int[] leaf = new int[2] { fp.getLeafIndexFromIndex(p.id), fp.getLeafColorIndexFromIndex(p.id)};
                    int[] stem = new int[2] { fp.getStemIndexFromIndex(p.id), fp.getStemColorIndexFromIndex(p.id)};
                    int[] position = new int[2] { -1, -1};

                    p.init(petal, stem, pistil, leaf, position, transform.root);
                    p.alive = true;

                    uic.setSlot(ii, p);
                    uic.setSlotCount(ii, hInventoryItem.getItem().count);
                    Destroy(newObj);
                }
                else
                {
                    //Make a decoration
                    newObj = Instantiate(fp.getDecorationObject(hInventoryItem.getItem().id));
                    Placeable p = newObj.GetComponent<Placeable>();
                    p.id = hInventoryItem.getItem().id;
                    p.isFlower = hInventoryItem.getItem().isFlower;
                    p.flowerGridPos = new int[2] { -1, -1 };
                    uic.setSlot(ii, p);
                    uic.setSlotCount(ii, hInventoryItem.getItem().count);
                    Destroy(newObj);
                }
            }
            else //If there's not an item in the home base player inventory slot
            {
                //Check to see if the player has an item in the inventory slot
                if (uic.getInventoryDataAtSlot(ii) != null)
                {
                    //The player had an item in the slot but moved it to the home base
                    //So clear the slot
                    uic.inventoryClearSlot(ii);
                }
            }
        }
    }

    //Go to the next page of flowers
    public void FlowerPageNext()
    {
        dropItem();

        //If there are enough flowers to fit on more than one page
        int temp = currentFlowerPage;
        currentFlowerPage += 1;
        if (currentFlowerPage > Mathf.FloorToInt(displayInventory.Count / (flowersPerPage)))
        {
            currentFlowerPage = 0;
        }
        if (temp != currentFlowerPage)
        {
            ClearTabItems();
            //Reset the tab
            SetFlowersTab();
            //Set the text for the page number
            setPageCountText();
        }
    }

    //Go to the next page of flowers
    public void FlowerPagePrevious()
    {
        dropItem();
        //If there are enough flowers to fit on more than one page
        int temp = currentFlowerPage;
        currentFlowerPage -= 1;
        if (currentFlowerPage < 0)
        {
            currentFlowerPage = Mathf.FloorToInt(displayInventory.Count / (flowersPerPage));
        }
        if (temp != currentFlowerPage)
        {
            ClearTabItems();
            //Reset the tab
            SetFlowersTab();
            //Set the text for the page number
            setPageCountText();
        }
    }

    public void setPageCountText()
    {
        if (currentTab == TabType.Flowers)
        {
            if (displayInventory != null)
            {
                string countText = string.Format("({0}/{1})", currentFlowerPage + 1, Mathf.FloorToInt(displayInventory.Count / (flowersPerPage)) + 1);
                GameObject.Find("pageCountText").GetComponent<Text>().text = countText;
            }
            else
            {
                GameObject.Find("pageCountText").GetComponent<Text>().text = "(1/1)";
            }
        }
    }

    public void ClearTabItems()
    {
        if (currentActionObject != null)
        {
            GameObject.DestroyImmediate(currentActionObject);
        }
        if (currentPageObject != null)
        {
            GameObject.DestroyImmediate(currentPageObject);
        }
        GetComponent<RectTransform>().ForceUpdateRectTransforms();
    }

    public void updateFlowerDisplaySlot(MenuSlotItem item)
    {
        if (currentTab == TabType.Flowers || currentTab == TabType.Decorations)
        {
            GameObject sellButton = GameObject.Find("sellButton");
            
            //Remove all the listeners because they ended up stacking on top of each other every time the display slot was updated
            sellButton.GetComponent<UnityEngine.UI.Button>().onClick.RemoveAllListeners();

            if (item != null)
            {
                if (item.isFlower)
                {
                    uint sellID = item.id;
                    int sellCount = item.count;
                    int sellPrice = HomeBaseController.instance.getFlowerValue(sellID);
                    sellButton.GetComponentInChildren<Text>().text = sellPrefix + ": " + sellPrice;
                    sellButton.GetComponent<UnityEngine.UI.Button>().interactable = true;
                    sellButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate { sellFlower(sellID, 1); });
                    updatePriceBreakdownButton(true);
                }
                else
                {

                    uint sellID = item.id;
                    int actuallySellable = 1;
                    try
                    {
                        if (homeDecorationsBought[sellID] < actuallySellable)
                        {
                            actuallySellable = homeDecorationsBought[sellID];
                        }
                    }
                    catch { actuallySellable = 0; }
                    int sellPrice = (int)(decorationValue[(uint)sellID] * 0.75f)*actuallySellable;
                    sellButton.GetComponentInChildren<Text>().text = sellPrefix + ": " + sellPrice;
                    sellButton.GetComponent<UnityEngine.UI.Button>().interactable = true;
                    sellButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate { sellDecoration(sellID, 1); });
                    updatePriceBreakdownButton(false,sellID,item.count);
                }
            }
            else
            {
                sellButton.GetComponentInChildren<Text>().text = sellPrefix;
                sellButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
                if (currentTab == TabType.Flowers)
                {
                    updatePriceBreakdownButton(true);
                }
                else if (currentTab == TabType.Decorations)
                {
                    updatePriceBreakdownButton(false);
                }
            }
        }
    }

    //Returns the first slot wit hthe gien item
    public MenuSlot findSlotWithItem(MenuSlotItem inItem)
    {
        //Grab all the slots
        MenuSlot[] slots = GameObject.FindObjectsOfType<MenuSlot>();

        //Loop through the slots
        foreach (MenuSlot s in slots)
        {
            //If the slot is in the home inventory or the display slot
            //(both slots hold items that are currently "in" the home inventory
            if (s.type == MenuSlot.SlotType.HomeInventory || s.type == MenuSlot.SlotType.DisplaySlot || s.type == MenuSlot.SlotType.DecorationSlot)
            {
                //If there's an item at the slot
                if (s.getItem() != null)
                {
                    //If it matches id and isFlower
                    if (s.getItem().id == inItem.id && s.getItem().isFlower == inItem.isFlower)
                    {
                        return s;
                    }
                }
            }
        }
        return null;
    }

    //Gathers all the items in the player inventory and stores them in the home inventory
    public void storeAllItems()
    {
        dropItem();
        //get the 0th home inventory slot
        //MenuSlot storeSlot = homeFlowerSlots[0].GetComponent<MenuSlot>();

        //Loop through the inventory
        for (int ii = 0; ii < fpc.inventorySize; ii += 1)
        {
            //Store the item if it's not null
            MenuSlot storeItemSlot = playerInventoryContainer.GetChild(ii).GetComponent<MenuSlot>();
            MenuSlotItem storeItem = storeItemSlot.getItem();
            if (storeItem != null)
            {
                storeItemSlot.storeItem(false);
            }
        }

        updateInventoryItemCount();

        //If the player is in the flower tab then refresh the tab
        if (currentTab == TabType.Flowers)
        {
            SetFlowersTab();
        }
    }

    public void setSearchParameters(uint flower, uint mask)
    {
        searchFlower = flower;
        searchMask = mask;
    }

    public void clearSearchParameters()
    {
        searchFlower = 0;
        searchMask = 0;
    }

    #region Changing Tabs
    public void SetFlowersTab()
    {
        dropItem();

        //Clear the slots from the list
        if (homeFlowerSlots != null)
        {
            homeFlowerSlots.Clear();
        }

        //Sets up the UI for the flowers
        //Clear out old action panel if it's coming from the upgraded

        if (currentActionObject != null)
        {
            GameObject.Destroy(currentActionObject);
            currentActionObject = null;
        }
        if (currentPageObject != null)
        {
            GameObject.Destroy(currentPageObject);
        }

        //Add the new objects
        currentActionObject = Instantiate(flowerActionPrefab, actionContainer);
        currentPageObject = Instantiate(flowerPagePrefab, pageContainer);

        GetComponent<RectTransform>().ForceUpdateRectTransforms();
        updateNextFrame = true;

        currentTab = TabType.Flowers;
    }

    public void dropItem()
    {
        if (grabbedSlot != null)
        {
            grabbedSlot.getItem().release();
            grabbedSlot.releaseItem(null);
            grabbedSlot = null;
        }
    }

    private void postSetFlowersTab()
    {
        //Fill in the display inventory
        if (searchMask != 0)
        {
            displayInventory = new Dictionary<uint, int>();

            for (int ii = 0; ii < homeFlowerInventory.Keys.Count; ii++)
            {
                if (shouldDisplayFlower(homeFlowerInventory.Keys.ElementAt(ii)))
                {
                    displayInventory.Add(homeFlowerInventory.Keys.ElementAt(ii), homeFlowerInventory.Values.ElementAt(ii));
                }
            }
        }
        else
        {
            displayInventory = homeFlowerInventory;
        }

        //Just add some slots and things
        GameObject flowerGridObject = GameObject.Find("flowerGrid");
        Transform grid = flowerGridObject.transform;

        //Figure out how many flowers to put on each page
        Canvas myCanvas = transform.GetComponentInParent<Canvas>();
        float rectWidth = grid.GetComponent<RectTransform>().rect.width * myCanvas.scaleFactor;
        float rectHeight = grid.GetComponent<RectTransform>().rect.height * myCanvas.scaleFactor;
        Debug.Log(rectWidth + "," + rectHeight + "  with a scale factor of " + myCanvas.scaleFactor);
        int gridWidth = (int)Mathf.Floor(rectWidth / slotSize);
        int gridHeight = (int)Mathf.Floor(rectHeight / slotSize);
        flowersPerPage = (gridHeight * gridWidth) - 1;

        //Add an empty slot
        //Fill the screen with flowers and slots
        int startPosition = flowersPerPage * currentFlowerPage;
        Debug.Log("start position is: " + startPosition);

        //Loop and fill the page with the right flowers
        for (int ii = startPosition; ii <= Mathf.Min(startPosition + flowersPerPage - 1, displayInventory.Count - 1); ii += 1)
        {
            uint flowerID = displayInventory.Keys.ElementAt(ii);
            int count = displayInventory.Values.ElementAt(ii);

            if (shouldDisplayFlower(flowerID))
            {
                GameObject slot = Instantiate(homeInventorySlotPrefab, grid);
                slot.GetComponent<MenuSlot>().slotIndex = ii;
                GameObject item = Instantiate(slotItemPrefab, slot.GetComponent<RectTransform>());
                MenuSlotItem itemObject = item.GetComponent<MenuSlotItem>();
                itemObject.setID(flowerID, true);
                itemObject.setCount(count);

                slot.GetComponent<MenuSlot>().setItem(itemObject);
                homeFlowerSlots.Add(slot);
            }
        }

        //If there are more flowersPerPage than flowers in the home inventory then...
        //fill in the rest of the screen with blank slots
        int ss = homeFlowerSlots.Count; //this is just for setting indexes
        while (homeFlowerSlots.Count <= flowersPerPage)
        {
            GameObject slot = Instantiate(homeInventorySlotPrefab, grid);
            slot.GetComponent<MenuSlot>().slotIndex = ss;
            homeFlowerSlots.Add(slot);
            ss += 1;
        }

        setPageCountText();
        updatePriceBreakdownButton(true);
        //Set up the sell display slot
        updateFlowerDisplaySlot(null);
    }

    //If there's a flower and mask to apply then see if the flower should be displayed
    private bool shouldDisplayFlower(uint id)
    {
        if (searchMask != 0)
        {
            return FlowerProperties.compareFlowers(id, searchFlower, searchMask);
        }
        else
        {
            return true;
        }
    }

    //Shifts all the flowers to the next slot in the list
    public void reSortFlowerList()
    {
        if (currentTab == TabType.Flowers)
        {
            int ii = 0;
            MenuSlotItem previousItem = null;
            while (ii < homeFlowerSlots.Count)
            {
                MenuSlot fromSlot = homeFlowerSlots[ii].GetComponent<MenuSlot>();

                MenuSlotItem tempItem = fromSlot.getItem();

                fromSlot.setItemNoSort(previousItem);
                previousItem = tempItem;

                //Break out of the lop
                if (previousItem == null)
                {
                    break;
                }
                ii += 1;
            }

            if (previousItem != null)
            {
                Destroy(previousItem.gameObject);
            }
        }
    }

    public void SetDecorationsTab()
    {
        dropItem();

        //Sets up the UI for the decorations
        //Clear out old objects
        if (currentActionObject != null)
        {
            GameObject.Destroy(currentActionObject);
            currentActionObject = null;
        }

        if (currentPageObject != null)
        {
            GameObject.Destroy(currentPageObject);
        }
        //Add the new objects
        currentActionObject = Instantiate(decorationActionPrefab, actionContainer);
        currentPageObject = Instantiate(decorationPagePrefab, pageContainer);
        updateNextFrame = true;
        currentTab = TabType.Decorations;
    }
    private void postSetDecorationsTab()
    {
        Transform grid = currentPageObject.transform.Find("decorationInventoryHolder");
        Canvas myCanvas = transform.GetComponentInParent<Canvas>();

        GetComponent<RectTransform>().ForceUpdateRectTransforms();

        float rectWidth = grid.GetComponent<RectTransform>().rect.width * myCanvas.scaleFactor * .5f;
        float rectHeight = grid.GetComponent<RectTransform>().rect.height * myCanvas.scaleFactor * .5f;
        rectHeight = Mathf.Min(rectWidth * .75f, rectHeight);

        List<GameObject> slotList = new List<GameObject>();
        for (int ii = 0; ii < 4; ii += 1)
        {
            uint itemID = (uint)decorationTabIDs[decorationTab, ii];

            //GetComponent<RectTransform>().ForceUpdateRectTransforms();
            GameObject seller = Instantiate(decorationPurchasePrefab, grid);

            GameObject slot = Instantiate(homeInventorySlotPrefab, seller.GetComponent<RectTransform>());
            slotList.Add(slot);

            Text priceText = seller.GetComponentInChildren<Text>();
            slot.transform.SetAsFirstSibling();
            slot.GetComponent<MenuSlot>().slotIndex = 0;
            slot.GetComponent<MenuSlot>().type = MenuSlot.SlotType.DecorationSlot;
            GameObject item = Instantiate(slotItemPrefab, slot.GetComponent<RectTransform>());
            MenuSlotItem itemObject = item.GetComponent<MenuSlotItem>();
            itemObject.setID(itemID, false);
            itemObject.setCount(getHomeInventoryCount(itemID, false));

            slot.GetComponent<MenuSlot>().setItemNoSort(itemObject);
            slot.GetComponent<MenuSlot>().getItem().updateModelScale(rectWidth * .5f, rectHeight);

            GameObject buyButtom = seller.transform.GetComponentInChildren<UnityEngine.UI.Button>().gameObject;

            priceText.text = "PRICE: " + decorationValue[(uint)decorationTabIDs[decorationTab, ii]];
            buyButtom.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate { purchaseDecoration( itemID, slot.GetComponent<MenuSlot>()); });
            buyButtom.GetComponentInChildren<Text>().text = "BUY";
        }

        GameObject priceBreakdownButton = GameObject.Find("priceBreakdownButton");
        priceBreakdownButton.GetComponentInChildren<Text>().text = sellAllPrefix;
        grid.GetComponent<GridLayoutGroup>().cellSize = new Vector2(rectWidth, rectHeight);

        //LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        GetComponent<RectTransform>().ForceUpdateRectTransforms();
        updatePriceBreakdownButton(false);
        //Set up the sell display slot
        updateFlowerDisplaySlot(null);
    }

    public void SetUpgradesTab()
    {
        dropItem();

        currentTab = TabType.Upgrades;

        //Sets up the UI for the upgrades
        //Clear out old objects
        if (currentActionObject != null)
        {
            GameObject.Destroy(currentActionObject);
            currentActionObject = null;
        }
        if (currentPageObject != null)
        {
            GameObject.Destroy(currentPageObject);
        }

        //Add the new objects
        currentActionObject = Instantiate(upgradeActionPrefab, actionContainer);
        currentPageObject = Instantiate(upgradePagePrefab, pageContainer);
        updateNextFrame = true;
        GetComponent<RectTransform>().ForceUpdateRectTransforms();

    }
    private void postSetUpgradesTab()
    {
        //Fill in the jetpack upgrade levels
        int jpl = WarpDriveController.instance.getJetpackLevel();
        Transform upgradeSlotHolder = GameObject.Find("jetpackUpgradePanel").transform;
        int holderCount = upgradeSlotHolder.childCount-2;
        for (int i = 0; i < Mathf.Min(jpl,holderCount); i++)
        {
            upgradeSlotHolder.GetChild(i + 2).GetComponent<UnityEngine.UI.Image>().sprite = upgradeFilledSprite;
            upgradeSlotHolder.GetChild(i + 2).GetComponent<UnityEngine.UI.Image>().color = Color.green;
        }
        setJetpackUpgradeButtonText(jpl);
        upgradeSlotHolder.GetComponent<RectTransform>().ForceUpdateRectTransforms();
    }

    private void setJetpackUpgradeButtonText(int inLevel)
    {
        if (inLevel > 15)
        {
            inLevel = 15;
        }

        string buttonText;
        switch(inLevel)
        {
            case 0: buttonText = "Purchase Jetpack: " + jetpackPurchasePrice; break;
            case 15: buttonText = "Jetpack Completely Upgraded"; break;
            default: buttonText = "Upgrade Jetpack Level:" + (jetpackUpgradePerLevel * inLevel); break;
        }

        if (inLevel == 1 && currentQuestID <= 18)
        {
            buttonText = "Complete quest to upgrade jetpack more";
        }

        GameObject.Find("purchaseUpgradeText").GetComponent<Text>().text = buttonText;
        GameObject.Find("purchaseUpgradeText").GetComponent<RectTransform>().ForceUpdateRectTransforms();
    }

    public void purchaseJetpackUpgrade()
    {
        //Get the level
        int jpl = WarpDriveController.instance.getJetpackLevel();

        //Figure out the price
        int price;
        switch (jpl)
        {
            case 0: price = jetpackPurchasePrice; break;
            case 1: price = warpDrivePurchasePrice; break;
            case 16: price = -1; break; 
            default: price = (jetpackUpgradePerLevel * (jpl)); break;
        }

        Debug.Log("current quest ID:" + currentQuestID);
        
        //Cap the level until the player visits the hot planet
        if (jpl == 1 && currentQuestID <= 17)
        {
            price = -1;
        }

        //Only buy it if the player has enough money (negative price values mean the upgrade is maxed out)
        if (fpc.money >= price && price > 0)
        {
            WarpDriveController.instance.setJetpackLevel(jpl + 1);
            fpc.money -= price;
            //Refresh the tab if the player does purchase
            SetUpgradesTab();
            updateMoneyText();
        }
    }

    public void setDecorationTabID(int id)
    {
        dropItem();
        decorationTab = id;
        SetDecorationsTab();
    }

    private void updateMoneyText()
    {
        GameObject.Find("moneyText").GetComponent<Text>().text = "MONEY: " + fpc.money;
    }
    #endregion

    #region Mouse Controls
    /*
     * Mouse controls pseudocode:
     * slotIsClicked - whether or not a slot is currently being clicked
     * clickHoldTimer - how long the user has been holding down the click on a slot
     * clickedOn - the GameObject that is currently being clicked on
     * grabbedSlot - the MenuSlot that is grabbed and currently following the mouse
     * 
     * updateMouseControls() is called every frame and will increment the clickHoldTimer
     * ClickSlot(GameObject) will either pick up the item in a slot or set it down
     * OnPointerDown(PointerEventData) will be called the first frame when the user clicks
     * OnPointerUp(PointerEventData) will be called when the user lets off of the mouse button
     * 
     * Flow of control
     * User clicks (User holds down click || User clicks and releases)
     *      -User holds down click
     *          -When clickHoldTimer passes clickHoldThreshold
     *          -Pick up item by calling ClickSlot()
     *          -When user releases
     *          -Release item by calling ClickSlot()
     *      -User clicks and releases (Item is grabbed || No item grabbed)
     *          -Item is grabbed
     *              -Release item by calling ClickSlot()
     *          -No item grabbed
     *              -Pick up item by calling ClickSlot()
     * User is not clicking
     *      -If an item is grabbed it will follow the mouse
     */


    private void updateMouseControls()
    {
        if (slotIsClicked)
        {
            if (clickHoldTimer > -.99f)
            {
                clickHoldTimer += Time.deltaTime;
            }

            if (clickHoldTimer > clickHoldThreshold)
            {
                ClickSlot(clickedOn);
                clickHoldTimer = -2f;
            }
        }
    }

    //When the mouse is being held down
    public void ClickSlot(GameObject slot)
    {
        clickedOn = slot;
        //Debug.Log("Home base clicked: " + data.pointerCurrentRaycast.gameObject.name);
        MenuSlot slotClicked = clickedOn.GetComponent<MenuSlot>();
        if (grabbedSlot == null)
        {
            //Grab the selected item if nothing is currently being selected
            if (slotClicked != null)
            {
                //If there's something in the slot to grab
                if (slotClicked.getItem() != null)
                {
                    if (slotClicked.getItem().count > 0)
                    {
                        //If it's a slot with an item then grab it
                        slotClicked.grabItem();
                        grabbedSlot = slotClicked;
                        grabbedSlot.getItem().grab();
                    }
                }
            }
        }
        else
        {
            if (slotClicked.gameObject.name == "sellFlowerSlot" && grabbedSlot.getItem().isFlower != (currentTab == TabType.Flowers))
            { }
            else
            {
                grabbedSlot.getItem().release();
                grabbedSlot.releaseItem(slotClicked);
                grabbedSlot = null;
            }
        }
    }


    public void OnPointerDown(PointerEventData data)
    {
        clickedOn = data.pointerCurrentRaycast.gameObject;
        if (clickedOn.GetComponent<MenuSlot>() != null)
        {
            slotIsClicked = true;
            clickHoldTimer = 0f;
        }
    }

    public void OnPointerUp(PointerEventData data)
    {
        if (slotIsClicked)
        {
            //If the click hold timer has not passed the threshold and has not been reset to -2
            if (clickHoldTimer < clickHoldThreshold && clickHoldTimer > -.001f)
            {
                ClickSlot(clickedOn);
                slotIsClicked = false;
            }
            else
            {
                clickedOn = data.pointerCurrentRaycast.gameObject;
                if (clickedOn.GetComponent<MenuSlot>() != null)
                {
                    ClickSlot(clickedOn);
                }
            }
        }
    }

    #endregion

    #region Inventory Management

    /// <summary>
    /// Adds the given number of items to the home inventory
    /// </summary>
    /// <param name="newKey"></param>
    /// <param name="newCount">How many to add</param>
    /// <param name="isFlower"></param>
    public void addHomeInventoryItem(uint newKey, int newCount, bool isFlower)
    {
        if (isFlower)
        {
            if (!homeFlowerInventory.ContainsKey(newKey))
            {
                homeFlowerInventory.Add(newKey, newCount);
                setPageCountText();
            }
            else
            {
                homeFlowerInventory[newKey] += newCount;
            }
        }
        else
        {
            if (!homeDecorationInventory.ContainsKey(newKey))
            {
                homeDecorationInventory.Add(newKey, newCount);
            }
            else
            {
                homeDecorationInventory[newKey] += newCount;
            }
        }

        //If the count is less than or equal to zero then remove the item from the inventory
        if (getHomeInventoryCount(newKey,isFlower) <= 0)
        {
            removeHomeInventoryItem(newKey, isFlower);
        }
    }

    public void removeHomeInventoryItem(uint removeKey, bool isFlower)
    {
        if (isFlower)
        {
            if (homeFlowerInventory.ContainsKey(removeKey))
            {
                homeFlowerInventory.Remove(removeKey);
            }
            setPageCountText();
        }
        else
        {
            if (homeDecorationInventory.ContainsKey(removeKey))
            {
                homeDecorationInventory[removeKey] = 0;
            }
        }
    }

    public void setHomeInventoryCount(uint key, int newCount, bool isFlower)
    {
        if (isFlower)
        {
            if (homeFlowerInventory.ContainsKey(key))
            {
                homeFlowerInventory[key] = newCount;
            }
            else
            {
                homeFlowerInventory.Add(key, newCount);
            }
        }
        else
        {
            if (homeDecorationInventory.ContainsKey(key))
            {
                homeDecorationInventory[key] = newCount;
            }
            else
            {
                homeDecorationInventory.Add(key, newCount);
            }
        }
        //If the count is less than or equal to zero then remove the item from the inventory
        if (getHomeInventoryCount(key, isFlower) <= 0)
        {
            removeHomeInventoryItem(key, isFlower);
        }
    }

    public int getHomeInventoryCount(uint key, bool isFlower)
    {
        if (isFlower)
        {
            if (homeFlowerInventory.ContainsKey(key))
            {
                return homeFlowerInventory[key];
            }
        }
        else
        {
            if (homeDecorationInventory.ContainsKey(key))
            {
                return homeDecorationInventory[key];
            }
        }
        return 0;
    }

    public void updateInventoryItemCount()
    {
        //Grab all the slots
        MenuSlot[] slots = GameObject.FindObjectsOfType<MenuSlot>();

        foreach(MenuSlot s in slots)
        {
            if (s.type != MenuSlot.SlotType.PlayerInventory)
            {
                if (s.getItem()!=null)
                {
                    s.setCount(getHomeInventoryCount(s.getItem().id, s.holdsFlower()));
                }
            }
        }
    }

    #endregion

    #region Outside actions


    //Get the flower's price
    private int getFlowerPrice(uint flowerID)
    {
        return HomeBaseController.instance.getFlowerValue(flowerID);
    }

    //Sell the flower the given number of times
    private void sellFlower(uint flowerID, int sellAmount)
    {
        if (grabbedSlot == null)
        {
            MenuSlot sellSlot = GameObject.Find("sellFlowerSlot").GetComponent<MenuSlot>();
            //this was for multiple sells since processflowersell allways sells one
            //fpc.money += HomeBaseController.instance.getFlowerValue(flowerID) * (sellAmount-1);
            HomeBaseController.instance.processFlowerSell(flowerID);

            //Reduce flower count by "adding" a negative value
            addHomeInventoryItem(flowerID, -sellAmount, true);
            sellSlot.setCount(sellSlot.getCount() - sellAmount);
            updateMoneyText();
            updateFlowerDisplaySlot(sellSlot.getItem());
        }
    }

    //Purchase an upgrade
    public void purchaseUpgrade(UpgradeType type)
    {
        if (grabbedSlot == null)
        {
            if (type == UpgradeType.Jetpack)
            {
                purchaseJetpackUpgrade();
            }
        }
    }

    private void purchaseDecoration(uint id, MenuSlot item)
    {
        if (grabbedSlot == null)
        {
            if (fpc.money >= decorationValue[(uint)id])
            {
                if (!homeDecorationsBought.ContainsKey(id))
                {
                    homeDecorationsBought.Add(id, 0);
                }
                homeDecorationsBought[id] += 1;
                fpc.money -= decorationValue[(uint)id];
                updateMoneyText();
                bool pushedToInventory = false;
                Transform playerInventoryPanel = GameObject.Find("playerInventoryPanel").transform;
                for (int i = 0; i < 10 && pushedToInventory==false; i++)
                {
                    if (playerInventoryPanel.GetChild(i).childCount != 0)
                    {
                        MenuSlotItem msi = playerInventoryPanel.GetChild(i).GetChild(0).GetComponent<MenuSlotItem>();
                        if (msi.isFlower == false && msi.id == id)
                        {
                            msi.setCount(msi.count + 1);
                            pushedToInventory = true;
                        }
                    }
                }
                if (!pushedToInventory)
                {
                    item.setCount(item.getCount() + 1);
                    addHomeInventoryItem(id, 1, false);
                }
                for (int i = 0; i < 10 && pushedToInventory == false; i++)
                {
                    if (playerInventoryPanel.GetChild(i).childCount == 0)
                    {
                        item.releaseItem(playerInventoryPanel.GetChild(i).GetComponent<MenuSlot>());
                        pushedToInventory = true;
                    }
                }
            }
        }
    }

    private void sellDecoration(uint id, int count)
    {
        if (grabbedSlot == null)
        {
            int toSell = count;
            try
            {
                if (homeDecorationsBought[id] < toSell)
                {
                    toSell = homeDecorationsBought[id];
                }
                homeDecorationsBought[id] -= toSell;
                fpc.money += (int)(toSell * decorationValue[(uint)id] * 0.75f);
            }
            catch { }
            //todo decrease item's count by count

            MenuSlot sellSlot = GameObject.Find("sellFlowerSlot").GetComponent<MenuSlot>();

            //Reduce flower count by "adding" a negative value
            addHomeInventoryItem(id, -count, false);
            sellSlot.setCount(sellSlot.getCount() - count);//this is neg for some reason
            updateMoneyText();


            updateFlowerDisplaySlot(sellSlot.getItem());
        }
    }
    
    private void updatePriceBreakdownButton(bool showPrinceBreakdown, uint toSell=0, int sellCount=0)
    {
        GameObject priceBreakdownButton = GameObject.Find("priceBreakdownButton");
        priceBreakdownButton.GetComponent<UnityEngine.UI.Button>().onClick.RemoveAllListeners();
        if (showPrinceBreakdown)
        {
            priceBreakdownButton.GetComponentInChildren<Text>().text = "PRICE BREAKDOWN";
            priceBreakdownButton.GetComponent<UnityEngine.UI.Button>().interactable = true;
            priceBreakdownButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate { dropItem(); flowerValueCanvas.SetActive(true); });
        }
        else
        {
            priceBreakdownButton.GetComponentInChildren<Text>().text = sellAllPrefix;
            priceBreakdownButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
            if (sellCount!=0)
            {
                int actuallySellable = sellCount;
                try
                {
                    if (homeDecorationsBought[toSell] < actuallySellable)
                    {
                        actuallySellable = homeDecorationsBought[toSell];
                    }
                }
                catch { actuallySellable = 0; }

                priceBreakdownButton.GetComponent<UnityEngine.UI.Button>().interactable = true;
                priceBreakdownButton.GetComponentInChildren<Text>().text += ": " + (int)(actuallySellable * decorationValue[(uint)toSell] * 0.75f);
                priceBreakdownButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate { sellDecoration(toSell, sellCount); });
            }
        }
    }

    #endregion

    #region Wait for update
    private void LateUpdate()
    {
        if (updateNextFrame)
        {
            updateThisFrame = true;
            updateNextFrame = false;
        }
    }

    private void performLayoutUpdate()
    {
        updateThisFrame = false;
        updateNextFrame = false;
        switch(currentTab)
        {
            case TabType.Flowers: postSetFlowersTab(); break;
            case TabType.Decorations: postSetDecorationsTab(); break;
            case TabType.Upgrades: postSetUpgradesTab(); break;
        }
    }
    #endregion
}
