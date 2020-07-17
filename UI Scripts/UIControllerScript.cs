//updated: 11-23-19
//version: .04
//Author: William Jones
/*
 * Draws the UI and manages it
 */
/* 
* Minimap of important public methods:
* ---inventoryPeekItem() //Gets the placeable at the currently selected inventory slot
* ---inventoryPopItem() //Removes the placeable at the currently selected slot  
* ---inventoryPushItem(placeable) //Adds the item to the appropriate slot
* ---inventoryHasItemToPlace() //If there's an item at the currently selected slot
* ---inventoryHasRoom(placeable) //If the given placeable can be added to the inventory
* ---getInventoryCountAtSlot(int)
* ---getInventoryDataAtSlot(int)
* ---getUIItemAtSlot(int) //Gets the UI item, not the Placeable
* ---getItemIsFlowerAtSlot(int)
* ---getItemIndexAtSlot(int)
* ...more will be added
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.CrossPlatformInput;

public class UIControllerScript : MonoBehaviour
{
    //The sprite to draw for each inventory slot
    public Image inventorySlotSprite;
    //The flower placement controller
    public GameObject placementController;

    //The sprites for selected and unselected
    public Sprite selectedImage;
    public Sprite notSelectedImage;

    //The ui flower prefab
    public GameObject uiFlowerPrefab;
    //How much to scale the flower
    private float uiFlowerScale = 1.7f;

    //The text prefab
    public GameObject uiTextPrefab;

    //The compass object
    public GameObject compassImage;

    //These will be gotten from the FlowerPlayerControler
    int inventorySize = 0;
    int selectedSlot = 0;

    //This is a list of objects for the inventory and their count
    private List<GameObject> inventoryModels = new List<GameObject>();
    private List<Placeable> inventoryData = new List<Placeable>();
    private List<int> inventoryCount = new List<int>();

    //The flower properties game object
    private FlowerProperties flowerProperties;

    //The instance of the objectives and quests
    private GameObject questPanelInstance;
    private GameObject challengeFlowerPanel;
    private GameObject tabObject;
    private GameObject showoffFlower;
    private Text objectiveText;
    private Text helpText;

    //The flower viewing variables
    private GameObject flowerToView;
    private GameObject viewBackgroundInstance;
    public bool isViewingFlower = false;
    private float flowerRotationSensitivity = 6;
    private float viewFlowerScale = 6;
    private float viewTransition = 0;
    private float viewTransitionTo = 0;
    private float viewTransitionSpeed; //Set when the slot flower is found and !null
    private bool onSameSlot = true;
    public GameObject cursorSprite;

    //Variables for slot flashing
    bool slotIsFlashing = false;
    Color slotColor = Color.white;
    float slotFlashSpeed = 5f;

    //Variables for pausing the game
    public bool isPaused = false;
    GameObject pauseMenuPanel;
    GameObject helpPanelInstance;
    GameObject controlsPanelInstance;
    GameObject buttonsPanelInstance;
    FirstPersonController fpsController;

    //The player inventory panel
    Transform playerInventoryPanel;
    float playerInventoryHiddenPosition = -500f;
    float playerInventoryShownPosition = 0f;
    float playerInventoryGoToPosition = 0f;
    float playerInventoryPosition = 0f;
    [SerializeField] float playerInventoryTransitionSpeed;

    //The panel that shows up when the player exits the atmosphere
    [SerializeField]GameObject spaceStationPanel;
    private bool isInSpaceStation = false;

    bool DEBUG_CHEATS_ENABLED = false;

    bool needNewChallengeFlower = false;
    bool flowerLoaded = false;

    [SerializeField] GameObject dialogPrefab;

    private SkyController skyControllerScript;
    private void Start()
    {
        //Find relevant instances
        pauseMenuPanel = GameObject.Find("pauseMenuPanel");
        helpPanelInstance = pauseMenuPanel.transform.Find("helpPanel").gameObject;
        controlsPanelInstance = pauseMenuPanel.transform.Find("controlsPanel").gameObject;
        buttonsPanelInstance = pauseMenuPanel.transform.Find("buttonsPanel").gameObject;
        pauseMenuPanel.SetActive(isPaused);
        questPanelInstance = GameObject.Find("objectivesPopup");
        challengeFlowerPanel = questPanelInstance.transform.Find("challengeFlowerPanel").gameObject;
        challengeFlowerPanel.SetActive(false);
        viewBackgroundInstance = GameObject.Find("flowerViewerBackground");
        fpsController = GameObject.Find("FPSController").GetComponent<FirstPersonController>();
        skyControllerScript = GameObject.Find("Sky").GetComponent<SkyController>();

        //Setting quest object instance variables
        objectiveText = questPanelInstance.transform.GetChild(0).gameObject.GetComponent<Text>();
        helpText = questPanelInstance.transform.GetChild(1).gameObject.GetComponent<Text>();
        tabObject = questPanelInstance.transform.GetChild(2).gameObject;
        setCurrentQuestId(0);

        //Get the player inventory panel
        playerInventoryPanel = transform.GetChild(1);

        flowerProperties = GameObject.Find("World").GetComponent<FlowerProperties>();

        //Create the UI with properties
        inventorySize = placementController.GetComponent<FlowerPlayerControler>().inventorySize;
        for (int ii = 0; ii < inventorySize; ii += 1)
        {
            inventoryModels.Add(null);
            inventoryData.Add(null);
            inventoryCount.Add(0);
        }

        selectedSlot = placementController.GetComponent<FlowerPlayerControler>().selectedIndex;
        drawInventorySlots(inventorySize);
        updateSelectedSlot(selectedSlot);

        if (PlayerPrefs.HasKey("current_challenge_flower"))
        {
            flowerLoaded = true;
            needNewChallengeFlower = false;
            challengeModeFlower = uint.Parse(PlayerPrefs.GetString("current_challenge_flower"));
        }
        else
        {
            needNewChallengeFlower = true;
            flowerLoaded = true;
        }

        setCurrentQuestId(PlayerPrefs.GetInt("quest_progress"));
    }

    public void OnEnable()
    {
        if (currentQuestId == 17)
        {
            int level = WarpDriveController.instance.getJetpackLevel();
            if (level > 0)
            {
                completeQuest();
            }
        }

        if (!DEBUG_CHEATS_ENABLED)
        {
            GetComponent<Text>().enabled = false;
        }
    }

    private void Update()
    {
        updateQuestPanel();
        updateSlotFlashing();
        updateFlowerViewer();
        updatePauseMenu();
        updateDebugCheats();
        updateCompass();
        updatePlayerInventoryPosition();
    }

    private void updatePlayerInventoryPosition()
    {
        if (Mathf.Abs(playerInventoryGoToPosition - playerInventoryPanel.GetComponent<RectTransform>().anchoredPosition.y) > 1)
        {
            Vector3 newPosition = playerInventoryPanel.GetComponent<RectTransform>().anchoredPosition;
            newPosition.y = Mathf.Lerp(newPosition.y, playerInventoryGoToPosition, playerInventoryTransitionSpeed * Time.deltaTime);
            playerInventoryPanel.GetComponent<RectTransform>().anchoredPosition = newPosition;
        }
    }

    public void hidePlayerInventory()
    {
        playerInventoryGoToPosition = playerInventoryHiddenPosition;
    }

    public void showPlayerInventory()
    {
        playerInventoryGoToPosition = playerInventoryShownPosition;
    }

    /// <summary>
    /// Updates the position of the compass image
    /// </summary>
    private void updateCompass()
    {
        float positionScale = ((GetComponent<RectTransform>().rect.width * .5f) / 180);
        float halfScreen = (GetComponent<RectTransform>().rect.width * .5f);
        float margin = 50f;
        Vector3 playerPosition = fpsController.gameObject.transform.position;

        float worldWidth = WorldManager.instance.getPlanetWidth();
        float worldHeight = WorldManager.instance.getPlanetHeight();

        playerPosition.x = positiveMod(playerPosition.x, worldWidth);
        playerPosition.z = positiveMod(playerPosition.z, worldHeight);
        playerPosition.y = 0;

        if (playerPosition.x > worldWidth * .5f)
        {
            playerPosition.x -= worldWidth;
        }
        if (playerPosition.z > worldHeight * .5f)
        {
            playerPosition.z -= worldHeight;
        }

        float playerDirection = fpsController.gameObject.transform.rotation.eulerAngles.y;
        float targetDirection = 180 + (Mathf.Atan2(playerPosition.x, playerPosition.z) * Mathf.Rad2Deg);

        float compassXPosition = Mathf.DeltaAngle(playerDirection, targetDirection) * positionScale;
        compassXPosition = Mathf.Clamp(compassXPosition, -(halfScreen - margin), halfScreen - margin);
        compassImage.transform.localPosition = new Vector3(compassXPosition, compassImage.transform.localPosition.y);

        float alpha = Mathf.Clamp((Vector3.Magnitude(playerPosition) - 5f) / (worldWidth*.5f),0, 1);
        alpha *= fpsController.GetComponent<SpaceStationController>().isEngaged() ? 0 : 1;
        compassImage.GetComponent<Image>().color = new Color(1,1,1,alpha);
    }

    private float positiveMod(float a, float b)
    {
        float c = a % b;
        return c < 0 ? c + b : c;
    }

    public void setSpaceStation(bool value)
    {
        isInSpaceStation = value;
        spaceStationPanel.SetActive(value);
    }

    private void updateFlowerViewer()
    {
        if (Input.GetKey(KeyCode.LeftControl) && onSameSlot && !isPaused)
        {
            //If there's not currently a flower being viewed
            if (flowerToView == null)
            {
                //Set the flowerToView to the uiFlower at the slot
                flowerToView = getUIItemAtSlot(selectedSlot);

                //If there is a flower in the slot do first time stuff
                if (flowerToView != null)
                {
                    //Get a reference to the flower control script
                    UIFlowerScript script = flowerToView.GetComponent<UIFlowerScript>();

                    script.fixBrokenFlower();

                    //A variable for the transitioning between viewing and not viewing
                    viewTransitionTo = 1;

                    viewTransitionSpeed = script.viewingSpeed;

                    //The UI is now viewing a flower
                    isViewingFlower = true;

                    //Tell the flower that it is the chosen one
                    script.isViewing = true;

                    //Tell the flower that it not done being viewed
                    script.isDoneViewing = false;

                    //Tell the flower where to go and what to scale to
                    Vector3 slotPos = flowerToView.transform.parent.localPosition;

                    //Tell the flower where to go and what to scale to
                    float yOff = GetComponent<RectTransform>().rect.height * .5f;
                    script.goToLocation = new Vector3(-slotPos.x, yOff - slotPos.y, 0);
                    script.goToScale = new Vector3(100 * viewFlowerScale, 100 * viewFlowerScale, 100 * viewFlowerScale);
                }
            }

            //Do continuous viewing stuff
            if (flowerToView != null)
            {
                //Rotate the flower
                flowerToView.transform.Rotate(-Camera.main.transform.up, CrossPlatformInputManager.GetAxis("Mouse X") * flowerRotationSensitivity, Space.World);
                flowerToView.transform.Rotate(Camera.main.transform.right, CrossPlatformInputManager.GetAxis("Mouse Y") * flowerRotationSensitivity, Space.World);
            }
        }
        else //If the player lets go of control or switches slot
        {
            onSameSlot = true;
            //When the player first lets go of control
            if (flowerToView != null)
            {
                viewTransitionTo = 0;

                //Get the attached script
                UIFlowerScript script = flowerToView.GetComponent<UIFlowerScript>();

                //Tell the flower we're done viewing
                //DO NOT TELL THE FLOWER THAT isViewing=false! It will do that itself when it's back to the right place
                script.isDoneViewing = true;

                //Tell the flower to return to the original location, scale, and rotation
                script.goToLocation = Vector3.zero;
                script.goToScale = script.returnScale;
                script.goToRotation = Quaternion.identity;

                //We're done telling the flower what to do now
                flowerToView = null;

                //The UI is no longer viewing a flower
                isViewingFlower = false;
            }

        }

        //Manage the view transitioning
        viewTransition = Mathf.Lerp(viewTransition, viewTransitionTo, Time.deltaTime * viewTransitionSpeed);

        //Manage the background
        float bfo = .6f;
        float beo = 0f;
        viewBackgroundInstance.GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0, Mathf.Lerp(beo, bfo, viewTransition));
        cursorSprite.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, Mathf.Lerp(1, 0, viewTransition));
    }

    /// <summary>
    /// Create the game objects that will be drawn for the slots
    /// </summary>
    /// <param name="slotCount"></param>
    private void drawInventorySlots(int slotCount)
    {
        //Set the local variable
        inventorySize = slotCount;

        //The panel that holds the slot images
        RectTransform layoutPanel = playerInventoryPanel.GetComponent<RectTransform>();

        //Instantiate them
        int ii = 0;
        while (ii < slotCount)
        {
            //The currently instantiated slot image
            Image currentSlotImage = Instantiate(inventorySlotSprite, layoutPanel);
            currentSlotImage.gameObject.layer = LayerMask.NameToLayer("UI");

            //Add the slot ui flower object
            GameObject slotItem = Instantiate(uiFlowerPrefab, currentSlotImage.transform);
            slotItem.layer = LayerMask.NameToLayer("UI");
            slotItem.transform.localScale = Vector3.one * uiFlowerScale;
            slotItem.name = "slotItem";

            //Add the flower counter text box
            GameObject textObject = Instantiate(uiTextPrefab, currentSlotImage.transform);
            textObject.transform.SetAsLastSibling();
            textObject.GetComponent<Text>().color = Color.clear;
            textObject.name = "slotText";

            ii += 1;
        }
    }

    /// <summary>
    /// Does cheating debug stuff
    /// </summary>
    private void updateDebugCheats()
    {
        //F5 toggles the ui, I don't think it's a cheat just something fun for screenshots
        if (Input.GetKeyDown(KeyCode.F1))
        {
            GameObject uiCam = GameObject.Find("UICamera");
            uiCam.GetComponent<Camera>().enabled = !uiCam.GetComponent<Camera>().enabled;
        }

        if (DEBUG_CHEATS_ENABLED && !isPaused)
        {
            //Numpad + adds to the current slot
            if (Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                if (inventoryHasItemToPlace()) { incInventoryCount(selectedSlot); }
            }

            if (Input.GetKeyDown(KeyCode.J))
            {
                int level = WarpDriveController.instance.getJetpackLevel();
                WarpDriveController.instance.setJetpackLevel(level + 1);
            }
            if (Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                completeQuest();
            }

            if (Input.GetKey(KeyCode.O))
            {
                Time.timeScale = 25;
                skyControllerScript.worldTime += Time.deltaTime;
                WorldManager.instance.worldTimer += Time.deltaTime;
            }
            else
            {
                Time.timeScale = 1;
            }

        }

    }

    /// <summary>
    /// Updates the inventory flashing variables
    /// </summary>
    private void updateSlotFlashing()
    {
        if (slotIsFlashing)
        {
            slotColor = Color.Lerp(slotColor, Color.white, Time.deltaTime * slotFlashSpeed);

            //The panel that holds the slot images
            RectTransform layoutPanel = playerInventoryPanel.GetComponent<RectTransform>();

            //Adjust the color for all slots
            for (int ii = 0; ii < layoutPanel.childCount; ii += 1)
            {
                layoutPanel.transform.GetChild(ii).gameObject.GetComponent<Image>().color = slotColor;
            }
        }
    }

    /// <summary>
    /// Flashes the inventory slots
    /// </summary>
    public void flashInventorySlots()
    {
        slotIsFlashing = true;
        slotColor = Color.red;
    }

    /// <summary>
    /// Changes which slot is selected
    /// </summary>
    public void updateSelectedSlot(int inputSlot)
    {
        //Tell the viewer to switch slots if it's a different slot
        if (inputSlot != selectedSlot)
        {
            onSameSlot = false;
        }

        //Set the local variable
        selectedSlot = inputSlot;

        //The panel that holds the slot images
        RectTransform layoutPanel = playerInventoryPanel.GetComponent<RectTransform>();

        //Loop through all the inventory slots
        int ii = 0;
        while (ii < inventorySize)
        {
            Image currentSlotImage = layoutPanel.GetChild(ii).gameObject.GetComponent<Image>();
            if (ii == inputSlot)
            {
                currentSlotImage.sprite = selectedImage;
                currentSlotImage.gameObject.transform.GetChild(0).gameObject.GetComponent<UIFlowerScript>().popup();

                Text slotText = currentSlotImage.gameObject.transform.GetChild(1).gameObject.GetComponent<Text>();
                Color currentTextColor = slotText.color;
                slotText.color = new Color(0, 0, 0, currentTextColor.a);

            }
            else
            {
                currentSlotImage.sprite = notSelectedImage;
                GameObject slotItem = currentSlotImage.gameObject.transform.GetChild(0).gameObject;
                if (slotItem != null)
                {
                    slotItem.GetComponent<UIFlowerScript>().popdown();
                }

                Text slotText = currentSlotImage.gameObject.transform.GetChild(1).gameObject.GetComponent<Text>();
                Color currentTextColor = slotText.color;
                slotText.color = new Color(1, 1, 1, currentTextColor.a);
            }
            ii += 1;
        }
        //print(currentQuestId);
        //If the player is attempting the challenge flower
        if (currentQuestId >= 17)
        {
            compareSelectedSlot();
        }
    }

    /// <summary>
    /// Tells the slot to update itself and refresh the flower model and labels
    /// </summary>
    /// <param name="slot"></param>
    private void updateSlot(int slot)
    {
        //The panel that holds the slot images
        RectTransform layoutPanel = playerInventoryPanel.GetComponent<RectTransform>();
        //Find the slot image and clear it out
        Image currentSlotImage = layoutPanel.GetChild(slot).GetComponent<Image>();

        if (inventoryCount[slot] <= 0)
        {
            //Clear the slot flower
            GameObject slotFlower = currentSlotImage.transform.Find("slotItem").gameObject;
            slotFlower.GetComponent<UIFlowerScript>().hideFlower();
            //Destroy the placeable at the slot
            if (inventoryData[slot] != null)
            {
                GameObject.Destroy(inventoryData[slot].gameObject);
            }
            inventoryData[slot] = null;
        }
        updateText(slot, currentSlotImage);

        if (currentQuestId >= 17)
        {
            compareSelectedSlot();
        }
    }

    private void updateText(int slot, Image currentSlotImage)
    {
        //Set the slot text
        Text slotText = currentSlotImage.gameObject.transform.Find("slotText").gameObject.GetComponent<Text>();
        slotText.text = inventoryCount[slot].ToString();
        Color currentTextColor = slotText.color;
        //Only make the text visible if there are more than 1 item
        if (inventoryCount[slot] > 1)
        {
            slotText.color = new Color(currentTextColor.r, currentTextColor.g, currentTextColor.b, 1);
        }
        else
        {
            slotText.color = new Color(currentTextColor.r, currentTextColor.g, currentTextColor.b, 0);
        }
    }

    /// <summary>
    /// Compares the challenge flower attributes to the selected slot
    /// </summary>
    public void compareSelectedSlot()
    {
        Transform attributesTransform = challengeFlowerPanel.transform.GetChild(0);
        //If there's a flower in the slot
        if (getInventoryCountAtSlot(selectedSlot) > 0)
        {
            //Get the index and the flower part indexes
            uint inIndex = getItemIndexAtSlot(selectedSlot);
            int pistilIndex = flowerProperties.getPistilIndexFromIndex(inIndex);
            int pistilColor = flowerProperties.getPistilColorIndexFromIndex(inIndex);
            int petalIndex = flowerProperties.getPetalIndexFromIndex(inIndex);
            int petalColor = flowerProperties.getPetalColorIndexFromIndex(inIndex);
            int leafIndex = flowerProperties.getLeafIndexFromIndex(inIndex);
            int leafColor = flowerProperties.getLeafColorIndexFromIndex(inIndex);
            int stemIndex = flowerProperties.getStemIndexFromIndex(inIndex);
            int stemColor = flowerProperties.getStemColorIndexFromIndex(inIndex);

            //Tell each one to compare
            attributesTransform.GetChild(0).gameObject.GetComponent<ChallengeCheckerSlotController>().compareIndex(pistilIndex, pistilColor);
            attributesTransform.GetChild(1).gameObject.GetComponent<ChallengeCheckerSlotController>().compareIndex(petalIndex, petalColor);
            attributesTransform.GetChild(2).gameObject.GetComponent<ChallengeCheckerSlotController>().compareIndex(leafIndex, leafColor);
            attributesTransform.GetChild(3).gameObject.GetComponent<ChallengeCheckerSlotController>().compareIndex(stemIndex, stemColor);
        }
        else
        {
            //Tell each one not to compare
            attributesTransform.GetChild(0).gameObject.GetComponent<ChallengeCheckerSlotController>().doNotCompareIndex();
            attributesTransform.GetChild(1).gameObject.GetComponent<ChallengeCheckerSlotController>().doNotCompareIndex();
            attributesTransform.GetChild(2).gameObject.GetComponent<ChallengeCheckerSlotController>().doNotCompareIndex();
            attributesTransform.GetChild(3).gameObject.GetComponent<ChallengeCheckerSlotController>().doNotCompareIndex();
        }
    }

    /// <summary>
    /// Gets the index of the item at the slot
    /// </summary>
    /// <param name="slot"></param>
    public uint getItemIndexAtSlot(int slot)
    {
        if (inventoryData[slot] != null)
        {
            return inventoryData[slot].id;
        }
        else
        {
            Debug.LogError("No item at the slot, check to make sure the slot has an item there");
            return 0;
        }
    }

    /// <summary>
    /// Gets the index of the item at the slot
    /// </summary>
    /// <param name="slot"></param>
    public bool getItemIsFlowerAtSlot(int slot)
    {
        if (inventoryData[slot] != null)
        {
            return inventoryData[slot].isFlower;
        }
        else
        {
            Debug.LogError("No item at the slot, check to make sure the slot has an item there");
            return false;
        }
    }

    /// <summary>
    /// Sets the slot's count (how many there are)
    /// </summary>
    /// <param name="slot"></param>
    /// <param name="count"></param>
    public void setSlotCount(int slot, int count)
    {
        inventoryCount[slot] = count;
        updateSlot(slot);
    }

    /// <summary>
    /// Returns the flower object if there is one
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    public GameObject getUIItemAtSlot(int slot)
    {
        /*
         * Previous versions of this method asked whether or not the count was greater than 0 or not
         * But this version returns the UIitem no matter what because there is always an "UIitem" in the slot
         * Even when it's invisible there is always an UIitem there
         */

        //The panel that holds the slot images
        RectTransform layoutPanel = playerInventoryPanel.GetComponent<RectTransform>();
        //Find the flower
        GameObject slotFlower = layoutPanel.GetChild(slot).Find("slotItem").gameObject;
        return slotFlower;
    }

    public Placeable getInventoryDataAtSlot(int slot)
    {
        return inventoryData[slot];
    }

    public int getInventoryCountAtSlot(int slot)
    {
        return inventoryCount[slot];
    }

    //Use inventoryPushItem instead
    private void incInventoryCount(int slot)
    {
        inventoryCount[slot]++;
        updateSlot(slot);
    }

    //Use inventoryPopItem instead
    private void decInventoryCount(int slot)
    {
        inventoryCount[slot]--;
        updateSlot(slot);
    }

    /// <summary>
    /// Clears out the given slot
    /// </summary>
    /// <param name="slot"></param>
    public void inventoryClearSlot(int slot)
    {
        inventoryCount[slot] = 1;
        decInventoryCount(slot);
    }

    /// <summary>
    /// Check to see if the given item can be picked up
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool inventoryHasRoom(Placeable item)
    {
        //Loop through each inventory slot
        for (int i = 0; i < inventorySize; i++)
        {
            //Get the item at the current slot
            Placeable slotItem = getInventoryDataAtSlot(i);
            //If the current slot is empty or is equal to the given item
            if (slotItem == null || slotItem.isEqual(item))
            {
                //The item can be picked up
                return true;
            }
        }
        return false;
    }

    public bool isEmpty()
    {
        for (int i = 0; i < inventorySize; i++)
        {
            //Get the item at the current slot
            Placeable slotItem = getInventoryDataAtSlot(i);
            //If the current slot is empty or is equal to the given item
            if (slotItem != null)
            {
                //The item can be picked up
                return false;
            }
        }
        return true;
    }

    public bool isFullOfFlowers()
    {
        for (int i = 0; i < inventorySize; i++)
        {
            //Get the item at the current slot
            Placeable slotItem = getInventoryDataAtSlot(i);
            //If the current slot is empty or is equal to the given item
            if (slotItem == null||!slotItem.isFlower)
            {
                //The item can be picked up
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Whether or not the selected inventory slot has something in it
    /// </summary>
    /// <returns></returns>
    public bool inventoryHasItemToPlace()
    {
        Placeable slotItem = getInventoryDataAtSlot(selectedSlot);
        int slotCount = getInventoryCountAtSlot(selectedSlot);
        return (slotItem != null && (slotCount > 0));
    }

    /// <summary>
    /// Pushes an item onto the inventory. The script finds a place to put it
    /// </summary>
    /// <param name="itemToAdd"></param>
    /// <returns>Returns whether or not the item was successfully added</returns>
    public bool inventoryPushItem(Placeable itemToAdd)
    {
        //Mostly copied from Vance's FlowerPlayerControler script
        int slotToStack = -1;
        int slotCheckStart = selectedSlot;
        int firstEmptySlot = -1;

        //Loop through every slot to find a place to add it
        //(stop when every slot has been checked or when finalSlot has been set)
        for (int i = 0; i < inventorySize && slotToStack == -1; i++)
        {
            //Math trick to start at the currently selected slot and loop around
            int currentSlot = (i + slotCheckStart) % inventorySize;

            Placeable itemAtCurrentSlot = getInventoryDataAtSlot(currentSlot);

            //Check to see if the slot is empty
            if (itemAtCurrentSlot == null)
            {
                //There's an empty spot for the item
                if (firstEmptySlot == -1)
                {
                    //If an empty spot hasn't been found then mark the first empty spot as found
                    firstEmptySlot = currentSlot;
                }
            }
            else //The slot isn't empty
            {
                if (itemAtCurrentSlot.isEqual(itemToAdd))
                {
                    //The items are the same so set the finalSlot to the currentSlot
                    slotToStack = currentSlot;
                }
            }
        }

        if (slotToStack != -1)
        {
            //Stack the item onto the slotToStack
            incInventoryCount(slotToStack);
            return true;
        }
        else
        {
            if (firstEmptySlot != -1)
            {
                //Add the item to the first empty slot that was found
                setSlot(firstEmptySlot, itemToAdd);
                compareSelectedSlot();
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Takes the item at the currently selected slot and removes it
    /// </summary>
    /// <returns></returns>
    public void inventoryPopItem()
    {
        decInventoryCount(selectedSlot);
    }

    /// <summary>
    /// Returns the item at the currently selected slot
    /// </summary>
    /// <returns></returns>
    public Placeable inventoryPeekItem()
    {
        return getInventoryDataAtSlot(selectedSlot);
    }

    /// <summary>
    /// Sets the slot to the given placeable
    /// </summary>
    /// <param name="slotIndex"></param>
    /// <param name="itemToAdd"></param>
    public void setSlot(int slotIndex, Placeable itemToAdd)
    {
        //If the given slot is null then build a model for it
        if (inventoryData[slotIndex] == null)
        {
            //Build the model based on the placable
            if (itemToAdd.isFlower)
            {
                //Get the empty holder at the given slot
                GameObject uiItem = getUIItemAtSlot(slotIndex);
                UIFlowerScript script = uiItem.GetComponent<UIFlowerScript>();
                if (script != null)
                {
                    script.buildFlower(
                        flowerProperties.getStemIndexFromIndex(itemToAdd.id),
                        flowerProperties.getLeafIndexFromIndex(itemToAdd.id),
                        flowerProperties.getPetalIndexFromIndex(itemToAdd.id),
                        flowerProperties.getPistilIndexFromIndex(itemToAdd.id));
                    script.colorFlower(
                        flowerProperties.getStemColorIndexFromIndex(itemToAdd.id),
                        flowerProperties.getLeafColorIndexFromIndex(itemToAdd.id),
                        flowerProperties.getPetalColorIndexFromIndex(itemToAdd.id),
                        flowerProperties.getPistilColorIndexFromIndex(itemToAdd.id));

                    //Rescale the object based on the placeable's UI scale
                    script.getRootObject().transform.localScale *= itemToAdd.uiScale;
                }

                //Duplicate the item that has just been picked up and hide it
                //This flower and placeable is used for when the player places a flower down
                //It's basically stored behind the scenes and disabled, and then cloned when the flower
                //at the slot is placed down
                inventoryData[slotIndex] = (Placeable)GameObject.Instantiate(itemToAdd, placementController.transform);

                //If it's a flower then rebuild it and make it right
                if (inventoryData[slotIndex].isFlower)
                {
                    ((FlowerObj)inventoryData[slotIndex]).rebuildFlower();
                }
                inventoryData[slotIndex].setHover(false);
                inventoryData[slotIndex].gameObject.SetActive(false);
                inventoryCount[slotIndex] = 1;
            }
            else
            {
                //Get the empty holder at the slot
                GameObject uiItem = getUIItemAtSlot(slotIndex);
                UIFlowerScript script = uiItem.GetComponent<UIFlowerScript>();
                if (script != null)
                {
                    script.buildDecoration(itemToAdd.id);

                    //Rescale the object based on the placeable's UI scale
                    script.getRootObject().transform.localScale *= itemToAdd.uiScale;
                    script.rootScale = itemToAdd.uiScale;
                }

                //Duplicate the item that has just been picked up and hide it
                inventoryData[slotIndex] = GameObject.Instantiate(itemToAdd, transform.root);
                inventoryData[slotIndex].gameObject.SetActive(false);
                inventoryCount[slotIndex] = 1;
            }
        }
    }

    /// <summary>
    /// Change the slot's flower index and count
    /// </summary>
    /// <param name="slot"></param>
    /// <param name="index"></param>
    /// <param name="count"></param>
    public void fillSlotWithLong(int slot, long index, int count)
    {
        /*inventoryIndex[slot] = index;
        inventoryCount[slot] = count;
        updateSlot(slot);*/
    }


    #region Pause Menu
    /// <summary>
    /// This region is for handling the pause menu
    /// </summary>
    private void updatePauseMenu()
    {
        //If escape is paused
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;
            pauseMenuPanel.SetActive(isPaused);
            cursorSprite.SetActive(!isPaused);
            fpsController.setCursorLocked(!isPaused);
            questPanelInstance.SetActive(!isPaused);
            spaceStationPanel.SetActive(!isPaused && isInSpaceStation);
            hidePanels();
        }
    }

    /// <summary>
    /// Pauses the game without bringing up the menu
    /// </summary>
    public void pauseWithoutMenu()
    {
        isPaused = true;
        fpsController.setCursorLocked(!isPaused);
        cursorSprite.SetActive(false);
    }

    //Closes the pause menu and resumes the game
    public void resumeGame()
    {
        isPaused = false;
        pauseMenuPanel.SetActive(isPaused);
        cursorSprite.SetActive(!isPaused);
        fpsController.setCursorLocked(!isPaused);
        questPanelInstance.SetActive(!isPaused);
        spaceStationPanel.SetActive(isInSpaceStation);
        hidePanels();
    }

    //Returns to the title screen
    public void quitGame()
    {
        //Fade out the screen
        GameObject.Find("fadeOverlay").GetComponent<FadeControllerScript>().instantFadeOut();

        //Quit
        if (SceneManager.GetActiveScene() != SceneManager.GetSceneByName("onlinePlanet"))
        {
            WorldManager.instance.saveWorld(WarpDriveController.instance.currentPlanetID);
            PlayerPrefs.SetInt("lastWorld", WarpDriveController.instance.currentPlanetID);
        }
        WarpDriveController.instance.currentPlanetID = 0;
        WarpDriveController.instance.targetPlanetID = 0;
        //SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(SceneManager.GetActiveScene().name));
        SceneManager.LoadSceneAsync("Resources/Scenes/titleScene");
    }

    //Shows a panel showing the controls
    public void showControls()
    {
        //Controls
        controlsPanelInstance.SetActive(true);
        buttonsPanelInstance.SetActive(false);
    }

    //Shows a panel showing the help
    public void showHelp()
    {
        //Help
        helpPanelInstance.SetActive(true);
        buttonsPanelInstance.SetActive(false);
    }

    //Closes the help and controls panel
    public void hidePanels()
    {
        controlsPanelInstance.SetActive(false);
        helpPanelInstance.SetActive(false);
        buttonsPanelInstance.SetActive(true);
    }

    public List<Placeable> getInventoryData() { return inventoryData; }
    public List<int> getInventoryCount() { return inventoryCount; }

    #endregion

    #region QuestStuff

    /// <summary>
    /// This region is for the quest/objective system
    /// </summary>

    bool showQuestPanel = false;
    float questPanelToPosition = 0;
    float questPanelHidePosition = 700f;
    float questPanelShowPosition = 1;
    float questPanelPosition = 700;
    float questPanelSpeed = 3f;

    /// <summary>
    /// Updates and performs changes to the quest panel
    /// </summary>
    void updateQuestPanel()
    {
        //Update the position
        questPanelPosition = Mathf.Lerp(questPanelPosition, questPanelToPosition, questPanelSpeed * Time.deltaTime);
        questPanelInstance.GetComponent<RectTransform>().anchoredPosition = new Vector3(questPanelPosition, 0, 0);

        //Tell the panel to move to either position depending on whether or not it should be hidden
        if (showQuestPanel)
        {
            questPanelToPosition = questPanelShowPosition;
        }
        else
        {
            questPanelToPosition = questPanelHidePosition;
        }

        //Toggle the panel's visibility when the user presses "tab"
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            //Hide the tab text
            tabObject.SetActive(false);

            //Toggle the quest panel
            showQuestPanel = !showQuestPanel;
        }
    }

    public void hideQuestPanel()
    {
        showQuestPanel = false;
    }

    //Serialized
    public string objectivePrefix;
    public string helpPrefix;

    string[] questString = {
        "Clear flowers to create an empty 4x4 area",
        "Using Q send all your flowers to your home base inventory",
        "Find and collect 2 flowers with the same petal shape",
        "Place the 2 flowers with the same petal shape in the clearing leaving one space between them",
        "Wait for a child flower to grow in the empty space and collect it",
        "Congratulations! Press 'Enter' to continue the objectives",
        "Clear flowers to create an empty 8X8 area",
        "Find and collect a flower with a yellow stem",
        "Find and collect a flower with yellow petals",
        "Place the 2 flowers in the empty area with one space between them",
        "Breed and collect a flower with yellow petals and a yellow stem",
        "Find 2 flowers with feather type leaves",
        "Breed a flower to have a yellow stem, yellow petals, and feather type leaves",
        "Find a flower with yellow pistil",
        "Find a flower with a star shaped pistil",
        "Breed a flower to have a yellow stem, yellow petals, feather shaped leaves, and a yellow star shaped pistil",
        "Congratulations! Press 'Enter' to continue",
        "Collect flowers and sell them to purchase the jetpack upgrade",
        "Use your jetpack to visit an alien planet with a 'Hot' atmosphere",
        "Congratulations! Press 'Enter' to begin challenge mode"
    };
    string[] helpQuestString = {
        "Collect flowers in your inventory and sell them at the home base",
        "On the home planet using 'Q' will send a flower directly to your home base without needing to manually put it in",
        "There is one set of petals that is usually attached to the end of the stem",
        "If two or more flowers are placed one space apart they will produce a child in the empty space. Flowers cannot breed diagonally",
        "Child flowers will grow after about 20 seconds",
        "Child flowers will only consist of traits from each parent",
        "You will need a larger empty space to do more complex breeding, remember to use 'Q' to clear flowers.",
        "The stem is the main section of the flower that all the other parts attach to",
        "Each flower has one set of petals that is usually attached to the end of the stem",
        "The new flower that eventually grows will be made up of random components from the parents on each side",
        "Breed multiple flowers simultaneously to have better chances of getting the flower you want",
        "Flowers have a set of leaves that appear on various places on the stem. Despite being called leaves, not all are leaf shaped",
        "If 2 flowers have the same flower part that part will always transfer to the child, plant them and the flower you bred around an empty square",
        "Matching colors increases a flower's value, but colors only transfer to children randomly",
        "The pistil is located inside the petals. Use 'Control' to get a better look at flowers in your inventory",
        "To speed up the process have one flower breed with multiple others at the same time",
        "Experiment with different methods to find the fastest way to breed flowers",
        "Flowers with matching colors sell for more money. As you sell the same flower the price will slowly drop, so be sure to include some variety",
        "Use 'Spacebar' to fly. Alien planets with different atmospheres contain flowers with different colors and shapes",
        "Challenge mode will task you with breeding exact flowers using colors and shapes from all planets"
    };

    public int currentQuestId = 0;
    public int flowersCollected = 0;
    public int flowersPlaced = 0;

    public uint challengeModeFlower;

    //Completes the current quest and increments the ID
    public void completeQuest()
    {
        enableTabText();

        flowersCollected = 0;
        flowersPlaced = 0;
        setCurrentQuestId(currentQuestId + 1);
        Debug.Log("Current quest ID: " + currentQuestId);
    }

    //Performs all the necessary actions to set the current quest ID to the given id
    public void setCurrentQuestId(int inId)
    {
        currentQuestId = inId;

        if (inId < questString.Length)
        {
            helpText.text = string.Format("{0}\n{1}", helpPrefix, helpQuestString[inId]);
            objectiveText.text = string.Format("{0} ({1}/{2})\n{3}", objectivePrefix, inId + 1, questString.Length, questString[inId]);
            challengeFlowerPanel.SetActive(false);
        }
        else
        {
            helpText.text = "";
            objectiveText.text = "";
            needNewChallengeFlower = true;
        }

        //If the quest is at the challenge flower then create one
        if (currentQuestId == questString.Length)
        {
            if (needNewChallengeFlower && !flowerLoaded)
            {
                //prep for challenge mode
                newchallengeModeFlower();
                compareSelectedSlot();
            }
            else
            {
                instantiateChallengeModeFlower(challengeModeFlower);
                flowerLoaded = false;
            }
        }

        //If the quest has gone past the first challenge flower
        if (currentQuestId > helpQuestString.Length)
        {
            //Go back to the challenge flower so the player will have to push enter again
            currentQuestId = helpQuestString.Length-1;
            //Shoot the confetti cannon
            transform.Find("confettiCannon").gameObject.GetComponent<ParticleSystem>().Play();

            //The text should say something about challenge mode
            helpText.text = "Have fun breeding new flowers and decorating your planet!";
            objectiveText.text = "Congratulations! Decorate your planet or press 'Enter' to try a new challenge flower!";
            challengeFlowerPanel.SetActive(false);
            needNewChallengeFlower = true;
        }

        //Complete the quest if the jetpack is already purchased
        if (currentQuestId == 17)
        {
            if (WarpDriveController.instance.getJetpackLevel() > 0)
            {
                completeQuest();
            }
        }
    }

    //Spawns a random flower that the player must build
    public void newchallengeModeFlower()
    {
        List<uint> randomFlowers = new List<uint>();
        randomFlowers.Add(flowerProperties.getRandomFlowerIDForPlanet(Planet.PlanetType.HOME));
        randomFlowers.Add(flowerProperties.getRandomFlowerIDForPlanet(Planet.PlanetType.HOT));

        int jpl = WarpDriveController.instance.getJetpackLevel();
        if (Galaxy.instance.coldPlanetVisited) { randomFlowers.Add(flowerProperties.getRandomFlowerIDForPlanet(Planet.PlanetType.COLD)); } 
        if (Galaxy.instance.neonPlanetVisited) { randomFlowers.Add(flowerProperties.getRandomFlowerIDForPlanet(Planet.PlanetType.NEON)); }

        uint challengeFlower = 0;

        int petal = flowerProperties.getPetalIndexFromIndex(randomFlowers[Random.Range(0, randomFlowers.Count)]);
        challengeFlower += (uint)petal;
        challengeFlower *= 16;
        int petalColor = flowerProperties.getPetalColorIndexFromIndex(randomFlowers[Random.Range(0, randomFlowers.Count)]);
        challengeFlower += (uint)petalColor;
        challengeFlower *= 16;
        int stem = flowerProperties.getStemIndexFromIndex(randomFlowers[Random.Range(0, randomFlowers.Count)]);
        challengeFlower += (uint)stem;
        challengeFlower *= 16;
        int stemColor = flowerProperties.getStemColorIndexFromIndex(randomFlowers[Random.Range(0, randomFlowers.Count)]);
        challengeFlower += (uint)stemColor;
        challengeFlower *= 16;
        int leaf = flowerProperties.getLeafIndexFromIndex(randomFlowers[Random.Range(0, randomFlowers.Count)]);
        challengeFlower += (uint)leaf;
        challengeFlower *= 16;
        int leafColor = flowerProperties.getLeafColorIndexFromIndex(randomFlowers[Random.Range(0, randomFlowers.Count)]);
        challengeFlower += (uint)leafColor;
        challengeFlower *= 16;
        int pistil = flowerProperties.getPistilIndexFromIndex(randomFlowers[Random.Range(0, randomFlowers.Count)]);
        challengeFlower += (uint)pistil;
        challengeFlower *= 16;
        int pistilColor = flowerProperties.getPistilColorIndexFromIndex(randomFlowers[Random.Range(0, randomFlowers.Count)]);
        challengeFlower += (uint)pistilColor;

        challengeModeFlower = challengeFlower; //set challengeModeFlower to the random flower
        HomeInventoryData.instance.challangeFlowerIds.Add(challengeFlower);
        PlayerPrefs.SetString("current_challenge_flower", challengeModeFlower + "");
        instantiateChallengeModeFlower(challengeModeFlower);
        //Debug.Log("New challenge flower: " + challengeModeFlower);
    }

    //Instantiates a challenge mode flower from the given uint index and sets the slots
    private void instantiateChallengeModeFlower(uint inIndex)
    {
        //Debug.Log("instantiatingChallengeModeFlowwer:" + inIndex);

        challengeFlowerPanel.SetActive(true);

        //Force the canvas to update so the panels will be the right size
        Canvas.ForceUpdateCanvases();

        //Get the index for each element
        int pistilIndex = flowerProperties.getPistilIndexFromIndex(inIndex);
        int pistilColor = flowerProperties.getPistilColorIndexFromIndex(inIndex);
        int petalIndex = flowerProperties.getPetalIndexFromIndex(inIndex);
        int petalColor = flowerProperties.getPetalColorIndexFromIndex(inIndex);
        int leafIndex = flowerProperties.getLeafIndexFromIndex(inIndex);
        int leafColor = flowerProperties.getLeafColorIndexFromIndex(inIndex);
        int stemIndex = flowerProperties.getStemIndexFromIndex(inIndex);
        int stemColor = flowerProperties.getStemColorIndexFromIndex(inIndex);

        Transform attributesTransform = challengeFlowerPanel.transform.GetChild(0);

        //Set the slots to the correct flower
        attributesTransform.GetChild(0).gameObject.GetComponent<ChallengeCheckerSlotController>()
            .setIndex(ChallengeCheckerSlotController.MeshType.PISTIL, pistilIndex, pistilColor);

        attributesTransform.GetChild(1).gameObject.GetComponent<ChallengeCheckerSlotController>()
            .setIndex(ChallengeCheckerSlotController.MeshType.PETAL, petalIndex, petalColor);

        attributesTransform.GetChild(2).gameObject.GetComponent<ChallengeCheckerSlotController>()
            .setIndex(ChallengeCheckerSlotController.MeshType.LEAF, leafIndex, leafColor);

        attributesTransform.GetChild(3).gameObject.GetComponent<ChallengeCheckerSlotController>()
            .setIndex(ChallengeCheckerSlotController.MeshType.STEM, stemIndex, stemColor);

        Transform showoffTransform = challengeFlowerPanel.transform.GetChild(1);
        //Build the combined UI flower
        if (showoffFlower != null)
        {
            GameObject.Destroy(showoffFlower);
        }
        showoffFlower = Instantiate(uiFlowerPrefab, showoffTransform);
        showoffFlower.layer = LayerMask.NameToLayer("UI");
        showoffFlower.transform.localScale = Vector3.one * uiFlowerScale * 2;
        showoffFlower.GetComponent<UIFlowerScript>().buildFlower(stemIndex, leafIndex, petalIndex, pistilIndex);
        showoffFlower.GetComponent<UIFlowerScript>().colorFlower(stemColor, leafColor, petalColor, pistilColor);
        showoffFlower.GetComponent<UIFlowerScript>().rotationSpeed = .5f;

        //Clear the slots so they aren't comparing
        attributesTransform.GetChild(0).gameObject.GetComponent<ChallengeCheckerSlotController>()
            .doNotCompareIndex();
        attributesTransform.GetChild(1).gameObject.GetComponent<ChallengeCheckerSlotController>()
            .doNotCompareIndex();
        attributesTransform.GetChild(2).gameObject.GetComponent<ChallengeCheckerSlotController>()
            .doNotCompareIndex();
        attributesTransform.GetChild(3).gameObject.GetComponent<ChallengeCheckerSlotController>()
            .doNotCompareIndex();
    }

    //Enables the tab text to alert the player that a new objective has appeared
    private void enableTabText()
    {
        //Hide the tab text
        tabObject.SetActive(false);

        //Toggle the quest panel
        showQuestPanel = true;
    }

    /*public bool hasStartedQuest()
    {

    }*/


    #endregion

    #region Planet Naming
    public void namePlanet()
    {
        GameObject newDialog = Instantiate(dialogPrefab, GameObject.Find("UICanvas").transform);
        pauseWithoutMenu();
        hidePlayerInventory();
        newDialog.GetComponent<textInputController>().caller = gameObject;
    }

    public void dialogReturn(string resultString)
    {
        if (resultString != "")
        {
            WarpDriveController.instance.GetCurrentPlanet().setName(resultString);
        }
        showPlayerInventory();
        resumeGame();
    }
    #endregion
}
