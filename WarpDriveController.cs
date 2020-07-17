using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WarpDriveController : MonoBehaviour
{
    public static WarpDriveController instance;
    public int currentPlanetID = 0;
    public int targetPlanetID = 0;
    private bool shouldStart = false;
    private bool isLoading = false;
    int jetpackLevel = 0;
    GameObject warpDriveEffects;
    GameObject player;
    public UIControllerScript uic;
    [SerializeField] GameObject warpDriveEffectsPrefab;
    [SerializeField] float insideRotation;
    [SerializeField] float outsideRotation;
    [SerializeField] float middleRotation;
    [SerializeField] float warpDelay;
    [SerializeField] float warpDriveTransitionInSpeed;
    [SerializeField] Texture homeSkyboxTexture;
    [SerializeField] Texture hotSkyboxTexture;
    [SerializeField] Texture coldSkyboxTexture;
    [SerializeField] Texture neonSkyboxTexture;

    private float warpDriveTransitionInAmount = 0;
    GameObject fadeOverlay;
    float warpDelayTimer;
    bool isFading = false;

    // Start is called before the first frame update
    void Start()
    {
        if (WarpDriveController.instance != null)
        {
            GameObject.DestroyImmediate(gameObject);
            return;
        }
        else
        {
            instance = this;
            //Load jeptack level
            setJetpackLevel(PlayerPrefs.GetInt("jetpack_level",0));
        }
        GameObject.DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += setupCurrentPlanet;
    }

    private void setupCurrentPlanet(Scene scene, LoadSceneMode load)
    {
        if (scene.name != "HomeScreenScene")
        {
            shouldStart = true;
            warpDelayTimer = 0;
            warpDriveTransitionInAmount = 0;

            //Set up the sky box
            switch(GetDestinationPlanet().getType())
            {
                case Planet.PlanetType.HOT: RenderSettings.skybox.mainTexture = hotSkyboxTexture; break;
                case Planet.PlanetType.COLD: RenderSettings.skybox.mainTexture = coldSkyboxTexture; break;
                case Planet.PlanetType.NEON: RenderSettings.skybox.mainTexture = neonSkyboxTexture; break;
                case Planet.PlanetType.HOME: RenderSettings.skybox.mainTexture = homeSkyboxTexture; break;
                default: RenderSettings.skybox.mainTexture = homeSkyboxTexture; break;
            }

        }
    }

    //First called when the user wants to go to a different planet
    public void goToPlanet(int planetID)
    {
        //Check to see if there is a fade overlay
        if (fadeOverlay == null)
        {
            fadeOverlay = GameObject.Find("fadeOverlay");
        }

        //Set the destination to the given planet id
        targetPlanetID = planetID;
        WorldManager.currentPlanetID = planetID;

        if (planetID != Galaxy.ONLINE_PLANET_ID)
        {
            //Tell the planet that it has been visited
            Galaxy.instance.planetList[planetID].setVisited(true);

            if (Galaxy.instance.planetList[planetID].getType() == Planet.PlanetType.COLD) { Galaxy.instance.coldPlanetVisited = true; }
            if (Galaxy.instance.planetList[planetID].getType() == Planet.PlanetType.HOT) { Galaxy.instance.hotPlanetVisited = true; }
            if (Galaxy.instance.planetList[planetID].getType() == Planet.PlanetType.NEON) { Galaxy.instance.neonPlanetVisited = true; }
        }

        //Tell LateUpdate() that the game is "loading"
        isLoading = true;

        //Get the ui controller
        uic = GameObject.Find("UICanvas").GetComponent<UIControllerScript>();
        if (uic.currentQuestId == 18)
        {
            if (GetDestinationPlanet().getType() == Planet.PlanetType.HOT)
            {
                uic.completeQuest();
            }
        }
        uic.hideQuestPanel();


        engageWarpDrive();
    }

    private void LateUpdate()
    {
        if (shouldStart)
        {
            fadeOverlay = GameObject.Find("fadeOverlay");
            isFading = false;

            //Check to make sure nothing happens when the home menu screen scene is loaded
            if (SceneManager.GetActiveScene().name != "HomeScreenScene")
            {
                //Find the player
                player = GameObject.Find("FPSController");
                //If the player exists
                if (player != null)//teleport
                {
                    player.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().Teleport(new Vector3(0, WorldManager.instance.atmosphereHeight, 0));
                    player.GetComponent<SpaceStationController>().disengage();
                    //player.transform.Find("FirstPersonCharacter").rotation = Quaternion.Euler(90, 0, 0);
                }
            }
            shouldStart = false;
            isLoading = false;
        }

        if (warpDriveEffects != null)
        {
            warpDriveTransitionInAmount += warpDriveTransitionInSpeed * Time.deltaTime;
            warpDriveTransitionInAmount = Mathf.Clamp(warpDriveTransitionInAmount, 0, 1);


            float oldRotation = warpDriveEffects.transform.GetChild(0).localRotation.eulerAngles.x;
            warpDriveEffects.transform.GetChild(0).localRotation = Quaternion.Euler(oldRotation + (Time.deltaTime * outsideRotation), -90, -90);
            warpDriveEffects.transform.GetChild(0).GetComponent<MeshRenderer>().material.SetFloat("_Transition", warpDriveTransitionInAmount);

            oldRotation = warpDriveEffects.transform.GetChild(1).localRotation.eulerAngles.x;
            warpDriveEffects.transform.GetChild(1).localRotation = Quaternion.Euler(oldRotation + (Time.deltaTime * middleRotation), -90, -90);
            warpDriveEffects.transform.GetChild(1).GetComponent<MeshRenderer>().material.SetFloat("_Transition", warpDriveTransitionInAmount);

            oldRotation = warpDriveEffects.transform.GetChild(2).localRotation.eulerAngles.x;
            warpDriveEffects.transform.GetChild(2).localRotation = Quaternion.Euler(oldRotation + (Time.deltaTime * insideRotation), -90, -90);
            warpDriveEffects.transform.GetChild(2).GetComponent<MeshRenderer>().material.SetFloat("_Transition", warpDriveTransitionInAmount);
        }


        //If the game is "loading"
        if (isLoading)
        {
            warpDelayTimer += Time.deltaTime;

            //Start fading out when the timer gets to 90% done
            if (warpDelayTimer > (warpDelay * .6f) && !isFading)
            {

                //If a fade overlay was found
                if (fadeOverlay != null)
                {
                    fadeOverlay.GetComponent<FadeControllerScript>().fadeOut();
                }

                isFading = true;
            }

            //Load the next scene
            if (warpDelayTimer > warpDelay)
            {
                StartCoroutine("loadNewScene");
                warpDelayTimer = Mathf.NegativeInfinity;
            }
        }
    }

    public Planet GetCurrentPlanet()
    {
        if (currentPlanetID == Galaxy.ONLINE_PLANET_ID)
        {
            return Galaxy.instance.onlinePlanet;
        }
        return Galaxy.instance.planetList[currentPlanetID];
    }

    public Planet GetDestinationPlanet()
    {
        if (targetPlanetID == Galaxy.ONLINE_PLANET_ID)
        {
            return Galaxy.instance.onlinePlanet;
        }
        if (targetPlanetID < 0 || targetPlanetID > Galaxy.instance.planetList.Count)
        {
            return Galaxy.instance.planetList[0];
        }
        return Galaxy.instance.planetList[targetPlanetID];
    }

    #region Warp Drive Effects
    private void engageWarpDrive()
    {
        //Only spawn the effects if there's a player
        if (player != null)
        {
            //Spawn the effects
            if (warpDriveEffects == null)
            {
                warpDriveEffects = Instantiate(warpDriveEffectsPrefab, player.transform.Find("FirstPersonCharacter"));
                musicPlayerScript.instance.fadeOutMusic(1f);
                AudioSource source = GetComponent<AudioSource>();
                source.Play();
                //See if the warp drive effects should be hidden
                bool shouldCoverWarp = true;
                if (SettingsControllerScript.instance != null)
                {
                    shouldCoverWarp = SettingsControllerScript.instance.shouldDisableFlashingColors();
                }
                else
                {
                    GameObject.DestroyImmediate(warpDriveEffects.transform.Find("flashCover").gameObject);
                }

                if (!shouldCoverWarp)
                {
                    GameObject.DestroyImmediate(warpDriveEffects.transform.Find("flashCover").gameObject);
                }
            }
        }   
    }

    IEnumerator loadNewScene()
    {
        //Save the world
        yield return WorldManager.instance.saveWorld(currentPlanetID);

        //Load the scene
        if (targetPlanetID != Galaxy.HOME_PLANET_ID)
        {
            if (targetPlanetID != Galaxy.ONLINE_PLANET_ID)
            {
                yield return SceneManager.LoadSceneAsync("tiletest2");
            }
            else
            {
                yield return SceneManager.LoadSceneAsync("onlinePlanet");
            }
        }
        else
        {
            yield return SceneManager.LoadSceneAsync("homePlanetScene");
        }

        currentPlanetID = targetPlanetID;
    }

    public void doneLoading()
    {
        isLoading = false;
        GameObject.Destroy(warpDriveEffects);
        warpDriveEffects = null;
    }
    #endregion

    public int getJetpackLevel()
    {
        return jetpackLevel;
    }

    public void setJetpackLevel(int level)
    {
        jetpackLevel = level;
        Galaxy.instance.unlockPlanetByJetpackLevel();
        GameObject canvasObject = GameObject.Find("UICanvas");
        uic = null;
        if (canvasObject != null)
        {
            uic = canvasObject.GetComponent<UIControllerScript>();
        }

        if (uic != null)
        {
            if (uic.currentQuestId == 17)
            {
                if (level > 0)
                {
                    uic.completeQuest();
                }
            }
        }
    }

    public bool isPlayingEffects()
    {
        return warpDriveEffects != null;
    }
}
