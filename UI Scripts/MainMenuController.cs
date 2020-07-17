/*
 * updated: 11-14-19
 * Scripts to control the main menu
 * Author: William Jones
 * */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    GameObject helpPanel; //The panel that shows the help contents
    GameObject controlsPanel; //The panel that shows the controls
    GameObject optionsPanel; //The panel with the options
    GameObject creditsPanel; //The panel with the options
    GameObject loadingPanel; //The boring loading panel
    
    GameObject buttonsPanel; //The panel that holds the buttons
    GameObject textImage; //The image with the game name
    GameObject spinner;
    GameObject clouds;
    GameObject flare1;
    GameObject flare2;
    GameObject flare3;

    float textFloatSpeed = .2f;
    float textFloatWidth = 40f;
    float textFloatHeight = 15f;
    float textFloatTime = 0f;
    public Vector3 originalPosition;

    bool shouldPlay = false;
    bool doneFakeLoading = false;
    float loadingTimer;
    [SerializeField] float maxLoadingTimer;
    GameObject fadeObject;

    void Start()
    {
        GameJoltAPIHelper.refresh();
        //Set all the variables to the proper instances

        helpPanel = GameObject.Find("helpPanel");
        controlsPanel = GameObject.Find("controlsPanel");
        optionsPanel = GameObject.Find("optionsPanel");
        creditsPanel = GameObject.Find("creditsPanel");
        loadingPanel = GameObject.Find("loadingPanel");
        GameJoltAPIHelper.refresh();
        buttonsPanel = GameObject.Find("buttonsPanel");
        textImage = GameObject.Find("textImage");
        spinner = GameObject.Find("speckleSpinner");
        originalPosition = textImage.transform.position;

        clouds = GameObject.Find("cloudsBackground");

        flare1 = GameObject.Find("flare1");
        flare2 = GameObject.Find("flare2");
        flare3 = GameObject.Find("flare3");

        fadeObject = GameObject.Find("fadeOverlay");

        //Close all the panels that are open by default
        closeMenus();

        loadingTimer = maxLoadingTimer;
    }

    void Update()
    {
        float px = originalPosition.x + Mathf.Cos(textFloatTime) * textFloatWidth;
        float py = originalPosition.y + Mathf.Cos(textFloatTime * .5f) * textFloatHeight;
        textFloatTime += textFloatSpeed * Time.deltaTime;
        textImage.transform.position = new Vector3(px,py,0);
        spinner.transform.Rotate(0, 0, textFloatSpeed * Time.deltaTime * 30f);

        clouds.transform.Translate(textFloatSpeed * Time.deltaTime * 100f, 0, 0);

        if (clouds.transform.localPosition.x > 1920)
        {
            clouds.transform.localPosition = new Vector3(0, 0, 0);
        }

        flare2.transform.Rotate(0, 0, textFloatSpeed * Time.deltaTime * 20);
        flare1.transform.Rotate(0, 0, textFloatSpeed * Time.deltaTime * -30);
        flare3.transform.Rotate(0, 0, textFloatSpeed * Time.deltaTime * -50 );

        //If the "Click to play" button is clicked and the fade has faded out
        if (doneFakeLoading && shouldPlay && fadeObject.GetComponent<FadeControllerScript>().fadeComplete())
        {
            //Make sure the screen is entirely black
            fadeObject.GetComponent<FadeControllerScript>().instantFadeOut();
            //Load the scene
            beginSceneLoad();
        }

        if (!doneFakeLoading && loadingTimer < 0)
        {
            finishFakeLoading();
        }
        else
        {
            loadingTimer -= Time.deltaTime;
        }

    }

    /// <summary>
    /// Pops up the "loading..." panel and loads the homePlanet scene 
    /// </summary>
    public void playGame()
    {
        //Hide all the panels
        helpPanel.SetActive(false);
        controlsPanel.SetActive(false);
        optionsPanel.SetActive(false);
        loadingPanel.SetActive(false);

        //Hide the main menu
        buttonsPanel.SetActive(false);
        textImage.SetActive(false);

        string[] statText = loadingPanel.transform.Find("statsText").GetComponent<Text>().text.Split('\n');
        if (GameJoltAPIHelper.return_flowers_picked == GameJoltAPIHelper.errorText)
        {
            loadingPanel.transform.Find("statsText").GetComponent<Text>().text = "You are playing off-line, connect to the internet to view flower stats and share your world online";

        }
        else
        {
            statText[0] += GameJoltAPIHelper.return_flowers_picked;
            statText[1] += GameJoltAPIHelper.return_flowers_planted;
            statText[2] += GameJoltAPIHelper.return_flowers_sold;
            loadingPanel.transform.Find("statsText").GetComponent<Text>().text = statText[0] + "\n" + statText[1] + "\n" + statText[2];
        }
        //Show the loading panel
        loadingPanel.SetActive(true);

        //Reset the timer and the bool variable
        loadingTimer = maxLoadingTimer;
        doneFakeLoading = false;

        if (!PlayerPrefs.HasKey("lastWorld"))
            PlayerPrefs.SetInt("lastWorld", 0);
        WarpDriveController.instance.currentPlanetID = PlayerPrefs.GetInt("lastWorld");
        WarpDriveController.instance.targetPlanetID = PlayerPrefs.GetInt("lastWorld");
    }

    private void beginSceneLoad()
    {
        //Load the scene
        if (WarpDriveController.instance.currentPlanetID == Galaxy.HOME_PLANET_ID)
        {
            SceneManager.LoadSceneAsync("Resources/Scenes/homePlanetScene");
        }
        else
        {
            WarpDriveController.instance.targetPlanetID = WarpDriveController.instance.currentPlanetID;
            SceneManager.LoadSceneAsync("tiletest2");
        }
        loadingTimer = 900000;
        shouldPlay = false;
    }

    public void clickToContinue()
    {
        fadeObject.GetComponent<FadeControllerScript>().fadeOut();
        musicPlayerScript.instance.fadeOutMusic(2f);
        shouldPlay = true;
    }

    private void finishFakeLoading()
    {
        doneFakeLoading = true;
        GameObject continueButton = GameObject.Find("loadingButtonBackground");
        if (continueButton != null)
        {
            continueButton.GetComponent<Button>().interactable = true;
            continueButton.GetComponentInChildren<Text>().text = "Click here to Start";
        }
    }

    /// <summary>
    /// Closes all the open menus and shows the buttons and text image
    /// </summary>
    public void closeMenus()
    {
        //Hide all the panels
        helpPanel.SetActive(false);
        controlsPanel.SetActive(false);
        optionsPanel.SetActive(false);
        loadingPanel.SetActive(false);
        creditsPanel.SetActive(false);

        //Show the main menu
        buttonsPanel.SetActive(true);
        textImage.SetActive(true);
    }

    /// <summary>
    /// Opens the help menu
    /// </summary>
    public void showHelp()
    {
        helpPanel.SetActive(true);

        buttonsPanel.SetActive(false);
        textImage.SetActive(false);
    }
    /// <summary>
    /// Shows the controls menu
    /// </summary>
    public void showControls()
    {
        controlsPanel.SetActive(true);

        buttonsPanel.SetActive(false);
        textImage.SetActive(false);
    }

    /// <summary>
    /// Opens the options menu
    /// </summary>
    public void showOptions()
    {
        optionsPanel.SetActive(true);
        SettingsControllerScript.instance.settingsSetup();

        buttonsPanel.SetActive(false);
        textImage.SetActive(false);
    }

    /// <summary>
    /// Opens the credits
    /// </summary>
    public void showCredits()
    {
        creditsPanel.SetActive(true);

        buttonsPanel.SetActive(false);
        textImage.SetActive(false);
    }

    /// <summary>
    /// Quits the game
    /// </summary>
    public void quitGame()
    {
        Application.Quit();
    }
    
    /// <summary>
    /// clears save
    /// </summary>
    public void deleteWorld()
    {
        string authPref = PlayerPrefs.GetString("auth");
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetString("auth", authPref);
        WarpDriveController.instance.setJetpackLevel(0);
        Galaxy.instance.resetSave();
        HomeInventoryData.instance.resetSave();
    }
}
