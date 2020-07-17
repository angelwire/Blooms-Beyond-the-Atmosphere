using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityStandardAssets.Characters.FirstPerson;

//updated: 11-2-19
//version: .05
//Author: William Jones
/* 
 * Controls the sky along with the day/night cycle
 */
public class SkyController : MonoBehaviour
{
    //Public variables that can be set
    public int cloudCount;
    public GameObject cloudPrefab;
    public float skySize;
    public float skyDepth;
    public float cloudHeight;
    public GameObject player;
    [SerializeField] Color darkColor;
    [SerializeField] Color sunsetColor;
    [SerializeField] Color daylightColor;
    [SerializeField] private GameObject planetHolder;
    [SerializeField] private GameObject[] clouds;
    [SerializeField] private GameObject miniPlanetPrefab;

    //The star skybox
    GameObject stars;

    public GameObject sunlight;

    private GameObject sunDirectionalLight;

    private float daylight = 1f;
    public float worldTime;
    float singleDayTime = 600; //Time for a single day to pass - currently 10 minutes

    float sunDistance;
    Vector3 sunScale;

    float atmosphereStart = 40f;
    float atmosphereEnd = 60f;

    [SerializeField] float atmosphereApplication = 1f;

    // Start is called before the first frame update
    void Start()
    {
        //Set the distance the sun should be from the ground
        sunDistance = GameObject.Find("Sun").transform.position.y;
        sunScale = GameObject.Find("Sun").transform.localScale;
        stars = GameObject.Find("Stars");

        //Spawn the clouds
        /*
        for (int ii = 0; ii < cloudCount; ii += 1)
        {
            GameObject cloudInstance = Instantiate(cloudPrefab, transform);
            cloudInstance.transform.Translate(Random.Range(-skySize, skySize), cloudHeight + Random.Range(-skyDepth, skyDepth), Random.Range(-skySize, skySize));
        }
        */

        //Reset the light
        setDaylight(1);

        //Set the time in the world
        worldTime = 0;

        //Spawn in the selectable planets for warping
        if (Galaxy.instance != null)
        {
            int disabledPlanetID = WarpDriveController.instance.GetComponent<WarpDriveController>().currentPlanetID;
            float planetCount = Galaxy.instance.planetList.Count;
            for (int i = 0; i < planetCount; i++)
            {
                if (i != disabledPlanetID)
                {
                    Vector3 currentGalacticPosition = WarpDriveController.instance.GetComponent<WarpDriveController>().GetCurrentPlanet().getGalacticPosition();
                    Planet currentPlanetInstance = Galaxy.instance.planetList[i];
                    GameObject createdPlanet = Instantiate(miniPlanetPrefab, currentPlanetInstance.getGalacticPosition() - currentGalacticPosition, Quaternion.identity, planetHolder.transform);
                    createdPlanet.GetComponent<PlanetController>().setupPlanet(currentPlanetInstance);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        moveToPlayer();
        rotateSky();
        determineDaylight();
        controlPlanetColor();
    }

    //Move the sky to the player
    private void moveToPlayer()
    {

        if (player != null)
        {
            Vector3 playerPosition = player.GetComponent<FirstPersonController>().GetCameraPosition();
            Vector3 newPosition = new Vector3(playerPosition.x, transform.position.y, playerPosition.z);
            stars.transform.position = playerPosition;
            transform.position = newPosition;

            atmosphereApplication = Mathf.Clamp(((atmosphereStart + atmosphereEnd - player.transform.position.y) / atmosphereEnd), 0f, 1f);

            musicPlayerScript.instance.setMuffle(1- atmosphereApplication);

            planetHolder.transform.localPosition = new Vector3(0, playerPosition.y, 0);
        }
    }

    //Rotate the sky depending on the day
    //Currently only rotates the sun, but this could be used in the future
    //to rotate depending on the player position
    private void rotateSky()
    {
        //Get the rotation in radians (and add 90 degrees)
        float sunRotation = ((worldTime / singleDayTime) * 360) + 90;
        GameObject sun = GameObject.Find("Sun");
        sun.transform.localPosition = new Vector3(Mathf.Cos(sunRotation * Mathf.Deg2Rad) * sunDistance, player.transform.position.y + Mathf.Sin(sunRotation * Mathf.Deg2Rad) * sunDistance, 0);

    }

    //Takes the world time and determines how much daylight there is
    //And then sets the appropriate daylight
    private void determineDaylight()
    {
        //Increment the world time
        worldTime += Time.deltaTime;

        //Loop the world time
        if (worldTime > singleDayTime)
        {
            worldTime -= singleDayTime;
        }

        //Set the clock time (0-1)
        float clockTime = worldTime / singleDayTime;
        clockTime = Mathf.Abs(clockTime - .5f) * 2;
        /*
         * Just a note, the clock time is calculated so that:
         *  0 will equal 1,
         * .5 will equal 0,
         *  1 will equal 1,
         * This is so the daylight will go back and forth between 0 and 1
         * instead of going to 1 and then popping back to 0
         */

        //Set the daylight depending on the clock time
        setDaylight(clockTime);
    }

    //Sets the dalight of the world
    public void setDaylight(float inDaylight)
    {
        //Change the values of things
        //List of things that need to change
        //Shader color - day: rgb(1,1,1) evening: rgb(.5,.5,.5) night:rgb()
        //Cloud color - material color - day:hsv(25,0,100) evening:hsv(25,30,100) night:hsvA(25,0,100,0)
        //Skybox color - material color - day:rgb(1,1,1) night:rgb(.5,.5,.5); 
        //Sun color - material color - day:rgb(1,1,0) night:rgb(1,.5,0)

        daylight = Mathf.Abs(inDaylight);
        //Make the daylight into a cosine wave
        daylight = (-Mathf.Cos(daylight * 3.14f) * .5f) + .5f;

        //Adjust the ambient lighting
        //RenderSettings.ambientLight = Color.Lerp(Color.white, Color.black, daylight);

        float rawSunsetAmount = daylight;
        //Adjust the daylight for the atmosphere
        daylight *= atmosphereApplication;


        //The sunset is at 1 when the day is at the sunset target
        float sunsetTarget = .4f;
        float sunsetAmount = 1 - (Mathf.Abs(daylight - sunsetTarget) / sunsetTarget);
        sunsetAmount *= sunsetAmount; //Square the sunset amount so it doesn't last as long


        Color globalColor = Color.Lerp(Color.Lerp(darkColor, sunsetColor, sunsetAmount), daylightColor, rawSunsetAmount);
        Shader.SetGlobalColor("_GlobalColor", globalColor);

        //Get the light direction
        float sunRotation = Mathf.Deg2Rad * (((worldTime / singleDayTime) * 360) + 90);
        Shader.SetGlobalVector("_GlobalDirection", new Vector3(Mathf.Cos(sunRotation), Mathf.Sin(sunRotation), 0));

        //Set the light's rotation
        sunlight.transform.localRotation = Quaternion.Euler(sunRotation * Mathf.Rad2Deg, -90, 0);
        sunlight.GetComponent<Light>().intensity = rawSunsetAmount;

        //Set the cloud colors
        Color sunsetCloudColor = new Color(1f, .8f, .9f, 1f);
        Color darkCloudColor = new Color(.0f, .0f, .0f, 1);

        //Set the color between the day and night
        Color cloudDayNightColor = Color.Lerp(darkCloudColor, Color.white, rawSunsetAmount);
        Color finalCloudColor = Color.Lerp(cloudDayNightColor, sunsetCloudColor, sunsetAmount);

        //Set all the cloud colors
        foreach (GameObject cloud in clouds)
        {
            cloud.GetComponent<MeshRenderer>().material.SetColor("_Color", finalCloudColor);
            //cloud.GetComponent<MeshRenderer>().material.SetFloat("_Alpha", cloudDayNightColor.a);
        }

        //Set the skybox color
        Color skyDarkColor = new Color(.0f, 0f, 0f);
        RenderSettings.skybox.SetColor("_Tint", Color.Lerp(Color.Lerp(skyDarkColor, sunsetCloudColor, sunsetAmount), Color.white, daylight));
        //Set the starbox color


        //Set the sun color and scale
        float sunScaleAmount = 1 + (.4f * atmosphereApplication);
        Vector3 sunsetScale = new Vector3(sunScale.x * sunScaleAmount, sunScale.y * sunScaleAmount, sunScale.z * sunScaleAmount);
        GameObject sun = GameObject.Find("Sun");

        //Adjust the sunset amount for just the sun
        if (daylight < sunsetTarget)
        {
            sunsetAmount = 1;
        }
        Color sunsetFinalColor = Color.Lerp(new Color(1f, 1f, .2f), new Color(1, .2f, .1f), atmosphereApplication);
        sun.GetComponent<MeshRenderer>().material.color =
            Color.Lerp(Color.yellow, sunsetFinalColor, sunsetAmount);
        float maxStrength = .04f;
        float strength = Mathf.Lerp(0, maxStrength * atmosphereApplication, sunsetAmount);
        sun.GetComponent<MeshRenderer>().material.SetFloat("_WaveStrength", strength);
        sun.transform.localScale = Vector3.Lerp(sunScale, sunsetScale, sunsetAmount);

        for (int ii = 0; ii < stars.transform.childCount; ii += 1)
        {
            stars.transform.GetChild(ii)
                .gameObject.GetComponent<MeshRenderer>().material
                .SetColor("_Color", new Color(1, 1, 1, 1 - daylight));
        }
    }

    /// <summary>
    /// Sets both the atmosphere and the daylight
    /// </summary>
    /// <param name="daylight"></param>
    /// <param name="atmosphere"></param>
    public void setDaylightWithAtmosphere(float inDaylight, float inAtmosphere)
    {
        atmosphereApplication = inAtmosphere;
        setDaylight(inDaylight);
    }

    /// <summary>
    /// Returns a value between the minimim and maximum based on the value.
    /// I wrote this because I couldn't think of what the function was called
    /// </summary>
    /// <param name="inMin"></param>
    /// <param name="inMax"></param>
    /// <param name="inValue"></param>
    /// <returns></returns>
    private float anormalize(float inMin, float inMax, float inValue)
    {
        return ((inMax - inMin) * inValue) + inMin;
    }

    private void controlPlanetColor()
    {
        foreach(GameObject planetObject in GameObject.FindGameObjectsWithTag("planetTag"))
        {
            planetObject.GetComponent<PlanetController>().setPlanetDarkness(Mathf.Min(atmosphereApplication,daylight));
        }
    }
}
