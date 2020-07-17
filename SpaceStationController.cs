using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.CrossPlatformInput;

/*
 * Note:
 * This is called a "space station" class... but there's not any sort of space station
 * It's just an abstract concept for when the player leaves the atmosphere
 * When the player leaves the atmosphere they stop falling and start orbiting the planet
 * So I just call it being attached to the space station
 * even though there isn't a space station in the game
 */

public class SpaceStationController : MonoBehaviour
{

    private FirstPersonController fpc;
    private GameObject fpcCamera;
    [SerializeField] private bool engaged = false;
    [SerializeField] private bool canDisengage = false;
    [SerializeField] private UIControllerScript ui;
    [SerializeField] private LayerMask planetsLayer;
    public GameObject spaceStationPanel;
    public GameObject warpDriveEffects;
    public Slider heldTimeSlider;
    public Image fillImage;
    public Text holdWText;

    float holdTime = 1f;
    [SerializeField] float heldAmount = 0f;

    string returnText = "Hold W to fall ";
    string toPlanetText = "Hold W to launch to ";

    bool isDisplayingAction = false;
    float transitionTime = 3f;

    //Figure out what planet the player is looking at
    int planetLookingAtID = -1;
    bool targetPlanetIsLocked = true;

    SpaceStationPointerController pointerController;
    Camera mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        fpc = GetComponent<FirstPersonController>();
        fpcCamera = fpc.transform.Find("FirstPersonCharacter").gameObject;
        heldTimeSlider.maxValue = holdTime;
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        mainCamera = Camera.main;
        //If the player is connected to the space station
        if (engaged)
        {
            planetLookingAtID = -1;
            Planet planetLookingAt = null;

            //If the player is looking back at the home planet (x rotation between 70 and 110)
            bool lookingAtHomePlanet = (fpcCamera.transform.localRotation.eulerAngles.x > 70);
            lookingAtHomePlanet = (lookingAtHomePlanet && fpcCamera.transform.localRotation.eulerAngles.x < 110);
            
            int currentPlanetID = -2;
            if (WarpDriveController.instance != null)
            {
                currentPlanetID = WarpDriveController.instance.currentPlanetID; ;
            }

            if (lookingAtHomePlanet)
            {
                planetLookingAtID = currentPlanetID;
            }
            else
            {
                //Shoot a ray to see if the player is looking at a planet
                RaycastHit hit;
                //Get the ray
                Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
                //Cast the ray looking for planets
                if (Physics.Raycast(ray, out hit, 40.0f, planetsLayer, QueryTriggerInteraction.Collide))
                {
                    planetLookingAt = hit.collider.gameObject.GetComponent<PlanetController>().getPlanet();
                    planetLookingAtID = planetLookingAt.getID();
                }
            }

            //If the player is looking at a planet
            if (planetLookingAtID > -1)
            {
                targetPlanetIsLocked = false;
                //Set the text depending on where the player is looking
                if (planetLookingAtID == currentPlanetID)
                {
                    holdWText.text = returnText;
                }
                else if (planetLookingAt != null)
                {
                    if (planetLookingAt.planetIsLocked())
                    {
                        holdWText.text = "Jetpack level [" + planetLookingAt.getID() + "] required";
                        targetPlanetIsLocked = true;
                    }
                    else
                    {
                        holdWText.text = toPlanetText + planetLookingAt.getDescriptionText();
                    }
                }
                //Show the action prompt
                displayAction(true);
            }
            else
            {
                //The player isn't looking at a planet so hide the action
                displayAction(false);
            }

            //Figure out what actions the player is trying to take
            getPlayerActions();
        }
        else
        {
            //If the player isn't "attached" to the "space station"
            //Hide the prompts and reset other variables
            displayAction(false);
            canDisengage = false;
            heldAmount = 0;
        }

        if (ui.isPaused)
        {
            displayAction(false);
        }
    }

    public void displayAction(bool display)
    {
        if (heldTimeSlider.gameObject.activeSelf != display)
        {
            //display action or don't display action
            heldTimeSlider.gameObject.SetActive(display);
            isDisplayingAction = display;
        }
    }

    public void getPlayerActions()
    {
        //If the player is trying to go somewhere
        if (Input.GetKey(KeyCode.W) && planetLookingAtID != -1 && !ui.isPaused)
        {
            if (!targetPlanetIsLocked)
            {
                int currentPlanetID = -2;
                if (WarpDriveController.instance != null)
                {
                    currentPlanetID = WarpDriveController.instance.currentPlanetID; ;
                }

                //If the player is allowed to disengage
                if (canDisengage)
                {
                    //Increase the timer and update the slider
                    heldAmount += Time.deltaTime;
                    heldTimeSlider.value = heldAmount;
                    float alpha = Mathf.Pow(heldTimeSlider.normalizedValue, .5f);
                    fillImage.color = new Color(1, 1, 1, alpha);
                    //Disengage when the timer goes off
                    if (heldAmount > holdTime)
                    {
                        if (planetLookingAtID == currentPlanetID)
                        {
                            disengage();
                        }
                        else
                        {
                            heldAmount = -1000f;
                            WarpDriveController.instance.goToPlanet(planetLookingAtID);
                            ui.setSpaceStation(false);
                            ui.hidePlayerInventory();
                        }
                    }
                }
            }
        }
        else
        {
            //When the player lets off the W key
            //Allow the player to disengage
            canDisengage = true;
            //Reset the held timer and set the slider values
            heldAmount = 0;
            heldTimeSlider.value = heldAmount;
            float alpha = Mathf.Pow(heldTimeSlider.normalizedValue, .5f);
            fillImage.color = new Color(1, 1, 1, alpha);
        }
    }

    public void engage()
    {
        ui.setSpaceStation(true);
        isDisplayingAction = true;
        displayAction(false);
        engaged = true;
    }

    public void disengage()
    {
        ui.setSpaceStation(false);
        displayAction(false);
        fpc.undock();
        engaged = false;
    }

    public bool isEngaged()
    {
        return engaged;
    }
}
