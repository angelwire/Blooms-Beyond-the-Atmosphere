//created: 9-30-19
//updated: 11-2-19
//version: .03
//author Vance Howald, William Jones
//Flower placememnt script and inventory
//TODO remove renaming flowers
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;

public class FlowerPlayerControler : MonoBehaviour
{
    public LayerMask placeableLayer; //flower colliders
    public LayerMask world; //world colliders
    public Canvas UIController;
    public FlowerObj[] flowerObjsInvetory = new FlowerObj[10]; //holder for flower data
    public int inventorySize = 10; //The size of the player's inventory (will be used to upgrade later in the game)
    //The index that the character is selecting (with the mouse scroll)
    public int selectedIndex = 0;
    public GameObject gridSelector;
    public GameObject flowerSelector;
    public Vector3 selectorBoxPos;
    public Vector3 selectorBoxDimensions;
    UIControllerScript uIControllerS;
    string[] regexFilters = {
        "...1....", //For case 7
        ".1......", //For case 8
        ".1.1....", //For case 10
        "....F...", //For case 11
        ".1.1F...", //For case 12
        ".......1", //For case 13
        "......6.", //For case 14
        ".1.1F.61"  //For case 15
    };
    Regex[] regexList;
    char petalTypeForQuest;
    List<uint> flowerMemory = new List<uint>();
    bool[] partFinder = { false, false, false, false };
    bool isFrozen = false;
    //This is whether or not the player is viewing the menu at the home base
    //Set by the HomeBaseScreenController and unset by HomeBaseUIController
    public bool isViewingHomeBase = false;
    GameObject hoveredPlaceable;
    public int money;
    Camera mainCamera;
    bool isOnOnlinePlanet;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        uIControllerS = UIController.GetComponent<UIControllerScript>();
        regexList = new Regex[regexFilters.Length];
        for (int i = 0; i < regexFilters.Length; i++)
        {
            regexList[i] = new Regex(regexFilters[i]);
        }
        isOnOnlinePlanet = SceneManager.GetActiveScene() == SceneManager.GetSceneByName("onlinePlanet");
    }

    // Update is called once per frame
    void Update()
    {
        isFrozen = uIControllerS.isViewingFlower || uIControllerS.isPaused || isViewingHomeBase || isOnOnlinePlanet;

        if (Input.GetKeyDown(KeyCode.Mouse0) && !isFrozen)//left click
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            if (Physics.Raycast(ray, out hit, 4.0f, placeableLayer, QueryTriggerInteraction.Collide))//check for flower
            {
                //string[] flowerGridPos = hit.transform.gameObject.name.Split(' ');//this should be changed

                selectorBoxDimensions = hit.collider.bounds.size;
                selectorBoxPos = hit.collider.bounds.center + hit.transform.position;

                Placeable currentObj = hit.transform.gameObject.GetComponent<Placeable>();
                if (currentObj != null)
                {
                    //This is stuff to test William's inventory UI
                    /*uint hitIndex = WorldManager.instance.flowerGrid[int.Parse(flowerGridPos[0]), int.Parse(flowerGridPos[1])].getFlowerIndex();
                    if ((long)hitIndex < 0)
                    {
                        print("WHOAH: " + (long)hitIndex);
                    }*/
                    bool canPickUp = uIControllerS.inventoryHasRoom(currentObj);
                    if (canPickUp)
                    {
                        Debug.Log("picking up: " + hit.transform.gameObject.name);
                        uint id = currentObj.id;
                        bool isFlower = currentObj.isFlower;
                        if (isFlower)
                        {
                            HomeInventoryData.instance.addToKnownFlowers(id.ToString("X8"));
                            GameJoltAPIHelper.IncDataStoreKey(GameJoltAPIHelper.flowers_picked);
                        }
                        currentObj.clearPos();
                        uIControllerS.inventoryPushItem(currentObj);
                        Destroy(hit.transform.gameObject);
                        flowerMemory.Remove(id);
                        int[] flowerGridLoc = WorldManager.instance.worldPointToFlowerGrid(hit.point);//find what grid the point is in
                        questStuff(id, isFlower, flowerGridLoc[0], flowerGridLoc[1], false);

                        if (id == 102 && !isFlower && WarpDriveController.instance.currentPlanetID != Galaxy.HOME_PLANET_ID)
                        {
                            //Removing flag from alien planet
                            WarpDriveController.instance.GetCurrentPlanet().setName("");
                        }

                    }
                    else
                    {
                        uIControllerS.flashInventorySlots();
                    }
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Mouse1) && !isFrozen) //right click
        {
            //New
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));

            if (Physics.Raycast(ray, out hit, 4.0f, world, QueryTriggerInteraction.Collide))
            {
                int[] flowerGridLoc = WorldManager.instance.worldPointToFlowerGrid(hit.point);//find what grid the point is in
                if (uIControllerS.inventoryHasItemToPlace() && uIControllerS.inventoryPeekItem().canBePlaced(hit))//do we have an item at the selected slot, is there nothing there
                {
                    if (!uIControllerS.inventoryPeekItem().isFlower && WarpDriveController.instance.currentPlanetID != Galaxy.HOME_PLANET_ID && uIControllerS.inventoryPeekItem().id != 102)//if it's a decoration and on alien planet and it's not a flag
                    {
                        uIControllerS.flashInventorySlots();
                    } 
                    else
                    {
                        //Peek the flower, clone it, and then pop it
                        //Because popping it will destroy the object and it can't be cloned after it's destroyed
                        Placeable itemToPlace = uIControllerS.inventoryPeekItem();
                        WorldManager.instance.removeFromFlowerList(flowerGridLoc[0], flowerGridLoc[1]);
                        GameObject placedFlower = itemToPlace.cloneObj(uIControllerS.inventoryPeekItem().gameObject, itemToPlace, hit);
                        if (itemToPlace.isFlower)
                        {
                            //WorldManager.instance.RecalcAfterAdd(flowerGridLoc[0], flowerGridLoc[1]);//tell flower grid there's a new flower
                            //Rotate the flower to match the player's rotation
                            placedFlower.transform.localRotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y - 90f, 0);

                            questStuff(itemToPlace.id, itemToPlace.isFlower, flowerGridLoc[0], flowerGridLoc[1], true);
                            GameJoltAPIHelper.IncDataStoreKey(GameJoltAPIHelper.flowers_planted);
                        }

                        if (itemToPlace.id == 102 && !itemToPlace.isFlower && WarpDriveController.instance.currentPlanetID != Galaxy.HOME_PLANET_ID)
                        {
                            //Planting flag on alien planet!
                            uIControllerS.namePlanet();
                        }
                        uIControllerS.inventoryPopItem(); //Pulls the item out of the inventory at the selected slot
                    }
                }
            }
            //New

        }
        //Control throwing away flowers with the "Q" key
        if (Input.GetKeyDown(KeyCode.Q) && !isFrozen && uIControllerS.inventoryPeekItem() != null)
        {
            if (WarpDriveController.instance.currentPlanetID == Galaxy.HOME_PLANET_ID)
            {
                HomeInventoryData.instance.addToInventory(uIControllerS.inventoryPeekItem());
                uIControllerS.inventoryPopItem();
                if (uIControllerS.currentQuestId == 1 && uIControllerS.isEmpty())
                {
                    uIControllerS.completeQuest();
                }
            }
            else
            {
                uIControllerS.flashInventorySlots();
            }
        }
        //Control mouse scrolling to select the index
        if (Input.GetAxis("Mouse ScrollWheel") < 0f) //Scroll down
        {
            selectedIndex += 1;
            if (selectedIndex >= inventorySize)
            {
                selectedIndex -= inventorySize;
            }
            //Tell the UIController to update the selected index
            uIControllerS.updateSelectedSlot(selectedIndex);
        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0f) //Scroll up
        {
            selectedIndex -= 1;
            if (selectedIndex < 0)
            {
                selectedIndex += inventorySize;
            }
            //Tell the UIController to update the selected index
            uIControllerS.updateSelectedSlot(selectedIndex);
        }
        //Control keyboard pressing to select the index
        //Loop through 0-9
        for (int ii = 0; ii < 10; ii += 1)
        {
            //If the number is pressed
            if (Input.GetKey("" + ii))
            {
                //Convert 0 to 9 so 0 will select the last slot
                //This is to make the UI match the keyboard
                selectedIndex = ii - 1;
                if (selectedIndex < 0)
                {
                    selectedIndex += inventorySize;
                }
                //Tell the UIController to update the selected index
                uIControllerS.updateSelectedSlot(selectedIndex);
            }
        }

        RaycastHit hitGrid;
        Ray rayGrid = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        if (Physics.Raycast(rayGrid, out hitGrid, 4.0f, world, QueryTriggerInteraction.Collide))
        {
            int[] gridPos = WorldManager.instance.worldPointToFlowerGrid(hitGrid.point);
            if (WorldManager.instance.placableGrid[gridPos[0], gridPos[1]] == null || !(WorldManager.instance.placableGrid[gridPos[0], gridPos[1]] is Unplacable))
            {
                //Check to see if it's a hover block object before activating the selector 
                if (!gridSelector.activeSelf)
                {
                    gridSelector.SetActive(true);
                }
                gridSelector.transform.position = WorldManager.instance.worldPointToGridOrgin(hitGrid.point) + new Vector3(0, hitGrid.point.y, 0);
            }
            else if (gridSelector.activeSelf)
            {
                gridSelector.SetActive(false);
            }
        }
        else if (gridSelector.activeSelf)
        {
            gridSelector.SetActive(false);
        }
        Ray rayFlower = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        if (Physics.Raycast(rayFlower, out hitGrid, 4.0f, placeableLayer, QueryTriggerInteraction.Collide))//check for flower
        {
            //If it's not the same hovered placeable as last frame
            if (hoveredPlaceable != hitGrid.transform.gameObject)
            {
                //Tell the hovered placeable that it is no longer hovered
                if (hoveredPlaceable != null)
                {
                    if (hoveredPlaceable.GetComponent<Placeable>() != null)
                    {
                        hoveredPlaceable.GetComponent<Placeable>().setHover(false);
                    }
                    else if (hoveredPlaceable.GetComponent<HomeBaseScreenController>() != null)
                    {
                        hoveredPlaceable.GetComponent<HomeBaseScreenController>().setHover(false);
                    }
                }

                if (hitGrid.transform.gameObject.layer != LayerMask.NameToLayer("HoverBlock"))
                {
                    hoveredPlaceable = hitGrid.transform.gameObject;
                    if (!flowerSelector.activeSelf)
                    {
                        flowerSelector.SetActive(true);
                    }

                    //flowerSelector.transform.position = new Vector3(hitGrid.transform.position.x, hitGrid.transform.position.y + (hitGrid.collider.bounds.size.y / 2), hitGrid.transform.position.z);
                    //flowerSelector.transform.localScale = hitGrid.collider.bounds.size;
                    flowerSelector.GetComponentInChildren<FlowerSelectorController>().fitToGameObject(hitGrid.transform.gameObject);

                    if (hitGrid.transform.gameObject.GetComponent<Placeable>() != null)
                    {
                        hitGrid.transform.gameObject.GetComponent<Placeable>().setHover(true);
                    }
                    else if (hitGrid.transform.gameObject.GetComponent<HomeBaseScreenController>() != null)
                    {
                        hitGrid.transform.gameObject.GetComponent<HomeBaseScreenController>().setHover(true);
                    }
                }
                else
                {
                    hoveredPlaceable = null;
                    if (flowerSelector.activeSelf)
                    {
                        flowerSelector.SetActive(false);
                    }
                }
            }
            
        }
        else if (flowerSelector.activeSelf)
        {
            //Tell the hovered placeable that it is no longer hovered
            if (hoveredPlaceable != null)
            {
                if (hoveredPlaceable.GetComponent<Placeable>() != null)
                {
                    hoveredPlaceable.GetComponent<Placeable>().setHover(false);
                }
                else if (hoveredPlaceable.GetComponent<HomeBaseScreenController>() != null)
                {
                    hoveredPlaceable.GetComponent<HomeBaseScreenController>().setHover(false);
                }
            }

            hoveredPlaceable = null;
            flowerSelector.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (uIControllerS.currentQuestId == 5 || uIControllerS.currentQuestId == 16 || uIControllerS.currentQuestId == 19)
            {
                uIControllerS.completeQuest();
            }
        }
    }


    void questStuff(uint hitIndex, bool isFlower, int x, int y, bool wasPlaced)//manages all the quests
    {
        switch (uIControllerS.currentQuestId)
        {
            case 0: //Clear flowers to create an empty 4x4 area
                if (isFlower)
                {
                    flowerMemory.Add(hitIndex);
                }
                for (int yOffset = 0; yOffset < 4; yOffset++)//top left corner for 4x4 area then move -x and -y (it says plus but it is subtracted later)
                {
                    for (int xOffset = 0; xOffset < 4; xOffset++)
                    {
                        bool passed = true;
                        for (int currentY = 0; currentY < 4; currentY++)//check a 4x4 area starting from the currently cleared flower and work +x +y
                        {
                            for (int currentX = 0; currentX < 4; currentX++)
                            {
                                if (WorldManager.instance.placableGrid[WorldManager.instance.trueX(x + currentX - xOffset), WorldManager.instance.trueY(y + currentY - yOffset)] != null)
                                {
                                    passed = false;
                                    currentY = 10;
                                    break;
                                }
                            }
                        }
                        if (passed)
                        {
                            uIControllerS.completeQuest();
                            yOffset = 10;
                            break;
                        }
                    }
                }
                break;
            case 1://Using Q send all your flowers to your home base inventory
                break;//completed elsewhere
            case 2://Find and collect 2 flowers with the same petal type
                if (wasPlaced || !isFlower)
                {
                    break;
                }
                char currentFlowerPetal = hitIndex.ToString("X8")[0];//figure out the current petal
                bool found = false;
                for (int i = 0; i < flowerMemory.Count; i++)//flowermemory is every flower collected after the grid was cleared
                {
                    if (flowerMemory[i].ToString("X8")[0] == currentFlowerPetal)//if the flower in memory also has the petal
                    {

                        for (int ii = 0; ii < 10; ii++)//make sure the flower is still in inventory
                        {
                            if (uIControllerS.getInventoryDataAtSlot(ii) != null && uIControllerS.getInventoryDataAtSlot(ii).isFlower && uIControllerS.getInventoryDataAtSlot(ii).id == flowerMemory[i])
                            {
                                petalTypeForQuest = currentFlowerPetal;
                                uIControllerS.completeQuest();
                                found = true;
                                i = 1000000;
                                break;
                            }
                        }
                    }
                }
                flowerMemory.Add(hitIndex);//add it once we done
                if (found)
                {
                    flowerMemory.Clear();
                }
                break;
            case 3://Place the 2 flowers in the clearing to breed them together
                if (!wasPlaced || !isFlower)
                {
                    break;
                }
                flowerMemory.Add(hitIndex);
                for (int yOffset = -2; yOffset <= 2; yOffset += 2)//checks the current pos and looks for flowers that were planted 2 tiles away with nothing inbetween the two
                {
                    for (int xOffset = -2; xOffset <= 2; xOffset += 2)
                    {
                        if (yOffset + xOffset == -2 || yOffset + xOffset == 2)
                        {
                            Placeable tempPlacable = WorldManager.instance.placableGrid[WorldManager.instance.trueX(x + xOffset), WorldManager.instance.trueY(y + yOffset)];
                            if (tempPlacable != null && tempPlacable.isFlower)
                            {
                                if (flowerMemory.Contains(((FlowerObj)tempPlacable).getFlowerIndex()) && ((FlowerObj)tempPlacable).getFlowerIndex().ToString("X8")[0] == hitIndex.ToString("X8")[0])
                                {
                                    int newX = xOffset / 2;
                                    int newY = yOffset / 2;
                                    if (WorldManager.instance.placableGrid[WorldManager.instance.trueX(x + newX), WorldManager.instance.trueY(y + newY)].isLivingFlower()==false&& WorldManager.instance.placableGrid[WorldManager.instance.trueX(x + newX), WorldManager.instance.trueY(y + newY)].isFlower)
                                    {
                                        uIControllerS.completeQuest();
                                        yOffset = 10;
                                        flowerMemory.Clear();
                                        petalTypeForQuest = hitIndex.ToString("X8")[0];
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                break;
            case 4: //Wait for a child flower to grow in the empty space and collect it
                if (wasPlaced || !isFlower)
                {
                    break;
                }
                bool isDone = false;
                if (petalTypeForQuest == hitIndex.ToString("X8")[0])//what the flower collected in the spawn spot and did it have the right petal
                {
                    /*if (((FlowerObj)WorldManager.instance.placableGrid[WorldManager.instance.trueX(x + 1), WorldManager.instance.trueY(y + 0)]).petalIndex[0] == ((FlowerObj)WorldManager.instance.placableGrid[WorldManager.instance.trueX(x - 1), WorldManager.instance.trueY(y + 0)]).petalIndex[0] && petalTypeForQuest == ((FlowerObj)WorldManager.instance.placableGrid[WorldManager.instance.trueX(x - 1), WorldManager.instance.trueY(y + 0)]).getFlowerIndex().ToString("X8")[0])
                    {
                        isDone = true;
                    }
                    else if (((FlowerObj)WorldManager.instance.placableGrid[WorldManager.instance.trueX(x + 0), WorldManager.instance.trueY(y + 1)]).petalIndex[0] == ((FlowerObj)WorldManager.instance.placableGrid[WorldManager.instance.trueX(x + 0), WorldManager.instance.trueY(y - 1)]).petalIndex[0] && petalTypeForQuest == ((FlowerObj)WorldManager.instance.placableGrid[WorldManager.instance.trueX(x + 0), WorldManager.instance.trueY(y - 1)]).getFlowerIndex().ToString("X8")[0])
                    {
                        isDone = true;
                    }*/
                    isDone = true; //this may need to change but removing checks to see if parrent flowers are nearby
                }
                if (isDone)
                {
                    uIControllerS.completeQuest();
                }
                break;
            case 5://Congratulations! Press 'Enter' to continue the objectives
                //completed elsewhere
                break;
            case 6://Clear flowers to create an empty 8X8 area
                for (int yOffset = 0; yOffset < 8; yOffset++)//same idea as case 0
                {
                    for (int xOffset = 0; xOffset < 8; xOffset++)
                    {
                        bool passed = true;
                        for (int currentY = 0; currentY < 8; currentY++)
                        {
                            for (int currentX = 0; currentX < 8; currentX++)
                            {
                                if ((WorldManager.instance.placableGrid[WorldManager.instance.trueX(x + currentX - xOffset), WorldManager.instance.trueY(y + currentY - yOffset)]) != null)
                                {
                                    passed = false;
                                    currentY = 10;
                                    break;
                                }
                            }
                        }
                        if (passed)
                        {
                            uIControllerS.completeQuest();
                            yOffset = 10;
                            break;
                        }
                    }
                }
                break;
            case 7://Find and collect a flower with a yellow stem
                if (wasPlaced || !isFlower)
                {
                    break;
                }
                if (regexList[0].IsMatch(hitIndex.ToString("X8")))//does it match a regex expression for black stem
                {
                    uIControllerS.completeQuest();
                }
                break;
            case 8://Find and collect a flower with yellow petals
                if (wasPlaced || !isFlower)
                {
                    break;
                }
                if (regexList[1].IsMatch(hitIndex.ToString("X8")))
                {
                    uIControllerS.completeQuest();
                }
                break;
            case 9://Place the 2 flowers in the empty area with one space between them
                if (!wasPlaced || !isFlower)
                {
                    break;
                }
                int otherMatch=-1;
                if (regexList[0].IsMatch(hitIndex.ToString("X8")))
                {
                    otherMatch = 1;
                }
                else if (regexList[1].IsMatch(hitIndex.ToString("X8")))
                {
                    otherMatch = 0;
                }
                else
                {
                    break;
                }
                for (int yOffset = -2; yOffset <= 2; yOffset += 2)//checks the current pos and looks for flowers that were planted 2 tiles away with nothing inbetween the two
                {
                    for (int xOffset = -2; xOffset <= 2; xOffset += 2)
                    {
                        if (yOffset + xOffset == -2 || yOffset + xOffset == 2)
                        {
                            Placeable tempPlacable = WorldManager.instance.placableGrid[WorldManager.instance.trueX(x + xOffset), WorldManager.instance.trueY(y + yOffset)];
                            if (tempPlacable != null && tempPlacable.isFlower)
                            {
                                if (regexList[otherMatch].IsMatch(((FlowerObj)tempPlacable).getFlowerIndex().ToString("X8")))
                                {
                                    int newX = xOffset / 2;
                                    int newY = yOffset / 2;
                                    if (WorldManager.instance.placableGrid[WorldManager.instance.trueX(x + newX), WorldManager.instance.trueY(y + newY)].isLivingFlower() == false && WorldManager.instance.placableGrid[WorldManager.instance.trueX(x + newX), WorldManager.instance.trueY(y + newY)].isFlower)
                                    {
                                        uIControllerS.completeQuest();
                                        yOffset = 10;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                break;
            case 10://Breed and collect a flower with yellow petals and a yellow stem
                if (wasPlaced || !isFlower)
                {
                    break;
                }
                if (regexList[2].IsMatch(hitIndex.ToString("X8")))
                {
                    uIControllerS.completeQuest();
                }
                break;
            case 11://Find 2 flowers with a mushroom type leaves
                if (wasPlaced || !isFlower)
                {
                    break;
                }
                if (regexList[3].IsMatch(hitIndex.ToString("X8")))
                {
                    int totalMatchCount = 0;
                    for (int ii = 0; ii < 10; ii++)//make sure the flower is still in inventory
                    {
                        if (uIControllerS.getInventoryDataAtSlot(ii) != null && uIControllerS.getInventoryDataAtSlot(ii).isFlower && regexList[3].IsMatch(uIControllerS.getInventoryDataAtSlot(ii).id.ToString("X8")))
                        {
                            totalMatchCount += uIControllerS.getInventoryCountAtSlot(ii);
                        }
                    }
                    if (totalMatchCount >= 2)
                    {
                        uIControllerS.completeQuest();
                    }
                }
                break;
            case 12://Breed a flower to have a yellow stem, yellow petals, and mushroom type leaves
                if (wasPlaced || !isFlower)
                {
                    break;
                }
                if (regexList[4].IsMatch(hitIndex.ToString("X8")))
                {
                    uIControllerS.completeQuest();
                }
                break;
            case 13://Find a flower with yellow pistil
                if (wasPlaced || !isFlower)
                {
                    break;
                }
                if (regexList[5].IsMatch(hitIndex.ToString("X8")))
                {
                    uIControllerS.completeQuest();
                }
                break;
            case 14://Find a flower with a star shaped pistil
                if (wasPlaced || !isFlower)
                {
                    break;
                }
                if (regexList[6].IsMatch(hitIndex.ToString("X8")))
                {
                    uIControllerS.completeQuest();
                }
                break;
            case 15://Breed a flower to have a yellow stem, yellow petals, a mushroom leaves, and yellow star shaped pistil
                if (wasPlaced || !isFlower)
                {
                    break;
                }
                if (regexList[7].IsMatch(hitIndex.ToString("X8")))
                {
                    uIControllerS.completeQuest();
                }
                break;
            case 16://Congratulations! Press 'Enter' to exit the tutorial and start challenge mode
                //completed elesewhere
                break;
            default: //TODO
                if (wasPlaced)
                {
                    break;
                }
                if (hitIndex == uIControllerS.challengeModeFlower)
                {
                    //uIControllerS.newchallengeModeFlower();
                    uIControllerS.completeQuest();
                }
                break;
        }
    }
}
