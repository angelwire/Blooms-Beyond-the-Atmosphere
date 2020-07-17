using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceStationPanelController : MonoBehaviour
{
    [SerializeField] GameObject onlinePanel;
    [SerializeField] GameObject popupPrefab;
    [SerializeField] GameObject uploadPrefab;
    [SerializeField] GameObject visitPrefab;
    [SerializeField] GameObject loadingPopup;
    SpaceStationController spaceStationController;
    bool dialogIsOpen = false;

    bool isUploadingPlanet = false;
    bool isVisitingPlanet = false;

    bool isWaiting = false;
    float transitionTime = 3f;
    string a;
    string[,] replaceList = {
        { "_","" },
        { " ","" },
        { ".","" },
        { "-","" },
        { "1","i" },
        { "3","e" },
        { "0","o" },
        { "(","c" },
        { "[)","d" },
        { "$","s" },
        { "!","i" },
        { "+","t" },
        { "z","s" },
        { "ooo","oo" },//for booob and all variations
        { "sss","ss" }//for asssssssssssss and all variations
    };
    string[] blockList = { "bitch", "cunt", "faggot", "fag", "damn", "shit","ass","fuck","cock","penis","vag","dick", "nigger", "nigga", "kkk"};

    private void OnEnable()
    {
        GetComponent<RectTransform>().localScale = Vector3.zero;
        GameObject.Find("UICanvas").GetComponent<UIControllerScript>().hidePlayerInventory();
        onlinePanel.SetActive(SettingsControllerScript.instance.isOnline);
    }

    private void OnDisable()
    {
        GameObject UIgo = GameObject.Find("UICanvas");
        if (UIgo != null)
        {
            UIgo.GetComponent<UIControllerScript>().showPlayerInventory();
        }
        GetComponent<RectTransform>().localScale = Vector3.zero;
        onlinePanel.SetActive(SettingsControllerScript.instance.isOnline);
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<RectTransform>().localScale = Vector3.Lerp(GetComponent<RectTransform>().localScale, Vector3.one, transitionTime * Time.deltaTime);

        if (SettingsControllerScript.instance.isOnline)
        {
            getOnlineInput();
        }
    }

    private void getOnlineInput()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            if (!dialogIsOpen && !isWaiting)
            {
                //Instantiates the search popup
                GameObject textPopup = Instantiate(visitPrefab, GameObject.Find("UICanvas").transform);
                textPopup.GetComponent<onlineSearchController>().caller = gameObject;
                textPopup.GetComponent<onlineSearchController>().setQuestionText("Enter planet name to visit or click the search button below");

                onlinePanel.SetActive(false);
                GameObject UIgo = GameObject.Find("UICanvas");
                if (UIgo != null)
                {
                    UIgo.GetComponent<UIControllerScript>().pauseWithoutMenu();
                }

                isVisitingPlanet = true;
                isUploadingPlanet = false;
                dialogIsOpen = true;
                GameObject.Find("whiteCursor").GetComponent<SpriteRenderer>().enabled = false;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            if (!dialogIsOpen)
            {
                GameObject textPopup = Instantiate(uploadPrefab, GameObject.Find("UICanvas").transform);
                textPopup.GetComponent<textInputController>().caller = gameObject;
                textPopup.GetComponent<textInputController>().setQuestionText("Enter your planet's name. Other players will use this name to visit your planet");
                onlinePanel.SetActive(false);

                GameObject UIgo = GameObject.Find("UICanvas");
                if (UIgo != null)
                {
                    UIgo.GetComponent<UIControllerScript>().pauseWithoutMenu();
                }

                isVisitingPlanet = false;
                isUploadingPlanet = true;
                dialogIsOpen = true;
                GameObject.Find("whiteCursor").GetComponent<SpriteRenderer>().enabled = false;
            }
        }
    }

    public void dialogReturn(string returnText)
    {
        if (returnText != "")
        {
            //Perform the action for uploading or visiting the planet
            if (isVisitingPlanet)
            {
                isWaiting = true;
                //The player wants to visit the planet named in the return text
                visitPlanet(returnText);
            }
            else if (isUploadingPlanet)
            {
                isWaiting = true;
                bool isSafe = true;
                string temp = returnText.ToLower();
                for (int i = 0; i < replaceList.GetLength(0); i++)
                {
                    while (temp.IndexOf(replaceList[i, 0]) != -1)
                    {
                        temp = temp.Replace(replaceList[i, 0], replaceList[i, 1]);
                    }
                }
                for (int i = 0; i < blockList.Length; i++)
                {
                    if (temp.IndexOf(blockList[i]) != -1)
                    {
                        isSafe = false;
                        break;
                    }
                }
                for (int i = 'a'; i <= 'z'; i++)//remove letter dupe tricks
                {
                    string toSearch = "" + (char)i;
                    toSearch += toSearch;
                    while (temp.IndexOf(toSearch) != -1)
                    {
                        temp=temp.Replace(toSearch, "" + (char)i);
                    }
                }
                for (int i = 0; i < blockList.Length; i++)
                {
                    if (temp.IndexOf(blockList[i]) != -1)
                    {
                        isSafe = false;
                        break;
                    }
                }
                //The player wants to upload a planet with the given name
                if (isSafe)
                {
                    uploadPlanet(returnText);
                }
                else
                {
                    showMessage("Planet naming error");
                    isWaiting = false;
                }
            }
        }

        resumeUI();
    }

    private void visitPlanet(string planetName)
    {

        GameJolt.API.DataStore.Get("playerplanet_" + planetName, true, value => {
            if (value != null)
            {
                isWaiting = false;
                //success message
                PlayerPrefs.SetString("ONLINE_WORLD", value);

                resumeUI();

                //warp to online world
                WarpDriveController.instance.goToPlanet(Galaxy.ONLINE_PLANET_ID);
            }
            else
            {
                isWaiting = false;
                //error message
                Debug.Log("CANNOT VISIT PLANET: " + planetName);
                showMessage("Error, cannot visit " + planetName);
                resumeUI();
            }
                
        });
    }

    private void uploadPlanet(string planetName)
    {
        GameObject searhingPanel = showLoading();
        searhingPanel.GetComponent<loadingPanelController>().setText("Uploading...");

        int homePlanetId = Galaxy.HOME_PLANET_ID;
        int flowerGridXLen = 60;
        int flowerGridYLen = 60;
        if (WarpDriveController.instance.currentPlanetID == homePlanetId)
        {
            WorldManager.instance.saveWorld(homePlanetId);
        }
        string toPush = "";
        for (int x = 0; x < flowerGridXLen; x++)
        {
            for (int y = 0; y < flowerGridYLen; y++)
            {

                if (PlayerPrefs.HasKey(homePlanetId + "x" + x + "y" + y + "_save"))
                {
                    toPush += PlayerPrefs.GetString(homePlanetId + "x" + x + "y" + y + "_save");
                }
                if (y + 1 != flowerGridYLen)
                {
                    toPush += "[";
                }
            }
            if (x + 1 != flowerGridXLen)
            {
                toPush += "[";
            }
        }
        int uploadSize = 50;
        if (toPush.Length > uploadSize) uploadSize = toPush.Length / 5 + 1;
        GameJolt.API.DataStore.Get("playerplanetauth_" + planetName, true, key => {

            if (key == null || key == PlayerPrefs.GetString("auth"))
            {
                GameJolt.API.DataStore.Set("playerplanetauth_" + planetName, PlayerPrefs.GetString("auth"), true, worked =>{
                    if (worked)
                    {
                        GameJolt.API.DataStore.SetSegmented("playerplanet_" + planetName, toPush, true, success =>
                        {
                            if (success)
                            {
                                hideLoading();
                                showMessage("Uploading " + planetName + " complete!");
                                //uploaded message
                                isWaiting = false;
                                resumeUI();
                            }
                            else
                            {
                                showMessage("Error, cannot upload planet");
                                //fail message
                                isWaiting = false;
                                hideLoading();
                                resumeUI();
                            }
                        }, progress => { }, uploadSize);
                    }
                    else
                    {
                        showMessage("Internet error, try again");
                        //fail message
                        hideLoading();
                        isWaiting = false;
                        resumeUI();
                    }
                });
            }
            else
            {
                showMessage("Error, name already in use");
                hideLoading();
                isWaiting = false;
                resumeUI();
            }

        });
    }

    private void resumeUI()
    {
        if (!isWaiting)
        {
            GameObject.Find("whiteCursor").GetComponent<SpriteRenderer>().enabled = true;
            //Reset the panels
            onlinePanel.SetActive(SettingsControllerScript.instance.isOnline);
            GameObject UIgo = GameObject.Find("UICanvas");
            if (UIgo != null)
            {
                UIgo.GetComponent<UIControllerScript>().resumeGame();
            }

            isVisitingPlanet = false;
            isUploadingPlanet = false;
            dialogIsOpen = false;
        }
    }

    public void showMessage(string message)
    {
        GameObject pop = Instantiate(popupPrefab, GameObject.Find("UICanvas").transform);
        pop.GetComponent<PopupPanelController>().setText(message);
    }

    public GameObject showLoading()
    {
        return showLoadingInTransform(GameObject.Find("UICanvas").transform);
    }

    public GameObject showLoadingInTransform(Transform t)
    {
        return Instantiate(loadingPopup, t);
    }
    public void hideLoading()
    {
        foreach(GameObject g in GameObject.FindGameObjectsWithTag("loadingTag"))
        {
            GameObject.Destroy(g);
        }
    }
}
