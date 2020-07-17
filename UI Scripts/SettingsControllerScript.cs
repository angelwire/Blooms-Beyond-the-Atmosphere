using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//updated: 11-18-19
//version: .02
//Author: William Jones
/*
 * This is the manager for the settings components (resolution, graphics options)
 */

public class SettingsControllerScript : MonoBehaviour
{
    public static SettingsControllerScript instance;
    Resolution[] resolutions; //A list of all available resolutions
    bool postProcessing = true; //Whether or not to use the snazzy post processing
    bool disableFlashingColors = false;
    [SerializeField] public Dropdown resolutionDropdown; //The dropdown menu with the resolutions
    [SerializeField] public Dropdown qualityDropdown; //The dropdown menu with the quality settings
    public bool isOnline = false;
    bool isSetUp = false;

    int graphicsLevel = 3;

    //Setup for the scene loading
    void OnEnable() { SceneManager.sceneLoaded += OnSceneLoad; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoad; }

    // Start is called before the first frame update
    void Awake()
    {
        Debug.Log("settings awake");
        isSetUp = false;
        DontDestroyOnLoad(gameObject);
        if (SettingsControllerScript.instance != null)
        {
            Destroy(gameObject);
            return;
        }
        SettingsControllerScript.instance = this;//singleton
    }

    public void settingsSetup()
    {
        isSetUp = false;
        if (resolutionDropdown != null && qualityDropdown != null)
        {
            isSetUp = true;
            //Get screen resolutions and fill the dropdown
            resolutions = Screen.resolutions;

            //The resolution to set the dropdown to
            Resolution current = Screen.currentResolution;
            int currentIndex = 0;

            //Fill a list with the array values
            List<string> resolutionStrings = new List<string>();
            foreach (Resolution res in resolutions)
            {
                resolutionStrings.Add(res.ToString());

                if (res.width == Screen.width
                    && res.height == Screen.height)
                {
                    currentIndex = resolutionStrings.Count;
                }
            }

            //Set the options
            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(resolutionStrings);
            resolutionDropdown.SetValueWithoutNotify(currentIndex);
            resolutionDropdown.onValueChanged.AddListener(setResolution);

            qualityDropdown.SetValueWithoutNotify(QualitySettings.GetQualityLevel());
            qualityDropdown.onValueChanged.AddListener(setGraphicsQuality);

            checkForOnlineConnection();

            Debug.Log("done setting up settings");
        }
    }

    public void LateUpdate()
    {
        if (!isSetUp)
        {
            Debug.Log("setting not set up: setting up settings");
            GameObject qualityDropdownObject = GameObject.Find("graphicsQualityDropdown");
            if (qualityDropdownObject != null)
            {
                qualityDropdown = qualityDropdownObject.GetComponent<Dropdown>();
            }

            GameObject resolutionDropdownObject = GameObject.Find("resolutionDropdown");
            if (resolutionDropdownObject != null)
            {
                resolutionDropdown = resolutionDropdownObject.GetComponent<Dropdown>();
            }
            settingsSetup();
        }
    }

    //When a scene is loaded
    void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        PostProcessVolume ppv = Camera.main.gameObject.GetComponent<PostProcessVolume>();
        //Check for post processing effects
        if (ppv != null)
        {
            ppv.enabled = postProcessing;
        }

        Debug.Log("Graphics quality at: " + QualitySettings.GetQualityLevel());
        QualitySettings.SetQualityLevel(graphicsLevel, true);
    }

    //Setters for the UI elements
    public void setFullscreen(bool isFullscreen) { Screen.fullScreen = isFullscreen; }

    public void setPostProcessing(bool isPost) { postProcessing = isPost; }

    public void setResolution(int inResolution)
    {
        //Since there's a glitch in unity 2.6.f1 get the value of the dropdown
        inResolution = GameObject.Find("resolutionDropdown").GetComponent<Dropdown>().value;

        //Now set the resolution
        int ww = resolutions[inResolution].width;
        int hh = resolutions[inResolution].height;
        Screen.SetResolution(ww,hh,Screen.fullScreen);
        GameObject.Find("TitleController").GetComponent<MainMenuController>().originalPosition = new Vector2(ww * .5f, hh * .9f);
    }

    public void setGraphicsQuality(int inResolution)
    {
        //Since there's a glitch in unity 2.6.f1 get the value of the dropdown
        inResolution = GameObject.Find("graphicsQualityDropdown").GetComponent<Dropdown>().value;
        Debug.Log("setting quality to:" + inResolution);
        graphicsLevel = inResolution;
        QualitySettings.SetQualityLevel(inResolution,true);
    }

    public bool shouldDisableFlashingColors()
    {
        return disableFlashingColors;
    }

    public void checkForOnlineConnection()
    {
        GameJolt.API.DataStore.Get("flowers_planted", true, value =>
        {
            if (value != null)
            {
                isOnline = true;
            }
            else
            {
                isOnline = false;
            }
        });
    }
}
